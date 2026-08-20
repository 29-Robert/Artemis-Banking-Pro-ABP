using ArtemisBankingPro.Application.DTOs.Commerces;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Commerces.Commands
{
    public class CreateCommerceCommand : IRequest<CommerceListItemDto>
    {
        public string BusinessName { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
