using Faryma.Composer.Api.Common.DependencyInjection;
using Faryma.Composer.Api.Common.Extensions;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Application.DependencyInjection;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace Faryma.Composer.Api
{
    public static class Program
    {
        public static async Task Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                Console.WriteLine("Критическая ошибка" + (e.ExceptionObject as Exception));
                Environment.Exit(1);
            };

            WebApplicationBuilder builder = WebApplication.CreateBuilder();

            builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

            builder.Services
                .AddConfiguration(builder.Configuration)
                .AddPersistenceAndIdentity(builder.Configuration)
                .AddJwtAuthentication(builder.Configuration)
                .AddAuthorization()
                .AddCoreServices();

            builder.Services.AddSingleton<IAuthorizationHandler, DisableAuthorizationHandler>();

            builder.Services.AddPresentationLayer(builder.Environment);

            WebApplication app = builder.Build();

            app.UseRouting();
            app.UseApiDocumentation();

            app.UseHttpsRedirection();
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

    internal sealed class DisableAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (IAuthorizationRequirement requirement in context.Requirements)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
