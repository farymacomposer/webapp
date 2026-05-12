using Faryma.Composer.Infrastructure.Options;
using Faryma.Composer.Infrastructure.Persistence.Queries;
using Faryma.Composer.Infrastructure.Persistence.Stores;
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

                options.UseNpgsql(postgreOptions.GetConnectionString(), npgOptions => npgOptions.MapEnum());
            });

            services
                .AddDataProtection()
                .PersistKeysToDbContext<AppDbContext>();

            services
                .AddScoped<UnitOfWork>()

                .AddScoped<ComposerStreamQueries>()
                .AddScoped<ReviewOrderQueries>()
                .AddScoped<UserNicknameQueries>()

                .AddScoped<ComposerStreamStore>()
                .AddScoped<RefreshTokenStore>()
                .AddScoped<ReviewStore>()
                .AddScoped<ReviewOrderStore>()
                .AddScoped<TransactionStore>()
                .AddScoped<UserNicknameStore>()
                .AddScoped<DateTimeService>();

            return services;
        }
    }
}
