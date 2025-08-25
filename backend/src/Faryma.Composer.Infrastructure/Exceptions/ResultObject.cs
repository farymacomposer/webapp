using System.Collections;

namespace Faryma.Composer.Infrastructure.Exceptions
{
    public sealed record ResultObject
    {
        public required string ExceptionType { get; init; }
        public required string Method { get; init; }
        public required string Message { get; init; }
        public required IDictionary Data { get; init; }
    }
}