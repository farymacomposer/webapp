using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Auth.Services;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Auth.Options;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Saunter;
using Saunter.AsyncApiSchema.v2;

namespace Faryma.Composer.Api.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptionsWithValidateOnStart<JwtOptions>()
                .Bind(configuration.GetRequiredSection("JWT"))
                .ValidateDataAnnotations();

            services
                .AddOptionsWithValidateOnStart<TwitchOptions>()
                .Bind(configuration.GetRequiredSection("TWITCH"))
                .ValidateDataAnnotations();

            services
                .AddOptionsWithValidateOnStart<PostgreOptions>()
                .Bind(configuration.GetRequiredSection("POSTGRES"))
                .ValidateDataAnnotations();

            services
                .AddOptionsWithValidateOnStart<AdminBootstrapOptions>()
                .Bind(configuration.GetRequiredSection("ADMIN_BOOTSTRAP"))
                .ValidateDataAnnotations();

            return services;
        }

        public static IServiceCollection AddPersistenceAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddScoped<AdminBootstrapService>()
                .AddPersistence(configuration)
                .AddIdentityCore<UserEntity>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddHttpClient<TwitchAuthClient>()
                .Services
                .AddScoped<AuthTokenService>()
                .AddScoped<TwitchAuthStateService>()
                .AddScoped<TwitchAuthService>()
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtOptions jwtOptions = configuration.GetRequiredSection("JWT").Get<JwtOptions>()!;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                    };
                });

            return services;
        }

        public static IServiceCollection AddPresentationLayer(this IServiceCollection services, IWebHostEnvironment environment)
        {
            services
                .AddProblemDetails()
                .AddMemoryCache()
                .AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    options.AddPolicy("auth-login", context =>
                    {
                        string partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                    });
                })
                .AddOpenApi()
                .AddAsyncApiSpecification(environment);

            services
                .AddSingleton<AppExceptionFilter>()
                .AddControllers(options => options.Filters.AddService<AppExceptionFilter>())
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services
                .AddSingleton<IOrderQueueNotificationService, OrderQueueNotificationService>()
                .AddSignalR()
                .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            return services;
        }

        private static IServiceCollection AddAsyncApiSpecification(this IServiceCollection services, IWebHostEnvironment environment)
        {
            return services.AddAsyncApiSchemaGeneration(options =>
            {
                options.AssemblyMarkerTypes = new[] { typeof(OrderQueueNotificationService) };
                options.AsyncApi = new AsyncApiDocument
                {
                    Info = new Info(environment.ApplicationName, "v1"),
                    Servers =
                    {
                        [IOrderQueueNotificationServer.HubServerName] = new Server(IOrderQueueNotificationServer.RoutePattern, "signalr")
                        {
                            Description = "Очередь заказов"
                        }
                    }
                };
            });
        }
    }
}