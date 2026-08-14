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

            await service.ProcessDepositAsync("123456789", 500m, "2");

            Assert.Equal(1500m, account.Balance);
            transactions.Verify(x => x.AddAsync(It.Is<Transaction>(t =>
                t.Type == TransactionType.Credito &&
                t.Amount == 500m &&
                t.Status == TransactionStatus.Aprobada)), Times.Once);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_WhenInsufficientFunds_Throws()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 100m, Status = AccountStatus.Activa };
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var service = CreateService(accounts, transactions);

            await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessWithdrawalAsync("123456789", 500m, "2"));
        }

        [Fact]
        public async Task ProcessCreditCardPaymentAsync_WhenOverpayment_Throws()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };
            var card = new CreditCard { CardNumber = "1234567812345678", CurrentDebt = 1000m, Status = "Activa" };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var cards = new Mock<ICreditCardRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            cards.Setup(x => x.GetByCardNumberAsync("1234567812345678")).ReturnsAsync(card);

            var service = CreateService(accounts, transactions, cards);

            await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentAsync("123456789", "1234567812345678", 1500m, "2"));
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

