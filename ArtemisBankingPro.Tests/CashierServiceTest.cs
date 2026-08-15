using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ArtemisBankingPro.Tests
{
    public class CashierServiceTest
    {
        [Fact]
        public async Task ProcessDepositAsync_IncreasesBalance()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 1000m, Status = AccountStatus.Activa };
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var service = CreateService(accounts, transactions);

            var depositDto = new ArtemisBankingPro.Application.DTOs.Cashier.DepositRequestDto { DestinationAccountNumber = "123456789", Amount = 500m };
            await service.ProcessDepositAsync(depositDto, 2);

            Assert.Equal(1500m, account.Balance);
            transactions.Verify(x => x.AddAsync(It.Is<Transaction>(t =>
                t.Type == TransactionType.Credito &&
                t.Amount == 500m &&
                t.Status == TransactionStatus.Aprobada)), Times.Once);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_WhenInsufficientFunds_ReturnsRejected()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 100m, Status = AccountStatus.Activa };
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var service = CreateService(accounts, transactions);

            var withdrawalDto = new ArtemisBankingPro.Application.DTOs.Cashier.WithdrawRequestDto { SourceAccountNumber = "123456789", Amount = 500m };
            var result = await service.ProcessWithdrawalAsync(withdrawalDto, 2);

            Assert.False(result.Approved);
            Assert.Equal("Fondos insuficientes en la cuenta de ahorros.", result.RejectionReason);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentAsync_WhenOverpayment_CapsToCurrentDebt()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };
            var card = new CreditCard { CardNumber = "1234567812345678", CurrentDebt = 1000m, Status = "Activa" };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var cards = new Mock<ICreditCardRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            cards.Setup(x => x.GetByCardNumberAsync("1234567812345678")).ReturnsAsync(card);

            var service = CreateService(accounts, transactions, cards);

            var payDto = new ArtemisBankingPro.Application.DTOs.Cashier.PayCreditCardRequestDto { SourceAccountNumber = "123456789", CardNumber = "1234567812345678", Amount = 1500m };
            var result = await service.ProcessCreditCardPaymentAsync(payDto, 2);

            Assert.True(result.Approved);
            Assert.Equal(1000m, result.AppliedAmount);
            Assert.Equal(4000m, account.Balance);
            Assert.Equal(0m, card.CurrentDebt);
        }

        private static CashierService CreateService(
            Mock<ISavingsAccountRepository> accounts,
            Mock<ITransactionRepository> transactions,
            Mock<ICreditCardRepository>? cards = null)
        {
            return new CashierService(
                Mock.Of<ILoanRepository>(),
                Mock.Of<ILoanInstallmentRepository>(),
                cards?.Object ?? Mock.Of<ICreditCardRepository>(),
                accounts.Object,
                transactions.Object);
        }
    }
}

