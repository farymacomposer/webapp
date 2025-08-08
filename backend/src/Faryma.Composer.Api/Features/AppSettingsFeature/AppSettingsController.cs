using Faryma.Composer.Api.Auth;
using Faryma.Composer.Core.Features.AppSettings;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.AppSettingsFeature
{
    /// <summary>
    /// Управление настройками приложения
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AppSettingsController(AppSettingsService appSettingsService) : ControllerBase
    {
        /// <summary>
        /// Возвращает текущие настройки
        /// </summary>
        [HttpGet(nameof(GetAppSettings))]
        [AuthorizeComposer]
        public ActionResult<AppSettingsDto> GetAppSettings() => Ok(AppSettingsDto.Map(appSettingsService.Settings));

        /// <summary>
        /// Обновляет настройки
        /// </summary>
        [HttpPost(nameof(UpdateAppSettings))]
        [AuthorizeComposer]
        public async Task<IActionResult> UpdateAppSettings(AppSettingsDto dto)
        {
            await appSettingsService.Update(dto.Map());

            return Ok(new { });
        }
    }
}