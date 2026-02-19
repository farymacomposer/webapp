using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Auth.Options;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

            return services;
        }

        public static IServiceCollection AddPersistenceAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddPersistence(configuration)
                .AddIdentityCore<UserEntity>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddHttpClient<TwitchOAuthClient>()
                .Services
                .AddScoped<AuthService>()
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
                .ConfigureSwagger(environment)
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

        private static IServiceCollection ConfigureSwagger(this IServiceCollection services, IWebHostEnvironment environment)
        {
            return services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = environment.ApplicationName,
                    Version = "v1",
                });

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                    }
                }

                options.UseAllOfToExtendReferenceSchemas();
            });
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