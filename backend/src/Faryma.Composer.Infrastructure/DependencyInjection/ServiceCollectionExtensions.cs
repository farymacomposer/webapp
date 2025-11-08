using Faryma.Composer.Infrastructure.QueryServices;
using Faryma.Composer.Infrastructure.Repositories.Read;
using Faryma.Composer.Infrastructure.Repositories.ReadWrite;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Faryma.Composer.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            NpgsqlDataSource dataSource = DbContextHelper.GetDataSource(configuration);

            services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(dataSource, npgOptions => npgOptions.MapEnum()));

            services
                .AddDataProtection()
                .PersistKeysToDbContext<AppDbContext>();

            services
                .AddScoped<UnitOfWork>()

                .AddScoped<ComposerStream_R_Repository>()
                .AddScoped<ReviewOrder_R_Repository>()
                .AddScoped<UserNickname_R_Repository>()

                .AddScoped<ComposerStream_RW_Repository>()
                .AddScoped<Review_RW_Repository>()
                .AddScoped<ReviewOrder_RW_Repository>()
                .AddScoped<Transaction_RW_Repository>()
                .AddScoped<UserAccount_RW_Repository>()
                .AddScoped<UserNickname_RW_Repository>();

            services
                .AddScoped<TrackQueryService>();

            return services;
        }
    }
}