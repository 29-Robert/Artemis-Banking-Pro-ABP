using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? CommerceId { get; set; }

        public decimal InitialAmount { get; set; }
        public string? ActivationUrlFormat { get; set; }
    }
}