using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler(
        IGenericRepository<User> userRepository,
        ISavingsAccountService savingsAccountService) : IRequestHandler<UpdateUserCommand>
    {
        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetByIdAsync(request.Id) ?? throw new Exception("El usuario no fue encontrado.");
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Cedula = request.Cedula;
            user.Email = request.Email;
            user.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await userRepository.SaveChangesAsync();

            if (request.AdditionalAmount > 0 && user.RoleId == (int)ArtemisBankingPro.Domain.Enums.Roles.Cliente)
            {
                await savingsAccountService.CreditToMainAsync(user.Id.ToString(), request.AdditionalAmount);
            }
        }
    }
}
