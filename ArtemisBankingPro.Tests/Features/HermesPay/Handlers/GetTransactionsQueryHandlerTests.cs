using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Features.HermesPay.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.HermesPay.Handlers
{
    public class GetTransactionsQueryHandlerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly GetTransactionsQueryHandler _handler;

        public GetTransactionsQueryHandlerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _handler = new GetTransactionsQueryHandler(_mockPaymentService.Object);
        }

        [Fact]
        public async Task Handle_Should_DelegateTo_GetTransactionsAsync_And_Return_Result()
        {
            // Arrange
            var query = new GetTransactionsQuery
            {
                CommerceId = 1,
                Page = 1,
                Limit = 10
            };

            var expectedResponse = new PagedTransactionsDto
            {
                Data = new List<PaymentTransactionDto>(),
                TotalCount = 0
            };

            _mockPaymentService.Setup(s => s.GetTransactionsAsync(query.CommerceId, query.Page, query.Limit))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            _mockPaymentService.Verify(s => s.GetTransactionsAsync(query.CommerceId, query.Page, query.Limit), Times.Once);
        }
    }
}
