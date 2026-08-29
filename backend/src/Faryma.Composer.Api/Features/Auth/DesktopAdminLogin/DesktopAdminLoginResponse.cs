using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.Auth.Dtos;

namespace Faryma.Composer.Api.Features.Auth.DesktopAdminLogin
{
    /// <summary>
    /// Ответ на десктопный вход администратора
    /// </summary>
    public sealed record DesktopAdminLoginResponse
    {
        /// <summary>
        /// Выданные JWT токены
        /// </summary>
        [Required]
        public required AuthTokensDto Tokens { get; init; }
    }
}
