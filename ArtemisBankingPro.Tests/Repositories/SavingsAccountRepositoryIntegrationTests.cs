using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class SavingsAccountRepositoryIntegrationTests : IntegrationTestBase
    {
        private readonly SavingsAccountRepository _repository;

        public SavingsAccountRepositoryIntegrationTests()
        {
            _repository = new SavingsAccountRepository(DbContext);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldFilterByStatusAndType()
        {
            var user = new User { FirstName = "Jose", LastName = "Almonte", Cedula = "40200000001", IsActive = true, RoleId = 3, Username = "jalmonte", Email = "j@example.com" };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var account1 = new SavingsAccount { AccountNumber = "100000001", Balance = 1000m, Status = AccountStatus.Activa, Type = AccountType.Principal, UserId = user.Id };
            var account2 = new SavingsAccount { AccountNumber = "100000002", Balance = 500m, Status = AccountStatus.Cancelada, Type = AccountType.Secundaria, UserId = user.Id };
            DbContext.SavingsAccounts.AddRange(account1, account2);
            await DbContext.SaveChangesAsync();

            var result = await _repository.GetPagedAsync(1, 10, AccountStatus.Activa, AccountType.Principal, "40200000001");

            Assert.Equal(1, result.TotalCount);
            var retrieved = Assert.Single(result.Data);
            Assert.Equal("100000001", retrieved.AccountNumber);
        }
    }
}
