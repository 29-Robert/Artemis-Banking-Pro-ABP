using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using global::ArtemisBankingPro.Application.Features.Loans.Commands;


    namespace ArtemisBankingPro.Application.Features.Loans.Commands
    {
        public class AssignLoanCommandValidator : AbstractValidator<AssignLoanCommand>
        {
            public AssignLoanCommandValidator()
            {
                RuleFor(x => x.ClientId).NotEmpty().WithMessage("El cliente es requerido.");
                RuleFor(x => x.CapitalAmount).GreaterThan(0).WithMessage("El monto a prestar debe ser mayor a cero.");
                RuleFor(x => x.AnnualInterestRate).GreaterThanOrEqualTo(0).WithMessage("La tasa de interés no puede ser negativa.");
                RuleFor(x => x.TermInMonths)
                    .Must(x => new[] { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 }.Contains(x))
                    .WithMessage("El plazo debe ser en intervalos de 6, entre 6 y 60 meses.");
            }
        }
    }

