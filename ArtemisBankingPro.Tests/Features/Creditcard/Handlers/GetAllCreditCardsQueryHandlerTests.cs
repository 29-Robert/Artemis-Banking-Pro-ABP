using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Handlers
{
    public class GetAllCreditCardsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_CallsServiceWithRequestParameters_AndReturnsResult()
        {
            var service = new Mock<ICreditCardService>();
            var expected = new PagedResult<CreditCardResponseDto>
            {
                Items = new List<CreditCardResponseDto> { new() { Id = 1 } },
                PageNumber = 2,
                PageSize = 10,
                TotalCount = 1
            };

            service.Setup(s => s.GetCreditCardsAsync("00187654321", "Activa", 2, 10))
                .ReturnsAsync(expected);

            var handler = new GetAllCreditCardsQueryHandler(service.Object);
            var query = new GetAllCreditCardsQuery { Cedula = "00187654321", Status = "Activa", PageNumber = 2, PageSize = 10 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Same(expected, result);
            service.Verify(s => s.GetCreditCardsAsync("00187654321", "Activa", 2, 10), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDefaultParameters_PassesDefaultsToService()
        {
            var service = new Mock<ICreditCardService>();
            service.Setup(s => s.GetCreditCardsAsync(null, null, 1, 20))
                .ReturnsAsync(new PagedResult<CreditCardResponseDto>());

            var handler = new GetAllCreditCardsQueryHandler(service.Object);
            var query = new GetAllCreditCardsQuery(); // Cedula/Status null, PageNumber=1, PageSize=20 por defecto

            await handler.Handle(query, CancellationToken.None);

            service.Verify(s => s.GetCreditCardsAsync(null, null, 1, 20), Times.Once);
        }
    }
}
