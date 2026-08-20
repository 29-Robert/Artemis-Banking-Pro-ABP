using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Handlers
{
    public class GetCommerceByIdQueryHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly GetCommerceByIdQueryHandler _handler;

        public GetCommerceByIdQueryHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new GetCommerceByIdQueryHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsCommerceDetailDto()
        {
            // Arrange
            var query = new GetCommerceByIdQuery { Id = 1 };

            var expectedDto = new CommerceDetailDto
            {
                Id = 1,
                BusinessName = "Test Commerce",
                RNC = "123456789",
                Email = "test@commerce.com",
                Phone = "1234567890",
                Address = "Test Address",
                IsActive = true
            };

            _commerceServiceMock.Setup(s => s.GetCommerceByIdAsync(query.Id))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.BusinessName, result.BusinessName);
            
            _commerceServiceMock.Verify(s => s.GetCommerceByIdAsync(query.Id), Times.Once);
        }
    }
}
