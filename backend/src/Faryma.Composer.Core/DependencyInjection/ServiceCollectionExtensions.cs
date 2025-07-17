using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Core.Features.ArtistFeature;
using Faryma.Composer.Core.Features.ComposerStreamFeature;
using Faryma.Composer.Core.Features.GenreFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.ReviewOrderFeature;
using Faryma.Composer.Core.Features.TrackFeature;
using Faryma.Composer.Core.Features.UserNicknameFeature;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services
                .AddSingleton<AppSettingsService>()
                .AddSingleton<OrderQueueService>()
                .AddScoped<ArtistService>()
                .AddScoped<ComposerStreamService>()
                .AddScoped<GenreService>()
                .AddScoped<ReviewOrderService>()
                .AddScoped<TrackService>()
                .AddScoped<UserNicknameService>();

            return services;
        }
    }
}