using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Handlers
{
    public class GetCreditCardsQueryValidatorTests
    {
        private readonly GetCreditCardsQueryValidator _validator = new();

        [Fact]
        public void Validate_WithValidPagination_IsValid()
        {
            var query = new GetAllCreditCardsQuery { PageNumber = 1, PageSize = 20 };

            var result = _validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithInvalidPageNumber_Fails(int pageNumber)
        {
            var query = new GetAllCreditCardsQuery { PageNumber = pageNumber, PageSize = 20 };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El número de página debe ser mayor que cero.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Validate_WithPageSizeOutOfRange_Fails(int pageSize)
        {
            var query = new GetAllCreditCardsQuery { PageNumber = 1, PageSize = pageSize };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "El tamaño de página debe estar entre 1 y 100.");
        }
    }
}
