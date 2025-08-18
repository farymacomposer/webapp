using System.Reflection;
using System.Text;
using Faryma.Composer.Api.Auth;
using Faryma.Composer.Api.Auth.Options;
using Faryma.Composer.Api.Features.OrderQueueFeature;
using Faryma.Composer.Core.Features.OrderQueueFeature.Contracts;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
                .Bind(configuration.GetSection("JWT"))
                .ValidateDataAnnotations();

            services
                .AddOptionsWithValidateOnStart<PostgreOptions>()
                .Bind(configuration.GetSection("POSTGRES"))
                .ValidateDataAnnotations();

            return services;
        }

        public static IServiceCollection AddPersistenceAndIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddPersistence(configuration)
                .AddIdentityCore<User>(options => options.Password.RequiredLength = 12)
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddScoped<AuthService>()
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtOptions jwtOptions = configuration.GetSection("JWT").Get<JwtOptions>()!;
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

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IWebHostEnvironment environment)
        {
            services
                .AddProblemDetails()
                .AddMemoryCache()
                .ConfigureSwagger(environment)
                .AddAsyncApiSpecification(environment)
                .AddSingleton<IOrderQueueNotificationService, OrderQueueNotificationService>()
                .AddSignalR();

            return services;
        }

        public static IServiceCollection AddGraphQL(this IServiceCollection services)
        {
            services
                .AddGraphQLServer()
                //.AddQueryType<TrackQuery>()
                .AddProjections()
                .AddFiltering()
                .AddSorting();

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
                    string xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                    }
                }

                options.CustomSchemaIds(x => x.FullName);
                options.UseAllOfToExtendReferenceSchemas();

                OpenApiSecurityScheme scheme = new()
                {
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(scheme.Reference.Id, scheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
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
                        [OrderQueueNotificationService.HubServerName] = new Server(OrderQueueNotificationHub.RoutePattern, "signalr")
                        {
                            Description = "События очереди заказов"
                        }
                    }
                };
            });
        }
    }
}