using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayCreditCardOwnAccountCommandValidator : AbstractValidator<PayCreditCardOwnAccountCommand>
    {
        public PayCreditCardOwnAccountCommandValidator()
        {
            RuleFor(x => x.SourceAccountNumber)
                .NotEmpty().WithMessage("La cuenta de origen es requerida.")
                .Length(9).WithMessage("La cuenta de origen debe tener exactamente 9 dígitos.")
                .Matches(@"^\d+$").WithMessage("La cuenta de origen solo debe contener números.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("El número de tarjeta es requerido.")
                .Length(16).WithMessage("El número de tarjeta debe tener exactamente 16 dígitos.")
                .Matches(@"^\d+$").WithMessage("El número de tarjeta solo debe contener números.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto a pagar debe ser mayor que cero.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El ID de usuario es requerido.");
        }
    }
}
