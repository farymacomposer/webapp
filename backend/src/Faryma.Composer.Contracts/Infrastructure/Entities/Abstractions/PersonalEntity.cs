using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Contracts.Infrastructure.Entities.Abstractions
{
    public abstract class PersonalEntity
    {
        /// <summary>PK</summary>
        [PersonalData]
        public Guid Id { get; set; }
    }
}