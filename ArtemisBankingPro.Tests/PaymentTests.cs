using ArtemisBankingPro.Application.DTOs.HermesPay;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests
{
    public class PaymentTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private IValidator<ProcessPaymentRequestDto> GetValidator()
        {
            return new ProcessPaymentRequestDtoValidator();
        }

        [Fact]
        public async Task ProcessPayment_WhenApproved_UpdatesBalanceAndLogsConsumption()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            
            // Seed Commerce
            var commerce = new Commerce
            {
                BusinessName = "Tienda ABC",
                RNC = "101002028",
                Email = "contacto@abc.com",
                Phone = "8095551111",
                Address = "SD",
                IsActive = true,
                PrincipalAccountNumber = "999888777"
            };
            db.Commerces.Add(commerce);
            await db.SaveChangesAsync();

            // Seed User Representative for Commerce
            var commerceUser = new User
            {
                FirstName = "Comercio",
                LastName = "ABC",
                Username = "101002028",
                Email = "contacto@abc.com",
                Cedula = "COM-101002028",
                PasswordHash = "hash",
                RoleId = 4,
                IsActive = true,
                CommerceId = commerce.Id
            };
            db.Users.Add(commerceUser);
            await db.SaveChangesAsync();

            // Seed Commerce Principal Account
            var commerceAccount = new SavingsAccount
            {
                UserId = commerceUser.Id,
                AccountNumber = "999888777",
                Balance = 1000m,
                IsPrincipal = true,
                Status = AccountStatus.Activa,
                Type = AccountType.Principal
            };
            db.SavingsAccounts.Add(commerceAccount);
            await db.SaveChangesAsync();

            // Seed Client User
            var clientUser = new User
            {
                FirstName = "Juan",
                LastName = "Perez",
                Username = "juanperez",
                Email = "juan@perez.com",
                Cedula = "40200000000",
                RoleId = 3, // Cliente
                IsActive = true
            };
            db.Users.Add(clientUser);
            await db.SaveChangesAsync();

            // Seed Credit Card
            var expirationDate = DateTime.UtcNow.AddYears(1);
            var card = new CreditCard
            {
                ClientId = clientUser.Id,
                CardNumber = "1234567812345678",
                CreditLimit = 5000m,
                CurrentDebt = 1000m, // available credit = 4000m
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                CvcHash = HashString("123"),
                Status = "Activa",
                AdminId = 1
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var commerceRepo = new CommerceRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var consumptionRepo = new CreditCardConsumptionRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var emailServiceMock = new Mock<IEmailService>();
            
            var service = new PaymentService(
                commerceRepo,
                cardRepo,
                consumptionRepo,
                userRepo,
                accountRepo,
                transactionRepo,
                emailServiceMock.Object,
                GetValidator()
            );

            var requestDto = new ProcessPaymentRequestDto
            {
                CardNumber = "1234567812345678",
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                Cvc = "123",
                Amount = 1500m,
                Description = "Compra de prueba"
            };

            // Act
            var result = await service.ProcessPaymentAsync(commerce.Id, requestDto, 1);

            // Assert
            Assert.Equal(TransactionStatus.Aprobada, result.Status);
            Assert.NotEmpty(result.AuthorizationCode);
            Assert.Equal(1500m, result.Amount);

            // Check Card debt increased
            var dbCard = await db.CreditCards.FindAsync(card.Id);
            Assert.Equal(2500m, dbCard.CurrentDebt); // 1000m + 1500m

            // Check Commerce balance increased
            var dbAccount = await db.SavingsAccounts.FindAsync(commerceAccount.Id);
            Assert.Equal(2500m, dbAccount.Balance); // 1000m + 1500m

            // Check Consumption logged
            var consumption = db.CreditCardConsumptions.FirstOrDefault(c => c.Id == result.Id);
            Assert.NotNull(consumption);
            Assert.Equal("APROBADA", consumption.Status);
            Assert.Equal(commerce.Id, consumption.CommerceId);
            Assert.Equal("Compra de prueba", consumption.Description);

            // Check Bank Transaction logged
            var bankTrans = db.Transactions.FirstOrDefault(t => t.AccountNumber == commerceAccount.AccountNumber);
            Assert.NotNull(bankTrans);
            Assert.Equal(1500m, bankTrans.Amount);
            Assert.Equal(TransactionType.Credito, bankTrans.Type);

            // Check Emails sent
            emailServiceMock.Verify(x => x.SendNotificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task ProcessPayment_WhenRejected_LogsRejectionAndDoesNotAlterBalances()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            
            // Seed Commerce
            var commerce = new Commerce
            {
                BusinessName = "Tienda ABC",
                RNC = "101002028",
                Email = "contacto@abc.com",
                Phone = "8095551111",
                Address = "SD",
                IsActive = true,
                PrincipalAccountNumber = "999888777"
            };
            db.Commerces.Add(commerce);
            await db.SaveChangesAsync();

            // Seed User Representative for Commerce
            var commerceUser = new User
            {
                FirstName = "Comercio",
                LastName = "ABC",
                Username = "101002028",
                Email = "contacto@abc.com",
                Cedula = "COM-101002028",
                RoleId = 4,
                IsActive = true,
                CommerceId = commerce.Id
            };
            db.Users.Add(commerceUser);
            await db.SaveChangesAsync();

            // Seed Commerce Principal Account
            var commerceAccount = new SavingsAccount
            {
                UserId = commerceUser.Id,
                AccountNumber = "999888777",
                Balance = 1000m,
                IsPrincipal = true,
                Status = AccountStatus.Activa,
                Type = AccountType.Principal
            };
            db.SavingsAccounts.Add(commerceAccount);
            await db.SaveChangesAsync();

            // Seed Credit Card
            var expirationDate = DateTime.UtcNow.AddYears(1);
            var card = new CreditCard
            {
                ClientId = 10,
                CardNumber = "1234567812345678",
                CreditLimit = 5000m,
                CurrentDebt = 4000m, // available credit = 1000m
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                CvcHash = HashString("123"),
                Status = "Activa",
                AdminId = 1
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var commerceRepo = new CommerceRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var consumptionRepo = new CreditCardConsumptionRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);
            var emailServiceMock = new Mock<IEmailService>();
            
            var service = new PaymentService(
                commerceRepo,
                cardRepo,
                consumptionRepo,
                userRepo,
                accountRepo,
                transactionRepo,
                emailServiceMock.Object,
                GetValidator()
            );

            // Request for 1500m (exceeds available 1000m)
            var requestDto = new ProcessPaymentRequestDto
            {
                CardNumber = "1234567812345678",
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                Cvc = "123",
                Amount = 1500m,
                Description = "Compra por encima del limite"
            };

            // Act
            var result = await service.ProcessPaymentAsync(commerce.Id, requestDto, 1);

            // Assert
            Assert.Equal(TransactionStatus.Rechazada, result.Status);
            Assert.Equal("Fondos insuficientes (crédito no disponible).", result.ErrorMessage);

            // Deuda remains identical
            var dbCard = await db.CreditCards.FindAsync(card.Id);
            Assert.Equal(4000m, dbCard.CurrentDebt);

            // Commerce balance remains identical
            var dbAccount = await db.SavingsAccounts.FindAsync(commerceAccount.Id);
            Assert.Equal(1000m, dbAccount.Balance);

            // Consumption logged as RECHAZADA
            var consumption = db.CreditCardConsumptions.FirstOrDefault(c => c.Id == result.Id);
            Assert.NotNull(consumption);
            Assert.Equal("RECHAZADA", consumption.Status);

            // No bank transaction logged
            var hasBankTrans = db.Transactions.Any(t => t.AccountNumber == commerceAccount.AccountNumber);
            Assert.False(hasBankTrans);
        }

        [Fact]
        public async Task ProcessPayment_WhenEmailServiceFails_PaymentSucceeds()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            
            // Seed Commerce
            var commerce = new Commerce
            {
                BusinessName = "Tienda ABC",
                RNC = "101002028",
                Email = "contacto@abc.com",
                Phone = "8095551111",
                Address = "SD",
                IsActive = true,
                PrincipalAccountNumber = "999888777"
            };
            db.Commerces.Add(commerce);
            await db.SaveChangesAsync();

            // Seed User
            var commerceUser = new User
            {
                FirstName = "Comercio",
                LastName = "ABC",
                Username = "101002028",
                Email = "contacto@abc.com",
                Cedula = "COM-101002028",
                RoleId = 4,
                IsActive = true,
                CommerceId = commerce.Id
            };
            db.Users.Add(commerceUser);
            await db.SaveChangesAsync();

            // Seed Account
            var commerceAccount = new SavingsAccount
            {
                UserId = commerceUser.Id,
                AccountNumber = "999888777",
                Balance = 1000m,
                IsPrincipal = true,
                Status = AccountStatus.Activa,
                Type = AccountType.Principal
            };
            db.SavingsAccounts.Add(commerceAccount);
            await db.SaveChangesAsync();

            // Seed Client User
            var clientUser = new User
            {
                FirstName = "Juan",
                LastName = "Perez",
                Username = "juanperez",
                Email = "juan@perez.com",
                Cedula = "40200000000",
                RoleId = 3, // Cliente
                IsActive = true
            };
            db.Users.Add(clientUser);
            await db.SaveChangesAsync();

            // Seed Card
            var expirationDate = DateTime.UtcNow.AddYears(1);
            var card = new CreditCard
            {
                ClientId = clientUser.Id,
                CardNumber = "1234567812345678",
                CreditLimit = 5000m,
                CurrentDebt = 1000m,
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                CvcHash = HashString("123"),
                Status = "Activa",
                AdminId = 1
            };
            db.CreditCards.Add(card);
            await db.SaveChangesAsync();

            var commerceRepo = new CommerceRepository(db);
            var cardRepo = new CreditCardRepository(db);
            var consumptionRepo = new CreditCardConsumptionRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new SavingsAccountRepository(db);
            var transactionRepo = new TransactionRepository(db);

            // Mock email service to throw SmtpException
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock
                .Setup(x => x.SendNotificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new SmtpException("SMTP host unavailable"));

            var service = new PaymentService(
                commerceRepo,
                cardRepo,
                consumptionRepo,
                userRepo,
                accountRepo,
                transactionRepo,
                emailServiceMock.Object,
                GetValidator()
            );

            var requestDto = new ProcessPaymentRequestDto
            {
                CardNumber = "1234567812345678",
                ExpirationMonth = expirationDate.ToString("MM"),
                ExpirationYear = expirationDate.ToString("yyyy"),
                Cvc = "123",
                Amount = 500m,
                Description = "Compra con email caido"
            };

            // Act & Assert (Should not throw and return approved result)
            var result = await service.ProcessPaymentAsync(commerce.Id, requestDto, 1);
            Assert.Equal(TransactionStatus.Aprobada, result.Status);

            // Balance still credited
            var dbAccount = await db.SavingsAccounts.FindAsync(commerceAccount.Id);
            Assert.Equal(1500m, dbAccount.Balance);
        }

        private static string HashString(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
