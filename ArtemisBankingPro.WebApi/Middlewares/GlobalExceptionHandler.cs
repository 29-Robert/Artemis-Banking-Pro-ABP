using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.WebApi.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception, "Ha ocurrido un error: {Message}", exception.Message);

            var problemDetails = exception switch
            {
                UnauthorizedAccessException => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "No autorizado",
                    Detail = exception.Message
                },
                KeyNotFoundException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso no encontrado",
                    Detail = exception.Message
                },
                FluentValidation.ValidationException validationEx => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de validación",
                    Detail = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))
                },
                InvalidOperationException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de negocio",
                    Detail = exception.Message
                },
                ArgumentException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Parámetros inválidos",
                    Detail = exception.Message
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = "Ha ocurrido un error inesperado. Intente nuevamente más tarde."
                }
            };

            problemDetails.Type = "https://tools.ietf.org/html/rfc7807";
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = problemDetails.Status!.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json", cancellationToken);

            return true;
        }
    }
}
