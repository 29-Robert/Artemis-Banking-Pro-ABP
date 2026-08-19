using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class TransactionalOperationsIntegrationTests : IntegrationTestBase
    {
        [Fact]
        public async Task TransactionRollback_ShouldRevertBalancesOnException()
        {
            var source = new User { FirstName = "Client", LastName = "Source", Cedula = "1111", Username = "csource", Email = "cs@test.com", RoleId = 3 };
            var dest = new User { FirstName = "Client", LastName = "Dest", Cedula = "2222", Username = "cdest", Email = "cd@test.com", RoleId = 3 };
            DbContext.Users.AddRange(source, dest);
            await DbContext.SaveChangesAsync();

            var sourceAccount = new SavingsAccount { AccountNumber = "111111111", Balance = 1000m, Status = AccountStatus.Activa, UserId = source.Id };
            var destAccount = new SavingsAccount { AccountNumber = "222222222", Balance = 200m, Status = AccountStatus.Activa, UserId = dest.Id };
            DbContext.SavingsAccounts.AddRange(sourceAccount, destAccount);
            await DbContext.SaveChangesAsync();

            using (var transaction = await DbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    sourceAccount.Balance -= 300m;
                    DbContext.SavingsAccounts.Update(sourceAccount);
                    await DbContext.SaveChangesAsync();

                    throw new InvalidOperationException("Fallo forzado en base de datos.");

                    destAccount.Balance += 300m;
                    DbContext.SavingsAccounts.Update(destAccount);
                    await DbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            }

            DbContext.ChangeTracker.Clear();

            var refreshedSource = await DbContext.SavingsAccounts.FindAsync(sourceAccount.Id);
            var refreshedDest = await DbContext.SavingsAccounts.FindAsync(destAccount.Id);

            Assert.Equal(1000m, refreshedSource.Balance);
            Assert.Equal(200m, refreshedDest.Balance);
        }
    }
}
