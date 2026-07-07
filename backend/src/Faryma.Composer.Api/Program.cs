using Faryma.Composer.Api.Common.DependencyInjection;
using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Common.Startup;
using Faryma.Composer.Api.Contracts.Features.OrderQueue;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Application.DependencyInjection;
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
                .AddApiAuthentication()
                .AddAuthorization()
                .AddCoreServices()
                .AddPresentationLayer(builder.Environment);

            WebApplication app = builder.Build();

            app.UseExceptionHandler();
            app.UseForwardedHeaders();
            app.UseApiDocumentation();
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirectionExceptApiDocumentation();

            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<OrderQueueNotificationHub>(IOrderQueueNotificationServer.RoutePattern);

            await app.Services.GetRequiredService<IApplicationStartupInitializer>().Initialize(app.Services);
            await app.RunAsync();
        }
    }
}
