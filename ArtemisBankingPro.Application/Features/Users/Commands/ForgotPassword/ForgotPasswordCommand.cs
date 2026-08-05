using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordCommandHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<ConfirmationToken> tokenRepository,
        IEmailService emailService) : IRequestHandler<ForgotPasswordCommand, bool>
    {
        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var users = await userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return true;

            var resetToken = Guid.NewGuid().ToString();
            var confirmationToken = new ConfirmationToken
            {
                UserId = user.Id,
                Token = resetToken,
                Type = TokenType.RestablecimientoContrasena,
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            await tokenRepository.AddAsync(confirmationToken);

            try
            {
                await emailService.SendActivationEmailAsync(user.Email, resetToken);
            }
            catch
            {
            }

            return true;
        }
    }
}