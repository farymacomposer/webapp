using System.ComponentModel.DataAnnotations;
using Faryma.Composer.Api.Features.Auth.Dtos;

namespace Faryma.Composer.Api.Features.Auth.BrowserAdminLogin
{
    /// <summary>
    /// Запрос браузерного входа администратора
    /// </summary>
    public sealed record BrowserAdminLoginRequest
    {
        /// <summary>
        /// Учетные данные администратора
        /// </summary>
        [Required]
        public required AdminCredentialsDto Credentials { get; init; }
    }
}
