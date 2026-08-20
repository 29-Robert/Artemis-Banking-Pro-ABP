using ArtemisBankingPro.Application.DTOs.Users;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Users.Commands.Login
{
    public class LoginCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService) : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Faltan parámetros requeridos.");
            }

            var user = await userRepository.GetByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Su cuenta se encuentra inactiva. Debe activar su cuenta antes de iniciar sesión.");
            }

            if (user.RoleId != 1 && user.RoleId != 4)
            {
                throw new UnauthorizedAccessException("Acceso denegado. No tiene permisos para utilizar este recurso.");
            }

            var token = jwtService.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                UserId = user.Id.ToString(),
                UserName = user.Username,
                Email = user.Email,
                Role = user.Role?.Name ?? (user.RoleId == 1 ? "Administrador" : "Comercio"),
                CommerceId = user.CommerceId,
                Expiration = DateTime.UtcNow.AddHours(1)
            };
        }
    }
}
