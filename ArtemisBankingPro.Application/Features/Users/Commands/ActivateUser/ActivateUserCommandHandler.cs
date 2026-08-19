using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser
{
    public class ActivateUserCommandHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<ConfirmationToken> tokenRepository) : IRequestHandler<ActivateUserCommand, bool>
    {
        public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            var tokens = await tokenRepository.GetAllAsync();
            var validToken = tokens.FirstOrDefault(t => t.Token == request.Token);

            if (validToken == null)
                throw new Exception("El token de activación es inválido o no existe.");

            if (validToken.IsUsed)
                throw new Exception("Este token ya fue utilizado anteriormente.");

            if (validToken.ExpirationDate < DateTime.UtcNow)
                throw new Exception("El token de activación ha expirado. Solicite uno nuevo.");

            var user = await userRepository.GetByIdAsync(validToken.UserId);
            if (user == null)
                throw new Exception("Usuario no encontrado.");

            if (user.IsActive)
                throw new Exception("Esta cuenta ya ha sido activada.");

            user.IsActive = true;
            await userRepository.UpdateAsync(user);

            validToken.IsUsed = true;
            await tokenRepository.UpdateAsync(validToken);

            await userRepository.SaveChangesAsync();
            await tokenRepository.SaveChangesAsync();

            return true;
        }
    }
}