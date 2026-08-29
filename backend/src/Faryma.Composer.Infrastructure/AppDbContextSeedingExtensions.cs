using Faryma.Composer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure
{
    public static class AppDbContextSeedingExtensions
    {
        public static DbContextOptionsBuilder UseAppDataSeeding(this DbContextOptionsBuilder options) => options
            .UseSeeding((appDbContext, _) => SeedAppSettings((AppDbContext)appDbContext))
            .UseAsyncSeeding((appDbContext, _, ct) => SeedAppSettingsAsync((AppDbContext)appDbContext, ct));

        private static void SeedAppSettings(AppDbContext appDbContext)
        {
            if (appDbContext.AppSettings.Any())
            {
                return;
            }

            appDbContext.AppSettings.Add(CreateDefaultAppSettings());
            appDbContext.SaveChanges();
        }

        private static async Task SeedAppSettingsAsync(AppDbContext appDbContext, CancellationToken ct)
        {
            if (await appDbContext.AppSettings.AnyAsync(ct))
            {
                return;
            }

            appDbContext.AppSettings.Add(CreateDefaultAppSettings());
            await appDbContext.SaveChangesAsync(ct);
        }

        private static AppSettingsEntity CreateDefaultAppSettings()
        {
            return new()
            {
                ReviewOrderNominalPrice = 1000,
                IncludedTrackDurationSeconds = 60 * 5,
                ReviewOrderExtraTrackSecondPrice = 3,
                ReviewOrderDetailedPrice = 1000,
            };
        }
    }
}
