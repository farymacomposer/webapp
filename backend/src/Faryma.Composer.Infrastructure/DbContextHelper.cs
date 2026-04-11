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
                .MapEnum<QueueCategory>("queue_category", SchemaName)
                .MapEnum<ReviewOrderStatus>("review_order_status", SchemaName)
                .MapEnum<ReviewOrderType>("review_order_type", SchemaName)
                .MapEnum<TransactionKind>("transaction_kind", SchemaName)
                .MapEnum<AccountTopUpProvider>("account_top_up_provider", SchemaName);
        }

        // Вспомогательный метод для выравнивания enum в БД по номеру, а не по названию
        public static void HasPostgresEnum(this ModelBuilder builder)
        {
            builder
                .HasPostgresEnum<ComposerStreamStatus>(SchemaName, "composer_stream_status")
                .HasPostgresEnum<ComposerStreamType>(SchemaName, "composer_stream_type")
                .HasPostgresEnum<QueueCategory>(SchemaName, "queue_category")
                .HasPostgresEnum<ReviewOrderStatus>(SchemaName, "review_order_status")
                .HasPostgresEnum<ReviewOrderType>(SchemaName, "review_order_type")
                .HasPostgresEnum<TransactionKind>(SchemaName, "transaction_kind")
                .HasPostgresEnum<AccountTopUpProvider>(SchemaName, "account_top_up_provider");
        }
    }
}