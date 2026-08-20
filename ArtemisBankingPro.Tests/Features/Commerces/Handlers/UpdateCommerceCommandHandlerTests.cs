using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Handlers
{
    public class UpdateCommerceCommandHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly UpdateCommerceCommandHandler _handler;

        public UpdateCommerceCommandHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new UpdateCommerceCommandHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUnitAndCallsService()
        {
            // Arrange
            var command = new UpdateCommerceCommand
            {
                Id = 1,
                BusinessName = "Updated Commerce",
                RNC = "987654321",
                Email = "updated@commerce.com",
                Phone = "0987654321",
                Address = "Updated Address"
            };

            _commerceServiceMock.Setup(s => s.UpdateCommerceAsync(It.IsAny<int>(), It.IsAny<UpdateCommerceDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(MediatR.Unit.Value, result);
            
            _commerceServiceMock.Verify(s => s.UpdateCommerceAsync(command.Id, It.Is<UpdateCommerceDto>(dto => 
                dto.BusinessName == command.BusinessName &&
                dto.RNC == command.RNC &&
                dto.Email == command.Email &&
                dto.Phone == command.Phone &&
                dto.Address == command.Address)), Times.Once);
        }
    }
}
