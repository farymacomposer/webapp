namespace Faryma.Composer.Contracts.Infrastructure
{
    public static class AppRoles
    {
        public const string Moderator = nameof(Moderator);
        public const string Composer = nameof(Composer);
        public const string Admins = Moderator + "," + Composer;
    }
}