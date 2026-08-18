using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Creditcard.Handlers
{
    public class GetCreditCardByNumberQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WhenCardExists_ReturnsEntity()
        {
            var repo = new Mock<ICreditCardRepository>();
            var card = new Domain.Entities.CreditCard { CardNumber = "1234567812345678", Status = "Activa" };

            repo.Setup(r => r.GetByCardNumberAsync("1234567812345678")).ReturnsAsync(card);

            var handler = new GetCreditCardByNumberQueryHandler(repo.Object);
            var query = new GetCreditCardByNumberQuery { CardNumber = "1234567812345678" };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("1234567812345678", result!.CardNumber);
        }

        [Fact]
        public async Task Handle_WhenCardDoesNotExist_ReturnsNull()
        {
            var repo = new Mock<ICreditCardRepository>();
            repo.Setup(r => r.GetByCardNumberAsync("0000000000000000")).ReturnsAsync((Domain.Entities.CreditCard?)null);

            var handler = new GetCreditCardByNumberQueryHandler(repo.Object);
            var query = new GetCreditCardByNumberQuery { CardNumber = "0000000000000000" };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Null(result);
        }
    }
}
