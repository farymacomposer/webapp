using Faryma.Composer.Contracts.Exceptions;

namespace Faryma.Composer.Desktop.Api.Exceptions
{
    public sealed class ApiException(ResultObject result, Exception ex) : Exception(result.Message, ex)
    {
        public ResultObject Result { get; } = result;
    }
}
