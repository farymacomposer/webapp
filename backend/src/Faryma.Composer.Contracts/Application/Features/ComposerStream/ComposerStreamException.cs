using System.Runtime.CompilerServices;
using Faryma.Composer.Contracts.Exceptions;
using Faryma.Composer.Contracts.Infrastructure.Entities;

namespace Faryma.Composer.Contracts.Application.Features.ComposerStream
{
    public sealed class ComposerStreamException : AppException
    {
        public ComposerStreamException(string? message, ComposerStreamEntity? stream = null, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
        {
            if (stream is not null)
            {
                Data.Add("Id", stream.Id);
                Data.Add("EventDate", stream.EventDate);
                Data.Add("Type", stream.Type.ToString());
                Data.Add("Status", stream.Status.ToString());
            }
        }
    }
}
