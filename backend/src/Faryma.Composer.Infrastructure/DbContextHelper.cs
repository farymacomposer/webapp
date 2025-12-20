using Faryma.Composer.Contracts.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Faryma.Composer.Infrastructure
{
    public static class DbContextHelper
    {
        public const string SchemaName = "app";

        public static string? GetConnectionString(IConfiguration configuration)
        {
            PostgreOptions? options = configuration.GetSection("POSTGRES").Get<PostgreOptions>();

            return options?.GetConnectionString();
        }

        // Основной способ добавления enum в БД
        public static NpgsqlDbContextOptionsBuilder MapEnum(this NpgsqlDbContextOptionsBuilder builder)
        {
            return builder
                .MapEnum<ComposerStreamStatus>("composer_stream_status", SchemaName)
                .MapEnum<ComposerStreamType>("composer_stream_type", SchemaName)
                .MapEnum<OrderCategoryType>("order_category_type", SchemaName)
                .MapEnum<ReviewOrderStatus>("review_order_status", SchemaName)
                .MapEnum<ReviewOrderType>("review_order_type", SchemaName)
                .MapEnum<TransactionType>("transaction_type", SchemaName);
        }

        // Вспомогательный метод для выравнивания enum в БД по номеру, а не по названию
        public static void HasPostgresEnum(this ModelBuilder builder)
        {
            builder
                .HasPostgresEnum<ComposerStreamStatus>(SchemaName, "composer_stream_status")
                .HasPostgresEnum<ComposerStreamType>(SchemaName, "composer_stream_type")
                .HasPostgresEnum<OrderCategoryType>(SchemaName, "order_category_type")
                .HasPostgresEnum<ReviewOrderStatus>(SchemaName, "review_order_status")
                .HasPostgresEnum<ReviewOrderType>(SchemaName, "review_order_type")
                .HasPostgresEnum<TransactionType>(SchemaName, "transaction_type");
        }
    }
}