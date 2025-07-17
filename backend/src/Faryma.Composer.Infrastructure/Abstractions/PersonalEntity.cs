using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Infrastructure.Abstractions
{
    public abstract class PersonalEntity
    {
        /// <summary>PK</summary>
        [Key]
        [PersonalData]
        public Guid Id { get; set; }
    }
}