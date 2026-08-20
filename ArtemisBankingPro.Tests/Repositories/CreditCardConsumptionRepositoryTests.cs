using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class CreditCardConsumptionRepositoryTests : IntegrationTestBase
    {
        private readonly CreditCardConsumptionRepository _repository;
        private User _admin;
        private User _client;
        private CreditCard _card;

        public CreditCardConsumptionRepositoryTests()
        {
            _repository = new CreditCardConsumptionRepository(DbContext);
        }

        private async Task SeedBaseDataAsync()
        {
            _admin = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Username = "ccadmin_" + Guid.NewGuid().ToString("N"),
                Email = "admin_" + Guid.NewGuid().ToString("N") + "@artemis.com",
                Cedula = "ADM" + Guid.NewGuid().ToString("N")[..8],
                RoleId = 1,
                IsActive = true,
                PasswordHash = "hash"
            };

            _client = new User
            {
                FirstName = "Client",
                LastName = "User",
                Username = "ccclient_" + Guid.NewGuid().ToString("N"),
                Email = "client_" + Guid.NewGuid().ToString("N") + "@artemis.com",
                Cedula = "CLI" + Guid.NewGuid().ToString("N")[..8],
                RoleId = 3,
                IsActive = true,
                PasswordHash = "hash"
            };

            DbContext.Users.AddRange(_admin, _client);
            await DbContext.SaveChangesAsync();

            _card = new CreditCard
            {
                ClientId = _client.Id,
                AdminId = _admin.Id,
                CardNumber = "12345678" + Guid.NewGuid().ToString("N")[..8],
                CreditLimit = 50000m,
                CurrentDebt = 0m,
                ExpirationMonth = "12",
                ExpirationYear = "2029",
                CvcHash = "hash",
                Status = "Activa"
            };

            DbContext.CreditCards.Add(_card);
            await DbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task AddAsync_ShouldPersistConsumptionsWithDifferentStatuses()
        {
            await SeedBaseDataAsync();

            var consumptionApproved = new CreditCardConsumption
            {
                CreditCardId = _card.Id,
                Amount = 1000m,
                CommerceName = "Supermercado A",
                Status = "APROBADA",
                TransactionDate = DateTime.UtcNow,
                Description = "Compra aprobada"
            };

            var consumptionRejected = new CreditCardConsumption
            {
                CreditCardId = _card.Id,
                Amount = 2500m,
                CommerceName = "Tienda B",
                Status = "RECHAZADA",
                TransactionDate = DateTime.UtcNow,
                Description = "Compra rechazada"
            };

            await _repository.AddAsync(consumptionApproved);
            await _repository.AddAsync(consumptionRejected);
            await DbContext.SaveChangesAsync();

            var retrievedApproved = await DbContext.CreditCardConsumptions.FindAsync(consumptionApproved.Id);
            var retrievedRejected = await DbContext.CreditCardConsumptions.FindAsync(consumptionRejected.Id);

            Assert.NotNull(retrievedApproved);
            Assert.Equal("APROBADA", retrievedApproved.Status);
            Assert.Equal(1000m, retrievedApproved.Amount);

            Assert.NotNull(retrievedRejected);
            Assert.Equal("RECHAZADA", retrievedRejected.Status);
            Assert.Equal(2500m, retrievedRejected.Amount);
        }

        [Fact]
        public async Task GetConsumptionsByCardAsync_ShouldReturnOrderedHistory()
        {
            await SeedBaseDataAsync();

            var baseTime = DateTime.UtcNow;

            var oldConsumption = new CreditCardConsumption
            {
                CreditCardId = _card.Id,
                Amount = 100m,
                CommerceName = "Shop C",
                Status = "APROBADA",
                TransactionDate = baseTime.AddMinutes(-10),
                Description = "Old"
            };

            var newConsumption = new CreditCardConsumption
            {
                CreditCardId = _card.Id,
                Amount = 200m,
                CommerceName = "Shop C",
                Status = "APROBADA",
                TransactionDate = baseTime.AddMinutes(10),
                Description = "New"
            };

            var midConsumption = new CreditCardConsumption
            {
                CreditCardId = _card.Id,
                Amount = 150m,
                CommerceName = "Shop C",
                Status = "APROBADA",
                TransactionDate = baseTime,
                Description = "Mid"
            };

            DbContext.CreditCardConsumptions.AddRange(oldConsumption, midConsumption, newConsumption);
            await DbContext.SaveChangesAsync();

            var history = await _repository.GetConsumptionsByCardAsync(_card.Id);

            Assert.Equal(3, history.Count);
            Assert.Equal(newConsumption.Id, history[0].Id);
            Assert.Equal(midConsumption.Id, history[1].Id);
            Assert.Equal(oldConsumption.Id, history[2].Id);
        }

        [Fact]
        public async Task GetPagedConsumptions_ShouldReturnCorrectSubsetAndOrder()
        {
            await SeedBaseDataAsync();

            var baseTime = DateTime.UtcNow;

            for (var i = 1; i <= 5; i++)
            {
                DbContext.CreditCardConsumptions.Add(new CreditCardConsumption
                {
                    CreditCardId = _card.Id,
                    Amount = i * 100m,
                    CommerceName = "Shop",
                    Status = "APROBADA",
                    TransactionDate = baseTime.AddMinutes(i),
                    Description = "Consumo " + i
                });
            }
            await DbContext.SaveChangesAsync();

            var page1 = await DbContext.CreditCardConsumptions
                .Where(c => c.CreditCardId == _card.Id)
                .OrderByDescending(c => c.TransactionDate)
                .Skip(0)
                .Take(3)
                .ToListAsync();

            var page2 = await DbContext.CreditCardConsumptions
                .Where(c => c.CreditCardId == _card.Id)
                .OrderByDescending(c => c.TransactionDate)
                .Skip(3)
                .Take(3)
                .ToListAsync();

            Assert.Equal(3, page1.Count);
            Assert.Equal(2, page2.Count);
            Assert.Equal("Consumo 5", page1[0].Description);
            Assert.Equal("Consumo 2", page2[0].Description);
        }

        [Fact]
        public async Task PersistedAmount_ShouldApplyDecimalPrecisionRule()
        {
            await SeedBaseDataAsync();

            var consumption = new CreditCardConsumption
            {
                CreditCardId = _card.Id,
                Amount = 125.8876m,
                CommerceName = "Shop D",
                Status = "APROBADA",
                TransactionDate = DateTime.UtcNow,
                Description = "Decimal test"
            };

            await _repository.AddAsync(consumption);
            await DbContext.SaveChangesAsync();

            var retrieved = await DbContext.CreditCardConsumptions.FindAsync(consumption.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(Math.Round(125.8876m, 2), Math.Round(retrieved.Amount, 2));
        }

        [Fact]
        public async Task DeleteCard_WithExistingConsumptions_ShouldThrowDbUpdateException()
        {
            await SeedBaseDataAsync();

            var consumption = new CreditCardConsumption
            {
                CreditCardId = _card.Id,
                Amount = 100m,
                CommerceName = "Shop E",
                Status = "APROBADA",
                TransactionDate = DateTime.UtcNow
            };

            DbContext.CreditCardConsumptions.Add(consumption);
            await DbContext.SaveChangesAsync();

            DbContext.Entry(consumption).State = EntityState.Detached;

            DbContext.CreditCards.Remove(_card);
            await Assert.ThrowsAsync<DbUpdateException>(async () => await DbContext.SaveChangesAsync());
        }
    }
}
