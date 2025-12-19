using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions
{
    public abstract class BaseEntity
    {
        /// <summary>PK</summary>
        [Key]
        public long Id { get; set; }
    }
}