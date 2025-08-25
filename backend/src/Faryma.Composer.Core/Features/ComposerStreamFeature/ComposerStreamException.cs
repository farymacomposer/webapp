using System.Runtime.CompilerServices;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Exceptions;

namespace Faryma.Composer.Core.Features.ComposerStreamFeature
{
    public sealed class ComposerStreamException : AppException
    {
        public ComposerStreamException(string? message, ComposerStream? stream = null, [CallerMemberName] string callerMemberName = null!) : base(message, callerMemberName)
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