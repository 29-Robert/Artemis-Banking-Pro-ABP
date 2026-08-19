using ArtemisBankingPro.Application.Features.Accounts.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Validators
{
    public class TransferCommandValidatorTests
    {
        private readonly TransferCommandValidator _validator;

        public TransferCommandValidatorTests()
        {
            _validator = new TransferCommandValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Have_Error_When_SourceAccountNumber_Is_Empty(string accountNumber)
        {
            var command = new TransferCommand { SourceAccountNumber = accountNumber, DestinationAccountNumber = "987654321", Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.SourceAccountNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Have_Error_When_DestinationAccountNumber_Is_Empty(string accountNumber)
        {
            var command = new TransferCommand { SourceAccountNumber = "123456789", DestinationAccountNumber = accountNumber, Amount = 100m };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.DestinationAccountNumber);
        }

        [Fact]
        public void Should_Have_Error_When_Source_And_Destination_Accounts_Are_Same()
        {
            var command = new TransferCommand 
            { 
                SourceAccountNumber = "123456789", 
                DestinationAccountNumber = "123456789", 
                Amount = 100m 
            };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.DestinationAccountNumber)
                  .WithErrorMessage("La cuenta de origen y destino no pueden ser la misma.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Should_Have_Error_When_Amount_Is_Zero_Or_Negative(decimal amount)
        {
            var command = new TransferCommand { SourceAccountNumber = "123456789", DestinationAccountNumber = "987654321", Amount = amount };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Transfer_Is_Valid()
        {
            var command = new TransferCommand { SourceAccountNumber = "123456789", DestinationAccountNumber = "987654321", Amount = 150.75m };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.SourceAccountNumber);
            result.ShouldNotHaveValidationErrorFor(x => x.DestinationAccountNumber);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }
    }
}
