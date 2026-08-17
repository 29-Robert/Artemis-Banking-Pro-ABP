using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class DepositCommandValidator : AbstractValidator<DepositCommand>
    {
        public DepositCommandValidator()
        {
            RuleFor(x => x.DestinationAccountNumber)
                .NotEmpty().WithMessage("El número de cuenta de destino es requerido.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto a depositar debe ser mayor que cero.");
        }
    }
}
