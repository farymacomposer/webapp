using Mediator;

namespace Faryma.Composer.Application.Test.Infrastructure
{
    internal static class TestServiceProviderExtensions
    {
        public static Task<TResponse> Send<TResponse>(this IServiceProvider services, IRequest<TResponse> request) =>
            services.GetRequiredService<ISender>().Send(request).AsTask();
    }
}
