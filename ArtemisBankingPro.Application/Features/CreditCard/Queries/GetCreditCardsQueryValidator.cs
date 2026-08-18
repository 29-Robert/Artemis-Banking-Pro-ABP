using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Queries
{
    public class GetCreditCardsQueryValidator : AbstractValidator<GetAllCreditCardsQuery>
    {
        public GetCreditCardsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("El número de página debe ser mayor que cero.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
        }
    }
}
