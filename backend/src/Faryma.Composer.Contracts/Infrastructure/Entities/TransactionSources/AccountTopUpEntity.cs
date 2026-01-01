using System.ComponentModel.DataAnnotations.Schema;
using Faryma.Composer.Contracts.Infrastructure.Enums;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.TransactionSources
{
    /// <summary>
    /// Пополнение счета пользователя
    /// </summary>
    [Table("account_top_ups")]
    public sealed class AccountTopUpEntity : TransactionSourceEntity
    {
        /// <summary>
        /// Провайдер/канал пополнения счета пользователя
        /// </summary>
        [Column(TypeName = DbEnumConst.AccountTopUpProviderEnum)]
        public required AccountTopUpProvider Provider { get; set; }
    }
}