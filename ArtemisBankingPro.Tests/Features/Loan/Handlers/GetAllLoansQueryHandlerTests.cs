using ArtemisBankingPro.Application.Common;
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
    public class GetAllLoansQueryHandlerTests
    {
        private readonly Mock<ILoanService> _serviceMock = new();

        [Fact]
        public async Task Handle_DeberiaPedirTodosLosRegistrosSinFiltros()
        {
            var paged = new PagedResult<LoanResponseDto>
            {
                Items = new List<LoanResponseDto> { new() { Id = 1 }, new() { Id = 2 } },
                PageNumber = 1,
                PageSize = int.MaxValue,
                TotalCount = 2
            };

            _serviceMock
                .Setup(s => s.GetLoansAsync(null, null, 1, int.MaxValue))
                .ReturnsAsync(paged);

            var handler = new GetAllLoansQueryHandler(_serviceMock.Object);

            var result = await handler.Handle(new GetAllLoansQuery(), CancellationToken.None);

            Assert.Equal(2, result.Count);
            _serviceMock.Verify(s => s.GetLoansAsync(null, null, 1, int.MaxValue), Times.Once);
        }

        [Fact]
        public async Task Handle_DeberiaDevolverListaVaciaSiNoHayPrestamos()
        {
            _serviceMock
                .Setup(s => s.GetLoansAsync(null, null, 1, int.MaxValue))
                .ReturnsAsync(new PagedResult<LoanResponseDto> { Items = new List<LoanResponseDto>() });

            var handler = new GetAllLoansQueryHandler(_serviceMock.Object);

            var result = await handler.Handle(new GetAllLoansQuery(), CancellationToken.None);

            Assert.Empty(result);
        }
    }
}
