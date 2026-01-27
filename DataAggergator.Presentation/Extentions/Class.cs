using DataAggergator.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DataAggergator.Presentation.Extentions
{
    public static class MigrationExtensions
    {
        public static async Task<IHost> ApplyMigrations(this WebApplication host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var dbContext = services.GetRequiredService<AnalyticsDbContext>();
                await dbContext.Database.MigrateAsync(); 
                Console.WriteLine("Database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
                throw; 
            }

            return host;
        }
    }
}
