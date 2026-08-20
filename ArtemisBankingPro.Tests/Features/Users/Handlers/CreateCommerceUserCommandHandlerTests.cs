using ArtemisBankingPro.Application.Features.Users.Commands.CreateUser;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using FluentAssertions;
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
    public class CreateCommerceUserCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
        private readonly Mock<IGenericRepository<SavingsAccount>> _accountRepositoryMock;
        private readonly Mock<IGenericRepository<ConfirmationToken>> _tokenRepositoryMock;
        private readonly Mock<IGenericRepository<DomainTransaction>> _transactionRepositoryMock;
        private readonly Mock<IGenericRepository<Commerce>> _commerceRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateCommerceUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _accountRepositoryMock = new Mock<IGenericRepository<SavingsAccount>>();
            _tokenRepositoryMock = new Mock<IGenericRepository<ConfirmationToken>>();
            _transactionRepositoryMock = new Mock<IGenericRepository<DomainTransaction>>();
            _commerceRepositoryMock = new Mock<IGenericRepository<Commerce>>();
            _emailServiceMock = new Mock<IEmailService>();
            _mapperMock = new Mock<IMapper>();

            _handler = new CreateUserCommandHandler(
                _userRepositoryMock.Object,
                _accountRepositoryMock.Object,
                _tokenRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _commerceRepositoryMock.Object,
                _emailServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_SuccessfulCommerceUserCreation_PersistsUserAndAccountAndToken_SendsEmail()
        {
            var command = new CreateUserCommand
            {
                FirstName = "Juan",
                LastName = "Perez",
                Cedula = "40200000001",
                Email = "juan@tienda.com",
                Username = "juan_comercio",
                Password = "Password123!",
                RoleId = 4,
                CommerceId = 10,
                InitialAmount = 5000m,
                ActivationUrlFormat = "http://localhost/activate?token=TOKENPLACEHOLDER"
            };

            var mappedUser = new User
            {
                Username = "juan_comercio",
                Email = "juan@tienda.com",
                Cedula = "40200000001",
                RoleId = 4,
                CommerceId = 10
            };

            _commerceRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Commerce { Id = 10, BusinessName = "Tienda X" });
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            _mapperMock.Setup(m => m.Map<User>(command)).Returns(mappedUser);

            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) =>
            {
                u.Id = 101;
                return u;
            });

            _accountRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SavingsAccount>())).ReturnsAsync((SavingsAccount a) => a);
            _tokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ConfirmationToken>())).ReturnsAsync((ConfirmationToken t) => t);
            _transactionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DomainTransaction>())).ReturnsAsync((DomainTransaction tr) => tr);

            var resultId = await _handler.Handle(command, CancellationToken.None);

            resultId.Should().Be(101);
            _commerceRepositoryMock.Verify(r => r.GetByIdAsync(10), Times.Once);
            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.IsActive == false && u.CommerceId == 10)), Times.Once);
            _accountRepositoryMock.Verify(r => r.AddAsync(It.Is<SavingsAccount>(a => a.UserId == 101 && a.Balance == 5000m)), Times.Once);
            _tokenRepositoryMock.Verify(r => r.AddAsync(It.Is<ConfirmationToken>(t => t.UserId == 101 && t.Type == TokenType.Activacion)), Times.Once);
            _emailServiceMock.Verify(s => s.SendActivationEmailAsync("juan@tienda.com", It.IsAny<string>(), It.Is<string>(url => url.Contains("activate?token="))), Times.Once);
        }

        [Fact]
        public async Task Handle_CommerceNotFound_ThrowsKeyNotFoundException()
        {
            var command = new CreateUserCommand
            {
                Username = "new_comm_user",
                Email = "new_comm@test.com",
                Cedula = "11111111111",
                RoleId = 4,
                CommerceId = 99
            };

            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User>());
            _commerceRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Commerce)null);

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("No se encontró el comercio con el ID 99.");
        }

        [Fact]
        public async Task Handle_CommerceAlreadyHasUser_ThrowsException()
        {
            var command = new CreateUserCommand
            {
                Username = "new_comm_user",
                Email = "new_comm@test.com",
                Cedula = "11111111111",
                RoleId = 4,
                CommerceId = 10
            };

            var existingUser = new User
            {
                RoleId = 4,
                CommerceId = 10
            };

            _commerceRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Commerce { Id = 10, BusinessName = "Tienda X" });
            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { existingUser });

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("El comercio seleccionado ya tiene un usuario asociado. Solo se permite un usuario por comercio.");
        }

        [Fact]
        public async Task Handle_DuplicateUsernameOrEmailOrCedula_ThrowsException()
        {
            var command = new CreateUserCommand
            {
                Username = "existing_user",
                Email = "new@test.com",
                Cedula = "12345678901",
                RoleId = 4,
                CommerceId = 10
            };

            var existingUser = new User
            {
                Username = "existing_user"
            };

            _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<User> { existingUser });

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Ya existe un usuario con ese nombre de usuario, correo o cédula.");
        }
    }
}
