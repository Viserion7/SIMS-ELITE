using Microsoft.EntityFrameworkCore;
using sims_aggregator.Models;

namespace sims_aggregator.Data
{
    public class IncidentContext: DbContext
    {
        public DbSet<Incident> Incidents { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var db_host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
            var db_user = Environment.GetEnvironmentVariable("POSTGRES_USER");
            var db_password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
            var db_database = Environment.GetEnvironmentVariable("POSTGRES_DB");

            var connectionString = $"Host={db_host};Username={db_user};Password={db_password};Database={db_database};";

            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Versuche es bis zu 5-mal mit kurzer Pause, falls Postgres noch hochfährt
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(3),
                    errorCodesToAdd: null);
            });
        }
    }
}
