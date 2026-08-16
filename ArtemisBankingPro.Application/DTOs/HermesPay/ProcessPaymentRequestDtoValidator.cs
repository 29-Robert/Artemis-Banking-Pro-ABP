using FluentValidation;
using System;

namespace ArtemisBankingPro.Application.DTOs.HermesPay
{
    public class ProcessPaymentRequestDtoValidator : AbstractValidator<ProcessPaymentRequestDto>
    {
        public ProcessPaymentRequestDtoValidator()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("El número de tarjeta es requerido.")
                .Length(16).WithMessage("El número de tarjeta debe tener exactamente 16 dígitos.")
                .Matches(@"^\d{16}$").WithMessage("El número de tarjeta debe contener solo números.");

            RuleFor(x => x.ExpirationMonth)
                .NotEmpty().WithMessage("El mes de expiración es requerido.")
                .Length(2).WithMessage("El mes de expiración debe tener 2 dígitos (ej: 08).")
                .Matches(@"^(0[1-9]|1[0-2])$").WithMessage("El mes de expiración debe estar entre 01 y 12.");

            RuleFor(x => x.ExpirationYear)
                .NotEmpty().WithMessage("El año de expiración es requerido.")
                .Length(4).WithMessage("El año de expiración debe tener 4 dígitos (ej: 2026).")
                .Matches(@"^\d{4}$").WithMessage("El año de expiración debe ser un año de 4 dígitos.")
                .Must(yearStr =>
                {
                    if (int.TryParse(yearStr, out var year))
                    {
                        return year >= DateTime.UtcNow.Year;
                    }
                    return false;
                }).WithMessage("El año de expiración no puede ser menor al año actual.");

            RuleFor(x => x.Cvc)
                .NotEmpty().WithMessage("El CVC es requerido.")
                .Length(3).WithMessage("El CVC debe tener exactamente 3 dígitos.")
                .Matches(@"^\d{3}$").WithMessage("El CVC debe contener solo números.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("El monto a procesar debe ser estrictamente mayor a cero.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La descripción es requerida.");
        }
    }
}
