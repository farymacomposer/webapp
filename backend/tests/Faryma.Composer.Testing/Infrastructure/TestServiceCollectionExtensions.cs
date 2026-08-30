using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Faryma.Composer.Testing.Infrastructure
{
    public static class TestServiceCollectionExtensions
    {
        public static IServiceCollection AddFixedDateTimeContext(this IServiceCollection services, DateTime now)
        {
            services.RemoveAll<DateTimeContext>();
            services.AddSingleton(new DateTimeContext(now));

            return services;
        }

        public static IServiceCollection AddTestOrderQueueNotificationService(this IServiceCollection services)
        {
            services.RemoveAll<IOrderQueueNotificationService>();
            services.AddSingleton<IOrderQueueNotificationService, TestOrderQueueNotificationService>();

            return services;
        }
    }
}
