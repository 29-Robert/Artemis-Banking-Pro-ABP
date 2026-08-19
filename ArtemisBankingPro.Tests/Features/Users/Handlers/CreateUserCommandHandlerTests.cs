using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using DomainTransaction = ArtemisBankingPro.Domain.Entities.Transaction;

namespace ArtemisBankingPro.Tests.Features.Users.Handlers
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<IGenericRepository<SavingsAccount>> _accountRepositoryMock;
        private readonly Mock<IGenericRepository<ConfirmationToken>> _tokenRepositoryMock;
        private readonly Mock<IGenericRepository<DomainTransaction>> _transactionRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _accountRepositoryMock = new Mock<IGenericRepository<SavingsAccount>>();
            _tokenRepositoryMock = new Mock<IGenericRepository<ConfirmationToken>>();
            _transactionRepositoryMock = new Mock<IGenericRepository<DomainTransaction>>();
            _emailServiceMock = new Mock<IEmailService>();
            _mapperMock = new Mock<IMapper>();

            _handler = new CreateUserCommandHandler(
                _userRepositoryMock.Object,
                _accountRepositoryMock.Object,
                _tokenRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _emailServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_ExistingUser_ThrowsException()
        {
            // Arrange
            var existingUser = new User { Username = "jdoe" };
            _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<User> { existingUser });

            var command = new CreateUserCommand { Username = "jdoe", Email = "new@test.com", Cedula = "123" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("Ya existe", ex.Message);
        }

        [Fact]
        public async Task Handle_ExistingCommerceUser_ThrowsException()
        {
            // Arrange
            var existingUser = new User { Username = "other", RoleId = 4, CommerceId = 1 };
            _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<User> { existingUser });

            var command = new CreateUserCommand { Username = "newuser", RoleId = 4, CommerceId = 1, Email = "test@test.com", Cedula = "123" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Contains("El comercio seleccionado ya tiene un usuario asociado", ex.Message);
        }

        [Fact]
        public async Task Handle_ValidClientUser_CreatesUserAccountAndToken()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<User>());
            
            var command = new CreateUserCommand 
            { 
                Username = "newclient", 
                Email = "test@test.com", 
                Password = "Password123!",
                RoleId = 3,
                InitialAmount = 1000m
            };

            var mappedUser = new User { Id = 0, Username = "newclient", RoleId = 3, Email = "test@test.com" };
            _mapperMock.Setup(m => m.Map<User>(command)).Returns(mappedUser);

            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => 
            {
                u.Id = 10;
                return u;
            });

            _accountRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SavingsAccount>())).ReturnsAsync((SavingsAccount a) => a);

            // Act
            var resultId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(10, resultId);
            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == "newclient" && u.IsActive == false && !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
            _accountRepositoryMock.Verify(r => r.AddAsync(It.Is<SavingsAccount>(a => a.UserId == 10 && a.Balance == 1000m && a.IsPrincipal == true)), Times.Once);
            _transactionRepositoryMock.Verify(r => r.AddAsync(It.Is<DomainTransaction>(t => t.Amount == 1000m && t.Type == TransactionType.Credito)), Times.Once);
            _tokenRepositoryMock.Verify(r => r.AddAsync(It.Is<ConfirmationToken>(t => t.UserId == 10 && t.Type == TokenType.Activacion)), Times.Once);
            _emailServiceMock.Verify(s => s.SendActivationEmailAsync("test@test.com", It.IsAny<string>()), Times.Once);
        }
    }
}
