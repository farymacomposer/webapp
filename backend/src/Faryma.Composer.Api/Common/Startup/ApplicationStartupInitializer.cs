using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;

namespace Faryma.Composer.Api.Common.Startup
{
    public interface IApplicationStartupInitializer
    {
        Task Initialize(IServiceProvider services);
    }

    public sealed class ApplicationStartupInitializer : IApplicationStartupInitializer
    {
        public async Task Initialize(IServiceProvider services)
        {
            await using (AsyncServiceScope scope = services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<AdminBootstrapService>().Initialize();
            }

            await services.GetRequiredService<AppSettingsService>().Initialize();
            await services.GetRequiredService<OrderQueueService>().Initialize();
        }
    }
}
