using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard
{
    public class CancelCreditCardCommandValidator : AbstractValidator<CancelCreditCardCommand>
    {
        public CancelCreditCardCommandValidator()
        {
            RuleFor(x => x.CardId).GreaterThan(0).WithMessage("ID de tarjeta inválido.");
        }
    }
}
