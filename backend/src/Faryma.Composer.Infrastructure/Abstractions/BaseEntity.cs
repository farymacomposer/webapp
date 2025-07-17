using System.ComponentModel.DataAnnotations;

namespace Faryma.Composer.Infrastructure.Abstractions
{
    public abstract class BaseEntity
    {
        /// <summary>PK</summary>
        [Key]
        public long Id { get; set; }
    }
}