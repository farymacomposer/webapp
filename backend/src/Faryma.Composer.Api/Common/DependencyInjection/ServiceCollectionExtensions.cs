using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Faryma.Composer.Api.Common.Filters;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Api.Features.Auth.Options;
using Faryma.Composer.Contracts.Api.Features.OrderQueue;
using Faryma.Composer.Contracts.Application.Features.OrderQueue;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Saunter;
using Saunter.AsyncApiSchema.v2;

namespace Faryma.Composer.Api.Common.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string _twitchPreferredUserNameClaimsParameter = """{"id_token":{"preferred_username":null}}""";

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
            JwtOptions jwtOptions = configuration.GetRequiredSection("JWT").Get<JwtOptions>()!;
            TwitchOptions twitchOptions = configuration.GetRequiredSection("TWITCH").Get<TwitchOptions>()!;
            PathString twitchCallbackPath = new(new Uri(twitchOptions.RedirectUri, UriKind.Absolute).AbsolutePath);

            services
                .AddScoped<AuthTokenService>()
                .AddScoped<AdminAuthService>()
                .AddScoped<TwitchAuthService>()
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = AppAuthenticationSchemes.DynamicScheme;
                    options.DefaultAuthenticateScheme = AppAuthenticationSchemes.DynamicScheme;
                    options.DefaultChallengeScheme = AppAuthenticationSchemes.DynamicScheme;
                    options.DefaultSignInScheme = AppAuthenticationSchemes.BrowserCookieScheme;
                    options.DefaultSignOutScheme = AppAuthenticationSchemes.BrowserCookieScheme;
                })
                .AddPolicyScheme(AppAuthenticationSchemes.DynamicScheme, "Selects cookie or bearer auth", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        string authorization = context.Request.Headers.Authorization.ToString();
                        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? JwtBearerDefaults.AuthenticationScheme
                            : AppAuthenticationSchemes.BrowserCookieScheme;
                    };
                })
                .AddCookie(AppAuthenticationSchemes.BrowserCookieScheme, options =>
                {
                    options.Cookie.Name = "faryma_browser_auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromDays(14);
                    options.SlidingExpiration = true;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context => HandleApiCookieRedirect(context, StatusCodes.Status401Unauthorized),
                        OnRedirectToAccessDenied = context => HandleApiCookieRedirect(context, StatusCodes.Status403Forbidden)
                    };
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
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
                })
                .AddOpenIdConnect(AppAuthenticationSchemes.TwitchOidcScheme, options =>
                {
                    options.SignInScheme = AppAuthenticationSchemes.BrowserCookieScheme;
                    options.Authority = TwitchOptions.OidcAuthority;
                    options.MetadataAddress = TwitchOptions.OidcMetadataAddress;
                    options.RequireHttpsMetadata = true;
                    options.ClientId = twitchOptions.ClientId;
                    options.ClientSecret = twitchOptions.ClientSecret;
                    options.CallbackPath = twitchCallbackPath;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.ResponseMode = OpenIdConnectResponseMode.Query;
                    options.UsePkce = true;
                    options.MapInboundClaims = false;
                    options.SaveTokens = false;
                    options.GetClaimsFromUserInfoEndpoint = false;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "preferred_username",
                        RoleClaimType = ClaimTypes.Role,
                        ValidateIssuer = true,
                        ValidIssuer = TwitchOptions.OidcAuthority,
                    };
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            context.ProtocolMessage.RedirectUri = twitchOptions.RedirectUri;
                            context.ProtocolMessage.SetParameter("claims", _twitchPreferredUserNameClaimsParameter);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            TwitchAuthService twitchAuthService = context.HttpContext.RequestServices.GetRequiredService<TwitchAuthService>();
                            context.Principal = await twitchAuthService.CreateBrowserPrincipal(context.Principal!, context.HttpContext.RequestAborted);
                        },
                        OnRemoteFailure = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect(twitchOptions.LoginFailureRedirectUri);
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect(twitchOptions.LoginFailureRedirectUri);
                            return Task.CompletedTask;
                        }
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

        private static Task HandleApiCookieRedirect(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }

        private static bool IsApiRequest(HttpRequest request) => request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }
}
