using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Application.Features.Loans.Commands
{
    public class UpdateLoanRateCommandValidator : AbstractValidator<UpdateLoanRateCommand>
    {
        public UpdateLoanRateCommandValidator()
        {
            RuleFor(x => x.LoanId)
                .GreaterThan(0).WithMessage("El ID del préstamo es inválido.");
            RuleFor(x => x.NewAnnualInterestRate)
                .GreaterThanOrEqualTo(0).WithMessage("La tasa no puede ser negativa.")
                .LessThanOrEqualTo(100).WithMessage("La tasa no puede superar el 100%.");
        }
    }
}
