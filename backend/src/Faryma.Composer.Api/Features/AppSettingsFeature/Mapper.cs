using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Api.Features.AppSettingsFeature
{
    public static class Mapper
    {
        public static AppSettingsDto Map(AppSettingsEntity item)
        {
            return new()
            {
                ReviewOrderNominalAmount = item.ReviewOrderNominalAmount,
            };
        }

        public static AppSettingsModel Map(AppSettingsDto item)
        {
            return new()
            {
                ReviewOrderNominalAmount = item.ReviewOrderNominalAmount,
            };
        }
    }
}