using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Users.Commands.RequestResendActivationEmail
{
    public class RequestResendActivationEmailCommand : IRequest<bool>
    {
        public string EmailOrUsername { get; set; } = string.Empty;
        public string ActivationUrlFormat { get; set; } = string.Empty;
    }

    public class RequestResendActivationEmailCommandHandler(
        IUserRepository userRepository,
        IGenericRepository<ConfirmationToken> tokenRepository,
        IEmailService emailService) : IRequestHandler<RequestResendActivationEmailCommand, bool>
    {
        public async Task<bool> Handle(RequestResendActivationEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByUsernameAsync(request.EmailOrUsername);
            
            if (user == null)
            {
                var users = await userRepository.GetAllAsync();
                user = users.FirstOrDefault(u => u.Email.Equals(request.EmailOrUsername, StringComparison.OrdinalIgnoreCase));
            }

            // Always return true to avoid disclosing if user exists
            if (user == null) return true;
            if (user.IsActive) return true; 

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
                // Ignore email errors in production
            }

            return true;
        }
    }
}
