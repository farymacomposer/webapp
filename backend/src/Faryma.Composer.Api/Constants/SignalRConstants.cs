using Faryma.Composer.Api.Features.OrderQueueFeature;

namespace Faryma.Composer.Api.Constants
{
    public static class SignalRConstants
    {
        public const string SignalRHubAsyncApiServer = "signalr-hub";
        public const string OrderQueueHubPath = $"/api/{nameof(OrderQueueNotificationHub)}";
    }
}