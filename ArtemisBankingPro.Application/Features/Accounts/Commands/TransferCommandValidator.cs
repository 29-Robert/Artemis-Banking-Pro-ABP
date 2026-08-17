using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class TransferCommandValidator : AbstractValidator<TransferCommand>
    {
        public TransferCommandValidator()
        {
            RuleFor(x => x.SourceAccountNumber)
                .NotEmpty().WithMessage("La cuenta de origen es requerida.");

            RuleFor(x => x.DestinationAccountNumber)
                .NotEmpty().WithMessage("La cuenta de destino es requerida.")
                .NotEqual(x => x.SourceAccountNumber).WithMessage("La cuenta de origen y destino no pueden ser la misma.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto a transferir debe ser mayor que cero.");
        }
    }
}
