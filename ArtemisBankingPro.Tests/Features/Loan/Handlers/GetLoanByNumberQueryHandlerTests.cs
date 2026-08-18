using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loans.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Loan.Handlers
{
    public class GetLoanByNumberQueryHandlerTests
    {
        private readonly Mock<ILoanService> _serviceMock = new();

        [Fact]
        public async Task Handle_ConNumeroExistente_DeberiaDevolverElPrestamo()
        {
            _serviceMock
                .Setup(s => s.GetLoanByNumberAsync("123456789"))
                .ReturnsAsync(new LoanResponseDto { Id = 1, LoanNumber = "123456789" });

            var handler = new GetLoanByNumberQueryHandler(_serviceMock.Object);

            var result = await handler.Handle(new GetLoanByNumberQuery { LoanNumber = "123456789" }, CancellationToken.None);

            Assert.Equal("123456789", result.LoanNumber);
        }

        [Fact]
        public async Task Handle_ConNumeroInexistente_DeberiaPropagarKeyNotFoundException()
        {
            _serviceMock
                .Setup(s => s.GetLoanByNumberAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException("El préstamo seleccionado no existe."));

            var handler = new GetLoanByNumberQueryHandler(_serviceMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new GetLoanByNumberQuery { LoanNumber = "000000000" }, CancellationToken.None));
        }
    }
}
