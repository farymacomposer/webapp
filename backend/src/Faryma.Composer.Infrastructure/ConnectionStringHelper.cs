using Faryma.Composer.Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace Faryma.Composer.Infrastructure
{
    static class ConnectionStringHelper
    {
        public static string? Get(IConfiguration configuration)
        {
            PostgreOptions? options = configuration.GetSection("POSTGRES").Get<PostgreOptions>();

            return options?.GetConnectionString();
        }
    }
}