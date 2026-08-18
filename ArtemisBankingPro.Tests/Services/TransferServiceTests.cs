using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.Transactions;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Services
{
    public class TransferServiceTests
    {
        private readonly Mock<ISavingsAccountRepository> _mockAccountRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly TransactionService _service;

        public TransferServiceTests()
        {
            _mockAccountRepo = new Mock<ISavingsAccountRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();

            _service = new TransactionService(
                _mockAccountRepo.Object,
                _mockTransactionRepo.Object,
                _mockEmailService.Object,
                _mockUnitOfWork.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task OwnAccountTransferAsync_ShouldTransferBalance_WhenValid()
        {
            // Arrange
            var clientId = "10";
            var dto = new OwnAccountTransferDto
            {
                SourceAccountNumber = "10001",
                DestinationAccountNumber = "10002",
                Amount = 300.50m
            };

            var srcAcc = new SavingsAccount
            {
                Id = 1,
                AccountNumber = "10001",
                Balance = 1000m,
                BlockedAmount = 0m,
                Status = AccountStatus.Activa,
                UserId = 10
            };

            var destAcc = new SavingsAccount
            {
                Id = 2,
                AccountNumber = "10002",
                Balance = 200m,
                BlockedAmount = 0m,
                Status = AccountStatus.Activa,
                UserId = 10
            };

            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("10001")).ReturnsAsync(srcAcc);
            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("10002")).ReturnsAsync(destAcc);
            _mockAccountRepo.Setup(r => r.CountActiveAccountsByClientIdAsync(10)).ReturnsAsync(2);
            _mockUserRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new User { Id = 10, IsActive = true, Email = "src@artemis.com" });

            // Act
            var result = await _service.OwnAccountTransferAsync(dto, clientId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            srcAcc.Balance.Should().Be(699.50m);
            destAcc.Balance.Should().Be(500.50m);

            _mockAccountRepo.Verify(r => r.UpdateAsync(srcAcc), Times.Once);
            _mockAccountRepo.Verify(r => r.UpdateAsync(destAcc), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task OwnAccountTransferAsync_ShouldFail_WhenSourceAndDestinationAreSame()
        {
            // Arrange
            var clientId = "10";
            var dto = new OwnAccountTransferDto
            {
                SourceAccountNumber = "10001",
                DestinationAccountNumber = "10001", // same
                Amount = 100m
            };

            // Act
            var result = await _service.OwnAccountTransferAsync(dto, clientId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("origen y la cuenta de destino no pueden ser la misma");

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task ExpressTransactionAsync_ShouldSucceed_EvenIfEmailServiceThrowsException()
        {
            // Arrange
            var dto = new ExpressTransactionDto
            {
                SourceAccountNumber = "10001",
                DestinationAccountNumber = "20002",
                Amount = 250m
            };

            var srcAcc = new SavingsAccount { Id = 1, AccountNumber = "10001", Balance = 1000m, BlockedAmount = 0m, Status = AccountStatus.Activa, UserId = 10 };
            var destAcc = new SavingsAccount { Id = 2, AccountNumber = "20002", Balance = 100m, BlockedAmount = 0m, Status = AccountStatus.Activa, UserId = 20 };
            var srcUser = new User { Id = 10, IsActive = true, Email = "src@artemis.com" };
            var destUser = new User { Id = 20, IsActive = true, Email = "dest@artemis.com" };

            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("10001")).ReturnsAsync(srcAcc);
            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("20002")).ReturnsAsync(destAcc);
            _mockUserRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(srcUser);
            _mockUserRepo.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(destUser);

            // Simular fallo SMTP
            _mockEmailService.Setup(e => e.SendNotificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP Down"));

            // Act
            var act = async () => await _service.ExpressTransactionAsync(dto);

            // Assert
            await act.Should().NotThrowAsync();
            srcAcc.Balance.Should().Be(750m);
            destAcc.Balance.Should().Be(350m);

            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task ExpressTransactionAsync_ShouldFail_WhenSourceAccountIsBlocked()
        {
            // Arrange
            var dto = new ExpressTransactionDto
            {
                SourceAccountNumber = "10001",
                DestinationAccountNumber = "20002",
                Amount = 250m
            };

            var srcAcc = new SavingsAccount { Id = 1, AccountNumber = "10001", Balance = 1000m, BlockedAmount = 0m, Status = AccountStatus.Activa, IsBlocked = true, UserId = 10 };

            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("10001")).ReturnsAsync(srcAcc);

            // Act
            var act = async () => await _service.ExpressTransactionAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("La cuenta de origen se encuentra bloqueada.");

            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }
    }
}
