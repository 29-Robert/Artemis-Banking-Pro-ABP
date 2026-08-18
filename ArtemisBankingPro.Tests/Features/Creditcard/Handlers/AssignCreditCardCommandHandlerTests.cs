using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Handlers
{
    public class AssignCreditCardCommandHandlerTests
    {
        [Fact]
        public async Task Handle_MapsCommandToDto_AndCallsServiceWithAdminId()
        {
            var service = new Mock<ICreditCardService>();
            var expected = new CreditCardCreatedResponseDto { CreditLimit = 50000m, Cvc = "123" };

            service.Setup(s => s.AssignCreditCardAsync(
                    It.Is<CreateCreditCardRequestDto>(dto => dto.ClientId == "20" && dto.CreditLimit == 50000m),
                    3))
                .ReturnsAsync(expected);

            var handler = new AssignCreditCardCommandHandler(service.Object);
            var command = new AssignCreditCardCommand { ClientId = "20", CreditLimit = 50000m, AdminId = 3 };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Same(expected, result);
            service.Verify(s => s.AssignCreditCardAsync(
                It.Is<CreateCreditCardRequestDto>(dto => dto.ClientId == "20" && dto.CreditLimit == 50000m),
                3), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenServiceThrowsInvalidOperation_PropagatesException()
        {
            var service = new Mock<ICreditCardService>();
            service.Setup(s => s.AssignCreditCardAsync(It.IsAny<CreateCreditCardRequestDto>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Solo se puede asignar tarjetas de crédito a clientes activos."));

            var handler = new AssignCreditCardCommandHandler(service.Object);
            var command = new AssignCreditCardCommand { ClientId = "20", CreditLimit = 50000m, AdminId = 3 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
