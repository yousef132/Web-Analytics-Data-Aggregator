using DataAggergator.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DataAggergator.Presentation.Extentions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrations(this WebApplication host)
        {
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            await db.Database.OpenConnectionAsync();


            //***** apply centralized lock to avoid multiple replicas applying migrations simultaneously *****
            // the lock will be held until the connection is closed
            // Try to acquire advisory lock
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT pg_try_advisory_lock(123456789);";
            var acquired = (bool)await cmd.ExecuteScalarAsync();

            if (acquired)
            {
                Console.WriteLine("Lock acquired → applying migrations...");
                await db.Database.MigrateAsync();

                // Release lock 
                cmd.CommandText = "SELECT pg_advisory_unlock(123456789);";
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                Console.WriteLine("Could not acquire lock → skipping migrations.");
            }
        }
    }
}
