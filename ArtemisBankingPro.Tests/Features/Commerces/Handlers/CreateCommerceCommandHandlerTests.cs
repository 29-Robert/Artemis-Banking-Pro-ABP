using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Handlers
{
    public class CreateCommerceCommandHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly CreateCommerceCommandHandler _handler;

        public CreateCommerceCommandHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new CreateCommerceCommandHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCommerceListItemDto()
        {
            // Arrange
            var command = new CreateCommerceCommand
            {
                BusinessName = "Test Commerce",
                RNC = "123456789",
                Email = "test@commerce.com",
                Password = "Password123!",
                Phone = "1234567890",
                Address = "Test Address"
            };

            var expectedDto = new CommerceListItemDto
            {
                Id = 1,
                BusinessName = "Test Commerce",
                RNC = "123456789",
                IsActive = true
            };

            _commerceServiceMock.Setup(s => s.CreateCommerceAsync(It.IsAny<CreateCommerceDto>()))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.BusinessName, result.BusinessName);
            Assert.Equal(expectedDto.RNC, result.RNC);
            
            _commerceServiceMock.Verify(s => s.CreateCommerceAsync(It.Is<CreateCommerceDto>(dto => 
                dto.BusinessName == command.BusinessName &&
                dto.RNC == command.RNC &&
                dto.Email == command.Email &&
                dto.Password == command.Password &&
                dto.Phone == command.Phone &&
                dto.Address == command.Address)), Times.Once);
        }
    }
}
