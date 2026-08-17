using MediatR;

namespace ArtemisBankingPro.Application.Features.Commerces.Commands
{
    public class ChangeCommerceStatusCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }
}
