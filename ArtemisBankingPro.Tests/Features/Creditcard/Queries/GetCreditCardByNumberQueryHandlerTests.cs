using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Queries
{
    public class GetCreditCardByNumberQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnDomainCreditCard_WhenCardNumberExists()
        {
            // Arrange
            var mockRepo = new Mock<ICreditCardRepository>();
            var expectedDomainCard = new CreditCard { CardNumber = "1234-5678-9012-3456" };
            mockRepo
                .Setup(r => r.GetByCardNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedDomainCard);

            var handler = new GetCreditCardByNumberQueryHandler(mockRepo.Object);
            var query = new GetCreditCardByNumberQuery { CardNumber = "1234-5678-9012-3456" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDomainCard.CardNumber, result.CardNumber);
            mockRepo.Verify(r => r.GetByCardNumberAsync(query.CardNumber), Times.Once);
        }
    }
}