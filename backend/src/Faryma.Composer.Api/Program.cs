using Faryma.Composer.Api.Common.DependencyInjection;
using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Application.DependencyInjection;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Serilog;

namespace Faryma.Composer.Api
{
    public partial class Program
    {
        public static async Task Main(string[]? args = null)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                Console.WriteLine("Критическая ошибка" + (e.ExceptionObject as Exception));
                Environment.Exit(1);
            };

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args ?? []);

            builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

            builder.Services
                .AddConfiguration(builder.Configuration)
                .AddPersistenceAndIdentity(builder.Configuration)
                .AddJwtAuthentication()
                .AddAuthorization()
                .AddCoreServices();

            builder.Services.AddPresentationLayer(builder.Environment);

            WebApplication app = builder.Build();

            app.UseExceptionHandler();
            app.UseApiDocumentation();
            app.UseHttpsRedirectionExceptApiDocumentation();

            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<OrderQueueNotificationHub>(IOrderQueueNotificationServer.RoutePattern);

            await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<AdminBootstrapService>().Initialize();
            }

            await app.Services.GetRequiredService<AppSettingsService>().Initialize();
            await app.Services.GetRequiredService<OrderQueueService>().Initialize();
            await app.RunAsync();
        }
    }
}
