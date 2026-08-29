using Faryma.Composer.Infrastructure.Features.Auth;
using Faryma.Composer.Infrastructure.Features.ComposerStream;
using Faryma.Composer.Infrastructure.Features.OrderQueue;
using Faryma.Composer.Infrastructure.Features.ReviewOrder;
using Faryma.Composer.Infrastructure.Features.User;
using Faryma.Composer.Infrastructure.Features.UserNickname;
using Faryma.Composer.Infrastructure.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Faryma.Composer.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptionsWithValidateOnStart<PostgreOptions>()
                .Bind(configuration.GetRequiredSection("POSTGRES"))
                .ValidateDataAnnotations();

            services.AddDbContextFactory<AppDbContext>((provider, options) =>
            {
                PostgreOptions postgreOptions = provider.GetRequiredService<IOptions<PostgreOptions>>().Value;
                options
                    .UseNpgsql(postgreOptions.GetConnectionString(), npgOptions => npgOptions.MapEnum())
                    .UseAppDataSeeding();
            });

            services
                .AddDataProtection()
                .PersistKeysToDbContext<AppDbContext>();

            services
                .AddScoped<DateTimeService>()

                // Auth
                .AddScoped<RefreshTokenStore>()

                // ComposerStream
                .AddScoped<ComposerStreamStore>()

                // OrderQueue
                .AddScoped<OrderQueueStore>()

                // ReviewOrder
                .AddScoped<ReviewOrderStore>()
                .AddScoped<ReviewStore>()
                .AddScoped<TransactionStore>()
                .AddScoped<UserEntitlementStore>()

                // User
                .AddScoped<UserStore>()

                // UserNickname
                .AddScoped<UserNicknameStore>();

            return services;
        }
    }
}
