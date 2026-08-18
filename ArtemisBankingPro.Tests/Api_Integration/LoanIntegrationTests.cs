using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ArtemisBankingPro.Application.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Api_Integration
{
    public class TestAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Administrador")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Test");

            return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
        }
    }

    public class LoanApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<ILoanService> _loanServiceMock = new Mock<ILoanService>();

        public LoanApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    
                    services.AddScoped(_ => _loanServiceMock.Object);

                    
                    services.AddAuthentication("Test")
                        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            });
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            return client;
        }

        [Fact]
        public async Task GetLoans_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/loan");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetLoans_WithAdminToken_ReturnsOk()
        {
            // Arrange
            _loanServiceMock
                .Setup(s => s.GetLoansAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedResult<LoanResponseDto>());

            var client = CreateAuthenticatedClient();

            // Act
            var response = await client.GetAsync("/api/loan?pageNumber=1&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithAdminToken_ReturnsOk()
        {
            // Arrange
            _loanServiceMock
                .Setup(s => s.GetLoanByIdAsync(1))
                .ReturnsAsync(new LoanResponseDto { Id = 1, LoanNumber = "LN-100" });

            var client = CreateAuthenticatedClient();

            // Act
            var response = await client.GetAsync("/api/loan/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AssignLoan_WithAdminToken_ReturnsCreated()
        {
            // Arrange
            var request = new CreateLoanRequestDto
            {
                ClientId = "1",
                CapitalAmount = 50000m,
                TermInMonths = 12,
                AnnualInterestRate = 18m
            };

            _loanServiceMock
                .Setup(s => s.AssignLoanAsync(It.IsAny<CreateLoanRequestDto>(), 1))
                .ReturnsAsync(new LoanResponseDto { Id = 99, LoanNumber = "LN-099" });

            var client = CreateAuthenticatedClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/loan", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task UpdateRate_WithAdminToken_ReturnsOk()
        {
            // Arrange
            var request = new UpdateLoanRateRequestDto { AnnualInterestRate = 15.5m };

            _loanServiceMock
                .Setup(s => s.UpdateInterestRateAsync(1, 15.5m))
                .ReturnsAsync(new LoanResponseDto { Id = 1, AnnualInterestRate = 15.5m });

            var client = CreateAuthenticatedClient();

            // Act
            var response = await client.PatchAsJsonAsync("/api/loan/1/rate", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
