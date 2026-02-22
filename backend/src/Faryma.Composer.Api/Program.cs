using Faryma.Composer.Api.DependencyInjection;
using Faryma.Composer.Api.Extensions;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Application.DependencyInjection;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
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
            string[] allowedCorsOrigins = builder.Configuration.GetSection("CORS:ALLOWED_ORIGINS").Get<string[]>()
                ?? ["http://localhost:5173", "http://localhost:3000"];

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy
                    .WithOrigins(allowedCorsOrigins)
                    .WithMethods("GET", "POST")
                    .AllowAnyHeader());
            });

            builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

            builder.Services
                .AddConfiguration(builder.Configuration)
                .AddPersistenceAndIdentity(builder.Configuration)
                .AddJwtAuthentication(builder.Configuration)
                .AddAuthorization()
                .AddCoreServices();

            //if (false && builder.Environment.IsDevelopment())
            //{
            //    builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();
            //}

            builder.Services.AddPresentationLayer(builder.Environment);

            WebApplication app = builder.Build();

            app.UseRouting();
            app.UseApiDocumentation();

            app.UseCors(config => config
                .WithOrigins(allowedCorsOrigins)
                .WithMethods("GET", "POST")
                .AllowAnyHeader());

            app.UseHttpsRedirection();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<OrderQueueNotificationHub>(IOrderQueueNotificationServer.RoutePattern);

            await app.Services.GetRequiredService<AppSettingsService>().Initialize();
            await app.Services.GetRequiredService<OrderQueueService>().Initialize();
            await app.RunAsync();
        }
    }
}