using Faryma.Composer.Api.Features.AppSettings.Get;
using Faryma.Composer.Api.Features.AppSettings.Update;
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
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public sealed class AppSettingsController(AppSettingsService appSettingsService) : ControllerBase
    {
        /// <summary>
        /// Возвращает текущие настройки
        /// </summary>
        [HttpGet]
        [AuthorizeAdmins]
        public ActionResult<GetResponse> Get()
        {
            AppSettingsEntity appSettings = appSettingsService.Settings;

            return Ok(new GetResponse
            {
                AppSettings = AppSettingsDto.Map(appSettings)
            });
        }

        /// <summary>
        /// Обновляет настройки
        /// </summary>
        [HttpPost]
        [AuthorizeAdmins]
        public async Task<ActionResult<UpdateResponse>> Update(UpdateRequest request, CancellationToken ct)
        {
            AppSettingsEntity appSettings = await appSettingsService.Update(new AppSettingsEntity
            {
                ReviewOrderNominalPrice = request.AppSettings.ReviewOrderNominalPrice,
                IncludedTrackDurationSeconds = request.AppSettings.IncludedTrackDurationSeconds,
                ReviewOrderExtraTrackSecondPrice = request.AppSettings.ReviewOrderExtraTrackSecondPrice,
                ReviewOrderDetailedPrice = request.AppSettings.ReviewOrderDetailedPrice,
            }, ct);

            return Ok(new UpdateResponse
            {
                AppSettings = AppSettingsDto.Map(appSettings)
            });
        }
    }
}
