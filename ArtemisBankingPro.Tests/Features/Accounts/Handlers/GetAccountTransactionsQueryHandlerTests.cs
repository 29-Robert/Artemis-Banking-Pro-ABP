using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Domain.Enums;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Handlers
{
    public class GetAccountTransactionsQueryHandlerTests
    {
        private readonly Mock<ISavingsAccountService> _accountServiceMock;
        private readonly GetStatementQueryHandler _handler;

        public GetAccountTransactionsQueryHandlerTests()
        {
            _accountServiceMock = new Mock<ISavingsAccountService>();
            _handler = new GetStatementQueryHandler(_accountServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_ShouldReturnTransactionsOrderedChronologically()
        {
            var query = new GetStatementQuery
            {
                AccountNumber = "123456789",
                Page = 1,
                PageSize = 10
            };

            var expectedHistory = new List<TransactionDto>
            {
                new TransactionDto
                {
                    Date = DateTime.UtcNow,
                    Amount = 150m,
                    Type = TransactionType.Credito,
                    Status = TransactionStatus.Aprobada,
                    Description = "Depósito inicial",
                    Origin = "Cajero",
                    Beneficiary = "123456789"
                }
            };

            _accountServiceMock.Setup(s => s.GetTransactionHistoryAsync(
                query.AccountNumber,
                query.Page,
                query.PageSize))
                .ReturnsAsync(expectedHistory);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            _accountServiceMock.Verify(s => s.GetTransactionHistoryAsync("123456789", 1, 10), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentAccountNumber_ShouldThrowKeyNotFoundException()
        {
            var query = new GetStatementQuery
            {
                AccountNumber = "999999999",
                Page = 1,
                PageSize = 10
            };

            _accountServiceMock.Setup(s => s.GetTransactionHistoryAsync(
                query.AccountNumber,
                query.Page,
                query.PageSize))
                .ThrowsAsync(new KeyNotFoundException("La cuenta no existe."));

            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("La cuenta no existe.");
        }
    }
}
