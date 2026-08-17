using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetBeneficiariesByClientIdQueryHandler(IBeneficiaryRepository beneficiaryRepository) : IRequestHandler<GetBeneficiariesByClientIdQuery, IEnumerable<Beneficiary>>
    {
        public async Task<IEnumerable<Beneficiary>> Handle(GetBeneficiariesByClientIdQuery request, CancellationToken cancellationToken)
        {
            return await beneficiaryRepository.GetByClientAsync(request.ClientId);
        }
    }
}
