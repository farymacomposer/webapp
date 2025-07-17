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
                .AddScoped<ArtistRepository>()
                .AddScoped<ComposerStreamRepository>()
                .AddScoped<GenreRepository>()
                .AddScoped<ReviewOrderRepository>()
                .AddScoped<ReviewRepository>()
                .AddScoped<TrackRepository>()
                .AddScoped<TransactionRepository>()
                .AddScoped<UserAccountRepository>()
                .AddScoped<UserNicknameRepository>()
                .AddScoped<UserRepository>()
                .AddScoped<UserTrackRatingRepository>();

            return services;
        }
    }
}