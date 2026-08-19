using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Handlers
{
    public class CreateSecondaryAccountCommandHandlerTests
    {
        private readonly Mock<ISavingsAccountService> _mockAccountService;
        private readonly CreateSecondaryAccountCommandHandler _handler;

        public CreateSecondaryAccountCommandHandlerTests()
        {
            _mockAccountService = new Mock<ISavingsAccountService>();
            _handler = new CreateSecondaryAccountCommandHandler(_mockAccountService.Object);
        }

        [Fact]
        public async Task Handle_Should_DelegateTo_CreateSecondaryAccountAsync_And_Return_Result()
        {
            // Arrange
            var command = new CreateSecondaryAccountCommand
            {
                ClientCedula = "00122233344",
                InitialBalance = 5000m
            };

            var expectedResponse = new SavingsAccountDetailDto
            {
                AccountNumber = "987654321",
                Balance = 5000m
            };

            _mockAccountService.Setup(s => s.CreateSecondaryAccountAsync(It.Is<CreateSecondaryAccountDto>(dto =>
                dto.ClientCedula == command.ClientCedula &&
                dto.InitialBalance == command.InitialBalance)))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.AccountNumber.Should().Be("987654321");
            result.Balance.Should().Be(5000m);
            _mockAccountService.Verify(s => s.CreateSecondaryAccountAsync(It.IsAny<CreateSecondaryAccountDto>()), Times.Once);
        }
    }
}
