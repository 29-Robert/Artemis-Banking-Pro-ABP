using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class ConfirmationTokenRepositoryTests
    {
        private DbContextOptions<ApplicationDbContext> _options;

        public ConfirmationTokenRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddAsync_AddsTokenToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new GenericRepository<ConfirmationToken>(context);
            
            var token = new ConfirmationToken 
            { 
                UserId = 1, 
                Token = "my-secret-token", 
                Type = TokenType.Activacion,
                ExpirationDate = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };

            // Act
            await repository.AddAsync(token);
            await context.SaveChangesAsync();

            // Assert
            var savedToken = await context.Set<ConfirmationToken>().FirstOrDefaultAsync(t => t.Token == "my-secret-token");
            Assert.NotNull(savedToken);
            Assert.Equal(TokenType.Activacion, savedToken.Type);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesTokenProperties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new GenericRepository<ConfirmationToken>(context);
            var token = new ConfirmationToken 
            { 
                UserId = 1, 
                Token = "token-to-update", 
                Type = TokenType.RestablecimientoContrasena,
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };
            
            context.Set<ConfirmationToken>().Add(token);
            await context.SaveChangesAsync();

            // Act
            token.IsUsed = true;
            await repository.UpdateAsync(token);
            await context.SaveChangesAsync();

            // Assert
            var updatedToken = await context.Set<ConfirmationToken>().FindAsync(token.Id);
            Assert.True(updatedToken.IsUsed);
        }
    }
}
