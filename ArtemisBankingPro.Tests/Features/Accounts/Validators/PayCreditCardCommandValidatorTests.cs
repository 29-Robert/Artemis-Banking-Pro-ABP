using ArtemisBankingPro.Application.Features.Accounts.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Validators
{
    public class PayCreditCardCommandValidatorTests
    {
        private readonly PayCreditCardCommandValidator _validator;

        public PayCreditCardCommandValidatorTests()
        {
            _validator = new PayCreditCardCommandValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Should_Have_Error_When_SourceAccountNumber_Is_Empty(string? accountNumber)
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = accountNumber!, CardNumber = "1234567812345678", Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData("1234567890")]
        public void Should_Have_Error_When_SourceAccountNumber_Length_Is_Not_Nine(string accountNumber)
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = accountNumber, CardNumber = "1234567812345678", Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData("12345678a")]
        [InlineData("1234-5678")]
        public void Should_Have_Error_When_SourceAccountNumber_Contains_Non_Digits(string accountNumber)
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = accountNumber, CardNumber = "1234567812345678", Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Should_Have_Error_When_CardNumber_Is_Empty(string? cardNumber)
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = "123456789", CardNumber = cardNumber!, Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.CardNumber);
        }

        [Theory]
        [InlineData("123456781234567")]
        [InlineData("12345678123456789")]
        public void Should_Have_Error_When_CardNumber_Length_Is_Not_Sixteen(string cardNumber)
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = "123456789", CardNumber = cardNumber, Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.CardNumber);
        }

        [Theory]
        [InlineData("123456781234567a")]
        [InlineData("1234-5678-1234-5678")]
        public void Should_Have_Error_When_CardNumber_Contains_Non_Digits(string cardNumber)
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = "123456789", CardNumber = cardNumber, Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.CardNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative(decimal amount)
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = "123456789", CardNumber = "1234567812345678", Amount = amount };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Command_Is_Valid()
        {
            var command = new PayCreditCardCommand { SourceAccountNumber = "123456789", CardNumber = "1234567812345678", Amount = 500m };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
