using Faryma.Composer.Infrastructure.Persistence.Queries;
using Faryma.Composer.Infrastructure.Persistence.Stores;
using Faryma.Composer.Infrastructure.QueryServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = DbContextHelper.GetConnectionString(configuration);

            services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString, npgOptions => npgOptions.MapEnum()));

            services
                .AddDataProtection()
                .PersistKeysToDbContext<AppDbContext>();

            services
                .AddScoped<UnitOfWork>()

                .AddScoped<ComposerStreamQueries>()
                .AddScoped<ReviewOrderQueries>()
                .AddScoped<UserNicknameQueries>()

                .AddScoped<ComposerStreamStore>()
                .AddScoped<ReviewStore>()
                .AddScoped<ReviewOrderStore>()
                .AddScoped<TransactionStore>()
                .AddScoped<UserNicknameStore>();

            services
                .AddScoped<TrackQueryService>();

            return services;
        }
    }
}