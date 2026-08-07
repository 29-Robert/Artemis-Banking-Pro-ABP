using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.CreditCard.Commands
{
    public class UpdateCreditLimitCommandValidator : AbstractValidator<UpdateCreditLimitCommand>
    {
        public UpdateCreditLimitCommandValidator()
        {
            RuleFor(x => x.CardId).GreaterThan(0).WithMessage("ID de tarjeta inválido.");
            RuleFor(x => x.NewCreditLimit).GreaterThan(0).WithMessage("El límite de crédito debe ser mayor a cero.");
        }
    }
}
