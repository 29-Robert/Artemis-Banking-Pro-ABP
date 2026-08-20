using ArtemisBankingPro.Application.DTOs.Users;
using ArtemisBankingPro.Application.Features.Users.Commands.Login;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Users.Handlers
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _handler = new LoginCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCredentialsActiveUser_ReturnsDtoWithTokenAndClaims()
        {
            var command = new LoginCommand { Username = "active_admin", Password = "123Password!" };

            var user = new User
            {
                Id = 1,
                Username = "active_admin",
                Email = "admin@artemis.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123Password!"),
                RoleId = 1,
                IsActive = true
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("active_admin")).ReturnsAsync(user);
            _jwtServiceMock.Setup(j => j.GenerateToken(user)).Returns("mock_token_abc");

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Token.Should().Be("mock_token_abc");
            result.UserId.Should().Be("1");
            result.UserName.Should().Be("active_admin");
            result.Email.Should().Be("admin@artemis.com");
            result.Role.Should().Be("Administrador");
            result.CommerceId.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidCommerceUser_ReturnsTokenWithCommerceId()
        {
            var command = new LoginCommand { Username = "commerce_user", Password = "123Password!" };

            var user = new User
            {
                Id = 2,
                Username = "commerce_user",
                Email = "comm@artemis.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123Password!"),
                RoleId = 4,
                CommerceId = 10,
                IsActive = true
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("commerce_user")).ReturnsAsync(user);
            _jwtServiceMock.Setup(j => j.GenerateToken(user)).Returns("mock_token_comm");

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Token.Should().Be("mock_token_comm");
            result.Role.Should().Be("Comercio");
            result.CommerceId.Should().Be(10);
        }

        [Fact]
        public async Task Handle_NonExistentUser_ThrowsUnauthorizedAccessException()
        {
            var command = new LoginCommand { Username = "nonexistent", Password = "password" };
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("nonexistent")).ReturnsAsync((User)null);

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Credenciales inválidas.");
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            var command = new LoginCommand { Username = "active_admin", Password = "WrongPassword!" };

            var user = new User
            {
                Username = "active_admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!"),
                IsActive = true
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("active_admin")).ReturnsAsync(user);

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Credenciales inválidas.");
        }

        [Fact]
        public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
        {
            var command = new LoginCommand { Username = "inactive_admin", Password = "123Password!" };

            var user = new User
            {
                Username = "inactive_admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123Password!"),
                IsActive = false
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("inactive_admin")).ReturnsAsync(user);

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Su cuenta se encuentra inactiva. Debe activar su cuenta antes de iniciar sesión.");
        }

        [Fact]
        public async Task Handle_UnauthorizedRole_ThrowsUnauthorizedAccessException()
        {
            var command = new LoginCommand { Username = "client_user", Password = "123Password!" };

            var user = new User
            {
                Username = "client_user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123Password!"),
                RoleId = 3,
                IsActive = true
            };

            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("client_user")).ReturnsAsync(user);

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Acceso denegado. No tiene permisos para utilizar este recurso.");
        }
    }
}
