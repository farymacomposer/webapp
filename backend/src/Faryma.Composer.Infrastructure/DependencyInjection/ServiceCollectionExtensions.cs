using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.QueryServices;
using Faryma.Composer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Faryma.Composer.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            string? connectionString = ConnectionStringHelper.Get(configuration);
            services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString, NpgsqlOptionsAction));

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

        public static void NpgsqlOptionsAction(this NpgsqlDbContextOptionsBuilder builder)
        {
            builder.MigrationsHistoryTable("__EFMigrationsHistory", "app");

            builder
                .MapEnum<ComposerStreamStatus>("composer_stream_status")
                .MapEnum<ComposerStreamType>("composer_stream_type")
                .MapEnum<OrderCategoryType>("order_category_type")
                .MapEnum<ReviewOrderStatus>("review_order_status")
                .MapEnum<ReviewOrderType>("review_order_type")
                .MapEnum<TransactionType>("transaction_type");
        }
    }
}