using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Testing.Infrastructure
{
    public static class PostgreSqlSchemaInitializer
    {
        public static async Task EnsureCreatedAsync(IConfiguration configuration)
        {
            ServiceCollection services = [];
            services.AddPersistence(configuration);

            await using ServiceProvider provider = services.BuildServiceProvider();
            IDbContextFactory<AppDbContext> factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using AppDbContext context = await factory.CreateDbContextAsync();

            await context.Database.EnsureCreatedAsync();
        }
    }
}
