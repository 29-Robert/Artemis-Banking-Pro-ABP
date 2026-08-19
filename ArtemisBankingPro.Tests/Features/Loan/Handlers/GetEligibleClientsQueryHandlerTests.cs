using ArtemisBankingPro.Application.Features.Loans.Queries;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Tests.Features.Loan.Handlers
{
    public class GetEligibleClientsQueryHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _mockUserRepository;
        private readonly GetEligibleClientsQueryHandler _handler;

        public GetEligibleClientsQueryHandlerTests()
        {
            // Arrange general
            _mockUserRepository = new Mock<IGenericRepository<User>>();
            _handler = new GetEligibleClientsQueryHandler(_mockUserRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredClients_WhenCedulaIsProvided()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "Juan", LastName = "Pérez", Cedula = "001-1234567-8" },
                new User { Id = 2, FirstName = "Liss", LastName = "Espiritu", Cedula = "402-9876543-2" }
            };

            _mockUserRepository.Setup(repo => repo.GetAllAsync())
                               .ReturnsAsync(users);

            var query = new GetEligibleClientsQuery
            {
                Cedula = "402", 
                PageNumber = 1,
                PageSize = 10
            };

            // Act: Ejecutamos el handler
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRecords); 
            Assert.Single(result.Data);
            Assert.Equal("402-9876543-2", result.Data.First().Cedula);
        }

        [Fact]
        public async Task Handle_ShouldApplyPaginationCorrectly()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "User1", Cedula = "111" },
                new User { Id = 2, FirstName = "User2", Cedula = "222" },
                new User { Id = 3, FirstName = "User3", Cedula = "333" }
            };

            _mockUserRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);

            var query = new GetEligibleClientsQuery
            {
                Cedula = string.Empty, 
                PageNumber = 2,        
                PageSize = 2           
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalRecords); 
            Assert.Equal(2, result.TotalPages);   
            Assert.Single(result.Data);           
            Assert.Equal("User3", result.Data.First().FirstName); 
        }
    }
}
