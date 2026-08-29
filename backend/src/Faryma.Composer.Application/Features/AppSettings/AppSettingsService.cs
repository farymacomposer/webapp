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
            await using AppDbContext appDbContext = await contextFactory.CreateDbContextAsync();

            Settings = await appDbContext.AppSettings
                .AsNoTracking()
                .SingleAsync();
        }

        public async Task<AppSettingsEntity> Update(AppSettingsEntity dto, CancellationToken ct)
        {
            await using AppDbContext appDbContext = await contextFactory.CreateDbContextAsync(ct);

            AppSettingsEntity entity = await appDbContext.AppSettings.SingleAsync(ct);

            entity.ReviewOrderNominalPrice = dto.ReviewOrderNominalPrice;
            entity.IncludedTrackDurationSeconds = dto.IncludedTrackDurationSeconds;
            entity.ReviewOrderExtraTrackSecondPrice = dto.ReviewOrderExtraTrackSecondPrice;
            entity.ReviewOrderDetailedPrice = dto.ReviewOrderDetailedPrice;

            await appDbContext.SaveChangesAsync(ct);

            Settings = entity;

            return entity;
        }
    }
}
