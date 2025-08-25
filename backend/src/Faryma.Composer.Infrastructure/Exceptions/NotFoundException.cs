namespace Faryma.Composer.Infrastructure.Exceptions
{
    public sealed class NotFoundException(string? message) : AppException(message)
    {
    }
}