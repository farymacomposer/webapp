namespace Faryma.Composer.Infrastructure.Exceptions
{
    public abstract class AppException(string? message) : Exception(message)
    {
        public ResultObject GetResultObject()
        {
            return new()
            {
                ExceptionType = GetType().FullName!,
                Message = Message,
                Data = Data
            };
        }
    }
}