using ArtemisBankingPro.Application.Features.CreditCard.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Handlers
{
    public class UpdateCreditLimitCommandHandlerTests
    {
        [Fact]
        public async Task Handle_CallsServiceWithCardIdAndNewLimit()
        {
           
            var dummyResponse = new CreditCardResponseDto();

            var service = new Mock<ICreditCardService>();

            
            service.Setup(s => s.UpdateCreditLimitAsync(7, 60000m))
                   .ReturnsAsync(dummyResponse);

            var handler = new UpdateCreditLimitCommandHandler(service.Object);
            var command = new UpdateCreditLimitCommand { CardId = 7, NewCreditLimit = 60000m };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.IsType<Unit>(result);
            Assert.Equal(Unit.Value, result);

            service.Verify(s => s.UpdateCreditLimitAsync(7, 60000m), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenNewLimitLowerThanDebt_PropagatesInvalidOperationException()
        {
            var service = new Mock<ICreditCardService>();
            service.Setup(s => s.UpdateCreditLimitAsync(It.IsAny<int>(), It.IsAny<decimal>()))
                .ThrowsAsync(new InvalidOperationException("El límite de la tarjeta no puede ser inferior al monto adeudado actualmente."));

            var handler = new UpdateCreditLimitCommandHandler(service.Object);
            var command = new UpdateCreditLimitCommand { CardId = 7, NewCreditLimit = 100m };

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}
