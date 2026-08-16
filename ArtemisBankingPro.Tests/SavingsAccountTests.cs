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
    }
}
