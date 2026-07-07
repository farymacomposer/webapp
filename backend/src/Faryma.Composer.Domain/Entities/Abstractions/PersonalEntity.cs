using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Domain.Entities.Abstractions
{
    public abstract class PersonalEntity
    {
        /// <summary>PK</summary>
        [PersonalData]
        public Guid Id { get; set; }
    }
}
