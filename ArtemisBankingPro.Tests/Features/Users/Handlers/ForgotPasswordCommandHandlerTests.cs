using ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword;
using ArtemisBankingPro.Application.Interfaces.Services;
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
    public class ForgotPasswordCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<IGenericRepository<ConfirmationToken>> _tokenRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly ForgotPasswordCommandHandler _handler;

        public ForgotPasswordCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _tokenRepositoryMock = new Mock<IGenericRepository<ConfirmationToken>>();
            _emailServiceMock = new Mock<IEmailService>();

            _handler = new ForgotPasswordCommandHandler(
                _userRepositoryMock.Object,
                _tokenRepositoryMock.Object,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            // Arrange
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            var command = new ForgotPasswordCommand { Username = "unknown" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UserFound_GeneratesTokenAndSendsEmail()
        {
            // Arrange
            var user = new User { Id = 1, Username = "test", Email = "test@test.com", IsActive = true };
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { user });
            var command = new ForgotPasswordCommand { Username = "test" };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(user.IsActive); // Should be deactivated temporarily
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _tokenRepositoryMock.Verify(r => r.AddAsync(It.Is<ConfirmationToken>(t => t.UserId == 1 && t.Type == TokenType.RestablecimientoContrasena)), Times.Once);
            _emailServiceMock.Verify(s => s.SendPasswordResetEmailAsync("test@test.com", It.IsAny<string>()), Times.Once);
        }
    }
}
