using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Queries;
using ArtemisBankingPro.Application.Interfaces.Services;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Handlers
{
    public class GetPagedCommercesQueryHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly GetPagedCommercesQueryHandler _handler;

        public GetPagedCommercesQueryHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new GetPagedCommercesQueryHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsPagedCommerceResponseDto()
        {
            // Arrange
            var query = new GetPagedCommercesQuery { Page = 1, Limit = 10 };

            var expectedDto = new PagedCommerceResponseDto
            {
                CurrentPage = 1,
                TotalPages = 1,
                TotalCount = 1,
                Data = new List<CommerceDetailDto>
                {
                    new CommerceDetailDto
                    {
                        Id = 1,
                        BusinessName = "Test Commerce",
                        RNC = "123456789",
                        IsActive = true
                    }
                }
            };

            _commerceServiceMock.Setup(s => s.GetPagedCommercesAsync(query.Page, query.Limit))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.CurrentPage, result.CurrentPage);
            Assert.Equal(expectedDto.TotalCount, result.TotalCount);
            Assert.Single(result.Data);
            
            _commerceServiceMock.Verify(s => s.GetPagedCommercesAsync(query.Page, query.Limit), Times.Once);
        }
    }
}
