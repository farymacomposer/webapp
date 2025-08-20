using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Core.Features.ComposerStreamFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.ReviewFeature;
using Faryma.Composer.Core.Features.ReviewOrderFeature;
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

                .AddScoped<ComposerStreamService>()
                .AddScoped<ReviewOrderService>()
                .AddScoped<UserNicknameService>()
                .AddScoped<ReviewService>();

            return services;
        }
    }
}