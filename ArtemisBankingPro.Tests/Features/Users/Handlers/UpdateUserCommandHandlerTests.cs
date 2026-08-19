using ArtemisBankingPro.Application.Features.Users.Commands.UpdateUser;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Users.Handlers
{
    public class UpdateUserCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<ISavingsAccountService> _savingsAccountServiceMock;
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _savingsAccountServiceMock = new Mock<ISavingsAccountService>();

            _handler = new UpdateUserCommandHandler(
                _userRepositoryMock.Object,
                _savingsAccountServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsException()
        {
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User)null);
            var command = new UpdateUserCommand { Id = 1 };

            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("El usuario no fue encontrado", ex.Message);
        }

        [Fact]
        public async Task Handle_ValidUpdate_UpdatesUserExceptRole()
        {
            var targetUser = new User { Id = 1, RoleId = 1, PasswordHash = "hash", FirstName = "Old", Email = "old@test.com" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(targetUser);

            var command = new UpdateUserCommand { Id = 1, FirstName = "New", Email = "new@test.com" };
            
            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("New", targetUser.FirstName);
            Assert.Equal("new@test.com", targetUser.Email);
            Assert.Equal(1, targetUser.RoleId); // El rol no debe cambiar
            Assert.Equal("hash", targetUser.PasswordHash); // El hash tampoco
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
