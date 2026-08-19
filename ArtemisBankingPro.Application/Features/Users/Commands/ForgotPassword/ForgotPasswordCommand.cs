using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<bool>
    {
        public string Username { get; set; } = string.Empty;
    }

    public class ForgotPasswordCommandHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<ConfirmationToken> tokenRepository,
        IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, bool>
    {
        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
                throw new Exception("No existe un usuario registrado con este nombre de usuario.");

            if (string.IsNullOrEmpty(user.Email))
                throw new Exception("Este usuario no tiene un correo electrónico registrado. No es posible enviar la solicitud de restablecimiento.");

            if (user.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Comercio)
                throw new Exception("Este usuario no tiene permisos para acceder a la aplicación web.");

            user.IsActive = false;

            var resetToken = Guid.NewGuid().ToString();
            var confirmationToken = new ConfirmationToken
            {
                UserId = user.Id,
                Token = resetToken,
                Type = TokenType.RestablecimientoContrasena,
                ExpirationDate = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };

            await tokenRepository.AddAsync(confirmationToken);

            await userRepository.SaveChangesAsync();
            await tokenRepository.SaveChangesAsync();

            try
            {
                await emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
            }
            catch
            {
            }

            return true;
        }
    }
}
