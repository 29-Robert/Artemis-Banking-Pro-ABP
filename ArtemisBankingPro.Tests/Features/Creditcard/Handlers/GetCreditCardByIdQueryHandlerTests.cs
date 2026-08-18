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
    public class GetCreditCardByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsCardFromService()
        {
            var service = new Mock<ICreditCardService>();
            var expected = new CreditCardResponseDto { Id = 5, Status = "Activa" };

            service.Setup(s => s.GetCreditCardByIdAsync(5)).ReturnsAsync(expected);

            var handler = new GetCreditCardByIdQueryHandler(service.Object);
            var query = new GetCreditCardByIdQuery { Id = 5 };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Same(expected, result);
            service.Verify(s => s.GetCreditCardByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenServiceThrowsKeyNotFound_PropagatesException()
        {
            var service = new Mock<ICreditCardService>();
            service.Setup(s => s.GetCreditCardByIdAsync(99))
                .ThrowsAsync(new KeyNotFoundException("La tarjeta seleccionada no existe."));

            var handler = new GetCreditCardByIdQueryHandler(service.Object);
            var query = new GetCreditCardByIdQuery { Id = 99 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(query, CancellationToken.None));
        }
    }
}
