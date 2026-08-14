/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.Services;

namespace ArtemisBankingPro.Tests
{
    public class LoanServiceTests
    {
        [Fact]
        public async Task GenerateAmortizationTableAsync_CreatesExpectedInstallments()
        {
            var service = new LoanService(null!, null!, null!);

            var result = await service.GenerateAmortizationTableAsync(12000m, 12m, 12);

            Assert.Equal(12, result.Count);
            Assert.All(result, x => Assert.True(x.InstallmentAmount > 0));
            Assert.Equal(1, result.First().InstallmentNumber);
            Assert.Equal(12, result.Last().InstallmentNumber);
        }

        [Fact]
        public async Task GenerateAmortizationTableAsync_WithZeroInterest_DividesCapitalEqually()
        {
            var service = new LoanService(null!, null!, null!);

            var result = await service.GenerateAmortizationTableAsync(12000m, 0m, 12);

            Assert.All(result, x => Assert.Equal(1000m, x.InstallmentAmount));
        }
    }
}
*/