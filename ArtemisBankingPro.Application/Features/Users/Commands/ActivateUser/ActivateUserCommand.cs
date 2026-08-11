using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.ActivateUser
{
    public class ActivateUserCommand : IRequest<bool>
    {
        public string Token { get; set; } = string.Empty;
    }
}