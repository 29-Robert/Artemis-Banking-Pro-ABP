using ArtemisBankingPro.Application.Features.Users.Commands.ResetPassword;
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
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<IGenericRepository<ConfirmationToken>> _tokenRepositoryMock;
        private readonly ResetPasswordCommandHandler _handler;

        public ResetPasswordCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _tokenRepositoryMock = new Mock<IGenericRepository<ConfirmationToken>>();

            _handler = new ResetPasswordCommandHandler(
                _userRepositoryMock.Object,
                _tokenRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidToken_ResetsPasswordAndActivatesUser()
        {
            // Arrange
            var token = new ConfirmationToken { Id = 1, Token = "valid-token", ExpirationDate = DateTime.UtcNow.AddHours(1), IsUsed = false, Type = TokenType.RestablecimientoContrasena, UserId = 10 };
            var user = new User { Id = 10, IsActive = false, PasswordHash = "oldhash" };

            _tokenRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<ConfirmationToken> { token });
            _userRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(user);

            var command = new ResetPasswordCommand { Token = "valid-token", NewPassword = "NewPassword123!" };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(user.IsActive);
            Assert.NotEqual("oldhash", user.PasswordHash); // Password must have been hashed
            Assert.True(token.IsUsed);
            _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
            _tokenRepositoryMock.Verify(r => r.UpdateAsync(token), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
