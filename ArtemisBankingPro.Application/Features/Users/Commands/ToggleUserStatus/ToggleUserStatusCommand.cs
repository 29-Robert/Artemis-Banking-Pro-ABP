using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.ToggleUserStatus
{
    public class ToggleUserStatusCommand : IRequest<bool>
    {
        public int UserId { get; set; }
    }

    public class ToggleUserStatusCommandHandler(
        IGenericRepository<User> userRepository) : IRequestHandler<ToggleUserStatusCommand, bool>
    {
        public async Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("Usuario no encontrado.");

            user.IsActive = !user.IsActive;
            await userRepository.UpdateAsync(user);
            await userRepository.SaveChangesAsync();

            return user.IsActive;
        }
    }
}