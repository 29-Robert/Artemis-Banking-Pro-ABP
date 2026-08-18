using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ArtemisBankingPro.Tests.Api_Integration
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Administrador")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
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

                   
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                        options.DefaultScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
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
        public async Task GetLoans_WithAdminToken_ReturnsOk()
        {
            // Arrange
            _loanServiceMock.Reset();
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
            _loanServiceMock.Reset();
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
            _loanServiceMock.Reset();
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
            _loanServiceMock.Reset();
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