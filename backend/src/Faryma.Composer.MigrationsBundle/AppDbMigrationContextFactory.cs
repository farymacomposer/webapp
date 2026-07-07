using Faryma.Composer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Faryma.Composer.MigrationsBundle
{
    public class AppDbMigrationContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddUserSecrets<AppDbMigrationContextFactory>()
                .AddEnvironmentVariables()
                .Build();

            DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();

            optionsBuilder.UseNpgsql(DbContextHelper.GetConnectionString(configuration), npgOptions => npgOptions
                .MapEnum()
                .MigrationsHistoryTable("__EFMigrationsHistory", DbContextHelper.SchemaName)
                .MigrationsAssembly(typeof(AppDbMigrationContextFactory).Assembly.GetName().Name));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
