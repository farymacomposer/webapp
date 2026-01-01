namespace Faryma.Composer.Contracts.Infrastructure.Enums
{
    /// <summary>
    /// Провайдер/канал пополнения счета пользователя
    /// </summary>
    public enum AccountTopUpProvider
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        Donationalerts = 1,
        Donatty = 2,
        TwitchChannelPoints = 3,

        /// <summary>
        /// Вручную, админом или модератором
        /// </summary>
        Manual = 100
    }
}