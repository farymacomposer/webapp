using Faryma.Composer.Contracts.Application.Features.AppSettings;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Application.Features.AppSettings
{
    /// <summary>
    /// Сервис параметров приложения
    /// </summary>
    public sealed class AppSettingsService(IDbContextFactory<AppDbContext> contextFactory)
    {
        public AppSettingsEntity Settings { get; private set; } = null!;

        public async Task Initialize()
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();

            Settings = await context.AppSettings
                .AsNoTracking()
                .SingleAsync();
        }

        public async Task<AppSettingsEntity> Update(AppSettingsModel item, CancellationToken ct)
        {
            if (Settings.ReviewOrderNominalAmount == item.ReviewOrderNominalAmount
                && Settings.ReviewOrderExtraTimeAmountPerSecond == item.ReviewOrderExtraTimeAmountPerSecond
                && Settings.ReviewOrderDetailedReviewAmount == item.ReviewOrderDetailedReviewAmount)
            {
                return Settings;
            }

            AppSettingsEntity entity = Clone(Settings);
            entity.ReviewOrderNominalAmount = item.ReviewOrderNominalAmount;
            entity.ReviewOrderExtraTimeAmountPerSecond = item.ReviewOrderExtraTimeAmountPerSecond;
            entity.ReviewOrderDetailedReviewAmount = item.ReviewOrderDetailedReviewAmount;

            await Save(entity, ct);
            Settings = entity;

            return entity;
        }

        private static AppSettingsEntity Clone(AppSettingsEntity item)
        {
            return new()
            {
                Id = item.Id,
                ReviewOrderNominalAmount = item.ReviewOrderNominalAmount,
                ReviewOrderExtraTimeAmountPerSecond = item.ReviewOrderExtraTimeAmountPerSecond,
                ReviewOrderDetailedReviewAmount = item.ReviewOrderDetailedReviewAmount,
            };
        }

        private async Task Save(AppSettingsEntity item, CancellationToken ct)
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            context.Update(item);
            await context.SaveChangesAsync(ct);
        }
    }
}
