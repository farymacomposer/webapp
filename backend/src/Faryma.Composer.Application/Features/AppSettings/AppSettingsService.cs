using Faryma.Composer.Domain.Entities;
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

        public async Task<AppSettingsEntity> Update(AppSettingsEntity dto, CancellationToken ct)
        {
            await using AppDbContext context = await contextFactory.CreateDbContextAsync(ct);

            AppSettingsEntity entity = await context.AppSettings.SingleAsync(ct);

            entity.ReviewOrderNominalPrice = dto.ReviewOrderNominalPrice;
            entity.IncludedTrackDurationSeconds = dto.IncludedTrackDurationSeconds;
            entity.ReviewOrderExtraTrackSecondPrice = dto.ReviewOrderExtraTrackSecondPrice;
            entity.ReviewOrderDetailedPrice = dto.ReviewOrderDetailedPrice;

            await context.SaveChangesAsync(ct);

            Settings = entity;

            return entity;
        }
    }
}
