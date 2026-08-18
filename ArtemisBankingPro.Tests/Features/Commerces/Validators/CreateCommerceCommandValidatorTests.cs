using ArtemisBankingPro.Application.Features.Commerces.Commands;
using FluentValidation.TestHelper;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Validators
{
    public class CreateCommerceCommandValidatorTests
    {
        private readonly CreateCommerceCommandValidator _validator;

        public CreateCommerceCommandValidatorTests()
        {
            _validator = new CreateCommerceCommandValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Have_Error_When_BusinessName_Is_Empty(string businessName)
        {
            var command = new CreateCommerceCommand { BusinessName = businessName };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.BusinessName);
        }

        [Fact]
        public void Should_Have_Error_When_BusinessName_Exceeds_MaxLength()
        {
            var command = new CreateCommerceCommand { BusinessName = new string('A', 151) };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.BusinessName);
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("1234567890")]
        [InlineData("123456789012")]
        public void Should_Have_Error_When_RNC_Is_Invalid_Length(string rnc)
        {
            var command = new CreateCommerceCommand { RNC = rnc };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.RNC)
                  .WithErrorMessage("El RNC debe tener exactamente 9 o 11 dígitos.");
        }

        [Fact]
        public void Should_Have_Error_When_RNC_Contains_Letters()
        {
            var command = new CreateCommerceCommand { RNC = "12345678A" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.RNC)
                  .WithErrorMessage("El RNC debe contener solo números.");
        }

        [Theory]
        [InlineData("131888251")]
        [InlineData("13188825122")]
        public void Should_Not_Have_Error_When_RNC_Is_Valid(string rnc)
        {
            var command = new CreateCommerceCommand { RNC = rnc };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.RNC);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        public void Should_Have_Error_When_Email_Is_Invalid(string email)
        {
            var command = new CreateCommerceCommand { Email = email };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Email_Is_Valid()
        {
            var command = new CreateCommerceCommand { Email = "test@artemis.com" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData("12345")]
        public void Should_Have_Error_When_Password_Is_Too_Short(string password)
        {
            var command = new CreateCommerceCommand { Password = password };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Theory]
        [InlineData("123456789")] // 9 digits
        [InlineData("12345678901")] // 11 digits
        [InlineData("123456789A")] // contains letter
        public void Should_Have_Error_When_Phone_Is_Invalid(string phone)
        {
            var command = new CreateCommerceCommand { Phone = phone };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Phone);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Phone_Is_Valid()
        {
            var command = new CreateCommerceCommand { Phone = "8095551234" };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.Phone);
        }
    }
}
