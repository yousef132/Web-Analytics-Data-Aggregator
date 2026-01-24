using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DataAggergator.Infrastructure;
using DataAggergator.Domain.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Reflection;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAsyncPolicy _publishPolicy;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
    private readonly int _batchSize = 100; // Process messages in batches

    public OutboxPublisherWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _publishPolicy = CreateResiliencyPolicy();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Outbox publisher worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                Log.Information("Outbox publisher worker is stopping");
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Outbox publisher worker encountered an error");
                // Back off on worker-level failure before retrying
                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }

        Log.Information("Outbox publisher worker stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();
        var assembly = Assembly.GetExecutingAssembly();

        // Fetch messages in batches to avoid memory issues
        var messages = await context.OutboxMessages
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            // No messages to process, wait before polling again
            await Task.Delay(_pollingInterval, cancellationToken);
            return;
        }

        int processedCount = 0;
        int failedCount = 0;
        bool circuitOpen = false;

        foreach (var outboxMessage in messages)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var result = await ProcessSingleMessageAsync(outboxMessage, bus, cancellationToken, assembly);

                if (result.success)
                {
                    context.OutboxMessages.Remove(outboxMessage);
                    processedCount++;
                }
                else
                {
                    failedCount++;

                    if (result.CircuitOpen)
                    {
                        circuitOpen = true;
                        Log.Warning("Circuit breaker is open, stopping processing of remaining messages");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                Log.Error(ex, "Failed handling outbox message {Id}", outboxMessage.Id);
            }
        }

        if (processedCount > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            Log.Information("Processed {ProcessedCount} outbox messages. Failed: {FailedCount}",
                processedCount, failedCount);
        }

        // If circuit is open, wait longer before next attempt
        if (circuitOpen)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
        else if (processedCount == _batchSize)
        {
            // If we processed a full batch, there might be more messages
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }
        else
        {
            // Wait for normal polling interval
            await Task.Delay(_pollingInterval, cancellationToken);
        }
    }

    private async Task<MessageProcessResult> ProcessSingleMessageAsync(
        OutboxMessage outboxMessage,
        IBus bus,
        CancellationToken cancellationToken,
        Assembly assembly)
    {
        var message = DeserializeMessage(outboxMessage, assembly);
        if (message == null)
        {
            return MessageProcessResult.Failure(false);
        }

        try
        {
            await _publishPolicy.ExecuteAsync(async ct =>
            {
                await bus.Publish(message, ct);
            }, cancellationToken);

            Log.Debug("Published outbox message {Id} to bus", outboxMessage.Id);
            return MessageProcessResult.Success();
        }
        catch (BrokenCircuitException bce)
        {
            Log.Warning(bce, "Circuit breaker is open for outbox message {Id}", outboxMessage.Id);
            return MessageProcessResult.Failure(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to publish outbox message {Id} after retries", outboxMessage.Id);
            return MessageProcessResult.Failure(false);
        }
    }

    private object? DeserializeMessage(OutboxMessage outboxMessage,Assembly assembly)
    {
        try
        {
            var messageType = Type.GetType(outboxMessage.Type);
            var deserializedMessage = JsonSerializer.Deserialize(outboxMessage.Content, messageType);
            if (deserializedMessage == null)
            {
                Log.Warning("Deserialized outbox message {Id} is null", outboxMessage.Id);
            }

            return deserializedMessage;
        }
        catch (JsonException jsonEx)
        {
            Log.Error(jsonEx, "Failed to deserialize outbox message {Id}", outboxMessage.Id);
            return null;
        }
    }

    private static IAsyncPolicy CreateResiliencyPolicy()
    {
        // Exponential backoff retry with jitter
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt))
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 250)),
                onRetry: (exception, timespan, attempt, context) =>
                {
                    Log.Warning(exception,
                        "Retry {Attempt} after {Delay} while publishing outbox message",
                        attempt, timespan);
                });

        // Circuit breaker: break after 5 consecutive failures, keep open for 30s
        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) =>
                {
                    Log.Warning(ex, "Outbox publisher circuit opened for {BreakDelay}", breakDelay);
                },
                onReset: () =>
                {
                    Log.Information("Outbox publisher circuit reset");
                },
                onHalfOpen: () =>
                {
                    Log.Information("Outbox publisher circuit is half-open — testing");
                });

        // Wrap policies: retry first, then circuit breaker
        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    private record MessageProcessResult(bool success, bool CircuitOpen)
    {
        public static MessageProcessResult Success() => new(true, false);
        public static MessageProcessResult Failure(bool circuitOpen) => new(false, circuitOpen);
    }
}