using ArtemisBankingPro.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace ArtemisBankingPro.WebApi
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title) = MapException(exception);

            
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                Log.Error(exception, "Error no controlado en {Path}", httpContext.Request.Path);
            }
            else
            {
                Log.Warning("{ExceptionType} en {Path}: {Message}",
                    exception.GetType().Name, httpContext.Request.Path, exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            
            if (exception is HighRiskClientException highRiskEx)
            {
                problemDetails.Extensions["riskType"] = highRiskEx.RiskType;
                problemDetails.Extensions["currentDebt"] = highRiskEx.CurrentDebt;
                problemDetails.Extensions["projectedDebt"] = highRiskEx.ProjectedDebt;
                problemDetails.Extensions["averageDebt"] = highRiskEx.AverageDebt;
            }

            
            if (exception is ValidationException validationEx)
            {
                problemDetails.Extensions["errors"] = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; 
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            ValidationException => (StatusCodes.Status400BadRequest, "Error de validación"),
            HighRiskClientException => (StatusCodes.Status409Conflict, "Cliente de alto riesgo"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operación no permitida"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Acceso denegado"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };
    }
}