using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.Auth.Dtos;

namespace Faryma.Composer.Api.Features.Auth.DesktopAdminLogin
{
    /// <summary>
    /// Запрос десктопного входа администратора
    /// </summary>
    public sealed record DesktopAdminLoginRequest
    {
        /// <summary>
        /// Учетные данные администратора
        /// </summary>
        [Required]
        public required AdminCredentialsDto Credentials { get; init; }
    }
}
