using ArtemisBankingPro.Application.Features.Users.Commands.ToggleUserStatus;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Users.Handlers
{
    public class ToggleUserStatusCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly ToggleUserStatusCommandHandler _handler;

        public ToggleUserStatusCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _handler = new ToggleUserStatusCommandHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_UserExists_TogglesStatus()
        {
            // Arrange
            var user = new User { Id = 1, IsActive = true };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var command = new ToggleUserStatusCommand { UserId = 1 };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(user.IsActive); // Toggled from true to false
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User)null);
            var command = new ToggleUserStatusCommand { UserId = 99 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("no encontrado", ex.Message);
        }
    }
}
