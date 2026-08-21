using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Users.Commands.ResendActivationEmail
{
    public class ResendActivationEmailCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public string ActivationUrlFormat { get; set; } = string.Empty;
    }

    public class ResendActivationEmailCommandHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<ConfirmationToken> tokenRepository,
        IEmailService emailService) : IRequestHandler<ResendActivationEmailCommand, bool>
    {
        public async Task<bool> Handle(ResendActivationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("Usuario no encontrado.");

            if (user.IsActive)
                throw new Exception("El usuario ya se encuentra activo.");

            var activationToken = Guid.NewGuid().ToString();

            var confirmationToken = new ConfirmationToken
            {
                Token = activationToken,
                ExpirationDate = DateTime.UtcNow.AddHours(24),
                IsUsed = false,
                UserId = user.Id
            };

            await tokenRepository.AddAsync(confirmationToken);
            await tokenRepository.SaveChangesAsync();

            var url = string.IsNullOrEmpty(request.ActivationUrlFormat) ? null : request.ActivationUrlFormat.Replace("TOKENPLACEHOLDER", activationToken);
            
            try
            {
                await emailService.SendActivationEmailAsync(user.Email, activationToken, url);
            }
            catch
            {
                // Ignorar error de email tal como en CreateUserCommandHandler
            }

            return true;
        }
    }
}
