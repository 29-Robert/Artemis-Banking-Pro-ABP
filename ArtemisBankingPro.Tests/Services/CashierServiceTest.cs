using ArtemisBankingPro.Application.DTOs.Cashier;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
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
            var withdrawalDto = new ArtemisBankingPro.Application.DTOs.Cashier.WithdrawRequestDto { SourceAccountNumber = "123456789", Amount = 500m };
            var result = await service.ProcessWithdrawalAsync(withdrawalDto, 2);

            Assert.False(result.Approved);
            Assert.Equal("El monto ingresado excede el saldo disponible de la cuenta.", result.RejectionReason);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentAsync_WhenOverpayment_CapsToCurrentDebt()
        {
            // Arrange
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };
            var card = new CreditCard { CardNumber = "1234567812345678", CurrentDebt = 1000m, Status = "Activa" };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var cards = new Mock<ICreditCardRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            cards.Setup(x => x.GetByCardNumberAsync("1234567812345678")).ReturnsAsync(card);

            var service = CreateService(accounts, transactions, cards);

            var dto = new PayCreditCardRequestDto
            {
                SourceAccountNumber = "123456789",
                CardNumber = "1234567812345678",
                Amount = 1500m
            };

            // Act
            var result = await service.ProcessCreditCardPaymentAsync(dto, 2);

            // Assert
            Assert.True(result.Approved);
            Assert.Equal(1000m, result.AppliedAmount);
            Assert.Equal(4000m, account.Balance);
            Assert.Equal(0m, card.CurrentDebt);
        }
        [Fact]
        public async Task ProcessWithdrawalAsync_DecreasesBalance()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 1000m, Status = AccountStatus.Activa };
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            var service = CreateService(accounts, transactions);

            var dto = new WithdrawRequestDto { SourceAccountNumber = "123456789", Amount = 300m };

            var response = await service.ProcessWithdrawalAsync(dto, 2);

            Assert.True(response.Approved);
            Assert.Equal(700m, account.Balance);
        }

        [Fact]
        public async Task ProcessDepositAsync_WhenAccountCancelled_ReturnsRejected()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 1000m, Status = AccountStatus.Cancelada };
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            var service = CreateService(accounts, transactions);

            var dto = new DepositRequestDto { DestinationAccountNumber = "123456789", Amount = 500m };

            var response = await service.ProcessDepositAsync(dto, 2);

            Assert.False(response.Approved);
            Assert.Equal(1000m, account.Balance);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentAsync_WhenCardCancelled_ReturnsRejected()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };
            var card = new CreditCard { CardNumber = "1234567812345678", CurrentDebt = 1000m, Status = "Cancelada" };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var cards = new Mock<ICreditCardRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            cards.Setup(x => x.GetByCardNumberAsync("1234567812345678")).ReturnsAsync(card);

            var service = CreateService(accounts, transactions, cards);

            var dto = new PayCreditCardRequestDto { SourceAccountNumber = "123456789", CardNumber = "1234567812345678", Amount = 500m };

            var result = await service.ProcessCreditCardPaymentAsync(dto, 2);

            Assert.False(result.Approved);
        }

        [Fact]
        public async Task ProcessLoanPaymentAsync_AppliesAcrossInstallmentsInOrder()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };
            var installment1 = new LoanInstallment { InstallmentNumber = 1, PendingInstallmentAmount = 300m, PaymentStatus = "Pendiente" };
            var installment2 = new LoanInstallment { InstallmentNumber = 2, PendingInstallmentAmount = 500m, PaymentStatus = "Pendiente" };
            var loan = new Loan { LoanNumber = "987654321", Status = "Activo", Installments = new List<LoanInstallment> { installment1, installment2 } };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var loans = new Mock<ILoanRepository>();
            var installmentsRepo = new Mock<ILoanInstallmentRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            loans.Setup(x => x.GetByLoanNumberWithInstallmentsAsync("987654321")).ReturnsAsync(loan);

            var service = CreateService(accounts, transactions, loans: loans, installments: installmentsRepo);

            var dto = new PayLoanRequestDto { SourceAccountNumber = "123456789", LoanNumber = "987654321", Amount = 700m };

            var result = await service.ProcessLoanPaymentAsync(dto, 2);

            Assert.True(result.Approved);
            Assert.Equal(700m, result.AppliedAmount);
            Assert.Equal(4300m, account.Balance);
            Assert.Equal("Pagada", installment1.PaymentStatus);
            Assert.Equal("Parcial", installment2.PaymentStatus);
            Assert.Equal(100m, installment2.PendingInstallmentAmount);
        }

        [Fact]
        public async Task ProcessLoanPaymentAsync_WhenFullyPaid_MarksLoanCompleted()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };
            var installment = new LoanInstallment { InstallmentNumber = 1, PendingInstallmentAmount = 300m, PaymentStatus = "Pendiente" };
            var loan = new Loan { LoanNumber = "987654321", Status = "Activo", Installments = new List<LoanInstallment> { installment } };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var loans = new Mock<ILoanRepository>();
            var installmentsRepo = new Mock<ILoanInstallmentRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            loans.Setup(x => x.GetByLoanNumberWithInstallmentsAsync("987654321")).ReturnsAsync(loan);

            var service = CreateService(accounts, transactions, loans: loans, installments: installmentsRepo);

            var dto = new PayLoanRequestDto { SourceAccountNumber = "123456789", LoanNumber = "987654321", Amount = 300m };

            await service.ProcessLoanPaymentAsync(dto, 2);

            Assert.Equal("Completado", loan.Status);
            loans.Verify(x => x.UpdateAsync(loan), Times.Once);
        }

        [Fact]
        public async Task ProcessThirdPartyTransferAsync_DebitsSource_AndCreditsDestination()
        {
            var source = new SavingsAccount { AccountNumber = "111111111", Balance = 1000m, Status = AccountStatus.Activa };
            var destination = new SavingsAccount { AccountNumber = "222222222", Balance = 200m, Status = AccountStatus.Activa };

            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();

            accounts.Setup(x => x.GetByAccountNumberAsync("111111111")).ReturnsAsync(source);
            accounts.Setup(x => x.GetByAccountNumberAsync("222222222")).ReturnsAsync(destination);

            var service = CreateService(accounts, transactions);

            var dto = new ThirdPartyTransferRequestDto { SourceAccountNumber = "111111111", DestinationAccountNumber = "222222222", Amount = 400m };

            var result = await service.ProcessThirdPartyTransferAsync(dto, 2);

            Assert.True(result.Approved);
            Assert.Equal(600m, source.Balance);
            Assert.Equal(600m, destination.Balance);
        }

        [Fact]
        public async Task ProcessThirdPartyTransferAsync_WhenSameAccount_ReturnsRejected()
        {
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var service = CreateService(accounts, transactions);

            var dto = new ThirdPartyTransferRequestDto { SourceAccountNumber = "111111111", DestinationAccountNumber = "111111111", Amount = 400m };

            var result = await service.ProcessThirdPartyTransferAsync(dto, 2);

            Assert.False(result.Approved);
        }

        [Fact]
        public async Task GetHomeIndicatorsAsync_CountsCorrectly_AfterBugFix()
        {
            var today = DateTime.UtcNow.Date;
            var todaysTransactions = new List<Transaction>
    {
        new() { Type = TransactionType.Credito, Description = "DEPÓSITO", Status = TransactionStatus.Aprobada },
        new() { Type = TransactionType.Debito, Description = "RETIRO", Status = TransactionStatus.Aprobada },
        new() { Type = TransactionType.Debito, Description = "PAGO A TARJETA 5678", Status = TransactionStatus.Aprobada },
        new() { Type = TransactionType.Debito, Description = "TRANSFERENCIA A TERCEROS 222222222", Status = TransactionStatus.Aprobada }
    };

            var transactions = new Mock<ITransactionRepository>();
            transactions.Setup(x => x.GetByPerformedUserAndDateAsync(2, today)).ReturnsAsync(todaysTransactions);

            var accounts = new Mock<ISavingsAccountRepository>();
            var service = CreateService(accounts, transactions);

            var result = await service.GetHomeIndicatorsAsync(2);

            Assert.Equal(1, result.DepositsToday);
            Assert.Equal(1, result.WithdrawalsToday);
            Assert.Equal(1, result.PaymentsToday); // ahora sí cuenta gracias al fix
            Assert.Equal(4, result.TotalTransactionsToday);
        }

        [Fact]
        public async Task GetAccountPreviewAsync_ReturnsExpectedData()
        {
            var account = new SavingsAccount
            {
                AccountNumber = "123456789",
                Status = AccountStatus.Activa,
                User = new User { FirstName = "María", LastName = "Gómez" }
            };
            var accounts = new Mock<ISavingsAccountRepository>();
            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var service = CreateService(accounts, new Mock<ITransactionRepository>());

            var result = await service.GetAccountPreviewAsync("123456789");

            Assert.Equal("María Gómez", result.AccountHolderFullName);
            Assert.Equal("Activa", result.Status);
        }

        [Fact]
        public async Task GetCreditCardPreviewAsync_ReturnsMaskedCardNumber()
        {
            var card = new CreditCard
            {
                CardNumber = "1234567812345678",
                CurrentDebt = 500m,
                Status = "Activa",
                Client = new User { FirstName = "Juan", LastName = "Pérez" }
            };
            var cards = new Mock<ICreditCardRepository>();
            cards.Setup(x => x.GetByCardNumberWithClientAsync("1234567812345678")).ReturnsAsync(card);

            var service = CreateService(new Mock<ISavingsAccountRepository>(), new Mock<ITransactionRepository>(), cards: cards);

            var result = await service.GetCreditCardPreviewAsync("1234567812345678");

            Assert.Equal("**** **** **** 5678", result.MaskedCardNumber);
            Assert.Equal("Juan Pérez", result.ClientFullName);
        }

        [Fact]
        public async Task GetLoanPreviewAsync_ReturnsRemainingBalance()
        {
            var loan = new Loan
            {
                LoanNumber = "987654321",
                Status = "Activo",
                Client = new User { FirstName = "María", LastName = "Gómez" },
                Installments = new List<LoanInstallment>
        {
            new() { PendingInstallmentAmount = 300m },
            new() { PendingInstallmentAmount = 500m }
        }
            };
            var loans = new Mock<ILoanRepository>();
            loans.Setup(x => x.GetByLoanNumberWithInstallmentsAsync("987654321")).ReturnsAsync(loan);

            var service = CreateService(new Mock<ISavingsAccountRepository>(), new Mock<ITransactionRepository>(), loans: loans);

            var result = await service.GetLoanPreviewAsync("987654321");

            Assert.Equal(800m, result.RemainingBalance);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_WhenAccountIsBlocked_ReturnsRejected()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Status = AccountStatus.Activa, IsBlocked = true, Balance = 1000m };
            var accounts = new Mock<ISavingsAccountRepository>();
            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var service = CreateService(accounts, new Mock<ITransactionRepository>());

            var request = new WithdrawRequestDto { SourceAccountNumber = "123456789", Amount = 100m };
            var result = await service.ProcessWithdrawalAsync(request, 2);

            Assert.False(result.Approved);
            Assert.Equal("La cuenta seleccionada está bloqueada.", result.RejectionReason);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_WhenUserIsInactive_ReturnsRejected()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Status = AccountStatus.Activa, IsBlocked = false, Balance = 1000m, UserId = 10 };
            var user = new User { Id = 10, IsActive = false };

            var accounts = new Mock<ISavingsAccountRepository>();
            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var users = new Mock<IUserRepository>();
            users.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(user);

            var service = CreateService(accounts, new Mock<ITransactionRepository>(), userRepository: users);

            var request = new WithdrawRequestDto { SourceAccountNumber = "123456789", Amount = 100m };
            var result = await service.ProcessWithdrawalAsync(request, 2);

            Assert.False(result.Approved);
            Assert.Equal("El usuario de la cuenta no se encuentra activo.", result.RejectionReason);
        }

        [Fact]
        public async Task ProcessWithdrawalAsync_WhenBlockedAmountExceedsAvailable_ReturnsRejected()
        {
            var account = new SavingsAccount { AccountNumber = "123456789", Status = AccountStatus.Activa, IsBlocked = false, Balance = 1000m, BlockedAmount = 800m, UserId = 10 };
            var user = new User { Id = 10, IsActive = true };

            var accounts = new Mock<ISavingsAccountRepository>();
            accounts.Setup(x => x.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var users = new Mock<IUserRepository>();
            users.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(user);

            var service = CreateService(accounts, new Mock<ITransactionRepository>(), userRepository: users);

            var request = new WithdrawRequestDto { SourceAccountNumber = "123456789", Amount = 300m }; // 1000 - 800 = 200 available
            var result = await service.ProcessWithdrawalAsync(request, 2);

            Assert.False(result.Approved);
            Assert.Equal("El monto ingresado excede el saldo disponible de la cuenta.", result.RejectionReason);
        }

        // Helper
        private static CashierService CreateService(
      Mock<ISavingsAccountRepository> accounts,
      Mock<ITransactionRepository> transactions,
      Mock<ICreditCardRepository>? cards = null,
      Mock<ILoanRepository>? loans = null,
      Mock<ILoanInstallmentRepository>? installments = null,
      Mock<IEmailService>? emailService = null,
      Mock<IUserRepository>? userRepository = null)
        {
            var userRepoMock = userRepository ?? new Mock<IUserRepository>();
            if (userRepository == null)
            {
                userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                    .ReturnsAsync((int id) => new User { Id = id, IsActive = true, Email = "test@example.com", FirstName = "Test", LastName = "User" });
            }

            return new CashierService(
                loans?.Object ?? Mock.Of<ILoanRepository>(),
                installments?.Object ?? Mock.Of<ILoanInstallmentRepository>(),
                cards?.Object ?? Mock.Of<ICreditCardRepository>(),
                accounts.Object,
                transactions.Object,
                emailService?.Object ?? Mock.Of<IEmailService>(),
                Mock.Of<IUnitOfWork>(),
                userRepoMock.Object,
                Mock.Of<ILogger<CashierService>>());
        }
    }
}

