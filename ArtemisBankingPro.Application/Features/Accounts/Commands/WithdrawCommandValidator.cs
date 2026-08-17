using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
    {
        public WithdrawCommandValidator()
        {
            RuleFor(x => x.SourceAccountNumber)
                .NotEmpty().WithMessage("El número de cuenta de origen es requerido.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto a retirar debe ser mayor que cero.");
        }
    }
}
