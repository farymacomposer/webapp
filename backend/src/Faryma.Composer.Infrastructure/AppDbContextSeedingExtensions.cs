using Faryma.Composer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Infrastructure
{
    public static class AppDbContextSeedingExtensions
    {
        public static DbContextOptionsBuilder UseAppDataSeeding(this DbContextOptionsBuilder options) => options
            .UseSeeding((context, _) => SeedAppSettings((AppDbContext)context))
            .UseAsyncSeeding((context, _, ct) => SeedAppSettingsAsync((AppDbContext)context, ct));

        private static void SeedAppSettings(AppDbContext context)
        {
            if (context.AppSettings.Any())
            {
                return;
            }

            context.AppSettings.Add(CreateDefaultAppSettings());
            context.SaveChanges();
        }

        private static async Task SeedAppSettingsAsync(AppDbContext context, CancellationToken ct)
        {
            if (await context.AppSettings.AnyAsync(ct))
            {
                return;
            }

            context.AppSettings.Add(CreateDefaultAppSettings());
            await context.SaveChangesAsync(ct);
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
