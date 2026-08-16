using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Persistence.Contexts;
using ArtemisBankingPro.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests
{
    public class CommerceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private (CreateCommerceDtoValidator, UpdateCommerceDtoValidator) GetValidators()
        {
            return (new CreateCommerceDtoValidator(), new UpdateCommerceDtoValidator());
        }

        [Fact]
        public async Task CreateCommerce_WithValidData_ShouldSucceed()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var commerceRepo = new CommerceRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new GenericRepository<SavingsAccount>(db);
            var (createVal, updateVal) = GetValidators();

            var service = new CommerceService(commerceRepo, userRepo, accountRepo, createVal, updateVal);

            var dto = new CreateCommerceDto
            {
                BusinessName = "Comercio Test S.A.",
                RNC = "101001018",
                Email = "contacto@comerciotest.com",
                Password = "password123",
                Phone = "8095551234",
                Address = "Santo Domingo, RD"
            };

            // Act
            var result = await service.CreateCommerceAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Comercio Test S.A.", result.BusinessName);
            Assert.Equal("101001018", result.RNC);
            Assert.True(result.IsActive);

            // Verificar DB
            var dbCommerce = await db.Commerces.FirstOrDefaultAsync(c => c.Id == result.Id);
            Assert.NotNull(dbCommerce);
            Assert.Equal("contacto@comerciotest.com", dbCommerce.Email);

            var user = await db.Users.FirstOrDefaultAsync(u => u.CommerceId == result.Id);
            Assert.NotNull(user);
            Assert.Equal(4, user.RoleId);

            var account = await db.SavingsAccounts.FirstOrDefaultAsync(a => a.UserId == user.Id);
            Assert.NotNull(account);
            Assert.True(account.IsPrincipal);
        }

        [Fact]
        public async Task CreateCommerce_WithDuplicateRnc_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var existing = new Commerce
            {
                BusinessName = "Farmacia X",
                RNC = "101001018",
                Email = "farmaciax@mail.com",
                IsActive = true,
                PrincipalAccountNumber = "999888777"
            };
            db.Commerces.Add(existing);
            await db.SaveChangesAsync();

            var commerceRepo = new CommerceRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new GenericRepository<SavingsAccount>(db);
            var (createVal, updateVal) = GetValidators();

            var service = new CommerceService(commerceRepo, userRepo, accountRepo, createVal, updateVal);

            var dto = new CreateCommerceDto
            {
                BusinessName = "Farmacia Y",
                RNC = "101001018", // RNC Duplicado
                Email = "farmaciay@mail.com",
                Password = "password123",
                Phone = "8095551234",
                Address = "Santo Domingo, RD"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateCommerceAsync(dto));
        }

        [Fact]
        public async Task UpdateCommerce_ShouldPreserveIsActiveStatus()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var commerce = new Commerce
            {
                BusinessName = "Supermercado A",
                RNC = "101001018",
                Email = "contacto@supera.com",
                Phone = "8095551234",
                Address = "Santo Domingo, RD",
                IsActive = true,
                PrincipalAccountNumber = "111222333"
            };
            db.Commerces.Add(commerce);
            await db.SaveChangesAsync();

            var commerceRepo = new CommerceRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new GenericRepository<SavingsAccount>(db);
            var (createVal, updateVal) = GetValidators();

            var service = new CommerceService(commerceRepo, userRepo, accountRepo, createVal, updateVal);

            var updateDto = new UpdateCommerceDto
            {
                BusinessName = "Supermercado A Modificado",
                RNC = "101001018",
                Email = "contacto_mod@supera.com",
                Phone = "8097771234",
                Address = "Santiago, RD"
            };

            // Act
            await service.UpdateCommerceAsync(commerce.Id, updateDto);

            // Assert
            var updated = await db.Commerces.FindAsync(commerce.Id);
            Assert.NotNull(updated);
            Assert.True(updated.IsActive); // Debe preservarse intacto
            Assert.Equal("Supermercado A Modificado", updated.BusinessName);
        }

        [Fact]
        public async Task DeactivateCommerce_ShouldInactivateAssociatedUser()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var commerce = new Commerce
            {
                BusinessName = "Comercio A",
                RNC = "101001018",
                Email = "contacto@comercio.com",
                Phone = "8095551234",
                Address = "SD",
                IsActive = true,
                PrincipalAccountNumber = "111222333"
            };
            db.Commerces.Add(commerce);
            await db.SaveChangesAsync();

            var user = new User
            {
                FirstName = "Comercio",
                LastName = "Comercio A",
                Username = "101001018",
                Email = "contacto@comercio.com",
                Cedula = "COM-101001018",
                RoleId = 4,
                IsActive = true,
                CommerceId = commerce.Id
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var commerceRepo = new CommerceRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new GenericRepository<SavingsAccount>(db);
            var (createVal, updateVal) = GetValidators();

            var service = new CommerceService(commerceRepo, userRepo, accountRepo, createVal, updateVal);

            // Act
            await service.ChangeStatusAsync(commerce.Id, false);

            // Assert
            var dbCommerce = await db.Commerces.FindAsync(commerce.Id);
            Assert.False(dbCommerce.IsActive);

            var dbUser = await db.Users.FindAsync(user.Id);
            Assert.False(dbUser.IsActive); // User must be deactivated
        }

        [Fact]
        public async Task ReactivateCommerce_ShouldNotActivateAssociatedUser()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var commerce = new Commerce
            {
                BusinessName = "Comercio B",
                RNC = "202002028",
                Email = "contacto@comerciob.com",
                Phone = "8095551234",
                Address = "SD",
                IsActive = false, // Inicialmente inactivo
                PrincipalAccountNumber = "111222334"
            };
            db.Commerces.Add(commerce);
            await db.SaveChangesAsync();

            var user = new User
            {
                FirstName = "Comercio",
                LastName = "Comercio B",
                Username = "202002028",
                Email = "contacto@comerciob.com",
                Cedula = "COM-202002028",
                RoleId = 4,
                IsActive = false, // Inicialmente inactivo
                CommerceId = commerce.Id
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var commerceRepo = new CommerceRepository(db);
            var userRepo = new GenericRepository<User>(db);
            var accountRepo = new GenericRepository<SavingsAccount>(db);
            var (createVal, updateVal) = GetValidators();

            var service = new CommerceService(commerceRepo, userRepo, accountRepo, createVal, updateVal);

            // Act
            await service.ChangeStatusAsync(commerce.Id, true); // Reactivar comercio

            // Assert
            var dbCommerce = await db.Commerces.FindAsync(commerce.Id);
            Assert.True(dbCommerce.IsActive);

            var dbUser = await db.Users.FindAsync(user.Id);
            Assert.False(dbUser.IsActive); // User must REMAIN INACTIVE!
        }
    }
}