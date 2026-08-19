using ArtemisBankingPro.Application.Features.HermesPay.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.HermesPay.Validators
{
    public class ProcessPaymentCommandValidatorTests
    {
        private readonly ProcessPaymentCommandValidator _validator;

        public ProcessPaymentCommandValidatorTests()
        {
            _validator = new ProcessPaymentCommandValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Have_Error_When_CommerceId_Is_Not_Positive(int commerceId)
        {
            var command = new ProcessPaymentCommand { CommerceId = commerceId };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.CommerceId);
        }

        [Fact]
        public void Should_Not_Have_Error_When_CommerceId_Is_Positive()
        {
            var command = new ProcessPaymentCommand { CommerceId = 1 };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.CommerceId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("123456789012345")] // 15 digits
        [InlineData("12345678901234567")] // 17 digits
        [InlineData("123456789012345A")] // non-digits
        public void Should_Have_Error_When_CardNumber_Is_Invalid(string cardNumber)
        {
            var command = new ProcessPaymentCommand { CardNumber = cardNumber };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.CardNumber);
        }

        [Fact]
        public void Should_Not_Have_Error_When_CardNumber_Is_Valid()
        {
            var command = new ProcessPaymentCommand { CardNumber = "1234567890123456" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.CardNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("13")]
        [InlineData("00")]
        [InlineData("A1")]
        public void Should_Have_Error_When_ExpirationMonth_Is_Invalid(string month)
        {
            var command = new ProcessPaymentCommand { ExpirationMonth = month };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.ExpirationMonth);
        }

        [Theory]
        [InlineData("01")]
        [InlineData("12")]
        public void Should_Not_Have_Error_When_ExpirationMonth_Is_Valid(string month)
        {
            var command = new ProcessPaymentCommand { ExpirationMonth = month };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.ExpirationMonth);
        }

        [Theory]
        [InlineData("")]
        [InlineData("202")]
        [InlineData("20245")]
        [InlineData("ABCD")]
        public void Should_Have_Error_When_ExpirationYear_Is_Invalid(string year)
        {
            var command = new ProcessPaymentCommand { ExpirationYear = year };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.ExpirationYear);
        }

        [Fact]
        public void Should_Not_Have_Error_When_ExpirationYear_Is_Valid()
        {
            var command = new ProcessPaymentCommand { ExpirationYear = "2029" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.ExpirationYear);
        }

        [Theory]
        [InlineData("")]
        [InlineData("12")]
        [InlineData("1234")]
        [InlineData("12A")]
        public void Should_Have_Error_When_Cvc_Is_Invalid(string cvc)
        {
            var command = new ProcessPaymentCommand { Cvc = cvc };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Cvc);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Cvc_Is_Valid()
        {
            var command = new ProcessPaymentCommand { Cvc = "123" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Cvc);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10.50)]
        public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative(decimal amount)
        {
            var command = new ProcessPaymentCommand { Amount = amount };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Amount_Is_Positive()
        {
            var command = new ProcessPaymentCommand { Amount = 100.50m };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }
    }
}
