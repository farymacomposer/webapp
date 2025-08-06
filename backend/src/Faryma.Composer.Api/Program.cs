using System.Text.Json.Serialization;
using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.DependencyInjection;
using Faryma.Composer.Api.Extensions;
using Faryma.Composer.Api.Features.OrderQueueFeature;
using Faryma.Composer.Core.DependencyInjection;
using Faryma.Composer.Core.Features.AppSettings;
using Faryma.Composer.Core.Features.OrderQueueFeature;
using Microsoft.AspNetCore.Authorization;
using Saunter;
using Saunter.AsyncApiSchema.v2;
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
                                ["signalr-hub"] = new Server("/api/OrderQueueNotificationHub", "websocket")
                                {
                                    Description = "SignalR WebSocket Hub для уведомлений о состоянии очереди заказов"
                                }
                            }
                        };
                    });

                    services.AddInfrastructure(builder.Environment);
                });

            WebApplication app = builder.Build();

            app.UseRouting();
            app.UseCustomSwagger();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<OrderQueueNotificationHub>($"/api/{nameof(OrderQueueNotificationHub)}");
            app.MapAsyncApiDocuments();
            app.MapAsyncApiUi();

            await app.Services.GetRequiredService<AppSettingsService>().Initialize();
            await app.Services.GetRequiredService<OrderQueueService>().Initialize();
            await app.RunAsync();
        }
    }
}