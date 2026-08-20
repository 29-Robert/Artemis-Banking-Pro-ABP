using ArtemisBankingPro.Application.Features.Accounts.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Validators
{
    public class CreateBeneficiaryCommandValidatorTests
    {
        private readonly CreateBeneficiaryCommandValidator _validator;

        public CreateBeneficiaryCommandValidatorTests()
        {
            _validator = new CreateBeneficiaryCommandValidator();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Have_Error_When_ClientId_Is_Not_Positive(int clientId)
        {
            var command = new CreateBeneficiaryCommand { ClientId = clientId, AccountNumber = "123456789", Alias = "John Doe" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.ClientId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Should_Have_Error_When_AccountNumber_Is_Empty(string? accountNumber)
        {
            var command = new CreateBeneficiaryCommand { ClientId = 1, AccountNumber = accountNumber!, Alias = "John Doe" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData("1234567890")]
        public void Should_Have_Error_When_AccountNumber_Length_Is_Not_Nine(string accountNumber)
        {
            var command = new CreateBeneficiaryCommand { ClientId = 1, AccountNumber = accountNumber, Alias = "John Doe" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
        }

        [Theory]
        [InlineData("12345678a")]
        [InlineData("1234-5678")]
        public void Should_Have_Error_When_AccountNumber_Contains_Non_Digits(string accountNumber)
        {
            var command = new CreateBeneficiaryCommand { ClientId = 1, AccountNumber = accountNumber, Alias = "John Doe" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.AccountNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Should_Have_Error_When_Alias_Is_Empty(string? alias)
        {
            var command = new CreateBeneficiaryCommand { ClientId = 1, AccountNumber = "123456789", Alias = alias! };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Alias);
        }

        [Fact]
        public void Should_Have_Error_When_Alias_Exceeds_100_Characters()
        {
            var longAlias = new string('A', 101);
            var command = new CreateBeneficiaryCommand { ClientId = 1, AccountNumber = "123456789", Alias = longAlias };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Alias);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Command_Is_Valid()
        {
            var command = new CreateBeneficiaryCommand { ClientId = 1, AccountNumber = "123456789", Alias = "John Doe" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
