using Faryma.Composer.Infrastructure.QueryServices;
using Faryma.Composer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = ConnectionStringHelper.Get(configuration);
            services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString));

            services
                .AddScoped<UnitOfWork>()
                .AddScoped<TrackArtistRepository>()
                .AddScoped<ComposerStreamRepository>()
                .AddScoped<ReviewOrderRepository>()
                .AddScoped<ReviewRepository>()
                .AddScoped<TrackRepository>()
                .AddScoped<TransactionRepository>()
                .AddScoped<UserAccountRepository>()
                .AddScoped<UserNicknameRepository>()
                .AddScoped<UserRepository>()
                .AddScoped<UserTrackRatingRepository>();

            services
                .AddScoped<TrackQueryService>();

            return services;
        }
    }
}