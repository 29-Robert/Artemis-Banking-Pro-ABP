using FluentValidation;

namespace ArtemisBankingPro.Application.Features.HermesPay.Commands
{
    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(x => x.CommerceId)
                .GreaterThan(0).WithMessage("El ID del comercio debe ser un entero positivo.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("El número de tarjeta es requerido.")
                .Length(16).WithMessage("El número de tarjeta debe tener exactamente 16 dígitos.")
                .Matches(@"^\d+$").WithMessage("El número de tarjeta solo debe contener números.");


            RuleFor(x => x.ExpirationMonth)
                .NotEmpty().WithMessage("El mes de expiración es requerido.")
                .Length(2).WithMessage("El mes de expiración debe tener 2 dígitos.")
                .Matches(@"^(0[1-9]|1[0-2])$").WithMessage("El mes de expiración debe estar entre 01 y 12.");

            RuleFor(x => x.ExpirationYear)
                .NotEmpty().WithMessage("El año de expiración es requerido.")
                .Length(4).WithMessage("El año de expiración debe tener 4 dígitos.")
                .Matches(@"^\d{4}$").WithMessage("El año de expiración debe ser un año de 4 dígitos.");

            RuleFor(x => x.Cvc)
                .NotEmpty().WithMessage("El código CVC es requerido.")
                .Length(3).WithMessage("El código CVC debe tener exactamente 3 dígitos.")
                .Matches(@"^\d{3}$").WithMessage("El código CVC solo debe contener números.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto de la transacción debe ser mayor que cero.");
        }
    }
}
