using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.AppSettings
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
            Settings = await context.AppSettings.SingleAsync();
        }

        public async Task<AppSettingsEntity> Update(AppSettingsModel item)
        {
            if (Settings.ReviewOrderNominalAmount == item.ReviewOrderNominalAmount)
            {
                return Settings;
            }

            AppSettingsEntity entity = Clone(Settings);
            entity.ReviewOrderNominalAmount = item.ReviewOrderNominalAmount;

            await Save(entity);
            Settings = entity;

            return entity;
        }

        private static AppSettingsEntity Clone(AppSettingsEntity item)
        {
            return new()
            {
                Id = item.Id,
                ReviewOrderNominalAmount = item.ReviewOrderNominalAmount,
            };
        }

        private async Task Save(AppSettingsEntity item)
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync();
            context.Update(item);
            await context.SaveChangesAsync();
        }
    }
}