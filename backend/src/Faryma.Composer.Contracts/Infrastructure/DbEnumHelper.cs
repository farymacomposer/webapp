using Faryma.Composer.Contracts.Infrastructure.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Faryma.Composer.Contracts.Infrastructure
{
    public static class DbEnumHelper
    {
        public const string SchemaName = "app";

        public const string ComposerStreamStatusEnum = "app.composer_stream_status";
        public const string ComposerStreamTypeEnum = "app.composer_stream_type";
        public const string OrderCategoryTypeEnum = "app.order_category_type";
        public const string ReviewOrderStatusEnum = "app.review_order_status";
        public const string ReviewOrderTypeEnum = "app.review_order_type";
        public const string TransactionTypeEnum = "app.transaction_type";

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