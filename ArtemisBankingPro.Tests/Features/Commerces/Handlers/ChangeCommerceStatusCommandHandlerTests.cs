using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Handlers
{
    public class ChangeCommerceStatusCommandHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly ChangeCommerceStatusCommandHandler _handler;

        public ChangeCommerceStatusCommandHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new ChangeCommerceStatusCommandHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUnitAndCallsService()
        {
            // Arrange
            var command = new ChangeCommerceStatusCommand
            {
                Id = 1,
                IsActive = false
            };

            _commerceServiceMock.Setup(s => s.ChangeStatusAsync(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(MediatR.Unit.Value, result);
            
            _commerceServiceMock.Verify(s => s.ChangeStatusAsync(command.Id, command.IsActive), Times.Once);
        }
    }
}
