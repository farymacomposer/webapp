namespace Faryma.Composer.Infrastructure.Exceptions
{
    public abstract class AppException(string? message, string callerMemberName) : Exception(message)
    {
        public ResultObject GetResultObject()
        {
            return new()
            {
                ExceptionType = GetType().FullName!,
                Method = callerMemberName,
                Message = Message,
                Data = Data
            };
        }
    }
}