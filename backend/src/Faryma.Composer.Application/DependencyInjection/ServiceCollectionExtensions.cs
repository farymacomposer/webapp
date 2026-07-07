using Faryma.Composer.Application.Common.Services;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.ComposerStream;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Application.Features.Review;
using Faryma.Composer.Application.Features.ReviewOrder;
using Faryma.Composer.Application.Features.ReviewOrder.Pricing;
using Faryma.Composer.Application.Features.UserNickname;
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
                .AddSingleton<OrderQueueEventChannel>()

                .AddHostedService<OrderQueueBackgroundWorker>()
                .AddHostedService<IdempotencyRecordCleanupService>()

                .AddScoped<ComposerStreamService>()
                .AddScoped<ReviewOrderPricingService>()
                .AddScoped<ReviewOrderService>()
                .AddScoped<UserNicknameService>()
                .AddScoped<ReviewService>();

            return services;
        }
    }
}
