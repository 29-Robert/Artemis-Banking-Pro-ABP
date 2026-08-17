using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CreateBeneficiaryCommandHandler(IBeneficiaryService beneficiaryService) : IRequestHandler<CreateBeneficiaryCommand, BeneficiaryDto>
    {
        public async Task<BeneficiaryDto> Handle(CreateBeneficiaryCommand request, CancellationToken cancellationToken)
        {
            var dto = new CreateBeneficiaryDto
            {
                AccountNumber = request.AccountNumber,
                Alias = request.Alias
            };
            return await beneficiaryService.AddBeneficiaryAsync(request.ClientId, dto);
        }
    }
}
