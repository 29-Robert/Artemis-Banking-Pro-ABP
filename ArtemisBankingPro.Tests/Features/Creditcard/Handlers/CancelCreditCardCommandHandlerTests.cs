using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Handlers
{
    public class CancelCreditCardCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CallsServiceWithCardId_AndReturnsUnitValue()
        {
            var service = new Mock<ICreditCardService>();
            service.Setup(s => s.CancelCreditCardAsync(7)).Returns(Task.CompletedTask);

            var handler = new CancelCreditCardCommandHandler(service.Object);
            var command = new CancelCreditCardCommand { CardId = 7 };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            service.Verify(s => s.CancelCreditCardAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenCardHasDebt_PropagatesInvalidOperationException()
        {
            var service = new Mock<ICreditCardService>();
            service.Setup(s => s.CancelCreditCardAsync(It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente."));

            var handler = new CancelCreditCardCommandHandler(service.Object);
            var command = new CancelCreditCardCommand { CardId = 7 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
