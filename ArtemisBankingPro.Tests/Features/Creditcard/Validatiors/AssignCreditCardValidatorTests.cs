using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Validatiors
{
    public class AssignCreditCardValidatorTests
    {
        private readonly AssignCreditCardValidator _validator = new();

        [Fact]
        public void Validate_WithValidData_IsValid()
        {
            var command = new AssignCreditCardCommand { ClientId = "20", CreditLimit = 50000m, AdminId = 1 };

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithEmptyClientId_Fails()
        {
            var command = new AssignCreditCardCommand { ClientId = "", CreditLimit = 50000m, AdminId = 1 };

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El cliente seleccionado es requerido.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Validate_WithInvalidCreditLimit_Fails(decimal creditLimit)
        {
            var command = new AssignCreditCardCommand { ClientId = "20", CreditLimit = creditLimit, AdminId = 1 };

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El límite de crédito debe ser mayor que cero.");
        }
    }
}
