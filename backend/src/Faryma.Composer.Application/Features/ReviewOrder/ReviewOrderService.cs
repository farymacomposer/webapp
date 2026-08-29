using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Domain.Entities;

namespace Faryma.Composer.Application.Features.ReviewOrder
{
    public sealed class ReviewOrderService(AppSettingsService appSettingsService)
    {
        public long GetTrackRequiredAmount(int? trackDurationSeconds)
        {
            AppSettingsEntity settings = appSettingsService.Settings;
            long result = settings.ReviewOrderNominalPrice;

            if (trackDurationSeconds > settings.IncludedTrackDurationSeconds)
            {
                int extraTrackSeconds = trackDurationSeconds.Value - settings.IncludedTrackDurationSeconds;
                result += extraTrackSeconds * settings.ReviewOrderExtraTrackSecondPrice;
            }

            return result;
        }
    }
}
