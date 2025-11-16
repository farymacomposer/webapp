using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.ComposerStreamFeature;
using Faryma.Composer.Application.Features.OrderQueueFeature;
using Faryma.Composer.Application.Features.ReviewFeature;
using Faryma.Composer.Application.Features.ReviewOrderFeature;
using Faryma.Composer.Application.Features.UserNicknameFeature;
using Microsoft.Extensions.DependencyInjection;

namespace Faryma.Composer.Application.DependencyInjection
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