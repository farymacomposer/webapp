namespace Faryma.Composer.Contracts.Infrastructure.Enums
{
    /// <summary>
    /// Провайдер/канал пополнения
    /// </summary>
    public enum TopUpProvider
    {
        /// <summary>
        /// Не задан
        /// </summary>
        Unspecified = 0,

        Donationalerts = 1,
        Donatty = 2,

        /// <summary>
        /// Вручную, админом или модератором
        /// </summary>
        Manual = 100
    }
}