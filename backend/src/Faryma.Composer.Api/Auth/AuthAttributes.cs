using Faryma.Composer.Contracts.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Faryma.Composer.Api.Auth
{
    public sealed class AuthorizeUserAttribute : AuthorizeAttribute
    {
        public AuthorizeUserAttribute() => Roles = AppRoles.User;
    }

    public sealed class AuthorizeModeratorAttribute : AuthorizeAttribute
    {
        public AuthorizeModeratorAttribute() => Roles = AppRoles.Moderator;
    }

    public sealed class AuthorizeComposerAttribute : AuthorizeAttribute
    {
        public AuthorizeComposerAttribute() => Roles = AppRoles.Composer;
    }

    public sealed class AuthorizeAdminsAttribute : AuthorizeAttribute
    {
        public AuthorizeAdminsAttribute() => Roles = AppRoles.Admins;
    }
}