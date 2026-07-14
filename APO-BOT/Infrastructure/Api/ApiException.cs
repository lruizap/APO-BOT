using System.Net;
using APO_BOT.Models;

namespace APO_BOT.Infrastructure.Api;

public class ApiException : Exception
{
    public ApiException(string message, HttpStatusCode? statusCode = null, ApiProblem? problem = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Problem = problem;
    }

    public HttpStatusCode? StatusCode { get; }
    public ApiProblem? Problem { get; }
}

public sealed class ApiNotConfiguredException()
    : ApiException("La conexion con el backend no esta configurada.");
