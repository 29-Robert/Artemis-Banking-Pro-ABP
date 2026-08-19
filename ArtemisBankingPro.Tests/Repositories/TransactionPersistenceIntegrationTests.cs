using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class TransactionPersistenceIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task DeleteAccount_WithExistingTransactions_ShouldThrowDbUpdateException()
        {
            var account = new SavingsAccount { AccountNumber = "500000001", Balance = 1000m, Status = AccountStatus.Activa, Type = AccountType.Principal, UserId = 3 };
            var tx = new Transaction { AccountNumber = "500000001", Type = TransactionType.Credito, Amount = 100m, Status = TransactionStatus.Aprobada, Description = "TEST", CreatedAt = DateTime.UtcNow };

            DbContext.SavingsAccounts.Add(account);
            DbContext.Transactions.Add(tx);
            await DbContext.SaveChangesAsync();

            DbContext.Entry(tx).State = EntityState.Detached;

            DbContext.SavingsAccounts.Remove(account);
            await Assert.ThrowsAsync<DbUpdateException>(async () => await DbContext.SaveChangesAsync());
        }

        [Fact]
        public async Task PersistedAmount_ShouldApplyDecimalPrecisionRule()
        {
            var account = new SavingsAccount { AccountNumber = "500000002", Balance = 1000m, Status = AccountStatus.Activa, Type = AccountType.Principal, UserId = 3 };
            var tx = new Transaction 
            { 
                AccountNumber = "500000002", 
                Type = TransactionType.Credito, 
                Amount = 150.8876m,
                Status = TransactionStatus.Aprobada, 
                Description = "TEST PRECISION", 
                CreatedAt = DateTime.UtcNow 
            };

            DbContext.SavingsAccounts.Add(account);
            DbContext.Transactions.Add(tx);
            await DbContext.SaveChangesAsync();

            var savedTx = await DbContext.Transactions.FindAsync(tx.Id);

            Assert.Equal(Math.Round(150.8876m, 2), Math.Round(savedTx.Amount, 2));
        }
    }
}
