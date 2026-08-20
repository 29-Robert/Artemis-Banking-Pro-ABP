using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class LoanInstallmentRepositoryTests : IntegrationTestBase
    {
        private readonly LoanInstallmentRepository _repository;
        private User _admin;
        private User _client;
        private Loan _loan;

        public LoanInstallmentRepositoryTests()
        {
            _repository = new LoanInstallmentRepository(DbContext);
        }

        private async Task SeedBaseDataAsync()
        {
            _admin = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Username = "loanadmin_" + Guid.NewGuid().ToString("N"),
                Email = "admin_" + Guid.NewGuid().ToString("N") + "@artemis.com",
                Cedula = "LAD" + Guid.NewGuid().ToString("N")[..8],
                RoleId = 1,
                IsActive = true,
                PasswordHash = "hash"
            };

            _client = new User
            {
                FirstName = "Client",
                LastName = "User",
                Username = "loanclient_" + Guid.NewGuid().ToString("N"),
                Email = "client_" + Guid.NewGuid().ToString("N") + "@artemis.com",
                Cedula = "LCL" + Guid.NewGuid().ToString("N")[..8],
                RoleId = 3,
                IsActive = true,
                PasswordHash = "hash"
            };

            DbContext.Users.AddRange(_admin, _client);
            await DbContext.SaveChangesAsync();

            _loan = new Loan
            {
                ClientId = _client.Id,
                AdminId = _admin.Id,
                LoanNumber = "LN-" + Guid.NewGuid().ToString("N")[..8],
                CapitalAmount = 12000m,
                TermInMonths = 12,
                AnnualInterestRate = 12m,
                Status = "Activo"
            };

            DbContext.Loans.Add(_loan);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task AddRangeAsync_ShouldPersistCompleteAmortizationTable()
        {
            await SeedBaseDataAsync();

            var installments = new List<LoanInstallment>();
            var baseDate = DateTime.UtcNow;

            for (var i = 1; i <= 12; i++)
            {
                installments.Add(new LoanInstallment
                {
                    LoanId = _loan.Id,
                    InstallmentNumber = i,
                    DueDate = baseDate.AddMonths(i),
                    InstallmentAmount = 1066.19m,
                    InterestAmount = 120m,
                    CapitalAmount = 946.19m,
                    PendingInstallmentAmount = 1066.19m,
                    PaymentStatus = "Pendiente",
                    IsLate = false
                });
            }

            await _repository.AddRangeAsync(installments);
            await DbContext.SaveChangesAsync();

            var savedInstallments = await DbContext.LoanInstallments
                .Where(i => i.LoanId == _loan.Id)
                .OrderBy(i => i.InstallmentNumber)
                .ToListAsync();

            Assert.Equal(12, savedInstallments.Count);
            Assert.Equal(1066.19m, savedInstallments.First().InstallmentAmount);
            Assert.Equal(946.19m, savedInstallments.First().CapitalAmount);
            Assert.Equal(120m, savedInstallments.First().InterestAmount);
            Assert.Equal("Pendiente", savedInstallments.First().PaymentStatus);
        }

        [Fact]
        public async Task GetPendingInstallmentsAsync_ShouldFilterByStatusAndOrderChronologically()
        {
            await SeedBaseDataAsync();

            var baseDate = DateTime.UtcNow;

            var inst1 = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 1,
                DueDate = baseDate.AddMonths(-1),
                InstallmentAmount = 1000m,
                PaymentStatus = "Pagada"
            };

            var inst2 = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 2,
                DueDate = baseDate.AddDays(15),
                InstallmentAmount = 1000m,
                PaymentStatus = "Pendiente"
            };

            var inst3 = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 3,
                DueDate = baseDate.AddMonths(1),
                InstallmentAmount = 1000m,
                PaymentStatus = "Pendiente"
            };

            DbContext.LoanInstallments.AddRange(inst1, inst3, inst2);
            await DbContext.SaveChangesAsync();

            var pending = await _repository.GetPendingInstallmentsAsync(_loan.Id);

            Assert.Equal(2, pending.Count);
            Assert.Equal(inst2.Id, pending[0].Id);
            Assert.Equal(inst3.Id, pending[1].Id);
        }

        [Fact]
        public async Task UpdateInstallments_ShouldReflectPaymentStatusAndPendingAmount()
        {
            await SeedBaseDataAsync();

            var installment = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 1,
                DueDate = DateTime.UtcNow.AddMonths(1),
                InstallmentAmount = 1000m,
                PendingInstallmentAmount = 1000m,
                PaymentStatus = "Pendiente"
            };

            DbContext.LoanInstallments.Add(installment);
            await DbContext.SaveChangesAsync();

            installment.PaymentStatus = "Pagada";
            installment.PendingInstallmentAmount = 0m;

            await _repository.UpdateAsync(installment);
            await DbContext.SaveChangesAsync();

            var retrieved = await DbContext.LoanInstallments.FindAsync(installment.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("Pagada", retrieved.PaymentStatus);
            Assert.Equal(0m, retrieved.PendingInstallmentAmount);
        }

        [Fact]
        public async Task RecalculateInterestRate_ShouldOnlyModifyFuturePendingInstallments()
        {
            await SeedBaseDataAsync();

            var baseDate = DateTime.UtcNow;

            var paidInstallment = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 1,
                DueDate = baseDate.AddMonths(-1),
                InstallmentAmount = 1000m,
                InterestAmount = 100m,
                CapitalAmount = 900m,
                PendingInstallmentAmount = 0m,
                PaymentStatus = "Pagada"
            };

            var lateInstallment = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 2,
                DueDate = baseDate.AddDays(-10),
                InstallmentAmount = 1000m,
                InterestAmount = 100m,
                CapitalAmount = 900m,
                PendingInstallmentAmount = 1000m,
                PaymentStatus = "Pendiente",
                IsLate = true
            };

            var futureInstallment1 = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 3,
                DueDate = baseDate.AddMonths(1),
                InstallmentAmount = 1000m,
                InterestAmount = 100m,
                CapitalAmount = 900m,
                PendingInstallmentAmount = 1000m,
                PaymentStatus = "Pendiente",
                IsLate = false
            };

            var futureInstallment2 = new LoanInstallment
            {
                LoanId = _loan.Id,
                InstallmentNumber = 4,
                DueDate = baseDate.AddMonths(2),
                InstallmentAmount = 1000m,
                InterestAmount = 100m,
                CapitalAmount = 900m,
                PendingInstallmentAmount = 1000m,
                PaymentStatus = "Pendiente",
                IsLate = false
            };

            DbContext.LoanInstallments.AddRange(paidInstallment, lateInstallment, futureInstallment1, futureInstallment2);
            await DbContext.SaveChangesAsync();

            var now = DateTime.UtcNow;
            var futurePending = await DbContext.LoanInstallments
                .Where(i => i.LoanId == _loan.Id && i.PaymentStatus == "Pendiente" && !i.IsLate && i.DueDate > now)
                .OrderBy(i => i.InstallmentNumber)
                .ToListAsync();

            Assert.Equal(2, futurePending.Count);

            foreach (var inst in futurePending)
            {
                inst.InterestAmount = 150m;
                inst.CapitalAmount = 850m;
                inst.InstallmentAmount = 1000m;
                await _repository.UpdateAsync(inst);
            }
            await DbContext.SaveChangesAsync();

            var dbPaid = await DbContext.LoanInstallments.FindAsync(paidInstallment.Id);
            Assert.Equal(100m, dbPaid.InterestAmount);

            var dbLate = await DbContext.LoanInstallments.FindAsync(lateInstallment.Id);
            Assert.Equal(100m, dbLate.InterestAmount);

            var dbFuture1 = await DbContext.LoanInstallments.FindAsync(futureInstallment1.Id);
            Assert.Equal(150m, dbFuture1.InterestAmount);

            var dbFuture2 = await DbContext.LoanInstallments.FindAsync(futureInstallment2.Id);
            Assert.Equal(150m, dbFuture2.InterestAmount);
        }
    }
}
