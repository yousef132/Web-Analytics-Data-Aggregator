using DataAggergator.Application.Abstractions.Reposioties;
using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Infrastructure.Consumers;
using DataAggergator.Infrastructure.Folder;
using DataAggergator.Infrastructure.Implementation.Repositories;
using DataAggergator.Infrastructure.Implementation.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using static MassTransit.MessageHeaders;

namespace DataAggergator.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register application services
            services.AddScoped<IGoogleAnalyticsService, GoogleAnalyticsService>();
            services.AddScoped<IPageSpeedInsightsService, PageSpeedInsightsService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRawDataRepository, RawDataRepository>();
            services.AddScoped<IDailyStatsRepository, DailyStatsRepository>();
            services.AddScoped<IOverViewService, OverViewService>();
            services.AddScoped<IKeycloakService, KeycloakService>();
            services.AddScoped<IUserService, UserService>();
            services.AddHttpClient<IKeycloakService, KeycloakService>();

            // Configure MassTransit with RabbitMQ
            services.AddMassTransit(cfg =>
            {
                cfg.AddConsumer<AnalyticsAggregatorConsumer>();
                cfg.SetKebabCaseEndpointNameFormatter();

                cfg.UsingRabbitMq((context, rabbitCfg) =>
                {
                    rabbitCfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"]!);
                        h.Password(configuration["RabbitMQ:Password"]!);
                    });

                    rabbitCfg.ReceiveEndpoint("analytics.raw.q", e =>
                    {
                        e.ConfigureConsumer<AnalyticsAggregatorConsumer>(context);

                        // Retry policy: 3 attempts, 2=>4=>8
                        e.UseMessageRetry(r => r.Exponential(
                             retryLimit: 3,                         
                             minInterval: TimeSpan.FromSeconds(2),  
                             maxInterval: TimeSpan.FromSeconds(10), 
                             intervalDelta: TimeSpan.FromSeconds(2) 
                         ));

                        // Dead-letter queue
                        e.BindDeadLetterQueue("analytics.dlq");
                    });
                });
            });

            // configure postgres 
            services.AddDbContext<AnalyticsDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("Postgres"),
                    npgsql =>
                    {
                        npgsql.MigrationsAssembly(typeof(AnalyticsDbContext).Assembly.FullName);
                    });
            });

            var seqOptions = configuration.GetSection("Seq").Get<SeqOptions>();
            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Information()
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .WriteTo.Seq(seqOptions.Url)  
                   .CreateLogger();

            // Configure Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var keycloakConfig = configuration.GetSection("Keycloak");

                options.Authority = keycloakConfig["Authority"];
                options.Audience = keycloakConfig["Audience"];
                options.MetadataAddress = keycloakConfig["MetadataAddress"];
                options.RequireHttpsMetadata = bool.Parse(keycloakConfig["RequireHttpsMetadata"] ?? "false");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = bool.Parse(keycloakConfig["ValidateIssuer"] ?? "true"),
                    ValidateAudience = bool.Parse(keycloakConfig["ValidateAudience"] ?? "true"),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = keycloakConfig["Authority"],
                    ValidAudience = keycloakConfig["Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });



            return services;
        }

    }
}
