using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBankingPro.Application.Common;
using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Features.CreditCard.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

    namespace ArtemisBankingPro.Tests.Features.Creditcard.Queries
    {
        public class CreditCardQueryHandlersTests
        {
            private readonly Mock<ICreditCardService> _mockCreditCardService;

            public CreditCardQueryHandlersTests()
            {
                _mockCreditCardService = new Mock<ICreditCardService>();
            }

            [Fact]
            public async Task GetAllCreditCardsQueryHandler_ShouldReturnPagedResult()
            {
                // Arrange
                var expectedResult = new PagedResult<CreditCardResponseDto>();
                _mockCreditCardService
                    .Setup(s => s.GetCreditCardsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(expectedResult);

                var handler = new GetAllCreditCardsQueryHandler(_mockCreditCardService.Object);
                var query = new GetAllCreditCardsQuery { Cedula = "123", Status = "Active", PageNumber = 1, PageSize = 10 };

                // Act
                var result = await handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                // Si decides usar FluentAssertions: result.Should().BeEquivalentTo(expectedResult);
                _mockCreditCardService.Verify(s => s.GetCreditCardsAsync(query.Cedula, query.Status, query.PageNumber, query.PageSize), Times.Once);
            }

            [Fact]
            public async Task GetCreditCardByIdQueryHandler_ShouldReturnCreditCard()
            {
                // Arrange
                var expectedCard = new CreditCardResponseDto { Id = 1 };
                _mockCreditCardService
                    .Setup(s => s.GetCreditCardByIdAsync(1))
                    .ReturnsAsync(expectedCard);

                var handler = new GetCreditCardByIdQueryHandler(_mockCreditCardService.Object);
                var query = new GetCreditCardByIdQuery { Id = 1 };

                // Act
                var result = await handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, result.Id);
                _mockCreditCardService.Verify(s => s.GetCreditCardByIdAsync(query.Id), Times.Once);
            }

            [Fact]
            public async Task GetEligibleCreditCardClientsQueryHandler_ShouldReturnEligibleClients()
            {
                // Arrange
                var expectedClients = new EligibleClientsResponseDto();
                _mockCreditCardService
                    .Setup(s => s.GetEligibleClientsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                    .ReturnsAsync(expectedClients);

                var handler = new GetEligibleCreditCardClientsQueryHandler(_mockCreditCardService.Object);
                var query = new GetEligibleCreditCardClientsQuery { Cedula = "001", PageNumber = 1, PageSize = 10 };

                // Act
                var result = await handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                _mockCreditCardService.Verify(s => s.GetEligibleClientsAsync(query.Cedula, query.PageNumber, query.PageSize), Times.Once);
            }
        }
    }

