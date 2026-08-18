using ArtemisBankingPro.Application.Features.Loans.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Loan.Validators
{
    public class AssignLoanCommandValidatorTests
    {
        private readonly AssignLoanCommandValidator _validator = new();

        private static AssignLoanCommand ValidCommand() => new()
        {
            ClientId = "5",
            CapitalAmount = 50000m,
            TermInMonths = 24,
            AnnualInterestRate = 12m,
            AdminId = "1"
        };

        [Fact]
        public void Validate_ConDatosValidos_DeberiaSerValido()
        {
            var result = _validator.Validate(ValidCommand());

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_SinClientId_DeberiaFallar()
        {
            var command = ValidCommand();
            command.ClientId = string.Empty;

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El cliente es requerido.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1000)]
        public void Validate_ConMontoMenorOIgualACero_DeberiaFallar(decimal monto)
        {
            var command = ValidCommand();
            command.CapitalAmount = monto;

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El monto a prestar debe ser mayor a cero.");
        }

        [Fact]
        public void Validate_ConTasaNegativa_DeberiaFallar()
        {
            var command = ValidCommand();
            command.AnnualInterestRate = -5m;

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "La tasa de interés no puede ser negativa.");
        }

        [Theory]
        [InlineData(6)]
        [InlineData(12)]
        [InlineData(60)]
        public void Validate_ConPlazoPermitido_DeberiaSerValido(int meses)
        {
            var command = ValidCommand();
            command.TermInMonths = meses;

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(61)]
        [InlineData(100)]
        public void Validate_ConPlazoNoPermitido_DeberiaFallar(int meses)
        {
            var command = ValidCommand();
            command.TermInMonths = meses;

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El plazo debe ser en intervalos de 6, entre 6 y 60 meses.");
        }
    }
}
