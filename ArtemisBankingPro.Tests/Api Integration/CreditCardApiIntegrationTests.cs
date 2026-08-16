using ArtemisBankingPro.WebApi;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ArtemisBankingPro.Tests
{
    public class CreditCardApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CreditCardApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetCreditCards_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/credit-card");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCreditCardById_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/credit-card/1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateCreditCard_WithoutToken_ReturnsUnauthorized()
        {
            var payload = new
            {
                clientId = "1",
                creditLimit = 10000
            };

            var response = await _client.PostAsJsonAsync("/api/credit-card", payload);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}