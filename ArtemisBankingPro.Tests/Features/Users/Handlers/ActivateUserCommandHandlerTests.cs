using ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Users.Handlers
{
    public class ActivateUserCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<IGenericRepository<ConfirmationToken>> _tokenRepositoryMock;
        private readonly ActivateUserCommandHandler _handler;

        public ActivateUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _tokenRepositoryMock = new Mock<IGenericRepository<ConfirmationToken>>();

            _handler = new ActivateUserCommandHandler(
                _userRepositoryMock.Object,
                _tokenRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_InvalidToken_ThrowsException()
        {
            // Arrange
            _tokenRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ConfirmationToken>());
            var command = new ActivateUserCommand { Token = "invalid-token" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("inválido", ex.Message);
        }

        [Fact]
        public async Task Handle_ExpiredToken_ThrowsException()
        {
            // Arrange
            var token = new ConfirmationToken { Token = "expired", ExpirationDate = DateTime.UtcNow.AddHours(-1), IsUsed = false, Type = TokenType.Activacion };
            _tokenRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ConfirmationToken> { token });
            var command = new ActivateUserCommand { Token = "expired" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("expirado", ex.Message);
        }

        [Fact]
        public async Task Handle_ValidToken_ActivatesUser()
        {
            // Arrange
            var token = new ConfirmationToken { Id = 1, Token = "valid", ExpirationDate = DateTime.UtcNow.AddHours(1), IsUsed = false, Type = TokenType.Activacion, UserId = 10 };
            var user = new User { Id = 10, IsActive = false };

            _tokenRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ConfirmationToken> { token });
            _userRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(user);

            var command = new ActivateUserCommand { Token = "valid" };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(user.IsActive);
            Assert.True(token.IsUsed);
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _tokenRepositoryMock.Verify(r => r.UpdateAsync(token), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
