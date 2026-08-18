using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Exceptions;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests
{
    public class LoanServiceTest
    {
        
        // ASIGNAR PRÉSTAMO
        
        [Fact]
        public async Task AssignLoanAsync_WhenValid_CreatesLoanAndDisbursesToAccount()
        {
            var client = new User { Id = 20, IsActive = true, Cedula = "00187654321", FirstName = "María", LastName = "Gómez", Email = "maria@artemis.com", Role = new Role { Name = "Cliente" } };
            var principalAccount = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa, Type = AccountType.Principal };

            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();
            var installments = new Mock<ILoanInstallmentRepository>();
            var cards = new Mock<ICreditCardRepository>();
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var emailService = new Mock<IEmailService>();
            var mapper = new Mock<IMapper>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User> { client });
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);
            accounts.Setup(x => x.GetPrincipalByClientAsync(20)).ReturnsAsync(principalAccount);
            loans.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(0m);
            cards.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(0m);
            loans.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);
            cards.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);
            loans.Setup(x => x.LoanNumberExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            accounts.Setup(x => x.GetByAccountNumberAsync(It.IsAny<string>())).ReturnsAsync((SavingsAccount)null);

            var createdLoan = new Loan { Id = 1, ClientId = 20, LoanNumber = "987654321", Status = "Activo" };
            loans.Setup(x => x.AddAsync(It.IsAny<Loan>())).ReturnsAsync(createdLoan);
            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(createdLoan);
            mapper.Setup(x => x.Map<LoanResponseDto>(createdLoan)).Returns(new LoanResponseDto { Id = 1, LoanNumber = "987654321" });

            var service = CreateService(users, loans, installments, cards, accounts, transactions, emailService, mapper);

            var request = new CreateLoanRequestDto
            {
                ClientId = "20",
                CapitalAmount = 100000m,
                TermInMonths = 12,
                AnnualInterestRate = 12m,
                ConfirmHighRisk = false
            };

            var result = await service.AssignLoanAsync(request, adminId: 1);

            Assert.Equal("987654321", result.LoanNumber);
            Assert.Equal(105000m, principalAccount.Balance);
            accounts.Verify(x => x.UpdateAsync(principalAccount), Times.Once);
            transactions.Verify(x => x.AddAsync(It.Is<Transaction>(t =>
                t.Type == TransactionType.Credito &&
                t.Amount == 100000m &&
                t.Status == TransactionStatus.Aprobada)), Times.Once);
            installments.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<LoanInstallment>>(list => list.Count() == 12)), Times.Once);
            emailService.Verify(x => x.SendNotificationEmailAsync(client.Email, "Préstamo aprobado", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AssignLoanAsync_WhenClientDoesNotExist_ThrowsKeyNotFound()
        {
            var users = new Mock<IGenericRepository<User>>();
            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync((User)null);

            var service = CreateService(users: users);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.AssignLoanAsync(new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = 10m }, adminId: 1));
        }

        [Fact]
        public async Task AssignLoanAsync_WhenClientInactive_ThrowsInvalidOperation()
        {
            var client = new User { Id = 20, IsActive = false };
            var users = new Mock<IGenericRepository<User>>();
            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);

            var service = CreateService(users: users);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignLoanAsync(new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = 10m }, adminId: 1));
        }

        [Fact]
        public async Task AssignLoanAsync_WhenClientHasActiveLoan_ThrowsInvalidOperation()
        {
            var client = new User { Id = 20, IsActive = true };
            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(true);

            var service = CreateService(users: users, loans: loans);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignLoanAsync(new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = 10m }, adminId: 1));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(61)]
        public async Task AssignLoanAsync_WhenTermIsInvalid_ThrowsArgumentException(int invalidTerm)
        {
            var client = new User { Id = 20, IsActive = true };
            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);

            var service = CreateService(users: users, loans: loans);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AssignLoanAsync(new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = invalidTerm, AnnualInterestRate = 10m }, adminId: 1));
        }

        [Fact]
        public async Task AssignLoanAsync_WhenCapitalAmountIsZeroOrLess_ThrowsArgumentException()
        {
            var client = new User { Id = 20, IsActive = true };
            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);

            var service = CreateService(users: users, loans: loans);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AssignLoanAsync(new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 0m, TermInMonths = 12, AnnualInterestRate = 10m }, adminId: 1));
        }

        [Fact]
        public async Task AssignLoanAsync_WhenNegativeInterestRate_ThrowsArgumentException()
        {
            var client = new User { Id = 20, IsActive = true };
            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);

            var service = CreateService(users: users, loans: loans);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AssignLoanAsync(new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = -1m }, adminId: 1));
        }

        [Fact]
        public async Task AssignLoanAsync_WhenClientHasNoActivePrincipalAccount_ThrowsInvalidOperation()
        {
            var client = new User { Id = 20, IsActive = true };
            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();
            var accounts = new Mock<ISavingsAccountRepository>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);
            accounts.Setup(x => x.GetPrincipalByClientAsync(20)).ReturnsAsync((SavingsAccount)null);

            var service = CreateService(users: users, loans: loans, accounts: accounts);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignLoanAsync(new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = 10m }, adminId: 1));
        }

        [Fact]
        public async Task AssignLoanAsync_WhenCurrentDebtExceedsAverage_AndNotConfirmed_ThrowsHighRisk_CurrentHighRisk()
        {
            var client = new User { Id = 20, IsActive = true, Role = new Role { Name = "Cliente" } };
            var principalAccount = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };

            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();
            var cards = new Mock<ICreditCardRepository>();
            var accounts = new Mock<ISavingsAccountRepository>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User> { client });
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);
            accounts.Setup(x => x.GetPrincipalByClientAsync(20)).ReturnsAsync(principalAccount);

            loans.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(60000m);
            cards.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(0m);
            loans.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(10000m);
            cards.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);

            var service = CreateService(users: users, loans: loans, cards: cards, accounts: accounts);

            var request = new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = 10m, ConfirmHighRisk = false };

            var ex = await Assert.ThrowsAsync<HighRiskClientException>(() => service.AssignLoanAsync(request, adminId: 1));
            Assert.Equal("CurrentHighRisk", ex.RiskType);
        }

        [Fact]
        public async Task AssignLoanAsync_WhenProjectedDebtExceedsAverage_AndNotConfirmed_ThrowsHighRisk_ProjectedHighRisk()
        {
            var client = new User { Id = 20, IsActive = true, Role = new Role { Name = "Cliente" } };
            var principalAccount = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };

            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();
            var cards = new Mock<ICreditCardRepository>();
            var accounts = new Mock<ISavingsAccountRepository>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User> { client });
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);
            accounts.Setup(x => x.GetPrincipalByClientAsync(20)).ReturnsAsync(principalAccount);

            loans.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(1000m);
            cards.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(0m);
            loans.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(5000m);
            cards.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);

            var service = CreateService(users: users, loans: loans, cards: cards, accounts: accounts);

            var request = new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 100000m, TermInMonths = 12, AnnualInterestRate = 12m, ConfirmHighRisk = false };

            var ex = await Assert.ThrowsAsync<HighRiskClientException>(() => service.AssignLoanAsync(request, adminId: 1));
            Assert.Equal("ProjectedHighRisk", ex.RiskType);
        }

        [Fact]
        public async Task AssignLoanAsync_WhenHighRiskButConfirmed_CreatesLoanAnyway()
        {
            var client = new User { Id = 20, IsActive = true, Email = "maria@artemis.com", Role = new Role { Name = "Cliente" } };
            var principalAccount = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };

            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();
            var installments = new Mock<ILoanInstallmentRepository>();
            var cards = new Mock<ICreditCardRepository>();
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var mapper = new Mock<IMapper>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User> { client });
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);
            accounts.Setup(x => x.GetPrincipalByClientAsync(20)).ReturnsAsync(principalAccount);
            loans.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(60000m);
            cards.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(0m);
            loans.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(10000m);
            cards.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);
            loans.Setup(x => x.LoanNumberExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            accounts.Setup(x => x.GetByAccountNumberAsync(It.IsAny<string>())).ReturnsAsync((SavingsAccount)null);

            var createdLoan = new Loan { Id = 1, ClientId = 20, LoanNumber = "987654321" };
            loans.Setup(x => x.AddAsync(It.IsAny<Loan>())).ReturnsAsync(createdLoan);
            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(createdLoan);
            mapper.Setup(x => x.Map<LoanResponseDto>(createdLoan)).Returns(new LoanResponseDto { Id = 1 });

            var service = CreateService(users: users, loans: loans, installments: installments, cards: cards, accounts: accounts, transactions: transactions, mapper: mapper);

            var request = new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = 10m, ConfirmHighRisk = true };

            var result = await service.AssignLoanAsync(request, adminId: 1);

            Assert.NotNull(result);
            loans.Verify(x => x.AddAsync(It.IsAny<Loan>()), Times.Once);
        }

        [Fact]
        public async Task AssignLoanAsync_WhenEmailFails_StillCreatesLoan_AndFlagsEmailNotificationFailed()
        {
            var client = new User { Id = 20, IsActive = true, Email = "maria@artemis.com", Role = new Role { Name = "Cliente" } };
            var principalAccount = new SavingsAccount { AccountNumber = "123456789", Balance = 5000m, Status = AccountStatus.Activa };

            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();
            var installments = new Mock<ILoanInstallmentRepository>();
            var cards = new Mock<ICreditCardRepository>();
            var accounts = new Mock<ISavingsAccountRepository>();
            var transactions = new Mock<ITransactionRepository>();
            var emailService = new Mock<IEmailService>();
            var mapper = new Mock<IMapper>();

            users.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(client);
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User> { client });
            loans.Setup(x => x.HasActiveLoanAsync(20)).ReturnsAsync(false);
            accounts.Setup(x => x.GetPrincipalByClientAsync(20)).ReturnsAsync(principalAccount);
            loans.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(0m);
            cards.Setup(x => x.GetTotalActiveDebtByClientAsync(20)).ReturnsAsync(0m);
            loans.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);
            cards.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);
            loans.Setup(x => x.LoanNumberExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            accounts.Setup(x => x.GetByAccountNumberAsync(It.IsAny<string>())).ReturnsAsync((SavingsAccount)null);

            var createdLoan = new Loan { Id = 1, ClientId = 20, LoanNumber = "987654321" };
            loans.Setup(x => x.AddAsync(It.IsAny<Loan>())).ReturnsAsync(createdLoan);
            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(createdLoan);
            mapper.Setup(x => x.Map<LoanResponseDto>(createdLoan)).Returns(new LoanResponseDto { Id = 1, EmailNotificationFailed = false });

            emailService.Setup(x => x.SendNotificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP down"));

            var service = CreateService(users: users, loans: loans, installments: installments, cards: cards, accounts: accounts, transactions: transactions, emailService: emailService, mapper: mapper);

            var request = new CreateLoanRequestDto { ClientId = "20", CapitalAmount = 1000m, TermInMonths = 12, AnnualInterestRate = 10m };

            var result = await service.AssignLoanAsync(request, adminId: 1);

            Assert.True(result.EmailNotificationFailed);
            loans.Verify(x => x.AddAsync(It.IsAny<Loan>()), Times.Once);
        }

        
        // EDITAR TASA DE INTERÉS
        
        [Fact]
        public async Task UpdateInterestRateAsync_RecalculatesOnlyFuturePendingInstallments()
        {
            var pastDue = DateTime.UtcNow.AddDays(-5);
            var future1 = DateTime.UtcNow.AddDays(10);
            var future2 = DateTime.UtcNow.AddDays(40);

            var paidInstallment = new LoanInstallment { InstallmentNumber = 1, PaymentStatus = "Pagada", DueDate = pastDue, CapitalAmount = 100m };
            var lateInstallment = new LoanInstallment { InstallmentNumber = 2, PaymentStatus = "Pendiente", IsLate = true, DueDate = pastDue, CapitalAmount = 100m };
            var futureInstallment1 = new LoanInstallment { InstallmentNumber = 3, PaymentStatus = "Pendiente", IsLate = false, DueDate = future1, CapitalAmount = 500m };
            var futureInstallment2 = new LoanInstallment { InstallmentNumber = 4, PaymentStatus = "Pendiente", IsLate = false, DueDate = future2, CapitalAmount = 500m };

            var loan = new Loan
            {
                Id = 1,
                LoanNumber = "987654321",
                Status = "Activo",
                AnnualInterestRate = 12m,
                Client = new User { FirstName = "María", LastName = "Gómez", Email = "maria@artemis.com" },
                Installments = new List<LoanInstallment> { paidInstallment, lateInstallment, futureInstallment1, futureInstallment2 }
            };

            var loans = new Mock<ILoanRepository>();
            var installments = new Mock<ILoanInstallmentRepository>();
            var emailService = new Mock<IEmailService>();
            var mapper = new Mock<IMapper>();

            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(loan);
            mapper.Setup(x => x.Map<LoanResponseDto>(loan)).Returns(new LoanResponseDto { Id = 1 });

            var service = CreateService(loans: loans, installments: installments, emailService: emailService, mapper: mapper);

            var result = await service.UpdateInterestRateAsync(loanId: 1, newAnnualRate: 8m);

            Assert.Equal(8m, loan.AnnualInterestRate);
            Assert.Equal("Pagada", paidInstallment.PaymentStatus);
            Assert.True(lateInstallment.IsLate);

            installments.Verify(x => x.UpdateAsync(futureInstallment1), Times.Once);
            installments.Verify(x => x.UpdateAsync(futureInstallment2), Times.Once);
            installments.Verify(x => x.UpdateAsync(paidInstallment), Times.Never);
            installments.Verify(x => x.UpdateAsync(lateInstallment), Times.Never);

            emailService.Verify(x => x.SendNotificationEmailAsync(loan.Client.Email, "Actualización de tasa de interés de préstamo", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateInterestRateAsync_WhenLoanDoesNotExist_ThrowsKeyNotFound()
        {
            var loans = new Mock<ILoanRepository>();
            loans.Setup(x => x.GetByIdWithDetailsAsync(99)).ReturnsAsync((Loan)null);

            var service = CreateService(loans: loans);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateInterestRateAsync(99, 10m));
        }

        [Fact]
        public async Task UpdateInterestRateAsync_WhenLoanNotActive_ThrowsInvalidOperation()
        {
            var loan = new Loan { Id = 1, Status = "Completado" };
            var loans = new Mock<ILoanRepository>();
            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(loan);

            var service = CreateService(loans: loans);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateInterestRateAsync(1, 10m));
        }

        [Fact]
        public async Task UpdateInterestRateAsync_WhenNewRateIsNegative_ThrowsArgumentException()
        {
            var loan = new Loan { Id = 1, Status = "Activo" };
            var loans = new Mock<ILoanRepository>();
            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(loan);

            var service = CreateService(loans: loans);

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateInterestRateAsync(1, -1m));
        }

        [Fact]
        public async Task UpdateInterestRateAsync_WhenNoFuturePendingInstallments_ThrowsInvalidOperation()
        {
            var loan = new Loan
            {
                Id = 1,
                Status = "Activo",
                Installments = new List<LoanInstallment>
                {
                    new() { PaymentStatus = "Pagada", DueDate = DateTime.UtcNow.AddDays(30) },
                    new() { PaymentStatus = "Pendiente", IsLate = true, DueDate = DateTime.UtcNow.AddDays(-5) }
                }
            };

            var loans = new Mock<ILoanRepository>();
            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(loan);

            var service = CreateService(loans: loans);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateInterestRateAsync(1, 10m));
        }

        
        // LISTADO PAGINADO
        
        [Fact]
        public async Task GetLoansAsync_ReturnsMappedPagedResult()
        {
            var loanEntities = new List<Loan> { new() { Id = 1, LoanNumber = "987654321" } };
            var mappedDtos = new List<LoanResponseDto> { new() { Id = 1, LoanNumber = "987654321" } };

            var loans = new Mock<ILoanRepository>();
            var mapper = new Mock<IMapper>();

            loans.Setup(x => x.SearchAsync(null, "Activos", 1, 20)).ReturnsAsync((loanEntities, 1));
            mapper.Setup(x => x.Map<List<LoanResponseDto>>(loanEntities)).Returns(mappedDtos);

            var service = CreateService(loans: loans, mapper: mapper);

            var result = await service.GetLoansAsync(null, "Activos", 1, 20);

            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetLoansAsync_WhenCedulaNotFound_ThrowsKeyNotFound()
        {
            var loans = new Mock<ILoanRepository>();
            var users = new Mock<IGenericRepository<User>>();

            loans.Setup(x => x.SearchAsync("000000000", null, 1, 20)).ReturnsAsync((new List<Loan>(), 0));
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User>());

            var service = CreateService(loans: loans, users: users);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetLoansAsync("000000000", null, 1, 20));
        }

        [Fact]
        public async Task GetLoansAsync_WhenClientExistsButHasNoLoans_ThrowsInvalidOperation()
        {
            var existingClient = new User { Cedula = "00187654321", Role = new Role { Name = "Cliente" } };

            var loans = new Mock<ILoanRepository>();
            var users = new Mock<IGenericRepository<User>>();

            loans.Setup(x => x.SearchAsync("00187654321", null, 1, 20)).ReturnsAsync((new List<Loan>(), 0));
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User> { existingClient });

            var service = CreateService(loans: loans, users: users);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetLoansAsync("00187654321", null, 1, 20));
        }

        
        // DETALLE
        
        [Fact]
        public async Task GetLoanByIdAsync_WhenExists_ReturnsMappedDto()
        {
            var loan = new Loan { Id = 1, LoanNumber = "987654321" };
            var loans = new Mock<ILoanRepository>();
            var mapper = new Mock<IMapper>();

            loans.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(loan);
            mapper.Setup(x => x.Map<LoanResponseDto>(loan)).Returns(new LoanResponseDto { Id = 1, LoanNumber = "987654321" });

            var service = CreateService(loans: loans, mapper: mapper);

            var result = await service.GetLoanByIdAsync(1);

            Assert.Equal("987654321", result.LoanNumber);
        }

        [Fact]
        public async Task GetLoanByIdAsync_WhenNotExists_ThrowsKeyNotFound()
        {
            var loans = new Mock<ILoanRepository>();
            loans.Setup(x => x.GetByIdWithDetailsAsync(99)).ReturnsAsync((Loan)null);

            var service = CreateService(loans: loans);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetLoanByIdAsync(99));
        }

        
        // CLIENTES ELEGIBLES
        
        [Fact]
        public async Task GetEligibleClientsAsync_ExcludesClientsWithActiveLoan()
        {
            var eligibleClient = new User { Id = 1, IsActive = true, Cedula = "001", FirstName = "Ana", LastName = "Ruiz", Email = "ana@artemis.com", Role = new Role { Name = "Cliente" } };
            var withActiveLoanClient = new User { Id = 2, IsActive = true, Cedula = "002", FirstName = "Luis", LastName = "Diaz", Email = "luis@artemis.com", Role = new Role { Name = "Cliente" } };

            var users = new Mock<IGenericRepository<User>>();
            var loans = new Mock<ILoanRepository>();
            var cards = new Mock<ICreditCardRepository>();

            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User> { eligibleClient, withActiveLoanClient });
            loans.Setup(x => x.HasActiveLoanAsync(1)).ReturnsAsync(false);
            loans.Setup(x => x.HasActiveLoanAsync(2)).ReturnsAsync(true);
            loans.Setup(x => x.GetTotalActiveDebtByClientAsync(1)).ReturnsAsync(0m);
            cards.Setup(x => x.GetTotalActiveDebtByClientAsync(1)).ReturnsAsync(0m);
            loans.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);
            cards.Setup(x => x.GetTotalActiveDebtSystemWideAsync()).ReturnsAsync(0m);

            var service = CreateService(users: users, loans: loans, cards: cards);

            var result = await service.GetEligibleClientsAsync(null, 1, 20);

            Assert.Single(result.Clients.Items);
            Assert.Equal("Ana Ruiz", result.Clients.Items[0].FullName);
        }

        [Fact]
        public async Task GetEligibleClientsAsync_WhenNoActiveClients_ReturnsZeroAverageDebt()
        {
            var users = new Mock<IGenericRepository<User>>();
            users.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<User>());

            var service = CreateService(users: users);

            var result = await service.GetEligibleClientsAsync(null, 1, 20);

            Assert.Equal(0m, result.SystemAverageDebt);
            Assert.Empty(result.Clients.Items);
        }

        
        // Factory
        
        private static LoanService CreateService(
            Mock<IGenericRepository<User>>? users = null,
            Mock<ILoanRepository>? loans = null,
            Mock<ILoanInstallmentRepository>? installments = null,
            Mock<ICreditCardRepository>? cards = null,
            Mock<ISavingsAccountRepository>? accounts = null,
            Mock<ITransactionRepository>? transactions = null,
            Mock<IEmailService>? emailService = null,
            Mock<IMapper>? mapper = null)
        {
            return new LoanService(
                loans?.Object ?? Mock.Of<ILoanRepository>(),
                installments?.Object ?? Mock.Of<ILoanInstallmentRepository>(),
                cards?.Object ?? Mock.Of<ICreditCardRepository>(),
                accounts?.Object ?? Mock.Of<ISavingsAccountRepository>(),
                transactions?.Object ?? Mock.Of<ITransactionRepository>(),
                users?.Object ?? Mock.Of<IGenericRepository<User>>(),
                emailService?.Object ?? Mock.Of<IEmailService>(),
                mapper?.Object ?? Mock.Of<IMapper>(),
                Mock.Of<ILogger<LoanService>>());
        }
    }
}