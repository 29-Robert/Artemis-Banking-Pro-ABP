using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;


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

            var dto = new DepositRequestDto
            {
                DestinationAccountNumber = "123456789",
                Amount = 500m
            };

            
            var response = await service.ProcessDepositAsync(dto, 2);

            
            Assert.True(response.Approved);
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

            var dto = new WithdrawRequestDto
            {
                SourceAccountNumber = "123456789",
                Amount = 500m
            };

            
            var response = await service.ProcessWithdrawalAsync(dto, 2);

            
            Assert.False(response.Approved);
            Assert.NotNull(response.RejectionReason);
            Assert.Equal(100m, account.Balance); 
        }

        [Fact]
        public async Task ProcessCreditCardPaymentAsync_WhenOverpayment_ReturnsRejected()
        {
            // Arrange
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };
            var card = new CreditCard { CardNumber = "1234567812345678", CurrentDebt = 1000m, Status = "Activa" };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var cards = new Mock<ICreditCardRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            cards.Setup(x => x.GetByCardNumberWithClientAsync("1234567812345678")).ReturnsAsync(card);

            var service = CreateService(accounts, transactions, cards);

            var dto = new PayCreditCardRequestDto
            {
                SourceAccountNumber = "123456789",
                CardNumber = "1234567812345678",
                Amount = 1500m
            };

   
            var response = await service.ProcessCreditCardPaymentAsync(dto, 2);

     
            Assert.False(response.Approved);
            Assert.NotNull(response.RejectionReason);
            Assert.Equal(5000m, account.Balance);
        }

        // Helper
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
                transactions.Object,
                Mock.Of<IEmailService>());
        }
    }
}

