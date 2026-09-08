using Faryma.Composer.Domain.Entities;

namespace Faryma.Composer.Domain.Exceptions
{
    public sealed class ComposerStreamException : AppException
    {
        public ComposerStreamException(string? message, ComposerStreamEntity? stream = null) : base(message)
        {
            if (stream is not null)
            {
                Context = new
                {
                    Id = stream.Id,
                    EventDate = stream.EventDate,
                    Type = stream.Type,
                    Status = stream.Status,
                };
            }
        }
    }
}
