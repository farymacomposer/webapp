using Faryma.Composer.Api.Contracts.Features.AppSettings;
using Faryma.Composer.Api.Features.Auth;
using Faryma.Composer.Application.Features.AppSettings;
using Faryma.Composer.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Faryma.Composer.Api.Features.AppSettings
{
    /// <summary>
    /// Управление настройками приложения
    /// </summary>
    [ApiController]
    [Route("api/app-settings")]
    [Produces("application/json")]
    public sealed class AppSettingsController(AppSettingsService appSettingsService) : ControllerBase
    {
        /// <summary>
        /// Возвращает текущие настройки
        /// </summary>
        [HttpGet]
        [AuthorizeAdmins]
        public ActionResult<AppSettingsDto> GetAppSettings()
        {
            AppSettingsEntity appSettings = appSettingsService.Settings;

            return Ok(AppSettingsDto.Map(appSettings));
        }

        /// <summary>
        /// Обновляет настройки
        /// </summary>
        [HttpPost("update")]
        [AuthorizeAdmins]
        public async Task<ActionResult<AppSettingsDto>> UpdateAppSettings(AppSettingsDto dto, CancellationToken ct)
        {
            AppSettingsEntity appSettings = await appSettingsService.Update(new AppSettingsEntity
            {
                ReviewOrderNominalPrice = dto.ReviewOrderNominalPrice,
                IncludedTrackDurationSeconds = dto.IncludedTrackDurationSeconds,
                ReviewOrderExtraTrackSecondPrice = dto.ReviewOrderExtraTrackSecondPrice,
                ReviewOrderDetailedPrice = dto.ReviewOrderDetailedPrice,
            }, ct);

            return Ok(AppSettingsDto.Map(appSettings));
        }
    }
}
