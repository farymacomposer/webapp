using System.Text.Json.Serialization;
using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.DependencyInjection;
using Faryma.Composer.Api.Extensions;
using Faryma.Composer.Api.Features.OrderQueueFeature;
using Faryma.Composer.Core.DependencyInjection;
using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Core.Features.OrderQueueFeature;
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
                Console.WriteLine("Критическая ошибка" + (e.ExceptionObject as Exception)?.ToString());
                Environment.Exit(1);
            };

            WebApplicationBuilder builder = WebApplication.CreateBuilder();

            builder.Host
                .UseSerilog((context, config) =>
                    config.ReadFrom.Configuration(context.Configuration))
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddConfiguration(context.Configuration)
                        .AddPersistenceAndIdentity(context.Configuration)
                        .AddAuthentication(context.Configuration)
                        .AddAuthorization()
                        .AddCoreServices();

                    if (builder.Environment.IsDevelopment())
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();
                    }

                    services
                        .AddControllers()
                        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

                    services.AddInfrastructure(builder.Environment);
                });

            WebApplication app = builder.Build();

            app.UseRouting();
            app.UseCustomApiDocumentation();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapHub<OrderQueueNotificationHub>(OrderQueueNotificationHub.RoutePattern);

            await app.Services.GetRequiredService<AppSettingsService>().Initialize();
            await app.Services.GetRequiredService<OrderQueueService>().Initialize();
            await app.RunAsync();
        }
    }
}