using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Validatiors
{
    public class UpdateCreditLimitCommandValidatorTests
    {
        private readonly UpdateCreditLimitCommandValidator _validator = new();

        [Fact]
        public void Validate_WithValidData_IsValid()
        {
            var command = new UpdateCreditLimitCommand { CardId = 1, NewCreditLimit = 60000m };

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidCardId_Fails(int cardId)
        {
            var command = new UpdateCreditLimitCommand { CardId = cardId, NewCreditLimit = 60000m };

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "ID de tarjeta inválido.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-500)]
        public void Validate_WithInvalidCreditLimit_Fails(decimal newLimit)
        {
            var command = new UpdateCreditLimitCommand { CardId = 1, NewCreditLimit = newLimit };

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El límite de crédito debe ser mayor a cero.");
        }
    }
}
