using Faryma.Composer.Infrastructure.Entities;

namespace Faryma.Composer.Api.Features.ComposerStreamFeature
{
    public static class Mapper
    {
        public static ComposerStreamDto Map(ComposerStream item)
        {
            return new()
            {
                Id = item.Id,
                EventDate = item.EventDate,
                Status = item.Status,
                Type = item.Type,
            };
        }
    }
}