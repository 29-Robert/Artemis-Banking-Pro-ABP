using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Repositories
{
    public class RoleRepositoryTests : IntegrationTestBase
    {
        private readonly GenericRepository<Role> _repository;

        public RoleRepositoryTests()
        {
            _repository = new GenericRepository<Role>(DbContext);
        }

        [Theory]
        [InlineData(1, "Administrador")]
        [InlineData(2, "Cajero")]
        [InlineData(3, "Cliente")]
        [InlineData(4, "Comercio")]
        public async Task GetByIdAsync_ShouldReturnSeededRoles(int id, string expectedName)
        {
            var role = await _repository.GetByIdAsync(id);

            Assert.NotNull(role);
            Assert.Equal(expectedName, role.Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSeededRoles()
        {
            var roles = await _repository.GetAllAsync();

            Assert.NotNull(roles);
            Assert.Equal(4, roles.Count);
            Assert.Contains(roles, r => r.Name == "Administrador");
            Assert.Contains(roles, r => r.Name == "Cajero");
            Assert.Contains(roles, r => r.Name == "Cliente");
            Assert.Contains(roles, r => r.Name == "Comercio");
        }

        [Fact]
        public async Task GetByName_UsingDbContext_ShouldReturnCorrectRole()
        {
            var role = await DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Cajero");

            Assert.NotNull(role);
            Assert.Equal(2, role.Id);
        }

        [Fact]
        public async Task AddUser_WithInvalidRoleId_ShouldThrowDbUpdateException()
        {
            var invalidUser = new User
            {
                FirstName = "Test",
                LastName = "User",
                Username = "test_user_invalid_role",
                Email = "test_invalid_role@artemis.com",
                Cedula = "40299999999",
                PasswordHash = "hash",
                RoleId = 99,
                IsActive = true
            };

            DbContext.Users.Add(invalidUser);

            await Assert.ThrowsAsync<DbUpdateException>(async () => await DbContext.SaveChangesAsync());
        }
    }
}
