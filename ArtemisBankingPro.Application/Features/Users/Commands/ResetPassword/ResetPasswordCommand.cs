using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public int? UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordCommandHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<ConfirmationToken> tokenRepository) : IRequestHandler<ResetPasswordCommand, bool>
    {
        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var tokens = await tokenRepository.GetAllAsync();
            var validToken = tokens.FirstOrDefault(t =>
                t.Token == request.Token &&
                t.Type == TokenType.RestablecimientoContrasena &&
                !t.IsUsed &&
                t.ExpirationDate > DateTime.UtcNow);

            if (validToken == null)
                throw new Exception("El enlace de restablecimiento ha expirado o no es válido.");

            if (request.UserId.HasValue && validToken.UserId != request.UserId.Value)
                throw new Exception("El token no pertenece al usuario indicado.");

            var user = await userRepository.GetByIdAsync(validToken.UserId);
            if (user == null)
                throw new Exception("Usuario no encontrado.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.IsActive = true;

            validToken.IsUsed = true;

            await userRepository.SaveChangesAsync();
            await tokenRepository.SaveChangesAsync();

            return true;
        }
    }
}