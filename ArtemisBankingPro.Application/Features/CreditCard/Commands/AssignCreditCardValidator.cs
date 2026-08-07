using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace ArtemisBankingPro.Application.Features.CreditCard.Commands
{
    public class AssignCreditCardValidator : AbstractValidator<AssignCreditCardCommand>
    {
        public AssignCreditCardValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("El cliente seleccionado es requerido.");
            RuleFor(x => x.CreditLimit)
                .GreaterThan(0).WithMessage("El límite de crédito debe ser mayor que cero.");
        }
    }
}
