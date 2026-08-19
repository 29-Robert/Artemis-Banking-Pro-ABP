using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Repositories;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class BeneficiaryRepositoryIntegrationTests : IntegrationTestBase
    {
        private readonly BeneficiaryRepository _repository;

        public BeneficiaryRepositoryIntegrationTests()
        {
            _repository = new BeneficiaryRepository(DbContext);
        }

        [Fact]
        public async Task AddAndGetBeneficiaries_ShouldMaintainClientIsolation()
        {
            var clientA = new User { FirstName = "Client", LastName = "A", Cedula = "111", Username = "ca", Email = "a@a.com", RoleId = 3 };
            var clientB = new User { FirstName = "Client", LastName = "B", Cedula = "222", Username = "cb", Email = "b@b.com", RoleId = 3 };
            DbContext.Users.AddRange(clientA, clientB);
            await DbContext.SaveChangesAsync();

            var beneficiary = new Beneficiary { ClientId = clientA.Id, Alias = "Alias A", BeneficiaryAccountNumber = "999999999" };
            await _repository.AddAsync(beneficiary);
            await DbContext.SaveChangesAsync();

            var listA = await _repository.GetByClientAsync(clientA.Id);
            var listB = await _repository.GetByClientAsync(clientB.Id);

            Assert.Single(listA);
            Assert.Empty(listB);
        }
    }
}
