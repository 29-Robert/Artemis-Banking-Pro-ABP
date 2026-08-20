using ArtemisBankingPro.Application.DTOs.Account;
using ArtemisBankingPro.Application.Features.Accounts.Queries;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Accounts.Handlers
{
    public class GetSavingsAccountsQueryHandlerTests
    {
        private readonly Mock<ISavingsAccountRepository> _accountRepositoryMock;
        private readonly GetAccountsQueryHandler _handler;

        public GetSavingsAccountsQueryHandlerTests()
        {
            _accountRepositoryMock = new Mock<ISavingsAccountRepository>();
            _handler = new GetAccountsQueryHandler(_accountRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_ShouldReturnPagedAccountsAndMetadata()
        {
            var query = new GetAccountsQuery
            {
                Page = 1,
                PageSize = 10,
                Status = AccountStatus.Activa,
                Type = AccountType.Principal,
                Cedula = "101001018"
            };

            var expectedData = new List<SavingsAccount>
            {
                new SavingsAccount { AccountNumber = "123456789", Balance = 1000m, Status = AccountStatus.Activa, Type = AccountType.Principal }
            };

            var expectedResponse = new PagedAccountResponseDto
            {
                Data = expectedData,
                TotalCount = 1
            };

            _accountRepositoryMock.Setup(r => r.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Status,
                query.Type,
                query.Cedula))
                .ReturnsAsync(expectedResponse);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1);
            result.Data.Should().HaveCount(1);
            _accountRepositoryMock.Verify(r => r.GetPagedAsync(1, 10, AccountStatus.Activa, AccountType.Principal, "101001018"), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ShouldReturnEmptyDataAndZeroCount()
        {
            var query = new GetAccountsQuery
            {
                Page = 2,
                PageSize = 20,
                Status = AccountStatus.Cancelada,
                Type = AccountType.Secundaria,
                Cedula = "000000000"
            };

            var expectedResponse = new PagedAccountResponseDto
            {
                Data = new List<SavingsAccount>(),
                TotalCount = 0
            };

            _accountRepositoryMock.Setup(r => r.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Status,
                query.Type,
                query.Cedula))
                .ReturnsAsync(expectedResponse);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            result.Data.Should().BeEmpty();
            _accountRepositoryMock.Verify(r => r.GetPagedAsync(2, 20, AccountStatus.Cancelada, AccountType.Secundaria, "000000000"), Times.Once);
        }
    }
}
