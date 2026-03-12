using System.Net;

namespace RESR.MAUI.Services;

public sealed class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
