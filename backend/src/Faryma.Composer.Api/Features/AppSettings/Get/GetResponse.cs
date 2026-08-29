using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.AppSettings.Get
{
    /// <summary>
    /// Текущие настройки приложения
    /// </summary>
    public sealed record GetResponse
    {
        /// <summary>
        /// Настройки приложения
        /// </summary>
        [Required]
        public required AppSettingsDto AppSettings { get; init; }
    }
}
