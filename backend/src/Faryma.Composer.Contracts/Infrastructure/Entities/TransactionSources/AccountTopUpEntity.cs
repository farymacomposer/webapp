using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources
{
    /// <summary>
    /// Пополнение счета
    /// </summary>
    [Table("account_top_ups")]
    public sealed class AccountTopUpEntity : TransactionSourceEntity
    {
        /// <summary>
        /// Провайдер/канал пополнения
        /// </summary>
        [Column(TypeName = DbEnumConst.TopUpProviderEnum)]
        public required TopUpProvider Provider { get; set; }
    }
}