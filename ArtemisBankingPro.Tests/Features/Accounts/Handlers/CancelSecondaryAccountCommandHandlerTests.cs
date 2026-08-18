using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Handlers
{
    public class CancelSecondaryAccountCommandHandlerTests
    {
        private readonly Mock<ISavingsAccountService> _mockAccountService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CancelSecondaryAccountCommandHandler _handler;

        public CancelSecondaryAccountCommandHandlerTests()
        {
            _mockAccountService = new Mock<ISavingsAccountService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new CancelSecondaryAccountCommandHandler(_mockAccountService.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_Should_CancelAccount_And_CommitTransaction_When_Successful()
        {
            // Arrange
            var command = new CancelSecondaryAccountCommand { AccountNumber = "123456789" };

            _mockAccountService.Setup(s => s.CancelSecondaryAccountAsync(command.AccountNumber))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockAccountService.Verify(s => s.CancelSecondaryAccountAsync(command.AccountNumber), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_RollbackTransaction_And_ThrowException_When_CancellationFails()
        {
            // Arrange
            var command = new CancelSecondaryAccountCommand { AccountNumber = "123456789" };
            var expectedException = new InvalidOperationException("Cannot cancel primary account");

            _mockAccountService.Setup(s => s.CancelSecondaryAccountAsync(command.AccountNumber))
                .ThrowsAsync(expectedException);

            // Act & Assert
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot cancel primary account");

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockAccountService.Verify(s => s.CancelSecondaryAccountAsync(command.AccountNumber), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }
    }
}
