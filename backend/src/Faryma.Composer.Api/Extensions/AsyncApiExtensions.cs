using Faryma.Composer.Api.Constants;
using Faryma.Composer.Api.Features.OrderQueueFeature;
using Saunter;
using Saunter.AsyncApiSchema.v2;

namespace Faryma.Composer.Api.Extensions
{
    public static class AsyncApiExtensions
    {
        public static IServiceCollection AddAsyncApiSpecification(this IServiceCollection services)
        {
            services.AddAsyncApiSchemaGeneration(options =>
            {
                options.AssemblyMarkerTypes = new[] { typeof(OrderQueueNotificationService) };
                options.AsyncApi = new AsyncApiDocument
                {
                    Info = new Info("Faryma Composer API", "1.0.0")
                    {
                        Description = "Async API для системы управления очередью заказов"
                    },
                    Servers =
                    {
                        [SignalRConstants.SignalRHubAsyncApiServer] = new Server(SignalRConstants.OrderQueueHubPath, "websocket")
                        {
                            Description = "SignalR WebSocket Hub для уведомлений о состоянии очереди заказов"
                        }
                    }
                };
            });

            return services;
        }

        public static void UseCustomAsyncApi(this WebApplication app)
        {
            app.MapAsyncApiDocuments();
            app.MapAsyncApiUi();
        }
    }
}