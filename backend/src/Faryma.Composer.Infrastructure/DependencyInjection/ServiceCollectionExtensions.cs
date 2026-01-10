using Faryma.Composer.Infrastructure.QueryServices;
using Faryma.Composer.Infrastructure.Repositories.Read;
using Faryma.Composer.Infrastructure.Repositories.Write;
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

                .AddScoped<ComposerStreamReadRepository>()
                .AddScoped<ReviewOrderReadRepository>()
                .AddScoped<UserNicknameReadRepository>()

                .AddScoped<ComposerStreamWriteRepository>()
                .AddScoped<ReviewWriteRepository>()
                .AddScoped<ReviewOrderWriteRepository>()
                .AddScoped<TransactionWriteRepository>()
                .AddScoped<UserNicknameWriteRepository>();

            services
                .AddScoped<TrackQueryService>();

            return services;
        }
    }
}