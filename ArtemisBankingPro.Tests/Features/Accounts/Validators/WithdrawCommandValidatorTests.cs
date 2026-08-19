using ArtemisBankingPro.Application.Features.Accounts.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Validators
{
    public class WithdrawCommandValidatorTests
    {
        private readonly WithdrawCommandValidator _validator;

        public WithdrawCommandValidatorTests()
        {
            _validator = new WithdrawCommandValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Should_Have_Error_When_SourceAccountNumber_Is_Empty(string? accountNumber)
        {
            var command = new WithdrawCommand { SourceAccountNumber = accountNumber!, Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative(decimal amount)
        {
            var command = new WithdrawCommand { SourceAccountNumber = "123456789", Amount = amount };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Command_Is_Valid()
        {
            var command = new WithdrawCommand { SourceAccountNumber = "123456789", Amount = 500m };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
