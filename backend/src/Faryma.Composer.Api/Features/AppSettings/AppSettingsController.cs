using Faryma.Composer.Api.Auth;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Contracts.Api.Features.AppSettings;
using Faryma.Composer.Contracts.Application.Features.AppSettings;
using Faryma.Composer.Contracts.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.AppSettings
{
    /// <summary>
    /// Управление настройками приложения
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public sealed class AppSettingsController(AppSettingsService appSettingsService) : ControllerBase
    {
        /// <summary>
        /// Возвращает текущие настройки
        /// </summary>
        [HttpGet(nameof(GetAppSettings))]
        [AuthorizeUser]
        public ActionResult<AppSettingsDto> GetAppSettings() => Ok(AppSettingsDto.Map(appSettingsService.Settings));

        /// <summary>
        /// Обновляет настройки
        /// </summary>
        [HttpPost(nameof(UpdateAppSettings))]
        [AuthorizeUser]
        public async Task<ActionResult<AppSettingsDto>> UpdateAppSettings(AppSettingsDto dto, CancellationToken ct)
        {
            AppSettingsEntity settings = await appSettingsService.Update(new AppSettingsModel
            {
                ReviewOrderNominalAmount = dto.ReviewOrderNominalAmount,
            }, ct);

            return Ok(AppSettingsDto.Map(settings));
        }
    }
}