using ArtemisBankingPro.Domain.Entities;
using MediatR;

namespace ArtemisBankingPro.Application.Features.CreditCard.Queries
{
    public class GetCreditCardByNumberQuery : IRequest<ArtemisBankingPro.Domain.Entities.CreditCard?>
    {
        public string CardNumber { get; set; } = string.Empty;
    }
}
