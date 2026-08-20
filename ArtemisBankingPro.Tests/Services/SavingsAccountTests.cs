using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.DTOs.Transactions;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests
{
    public class SavingsAccountTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        #region Existing Tests

        [Fact]
        public async Task CreateSecondaryAccount_WithPositiveBalance_ShouldSucceed()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 1,
                FirstName = "Juan",
                LastName = "Perez",
                Cedula = "402-1234567-8",
                Email = "juan@mail.com",
                Username = "juan",
                PasswordHash = "hash",
                PhoneNumber = "8095551234",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var principalAccount = new SavingsAccount
            {
                UserId = 1,
                AccountNumber = "100000001",
                Balance = 5000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(principalAccount);
            await db.SaveChangesAsync();

            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            var dto = new CreateSecondaryAccountDto
            {
                ClientCedula = "402-1234567-8",
                InitialBalance = 1000.00m
            };

            // Act
            var result = await service.CreateSecondaryAccountAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000.00m, result.Balance);
            Assert.Equal(AccountStatus.Activa, result.Status);

            var createdAccount = await accountRepo.GetByAccountNumberAsync(result.AccountNumber);
            Assert.NotNull(createdAccount);
            Assert.Equal(1, createdAccount.UserId);
            Assert.False(createdAccount.IsPrincipal);
        }

        [Fact]
        public async Task CreateSecondaryAccount_WithNegativeBalance_ShouldThrowException()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            var dto = new CreateSecondaryAccountDto
            {
                ClientCedula = "402-1234567-8",
                InitialBalance = -50.00m
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => service.CreateSecondaryAccountAsync(dto));
            Assert.Contains("El balance inicial no puede ser negativo", exception.Message);
        }

        [Fact]
        public async Task CancelSecondaryAccount_ShouldTransferFundsToPrincipalAndCancel()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 2,
                FirstName = "Juan",
                LastName = "Perez",
                Cedula = "402-1234567-8",
                Email = "juan@mail.com",
                Username = "juan2",
                PasswordHash = "hash",
                PhoneNumber = "8095551234",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var principalAccount = new SavingsAccount
            {
                UserId = 2,
                AccountNumber = "200000001",
                Balance = 1000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            var secondaryAccount = new SavingsAccount
            {
                UserId = 2,
                AccountNumber = "200000002",
                Balance = 500.00m,
                Type = AccountType.Secundaria,
                Status = AccountStatus.Activa,
                IsPrincipal = false
            };

            db.SavingsAccounts.AddRange(principalAccount, secondaryAccount);
            await db.SaveChangesAsync();

            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            // Act
            await service.CancelSecondaryAccountAsync("200000002");

            // Assert
            var cancelledAcc = await accountRepo.GetByAccountNumberAsync("200000002");
            var principalAcc = await accountRepo.GetByAccountNumberAsync("200000001");

            Assert.Equal(AccountStatus.Cancelada, cancelledAcc.Status);
            Assert.Equal(0.00m, cancelledAcc.Balance);
            Assert.Equal(1500.00m, principalAcc.Balance);
        }

        [Fact]
        public async Task BlockAccount_ShouldFreezeAccountAndBlockDebits()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var client = new User
            {
                Id = 3,
                FirstName = "Juan",
                LastName = "Perez",
                Cedula = "402-1234567-8",
                Email = "juan@mail.com",
                Username = "juan3",
                PasswordHash = "hash",
                PhoneNumber = "8095551234",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 3,
                AccountNumber = "300000001",
                Balance = 3000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(account);
            await db.SaveChangesAsync();

            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            // Act
            await service.BlockAccountAsync("300000001");

            // Assert
            var updatedAcc = await accountRepo.GetByAccountNumberAsync("300000001");
            Assert.True(updatedAcc.IsBlocked);

            // Verify a payment using this blocked account is rejected
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentOwnAccountAsync("300000001", "1234567812345678", 500.00m, "3")
            );
            Assert.Contains("bloqueada", exception.Message);
        }

        [Fact]
        public async Task SetBlockedAmount_ShouldRestrictAvailableBalance()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var client = new User
            {
                Id = 4,
                FirstName = "Juan",
                LastName = "Perez",
                Cedula = "402-1234567-8",
                Email = "juan@mail.com",
                Username = "juan4",
                PasswordHash = "hash",
                PhoneNumber = "8095551234",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 4,
                AccountNumber = "400000001",
                Balance = 5000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(account);
            await db.SaveChangesAsync();

            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            // Act: Retain/embargo RD$ 4,000 (Available = 5000 - 4000 = 1000)
            await service.SetBlockedAmountAsync("400000001", 4000.00m);

            // Assert
            var updatedAcc = await accountRepo.GetByAccountNumberAsync("400000001");
            Assert.Equal(4000.00m, updatedAcc.BlockedAmount);

            // Attempting to withdraw or pay RD$ 1,500 should throw because only RD$ 1,000 is available
            var card = new CreditCard
            {
                CardNumber = "4444555566667777",
                CreditLimit = 10000.00m,
                CurrentDebt = 3000.00m,
                Status = "Activa",
                ExpirationMonth = "12",
                ExpirationYear = "2028"
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentOwnAccountAsync("400000001", "4444555566667777", 1500.00m, "4")
            );
            Assert.Contains("Fondos insuficientes", exception.Message);
        }

        [Fact]
        public async Task CreditToMainAsync_ShouldIncreaseBalanceAndAddTransaction_WhenAccountIsActive()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 5,
                FirstName = "Pedro",
                LastName = "Rosario",
                Cedula = "402-9999999-9",
                Email = "pedro@mail.com",
                Username = "pedro",
                PasswordHash = "hash",
                PhoneNumber = "8095554321",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var principalAccount = new SavingsAccount
            {
                UserId = 5,
                AccountNumber = "500000001",
                Balance = 1000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(principalAccount);
            await db.SaveChangesAsync();

            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            // Act
            await service.CreditToMainAsync("5", 5000.00m);

            // Assert
            var updatedAccount = await accountRepo.GetByAccountNumberAsync("500000001");
            Assert.NotNull(updatedAccount);
            Assert.Equal(6000.00m, updatedAccount.Balance);

            var transactions = await transactionRepo.GetPagedByAccountAsync("500000001", 1, 10);
            var disbursementTx = transactions.FirstOrDefault(t => t.Description.Contains("Desembolso"));
            Assert.NotNull(disbursementTx);
            Assert.Equal(5000.00m, disbursementTx.Amount);
            Assert.Equal(TransactionType.Credito, disbursementTx.Type);
            Assert.Equal(TransactionStatus.Aprobada, disbursementTx.Status);
        }

        [Fact]
        public async Task ProcessCashAdvance_WhenAccountIsCancelled_ShouldThrowException()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 6,
                FirstName = "Laura",
                LastName = "Gomez",
                Cedula = "402-8888888-8",
                Email = "laura@mail.com",
                Username = "laura",
                PasswordHash = "hash",
                PhoneNumber = "8095558888",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 6,
                AccountNumber = "600000001",
                Balance = 1000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Cancelada, // Cancelada
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(account);

            var card = new CreditCard
            {
                Id = 2,
                ClientId = 6,
                CardNumber = "4444555566668888",
                CreditLimit = 10000m,
                CurrentDebt = 0m,
                Status = "Activa",
                CvcHash = "hash",
                ExpirationMonth = "12",
                ExpirationYear = "2028"
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(() =>
                service.ProcessCashAdvanceAsync("600000001", "4444555566668888", 500m, "6")
            );
            Assert.Contains("cancelada", exception.Message);
        }

        [Fact]
        public async Task CreditToMainAsync_WithDecimalPrecision_ShouldCalculateDecimalBalanceCorrectly()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 7,
                FirstName = "Maria",
                LastName = "Santos",
                Cedula = "402-7777777-7",
                Email = "maria@mail.com",
                Username = "maria",
                PasswordHash = "hash",
                PhoneNumber = "8095557777",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 7,
                AccountNumber = "700000001",
                Balance = 1000.55m, // Decimal starting balance
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(account);
            await db.SaveChangesAsync();

            var userRepo = new UserRepository(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var loanRepo = new LoanRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var emailServiceMock = new Mock<IEmailService>();

            var service = new SavingsAccountService(accountRepo, transactionRepo, userRepo, loanRepo, cardRepo, emailServiceMock.Object);

            // Act
            await service.CreditToMainAsync("7", 500.44m); // Decimal credit amount

            // Assert
            var updatedAcc = await accountRepo.GetByAccountNumberAsync("700000001");
            Assert.NotNull(updatedAcc);
            Assert.Equal(1500.99m, updatedAcc.Balance); // Exactly 1500.99m
        }

        #endregion

        #region ProcessCreditCardPaymentOwnAccountAsync Tests

        [Fact]
        public async Task ProcessCreditCardPaymentOwnAccount_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 8,
                FirstName = "Carlos",
                LastName = "Ramirez",
                Cedula = "402-8888888-1",
                Email = "carlos@mail.com",
                Username = "carlos8",
                PasswordHash = "hash",
                PhoneNumber = "8095558888",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 8,
                AccountNumber = "800000001",
                Balance = 3000.00m,
                BlockedAmount = 0.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true,
                IsBlocked = false
            };
            db.SavingsAccounts.Add(account);

            var card = new CreditCard
            {
                Id = 80,
                ClientId = 8,
                CardNumber = "4444111122223333",
                CreditLimit = 10000.00m,
                CurrentDebt = 1500.00m,
                Status = "Activa",
                CvcHash = "hash",
                ExpirationMonth = "12",
                ExpirationYear = "2028"
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var service = new SavingsAccountService(
                new SavingsAccountRepository(db),
                new TransactionRepository(db),
                new UserRepository(db),
                new LoanRepository(db),
                new CreditCardRepository(db),
                new Mock<IEmailService>().Object);

            // Act
            await service.ProcessCreditCardPaymentOwnAccountAsync("800000001", "4444111122223333", 500.00m, "8");

            // Assert
            var updatedAcc = await db.SavingsAccounts.FirstOrDefaultAsync(a => a.AccountNumber == "800000001");
            var updatedCard = await db.CreditCards.FirstOrDefaultAsync(c => c.CardNumber == "4444111122223333");
            var transaction = await db.Transactions.FirstOrDefaultAsync(t => t.AccountNumber == "800000001");

            Assert.Equal(2500.00m, updatedAcc.Balance);
            Assert.Equal(1000.00m, updatedCard.CurrentDebt);
            Assert.NotNull(transaction);
            Assert.Equal(TransactionType.Debito, transaction.Type);
            Assert.Equal(TransactionStatus.Aprobada, transaction.Status);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentOwnAccount_WhenAmountIsZeroOrLess_ShouldThrowException()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new SavingsAccountService(
                new SavingsAccountRepository(db),
                new TransactionRepository(db),
                new UserRepository(db),
                new LoanRepository(db),
                new CreditCardRepository(db),
                new Mock<IEmailService>().Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentOwnAccountAsync("800000001", "4444111122223333", 0.00m, "8"));

            Assert.Contains("El monto a pagar debe ser mayor que cero.", exception.Message);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentOwnAccount_WhenAccountNotFoundOrNotOwner_ShouldThrowException()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new SavingsAccountService(
                new SavingsAccountRepository(db),
                new TransactionRepository(db),
                new UserRepository(db),
                new LoanRepository(db),
                new CreditCardRepository(db),
                new Mock<IEmailService>().Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentOwnAccountAsync("999999999", "4444111122223333", 100.00m, "8"));

            Assert.Contains("La cuenta de ahorro de origen no existe o no le pertenece.", exception.Message);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentOwnAccount_WhenCardNotFound_ShouldThrowException()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 9,
                FirstName = "Ana",
                LastName = "Lopez",
                Cedula = "402-9999999-1",
                Email = "ana@mail.com",
                Username = "ana9",
                PasswordHash = "hash",
                PhoneNumber = "8095559999",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 9,
                AccountNumber = "900000001",
                Balance = 2000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(account);
            await db.SaveChangesAsync();

            var service = new SavingsAccountService(
                new SavingsAccountRepository(db),
                new TransactionRepository(db),
                new UserRepository(db),
                new LoanRepository(db),
                new CreditCardRepository(db),
                new Mock<IEmailService>().Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentOwnAccountAsync("900000001", "0000000000000000", 100.00m, "9"));

            Assert.Contains("La tarjeta de crédito no existe.", exception.Message);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentOwnAccount_WhenCardIsCanceled_ShouldThrowException()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 10,
                FirstName = "Luis",
                LastName = "Mendez",
                Cedula = "402-1010101-0",
                Email = "luis@mail.com",
                Username = "luis10",
                PasswordHash = "hash",
                PhoneNumber = "8095551010",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 10,
                AccountNumber = "1000000001",
                Balance = 2000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(account);

            var card = new CreditCard
            {
                Id = 100,
                ClientId = 10,
                CardNumber = "4444999988887777",
                CreditLimit = 5000.00m,
                CurrentDebt = 500.00m,
                Status = "Cancelada",
                CvcHash = "hash",
                ExpirationMonth = "12",
                ExpirationYear = "2028"
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var service = new SavingsAccountService(
                new SavingsAccountRepository(db),
                new TransactionRepository(db),
                new UserRepository(db),
                new LoanRepository(db),
                new CreditCardRepository(db),
                new Mock<IEmailService>().Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentOwnAccountAsync("1000000001", "4444999988887777", 100.00m, "10"));

            Assert.Contains("La tarjeta de crédito se encuentra cancelada.", exception.Message);
        }

        [Fact]
        public async Task ProcessCreditCardPaymentOwnAccount_WhenOverpaying_ShouldThrowException()
        {
            // Arrange
            var db = GetInMemoryDbContext();

            var role = new Role { Id = 3, Name = "Cliente" };
            db.Roles.Add(role);

            var client = new User
            {
                Id = 11,
                FirstName = "Rosa",
                LastName = "Diaz",
                Cedula = "402-1111111-1",
                Email = "rosa@mail.com",
                Username = "rosa11",
                PasswordHash = "hash",
                PhoneNumber = "8095551111",
                RoleId = 3,
                IsActive = true
            };
            db.Users.Add(client);

            var account = new SavingsAccount
            {
                UserId = 11,
                AccountNumber = "1100000001",
                Balance = 5000.00m,
                Type = AccountType.Principal,
                Status = AccountStatus.Activa,
                IsPrincipal = true
            };
            db.SavingsAccounts.Add(account);

            var card = new CreditCard
            {
                Id = 110,
                ClientId = 11,
                CardNumber = "4444333322221111",
                CreditLimit = 5000.00m,
                CurrentDebt = 200.00m,
                Status = "Activa",
                CvcHash = "hash",
                ExpirationMonth = "12",
                ExpirationYear = "2028"
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var service = new SavingsAccountService(
                new SavingsAccountRepository(db),
                new TransactionRepository(db),
                new UserRepository(db),
                new LoanRepository(db),
                new CreditCardRepository(db),
                new Mock<IEmailService>().Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.ProcessCreditCardPaymentOwnAccountAsync("1100000001", "4444333322221111", 500.00m, "11"));

            Assert.Contains("No se permite sobrepagar la tarjeta", exception.Message);
        }

        #endregion
    }
}