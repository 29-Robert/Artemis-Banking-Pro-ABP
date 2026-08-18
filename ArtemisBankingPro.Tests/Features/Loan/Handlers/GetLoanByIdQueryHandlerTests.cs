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
    public class GetLoanByIdQueryHandlerTests
    {
        private readonly Mock<ILoanService> _serviceMock = new();

        [Fact]
        public async Task Handle_ConIdExistente_DeberiaDevolverElPrestamo()
        {
            _serviceMock
                .Setup(s => s.GetLoanByIdAsync(10))
                .ReturnsAsync(new LoanResponseDto { Id = 10, LoanNumber = "987654321" });

            var handler = new GetLoanByIdQueryHandler(_serviceMock.Object);

            var result = await handler.Handle(new GetLoanByIdQuery { Id = 10 }, CancellationToken.None);

            Assert.Equal("987654321", result.LoanNumber);
        }

        [Fact]
        public async Task Handle_ConIdInexistente_DeberiaPropagarKeyNotFoundException()
        {
            _serviceMock
                .Setup(s => s.GetLoanByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("El préstamo seleccionado no existe."));

            var handler = new GetLoanByIdQueryHandler(_serviceMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => handler.Handle(new GetLoanByIdQuery { Id = 999 }, CancellationToken.None));
        }
    }
}
