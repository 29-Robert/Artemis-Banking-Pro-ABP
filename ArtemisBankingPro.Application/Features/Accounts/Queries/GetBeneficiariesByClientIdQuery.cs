using ArtemisBankingPro.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace ArtemisBankingPro.Application.Features.Accounts.Queries
{
    public class GetBeneficiariesByClientIdQuery : IRequest<IEnumerable<Beneficiary>>
    {
        public int ClientId { get; set; }
    }
}
