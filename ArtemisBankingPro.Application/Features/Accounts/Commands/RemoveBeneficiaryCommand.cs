using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class RemoveBeneficiaryCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }
}
