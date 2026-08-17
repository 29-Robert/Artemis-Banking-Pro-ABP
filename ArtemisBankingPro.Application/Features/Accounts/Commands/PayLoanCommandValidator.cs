using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class PayLoanCommandValidator : AbstractValidator<PayLoanCommand>
    {
        public PayLoanCommandValidator()
        {
            RuleFor(x => x.SourceAccountNumber)
                .NotEmpty().WithMessage("La cuenta de origen es requerida.")
                .Length(9).WithMessage("La cuenta de origen debe tener exactamente 9 dígitos.")
                .Matches(@"^\d+$").WithMessage("La cuenta de origen solo debe contener números.");

            RuleFor(x => x.LoanNumber)
                .NotEmpty().WithMessage("El número de préstamo es requerido.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto a pagar debe ser mayor que cero.");
        }
    }
}
