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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<AppSettingsDto> GetAppSettings() => Ok(Mapper.Map(appSettingsService.Settings));

        /// <summary>
        /// Обновляет настройки
        /// </summary>
        [HttpPost(nameof(UpdateAppSettings))]
        [AuthorizeComposer]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAppSettings(AppSettingsDto dto)
        {
            await appSettingsService.Update(Mapper.Map(dto));

            return Ok(new { });
        }
    }
}