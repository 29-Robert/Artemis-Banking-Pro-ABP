using ArtemisBankingPro.Application.Features.Accounts.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Validators
{
    public class PayLoanCommandValidatorTests
    {
        private readonly PayLoanCommandValidator _validator;

        public PayLoanCommandValidatorTests()
        {
            _validator = new PayLoanCommandValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Should_Have_Error_When_SourceAccountNumber_Is_Empty(string? accountNumber)
        {
            var command = new PayLoanCommand { SourceAccountNumber = accountNumber!, LoanNumber = "LN-12345", Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData("1234567890")]
        public void Should_Have_Error_When_SourceAccountNumber_Length_Is_Not_Nine(string accountNumber)
        {
            var command = new PayLoanCommand { SourceAccountNumber = accountNumber, LoanNumber = "LN-12345", Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData("12345678a")]
        [InlineData("1234-5678")]
        public void Should_Have_Error_When_SourceAccountNumber_Contains_Non_Digits(string accountNumber)
        {
            var command = new PayLoanCommand { SourceAccountNumber = accountNumber, LoanNumber = "LN-12345", Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Should_Have_Error_When_LoanNumber_Is_Empty(string? loanNumber)
        {
            var command = new PayLoanCommand { SourceAccountNumber = "123456789", LoanNumber = loanNumber!, Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.LoanNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative(decimal amount)
        {
            var command = new PayLoanCommand { SourceAccountNumber = "123456789", LoanNumber = "LN-12345", Amount = amount };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Command_Is_Valid()
        {
            var command = new PayLoanCommand { SourceAccountNumber = "123456789", LoanNumber = "LN-12345", Amount = 500m };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
