namespace Faryma.Composer.Domain.Exceptions
{
    public abstract class AppException(string? message, string callerMemberName) : Exception(message)
    {
        public const int StatusCode = 666;

        public ResultObject GetResultObject()
        {
            return new()
            {
                ExceptionType = GetType().Name,
                Method = callerMemberName,
                Message = Message,
                Data = Data
            };
        }
    }
}
