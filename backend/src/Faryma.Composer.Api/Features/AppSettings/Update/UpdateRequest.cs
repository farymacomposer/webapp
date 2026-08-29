using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Api.Features.AppSettings.Update
{
    /// <summary>
    /// Запрос обновления настроек приложения
    /// </summary>
    public sealed record UpdateRequest
    {
        /// <summary>
        /// Настройки приложения
        /// </summary>
        [Required]
        public required AppSettingsDto AppSettings { get; init; }
    }
}
