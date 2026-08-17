using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using MediatR;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CreateBeneficiaryCommand : IRequest<BeneficiaryDto>
    {
        public int ClientId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
    }
}
