using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private DbContextOptions<ApplicationDbContext> _options;

        public UserRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddAsync_AddsUserToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();
            var repository = new UserRepository(context);
            var user = new User { FirstName = "John", LastName = "Doe", Username = "jdoe", Email = "jdoe@test.com", Cedula = "123", IsActive = true, RoleId = 1 };

            // Act
            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            // Assert
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "jdoe");
            Assert.NotNull(savedUser);
            Assert.Equal("John", savedUser.FirstName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();
            var user = new User { FirstName = "Jane", LastName = "Doe", Username = "janed", Email = "jane@test.com", Cedula = "456", IsActive = true, RoleId = 1 };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Act
            var retrievedUser = await repository.GetByIdAsync(user.Id);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("Jane", retrievedUser.FirstName);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUserProperties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();
            var user = new User { FirstName = "Mark", LastName = "Smith", Username = "marks", Email = "mark@test.com", Cedula = "789", IsActive = true, RoleId = 1 };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Act
            user.FirstName = "Marcus";
            user.IsActive = false;
            await repository.UpdateAsync(user);
            await context.SaveChangesAsync();

            // Assert
            var updatedUser = await context.Users.FindAsync(user.Id);
            Assert.Equal("Marcus", updatedUser.FirstName);
            Assert.False(updatedUser.IsActive);
        }
    }
}
