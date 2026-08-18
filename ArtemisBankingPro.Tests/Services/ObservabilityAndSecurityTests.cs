using ArtemisBankingPro.Application.Extensions;
using ArtemisBankingPro.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Services
{
    public class ObservabilityAndSecurityTests
    {
        [Theory]
        [InlineData("1234567812345678", "****-****-****-5678")]
        [InlineData("4111111111111111", "****-****-****-1111")]
        [InlineData("9876", "****-****-****-9876")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void MaskCardNumber_ShouldMaskPanProperly(string? input, string expected)
        {
            var result = input.MaskCardNumber();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MaskCvc_ShouldHideCvc()
        {
            var result = "123".MaskCvc();
            Assert.Equal("***", result);
        }

        [Fact]
        public async Task GlobalExceptionHandler_ShouldReturnProblemDetailsWithTraceId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
            var handler = new GlobalExceptionHandler(loggerMock.Object);

            var context = new DefaultHttpContext();
            var stream = new MemoryStream();
            context.Response.Body = stream;
            context.TraceIdentifier = "TestTraceId-12345";

            var exception = new Exception("Algo salió mal");

            // Act
            var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            Assert.True(handled);
            Assert.Equal("application/problem+json", context.Response.ContentType);
            Assert.Equal(500, context.Response.StatusCode);

            stream.Position = 0;
            using var reader = new StreamReader(stream);
            var responseBody = await reader.ReadToEndAsync();

            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(problemDetails);
            Assert.Equal(500, problemDetails.Status);
            Assert.Equal("An error occurred while processing your request.", problemDetails.Title);
            Assert.Equal("Algo salió mal", problemDetails.Detail);

            // Check traceId extension
            var jsonDoc = JsonDocument.Parse(responseBody);
            var root = jsonDoc.RootElement;
            Assert.True(root.TryGetProperty("traceId", out var traceIdProp));
            Assert.Equal("TestTraceId-12345", traceIdProp.GetString());
        }
    }
}
