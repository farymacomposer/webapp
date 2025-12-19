using System.ComponentModel;

namespace Faryma.Composer.Contracts.Infrastructure.Enums
{
    /// <summary>
    /// Тип стрима
    /// </summary>
    public enum ComposerStreamType
    {
        /// <summary>
        /// Не задан
        /// </summary>
        [Description("Не задан")]
        Unspecified = 0,

        /// <summary>
        /// Донатный
        /// </summary>
        [Description("Донатный")]
        Donation = 1,

        /// <summary>
        /// Долговой
        /// </summary>
        [Description("Долговой")]
        Debt = 2,

        /// <summary>
        /// Благотворительный
        /// </summary>
        [Description("Благотворительный")]
        Charity = 3
    }
}