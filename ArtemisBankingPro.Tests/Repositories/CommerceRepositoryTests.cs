using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class CommerceRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public CommerceRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private Commerce CreateTestCommerce(string rnc = "123456789", string email = "commerce@test.com", string businessName = "Test Business")
        {
            return new Commerce
            {
                BusinessName = businessName,
                RNC = rnc,
                Email = email,
                Phone = "8091234567",
                Address = "Test Address",
                IsActive = true,
                PrincipalAccountNumber = "100200300"
            };
        }

        [Fact]
        public async Task GetByRncAsync_ReturnsCommerce_WhenRncExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var commerce = CreateTestCommerce(rnc: "987654321");
            context.Commerces.Add(commerce);
            await context.SaveChangesAsync();

            var repository = new CommerceRepository(context);

            // Act
            var result = await repository.GetByRncAsync("987654321");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("987654321", result.RNC);
            Assert.Equal("Test Business", result.BusinessName);
        }

        [Fact]
        public async Task GetByRncAsync_ReturnsNull_WhenRncDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new CommerceRepository(context);

            // Act
            var result = await repository.GetByRncAsync("000000000");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsCommerce_WhenEmailExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var commerce = CreateTestCommerce(email: "unique@shop.com");
            context.Commerces.Add(commerce);
            await context.SaveChangesAsync();

            var repository = new CommerceRepository(context);

            // Act
            var result = await repository.GetByEmailAsync("unique@shop.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("unique@shop.com", result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new CommerceRepository(context);

            // Act
            var result = await repository.GetByEmailAsync("nonexistent@shop.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdWithUserAsync_ReturnsCommerceWithUser_WhenExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var role = new Role { Name = "Comercio" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var user = new User
            {
                FirstName = "Commerce",
                LastName = "User",
                Username = "commerceuser",
                Email = "cuser@test.com",
                PasswordHash = "hashedpassword",
                Cedula = "40212345678",
                PhoneNumber = "8091234567",
                RoleId = role.Id,
                IsActive = true
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var commerce = CreateTestCommerce(rnc: "111222333", email: "withuser@shop.com");
            commerce.User = user;
            context.Commerces.Add(commerce);
            await context.SaveChangesAsync();

            var repository = new CommerceRepository(context);

            // Act
            var result = await repository.GetByIdWithUserAsync(commerce.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Equal("commerceuser", result.User.Username);
            Assert.Equal("111222333", result.RNC);
        }

        [Fact]
        public async Task GetByIdWithUserAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new CommerceRepository(context);

            // Act
            var result = await repository.GetByIdWithUserAsync(9999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_AddsCommerceToDatabase()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new CommerceRepository(context);
            var commerce = CreateTestCommerce(rnc: "555666777", email: "new@shop.com", businessName: "New Business");

            // Act
            await repository.AddAsync(commerce);
            await context.SaveChangesAsync();

            // Assert
            var saved = await context.Commerces.FirstOrDefaultAsync(c => c.RNC == "555666777");
            Assert.NotNull(saved);
            Assert.Equal("New Business", saved.BusinessName);
            Assert.True(saved.IsActive);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesCommerceProperties()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var commerce = CreateTestCommerce(rnc: "888999000", email: "update@shop.com", businessName: "Original Name");
            context.Commerces.Add(commerce);
            await context.SaveChangesAsync();

            var repository = new CommerceRepository(context);

            // Act
            commerce.BusinessName = "Updated Name";
            commerce.IsActive = false;
            await repository.UpdateAsync(commerce);
            await context.SaveChangesAsync();

            // Assert
            var updated = await context.Commerces.FindAsync(commerce.Id);
            Assert.Equal("Updated Name", updated.BusinessName);
            Assert.False(updated.IsActive);
        }
    }
}
