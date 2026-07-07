using System.ComponentModel;

namespace Faryma.Composer.Domain.Enums
{
    /// <summary>
    /// Провайдер/канал пополнения счета пользователя
    /// </summary>
    public enum AccountTopUpProvider
    {
        /// <summary>
        /// Не задан
        /// </summary>
        [Description("Не задан")]
        Unspecified = 0,

        /// <summary>
        /// DonationAlerts
        /// </summary>
        [Description("DonationAlerts")]
        Donationalerts = 1,

        /// <summary>
        /// Donatty
        /// </summary>
        [Description("Donatty")]
        Donatty = 2,

        /// <summary>
        /// Баллы канала Twitch
        /// </summary>
        [Description("Баллы канала Twitch")]
        TwitchChannelPoints = 3,

        /// <summary>
        /// Вручную, админом или модератором
        /// </summary>
        [Description("Ручное пополнение")]
        Manual = 100
    }
}
