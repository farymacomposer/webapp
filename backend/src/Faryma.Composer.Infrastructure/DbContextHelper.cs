using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Faryma.Composer.Infrastructure
{
    public static class DbContextHelper
    {
        public const string ComposerStreamStatusEnum = "composer_stream_status";
        public const string ComposerStreamTypeEnum = "composer_stream_type";
        public const string OrderCategoryTypeEnum = "order_category_type";
        public const string ReviewOrderStatusEnum = "review_order_status";
        public const string ReviewOrderTypeEnum = "review_order_type";
        public const string TransactionTypeEnum = "transaction_type";

        public static string? GetConnectionString(IConfiguration configuration)
        {
            PostgreOptions? options = configuration.GetSection("POSTGRES").Get<PostgreOptions>();

            return options?.GetConnectionString();
        }

        public static NpgsqlDbContextOptionsBuilder MapEnum(this NpgsqlDbContextOptionsBuilder builder)
        {
            return builder
                .MapEnum<ComposerStreamStatus>(ComposerStreamStatusEnum)
                .MapEnum<ComposerStreamType>(ComposerStreamTypeEnum)
                .MapEnum<OrderCategoryType>(OrderCategoryTypeEnum)
                .MapEnum<ReviewOrderStatus>(ReviewOrderStatusEnum)
                .MapEnum<ReviewOrderType>(ReviewOrderTypeEnum)
                .MapEnum<TransactionType>(TransactionTypeEnum);
        }
    }
}