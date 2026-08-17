using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayCreditCardCommandValidator : AbstractValidator<PayCreditCardCommand>
    {
        public PayCreditCardCommandValidator()
        {
            RuleFor(x => x.SourceAccountNumber)
                .NotEmpty().WithMessage("La cuenta de origen es requerida.")
                .Length(9).WithMessage("La cuenta de origen debe tener exactamente 9 dígitos.")
                .Matches(@"^\d+$").WithMessage("La cuenta de origen solo debe contener números.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("El número de tarjeta de crédito es requerido.")
                .Length(16).WithMessage("El número de tarjeta de crédito debe tener exactamente 16 dígitos.")
                .Matches(@"^\d+$").WithMessage("El número de tarjeta de crédito solo debe contener números.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto a pagar debe ser mayor que cero.");
        }
    }
}
