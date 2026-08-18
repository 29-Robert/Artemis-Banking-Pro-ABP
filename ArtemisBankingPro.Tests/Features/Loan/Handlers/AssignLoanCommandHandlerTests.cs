using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.Features.Loans.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Loan.Handlers
{
    public class AssignLoanCommandHandlerTests
    {
        private readonly Mock<ILoanService> _serviceMock = new();

        [Fact]
        public async Task Handle_DeberiaConvertirAdminIdAIntYLlamarAlServicio()
        {
            var expectedResponse = new LoanResponseDto { Id = 1, LoanNumber = "123456789" };

            _serviceMock
                .Setup(s => s.AssignLoanAsync(
                    It.Is<CreateLoanRequestDto>(r =>
                        r.ClientId == "5" &&
                        r.CapitalAmount == 50000m &&
                        r.TermInMonths == 24 &&
                        r.AnnualInterestRate == 12m &&
                        r.ConfirmHighRisk == false),
                    7))
                .ReturnsAsync(expectedResponse);

            var handler = new AssignLoanCommandHandler(_serviceMock.Object);
            var command = new AssignLoanCommand
            {
                ClientId = "5",
                CapitalAmount = 50000m,
                TermInMonths = 24,
                AnnualInterestRate = 12m,
                ConfirmHighRisk = false,
                AdminId = "7"
            };

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.Equal("123456789", result.LoanNumber);
            _serviceMock.Verify(s => s.AssignLoanAsync(It.IsAny<CreateLoanRequestDto>(), 7), Times.Once);
        }

        [Fact]
        public async Task Handle_DeberiaPropagarConfirmHighRiskEnTrue()
        {
            _serviceMock
                .Setup(s => s.AssignLoanAsync(It.IsAny<CreateLoanRequestDto>(), It.IsAny<int>()))
                .ReturnsAsync(new LoanResponseDto());

            var handler = new AssignLoanCommandHandler(_serviceMock.Object);
            var command = new AssignLoanCommand
            {
                ClientId = "5",
                CapitalAmount = 50000m,
                TermInMonths = 24,
                AnnualInterestRate = 12m,
                ConfirmHighRisk = true,
                AdminId = "1"
            };

            await handler.Handle(command, CancellationToken.None);

            _serviceMock.Verify(s => s.AssignLoanAsync(
                It.Is<CreateLoanRequestDto>(r => r.ConfirmHighRisk == true), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SiElServicioLanzaHighRiskClientException_DeberiaPropagarla()
        {
            _serviceMock
                .Setup(s => s.AssignLoanAsync(It.IsAny<CreateLoanRequestDto>(), It.IsAny<int>()))
                .ThrowsAsync(new ArtemisBankingPro.Application.Exceptions.HighRiskClientException(
                    "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema.",
                    "CurrentHighRisk", 5000m, 8000m, 3000m));

            var handler = new AssignLoanCommandHandler(_serviceMock.Object);
            var command = new AssignLoanCommand { ClientId = "5", AdminId = "1" };

            var ex = await Assert.ThrowsAsync<ArtemisBankingPro.Application.Exceptions.HighRiskClientException>(
                () => handler.Handle(command, CancellationToken.None));

            Assert.Equal("CurrentHighRisk", ex.RiskType);
        }
    }
}
