using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Queries
{
    public class GetCreditCardByNumberQueryHandler(Domain.Interfaces.Repositories.ICreditCardRepository creditCardRepository) 
        : IRequestHandler<GetCreditCardByNumberQuery, Domain.Entities.CreditCard?>
    {
        public async Task<Domain.Entities.CreditCard?> Handle(GetCreditCardByNumberQuery request, CancellationToken cancellationToken)
        {
            return await creditCardRepository.GetByCardNumberAsync(request.CardNumber);
        }
    }
}
