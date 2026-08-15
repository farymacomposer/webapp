using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Faryma.Composer.Api.Common.Exceptions;
using Faryma.Composer.Api.Common.Filters;
using Faryma.Composer.Api.Common.Options;
using Faryma.Composer.Api.Common.Startup;
using Faryma.Composer.Api.Contracts.Features.Auth.Options;
using Faryma.Composer.Api.Contracts.Features.OrderQueue;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Api.Features.Auth.Services;
using Faryma.Composer.Api.Features.OrderQueue;
using Faryma.Composer.Application.Features.OrderQueue;
using Faryma.Composer.Domain.Entities;
using Faryma.Composer.Infrastructure;
using Faryma.Composer.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
                .AddOptionsWithValidateOnStart<AdminBootstrapOptions>()
                .Bind(configuration.GetRequiredSection("ADMIN_BOOTSTRAP"))
                .ValidateDataAnnotations();

            services
                .AddOptionsWithValidateOnStart<ForwardedHeadersSettings>()
                .Bind(configuration.GetRequiredSection("FORWARDED_HEADERS"))
                .Validate<IWebHostEnvironment>(
                    (options, environment) => environment.IsDevelopment() || options.HasTrustedForwarders,
                    "Вне окружения Development должен быть настроен хотя бы один доверенный прокси или сеть")
                .Validate(ForwardedHeadersSettings.HasValidKnownProxies, "Доверенные прокси должны быть валидными IP-адресами")
                .Validate(ForwardedHeadersSettings.HasValidKnownNetworks, "Доверенные сети должны быть валидными CIDR-диапазонами");

            services
                .AddOptions<ForwardedHeadersOptions>()
                .Configure<IOptions<ForwardedHeadersSettings>>((options, settingsAccessor) =>
                {
                    ForwardedHeadersSettings settings = settingsAccessor.Value;

                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    if (!settings.HasTrustedForwarders)
                    {
                        return;
                    }

                    options.KnownProxies.Clear();
                    options.KnownIPNetworks.Clear();

                    foreach (string knownProxy in settings.KnownProxies)
                    {
                        options.KnownProxies.Add(IPAddress.Parse(knownProxy));
                    }

                    foreach (string knownNetwork in settings.KnownNetworks)
                    {
                        options.KnownIPNetworks.Add(ForwardedHeadersSettings.ParseKnownNetwork(knownNetwork));
                    }
                });

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

        public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
        {
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
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme)
                .AddOpenIdConnect(AppAuthenticationSchemes.TwitchOidcScheme, _ => { });

            services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
                {
                    JwtOptions jwtOptions = jwtOptionsAccessor.Value;

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

            services
                .AddOptions<OpenIdConnectOptions>(AppAuthenticationSchemes.TwitchOidcScheme)
                .Configure<IOptions<TwitchOptions>>((options, twitchOptionsAccessor) =>
                {
                    TwitchOptions twitchOptions = twitchOptionsAccessor.Value;
                    PathString twitchCallbackPath = new(new Uri(twitchOptions.RedirectUri, UriKind.Absolute).AbsolutePath);

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
                .AddSingleton<IApplicationStartupInitializer, ApplicationStartupInitializer>()
                .AddExceptionHandler<ApiExceptionHandler>()
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
                .Configure<JsonOptions>(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))
                .AddAsyncApiSpecification(environment);

            services
                .AddScoped<IdempotentFilter>()
                .AddControllers()
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
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    }
}
