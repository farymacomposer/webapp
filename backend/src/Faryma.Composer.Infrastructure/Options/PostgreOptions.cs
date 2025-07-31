using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Faryma.Composer.Infrastructure.Options
{
    public sealed class PostgreOptions
    {
        [ConfigurationKeyName("HOST")]
        [Required]
        public required string Host { get; set; }

        [ConfigurationKeyName("PORT")]
        [Required]
        public required int Port { get; set; }

        [ConfigurationKeyName("DATABASE")]
        [Required]
        public required string Database { get; set; }

        [ConfigurationKeyName("USERNAME")]
        [Required]
        public required string Username { get; set; }

        //TODO: Для локальной разработки ограничение пароля может мешать, когда будет релиз, в целях безопасности стоит вернуть
        // [MinLength(12)]
        [ConfigurationKeyName("PASSWORD")]
        [Required]
        public required string Password { get; set; }

        public string GetConnectionString()
        {
            NpgsqlConnectionStringBuilder builder = new()
            {
                Host = Host,
                Port = Port,
                Database = Database,
                Username = Username,
                Password = Password,
            };

            return builder.ToString();
        }
    }
}