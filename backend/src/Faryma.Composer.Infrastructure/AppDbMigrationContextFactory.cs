using Faryma.Composer.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Faryma.Composer.Infrastructure
{
    public class AppDbMigrationContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddUserSecrets<AppDbContext>()
                .AddEnvironmentVariables()
                .Build();

            PostgreOptions? options = configuration.GetSection("POSTGRES").Get<PostgreOptions>();
            string? connectionString = options?.GetConnectionString();

            DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
            optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "app"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}