using ArtemisBankingPro.Application.Features.Loans.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Loan.Validators
{
    public class UpdateLoanRateCommandValidatorTests
    {
        private readonly UpdateLoanRateCommandValidator _validator = new();

        [Fact]
        public void Validate_ConDatosValidos_DeberiaSerValido()
        {
            var command = new UpdateLoanRateCommand { LoanId = 1, NewAnnualInterestRate = 15m };

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_ConLoanIdInvalido_DeberiaFallar(int loanId)
        {
            var command = new UpdateLoanRateCommand { LoanId = loanId, NewAnnualInterestRate = 10m };

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El ID del préstamo es inválido.");
        }

        [Fact]
        public void Validate_ConTasaNegativa_DeberiaFallar()
        {
            var command = new UpdateLoanRateCommand { LoanId = 1, NewAnnualInterestRate = -1m };

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "La tasa no puede ser negativa.");
        }

        [Fact]
        public void Validate_ConTasaMayorA100_DeberiaFallar()
        {
            var command = new UpdateLoanRateCommand { LoanId = 1, NewAnnualInterestRate = 101m };

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "La tasa no puede superar el 100%.");
        }

        [Fact]
        public void Validate_ConTasaExactamente100_DeberiaSerValido()
        {
            var command = new UpdateLoanRateCommand { LoanId = 1, NewAnnualInterestRate = 100m };

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }
    }
}

