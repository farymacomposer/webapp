using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Faryma.Composer.Infrastructure
{
    public static class DbContextHelper
    {
        public const string SchemaName = "app";

        public const string ComposerStreamStatusEnum = SchemaName + "." + _composerStreamStatus;
        public const string ComposerStreamTypeEnum = SchemaName + "." + _composerStreamType;
        public const string OrderCategoryTypeEnum = SchemaName + "." + _orderCategoryType;
        public const string ReviewOrderStatusEnum = SchemaName + "." + _reviewOrderStatus;
        public const string ReviewOrderTypeEnum = SchemaName + "." + _reviewOrderType;
        public const string TransactionTypeEnum = SchemaName + "." + _transactionType;

        private const string _composerStreamStatus = "composer_stream_status";
        private const string _composerStreamType = "composer_stream_type";
        private const string _orderCategoryType = "order_category_type";
        private const string _reviewOrderStatus = "review_order_status";
        private const string _reviewOrderType = "review_order_type";
        private const string _transactionType = "transaction_type";

        public static string? GetConnectionString(IConfiguration configuration)
        {
            PostgreOptions? options = configuration.GetSection("POSTGRES").Get<PostgreOptions>();

            return options?.GetConnectionString();
        }

        public static NpgsqlDbContextOptionsBuilder MapEnum(this NpgsqlDbContextOptionsBuilder builder)
        {
            return builder
                .MapEnum<ComposerStreamStatus>(_composerStreamStatus, SchemaName)
                .MapEnum<ComposerStreamType>(_composerStreamType, SchemaName)
                .MapEnum<OrderCategoryType>(_orderCategoryType, SchemaName)
                .MapEnum<ReviewOrderStatus>(_reviewOrderStatus, SchemaName)
                .MapEnum<ReviewOrderType>(_reviewOrderType, SchemaName)
                .MapEnum<TransactionType>(_transactionType, SchemaName);
        }

        public static void HasPostgresEnum(this ModelBuilder builder)
        {
            builder
                .HasPostgresEnum<ComposerStreamStatus>(SchemaName, _composerStreamStatus)
                .HasPostgresEnum<ComposerStreamType>(SchemaName, _composerStreamType)
                .HasPostgresEnum<OrderCategoryType>(SchemaName, _orderCategoryType)
                .HasPostgresEnum<ReviewOrderStatus>(SchemaName, _reviewOrderStatus)
                .HasPostgresEnum<ReviewOrderType>(SchemaName, _reviewOrderType)
                .HasPostgresEnum<TransactionType>(SchemaName, _transactionType);
        }
    }
}