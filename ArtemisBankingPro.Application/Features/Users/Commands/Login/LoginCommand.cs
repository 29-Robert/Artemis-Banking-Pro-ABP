using ArtemisBankingPro.Application.DTOs.Users;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Users.Commands.Login
{
    public class LoginCommand : IRequest<LoginResponseDto>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
