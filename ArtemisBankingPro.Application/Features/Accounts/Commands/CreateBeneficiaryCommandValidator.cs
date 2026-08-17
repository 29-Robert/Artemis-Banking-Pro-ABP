using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Accounts.Commands
{
    public class CreateBeneficiaryCommandValidator : AbstractValidator<CreateBeneficiaryCommand>
    {
        public CreateBeneficiaryCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("El ID del cliente debe ser un entero positivo.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("El número de cuenta es requerido.")
                .Length(9).WithMessage("El número de cuenta debe tener exactamente 9 dígitos.")
                .Matches(@"^\d+$").WithMessage("El número de cuenta solo debe contener números.");

            RuleFor(x => x.Alias)
                .NotEmpty().WithMessage("El alias del beneficiario es requerido.")
                .MaximumLength(100).WithMessage("El alias no puede superar los 100 caracteres.");
        }
    }
}
