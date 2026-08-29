using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.AppSettings.Update
{
    /// <summary>
    /// Обновленные настройки приложения
    /// </summary>
    public sealed record UpdateResponse
    {
        /// <summary>
        /// Настройки приложения
        /// </summary>
        [Required]
        public required AppSettingsDto AppSettings { get; init; }
    }
}
