using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Features.HermesPay.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.HermesPay.Handlers
{
    public class ProcessPaymentCommandHandlerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly ProcessPaymentCommandHandler _handler;

        public ProcessPaymentCommandHandlerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _handler = new ProcessPaymentCommandHandler(_mockPaymentService.Object);
        }

        [Fact]
        public async Task Handle_Should_DelegateTo_ProcessPaymentAsync_And_Return_Result()
        {
            // Arrange
            var command = new ProcessPaymentCommand
            {
                CommerceId = 1,
                CardNumber = "1234567890123456",
                ExpirationMonth = "12",
                ExpirationYear = "2029",
                Cvc = "123",
                Amount = 1500m,
                Description = "Buying widgets",
                UserId = 10
            };

            var expectedResponse = new TransactionResponseDto
            {
                Id = 42,
                Status = Domain.Enums.TransactionStatus.Aprobada,
                Amount = 1500m,
                TransactionDate = System.DateTime.UtcNow,
                AuthorizationCode = "AUTH123"
            };

            _mockPaymentService.Setup(s => s.ProcessPaymentAsync(
                command.CommerceId,
                It.Is<ProcessPaymentRequestDto>(dto =>
                    dto.CardNumber == command.CardNumber &&
                    dto.ExpirationMonth == command.ExpirationMonth &&
                    dto.ExpirationYear == command.ExpirationYear &&
                    dto.Cvc == command.Cvc &&
                    dto.Amount == command.Amount &&
                    dto.Description == command.Description),
                command.UserId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(42);
            result.Status.Should().Be(Domain.Enums.TransactionStatus.Aprobada);
            _mockPaymentService.Verify(s => s.ProcessPaymentAsync(It.IsAny<int>(), It.IsAny<ProcessPaymentRequestDto>(), It.IsAny<int>()), Times.Once);
        }
    }
}
