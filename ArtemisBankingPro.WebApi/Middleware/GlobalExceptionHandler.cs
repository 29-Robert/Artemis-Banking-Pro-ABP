using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.WebApi.Middleware
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
            var traceId = httpContext.TraceIdentifier;

            var statusCode = exception switch
            {
                FluentValidation.ValidationException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                InvalidOperationException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

            var title = exception switch
            {
                FluentValidation.ValidationException => "Validation Error",
                KeyNotFoundException => "Resource Not Found",
                InvalidOperationException => "Invalid Operation",
                ArgumentException => "Invalid Argument",
                UnauthorizedAccessException => "Unauthorized Access",
                _ => "An error occurred while processing your request."
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Message: {Message}", traceId, httpContext.Request.Path, exception.Message);
            }
            else
            {
                _logger.LogWarning("Handled business/validation exception. TraceId: {TraceId}, Path: {Path}, Message: {Message}", traceId, httpContext.Request.Path, exception.Message);
            }

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Type = "https://tools.ietf.org/html/rfc7807",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions.Add("traceId", traceId);

            if (exception is FluentValidation.ValidationException valEx)
            {
                var validationErrors = new List<string>();
                foreach (var err in valEx.Errors)
                {
                    validationErrors.Add($"{err.PropertyName}: {err.ErrorMessage}");
                }
                problemDetails.Extensions.Add("errors", validationErrors);
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";

            var json = System.Text.Json.JsonSerializer.Serialize(problemDetails);
            await httpContext.Response.WriteAsync(json, cancellationToken);

            return true;
        }
    }
}
