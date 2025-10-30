namespace Faryma.Composer.Desktop.Api.Exceptions
{
    public sealed class ApiException(ResultObject result) : Exception()
    {
        public ResultObject Result { get; } = result;
    }
}