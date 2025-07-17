using Microsoft.AspNetCore.Authorization;

namespace Faryma.Composer.Api.Auth
{
    public sealed class AuthorizeUserAttribute : AuthorizeAttribute
    {
        public AuthorizeUserAttribute() => Roles = "User";
    }

    public sealed class AuthorizeModeratorAttribute : AuthorizeAttribute
    {
        public AuthorizeModeratorAttribute() => Roles = "Moderator";
    }

    public sealed class AuthorizeComposerAttribute : AuthorizeAttribute
    {
        public AuthorizeComposerAttribute() => Roles = "Composer";
    }

    public sealed class AuthorizeAdminsAttribute : AuthorizeAttribute
    {
        public AuthorizeAdminsAttribute() => Roles = "Moderator,Composer";
    }
}