using FluentValidation;

namespace ArtemisBankingPro.Application.Features.Commerces.Commands
{
    public class UpdateCommerceCommandValidator : AbstractValidator<UpdateCommerceCommand>
    {
        public UpdateCommerceCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del comercio debe ser un entero positivo.");

            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("El nombre comercial es requerido.")
                .MaximumLength(150).WithMessage("El nombre comercial no puede exceder los 150 caracteres.");

            RuleFor(x => x.RNC)
                .NotEmpty().WithMessage("El RNC es requerido.")
                .Must(rnc => rnc.Length == 9 || rnc.Length == 11).WithMessage("El RNC debe tener exactamente 9 o 11 dígitos.")
                .Matches(@"^\d+$").WithMessage("El RNC debe contener solo números.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo electrónico es requerido.")
                .EmailAddress().WithMessage("El correo electrónico debe ser válido.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("El teléfono es requerido.")
                .Matches(@"^\d{10}$").WithMessage("El teléfono debe tener exactamente 10 dígitos numéricos.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("La dirección es requerida.");
        }
    }
}
