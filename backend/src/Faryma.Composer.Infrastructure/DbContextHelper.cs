using Faryma.Composer.Domain.Enums;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Faryma.Composer.Infrastructure
{
    public static class DbContextHelper
    {
        public const string SchemaName = "app";
        public const string ComposerStreamStatusEnum = SchemaName + "." + nameof(ComposerStreamStatus);
        public const string ComposerStreamTypeEnum = SchemaName + "." + nameof(ComposerStreamType);
        public const string QueueCategoryEnum = SchemaName + "." + nameof(QueueCategory);
        public const string ReviewOrderStatusEnum = SchemaName + "." + nameof(ReviewOrderStatus);
        public const string ReviewOrderTypeEnum = SchemaName + "." + nameof(ReviewOrderType);
        public const string TransactionKindEnum = SchemaName + "." + nameof(TransactionKind);
        public const string AccountTopUpProviderEnum = SchemaName + "." + nameof(AccountTopUpProvider);
        public const string UserEntitlementTargetEnum = SchemaName + "." + nameof(UserEntitlementTarget);

        public static string? GetConnectionString(IConfiguration configuration)
        {
            PostgreOptions? options = configuration.GetSection("POSTGRES").Get<PostgreOptions>();

            return options?.GetConnectionString();
        }

        // Основной способ добавления enum в БД
        public static NpgsqlDbContextOptionsBuilder MapEnum(this NpgsqlDbContextOptionsBuilder builder)
        {
            return builder
                .MapEnum<ComposerStreamStatus>(nameof(ComposerStreamStatus), SchemaName)
                .MapEnum<ComposerStreamType>(nameof(ComposerStreamType), SchemaName)
                .MapEnum<QueueCategory>(nameof(QueueCategory), SchemaName)
                .MapEnum<ReviewOrderStatus>(nameof(ReviewOrderStatus), SchemaName)
                .MapEnum<ReviewOrderType>(nameof(ReviewOrderType), SchemaName)
                .MapEnum<TransactionKind>(nameof(TransactionKind), SchemaName)
                .MapEnum<AccountTopUpProvider>(nameof(AccountTopUpProvider), SchemaName)
                .MapEnum<UserEntitlementTarget>(nameof(UserEntitlementTarget), SchemaName);
        }

        // Вспомогательный метод для выравнивания enum в БД по номеру, а не по названию
        public static void HasPostgresEnum(this ModelBuilder builder)
        {
            builder
                .HasPostgresEnum<ComposerStreamStatus>(SchemaName, nameof(ComposerStreamStatus))
                .HasPostgresEnum<ComposerStreamType>(SchemaName, nameof(ComposerStreamType))
                .HasPostgresEnum<QueueCategory>(SchemaName, nameof(QueueCategory))
                .HasPostgresEnum<ReviewOrderStatus>(SchemaName, nameof(ReviewOrderStatus))
                .HasPostgresEnum<ReviewOrderType>(SchemaName, nameof(ReviewOrderType))
                .HasPostgresEnum<TransactionKind>(SchemaName, nameof(TransactionKind))
                .HasPostgresEnum<AccountTopUpProvider>(SchemaName, nameof(AccountTopUpProvider))
                .HasPostgresEnum<UserEntitlementTarget>(SchemaName, nameof(UserEntitlementTarget));
        }
    }
}
