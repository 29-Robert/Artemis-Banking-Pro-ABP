using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loans.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Loan.Handlers
{
    public class UpdateLoanRateCommandHandlerTests
    {

        private readonly Mock<ILoanService> _serviceMock = new();

        [Fact]
        public async Task Handle_DeberiaLlamarAUpdateInterestRateAsyncConLosDatosCorrectos()
        {
            _serviceMock
                .Setup(s => s.UpdateInterestRateAsync(3, 18m))
                .ReturnsAsync(new LoanResponseDto { Id = 3, AnnualInterestRate = 18m });

            var handler = new UpdateLoanRateCommandHandler(_serviceMock.Object);
            var command = new UpdateLoanRateCommand { LoanId = 3, NewAnnualInterestRate = 18m };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            _serviceMock.Verify(s => s.UpdateInterestRateAsync(3, 18m), Times.Once);
        }

        [Fact]
        public async Task Handle_SiNoHayCuotasFuturasPendientes_DeberiaPropagarInvalidOperationException()
        {
            _serviceMock
                .Setup(s => s.UpdateInterestRateAsync(It.IsAny<int>(), It.IsAny<decimal>()))
                .ThrowsAsync(new InvalidOperationException("No existen cuotas futuras pendientes para recalcular."));

            var handler = new UpdateLoanRateCommandHandler(_serviceMock.Object);
            var command = new UpdateLoanRateCommand { LoanId = 3, NewAnnualInterestRate = 18m };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(command, CancellationToken.None));
        }
    }
}

