using Faryma.Composer.Infrastructure.Enums;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Faryma.Composer.Infrastructure
{
    public static class DbContextHelper
    {
        public const string ComposerStreamStatusEnum = "app.composer_stream_status";
        public const string ComposerStreamTypeEnum = "app.composer_stream_type";
        public const string OrderCategoryTypeEnum = "app.order_category_type";
        public const string ReviewOrderStatusEnum = "app.review_order_status";
        public const string ReviewOrderTypeEnum = "app.review_order_type";
        public const string TransactionTypeEnum = "app.transaction_type";

        public static NpgsqlDataSource GetDataSource(IConfiguration configuration)
        {
            PostgreOptions? options = configuration.GetSection("POSTGRES").Get<PostgreOptions>();
            string? connectionString = options?.GetConnectionString();

            NpgsqlDataSourceBuilder builder = new(connectionString);

            builder.MapEnum<ComposerStreamStatus>(ComposerStreamStatusEnum);
            builder.MapEnum<ComposerStreamType>(ComposerStreamTypeEnum);
            builder.MapEnum<OrderCategoryType>(OrderCategoryTypeEnum);
            builder.MapEnum<ReviewOrderStatus>(ReviewOrderStatusEnum);
            builder.MapEnum<ReviewOrderType>(ReviewOrderTypeEnum);
            builder.MapEnum<TransactionType>(TransactionTypeEnum);

            return builder.Build();
        }

        public static NpgsqlDbContextOptionsBuilder MapEnum(this NpgsqlDbContextOptionsBuilder builder)
        {
            return builder
                .MapEnum<ComposerStreamStatus>("composer_stream_status", "app")
                .MapEnum<ComposerStreamType>("composer_stream_type", "app")
                .MapEnum<OrderCategoryType>("order_category_type", "app")
                .MapEnum<ReviewOrderStatus>("review_order_status", "app")
                .MapEnum<ReviewOrderType>("review_order_type", "app")
                .MapEnum<TransactionType>("transaction_type", "app");
        }
    }
}