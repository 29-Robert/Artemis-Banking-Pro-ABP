using ArtemisBankingPro.Application.DTOs.Beneficiaries;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Enums;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Services
{
    public class BeneficiaryServiceTests
    {
        private readonly Mock<IBeneficiaryRepository> _mockBeneficiaryRepo;
        private readonly Mock<ISavingsAccountRepository> _mockAccountRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly BeneficiaryService _service;

        public BeneficiaryServiceTests()
        {
            _mockBeneficiaryRepo = new Mock<IBeneficiaryRepository>();
            _mockAccountRepo = new Mock<ISavingsAccountRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IUserRepository>();

            _service = new BeneficiaryService(
                _mockBeneficiaryRepo.Object,
                _mockAccountRepo.Object,
                _mockTransactionRepo.Object,
                _mockEmailService.Object,
                _mockUnitOfWork.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task AddBeneficiaryAsync_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            int clientId = 1;
            var dto = new CreateBeneficiaryDto
            {
                AccountNumber = "987654321",
                Alias = "My Friend"
            };

            var targetAccount = new SavingsAccount
            {
                Id = 2,
                AccountNumber = "987654321",
                Status = AccountStatus.Activa,
                UserId = 2,
                User = new User { FirstName = "Friend", LastName = "User" }
            };

            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("987654321")).ReturnsAsync(targetAccount);
            _mockBeneficiaryRepo.Setup(r => r.GetByClientAsync(clientId)).ReturnsAsync(new List<Beneficiary>());
            _mockBeneficiaryRepo.Setup(r => r.AddAsync(It.IsAny<Beneficiary>()))
                .ReturnsAsync((Beneficiary b) => b);

            // Act
            var result = await _service.AddBeneficiaryAsync(clientId, dto);

            // Assert
            result.Should().NotBeNull();
            result.AccountNumber.Should().Be("987654321");
            result.Alias.Should().Be("My Friend");

            _mockBeneficiaryRepo.Verify(r => r.AddAsync(It.IsAny<Beneficiary>()), Times.Once);
            _mockBeneficiaryRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddBeneficiaryAsync_ShouldThrowException_WhenAddingSelf()
        {
            // Arrange
            int clientId = 1;
            var dto = new CreateBeneficiaryDto
            {
                AccountNumber = "111111111",
                Alias = "My Self"
            };

            var targetAccount = new SavingsAccount
            {
                Id = 1,
                AccountNumber = "111111111",
                Status = AccountStatus.Activa,
                UserId = 1 // Same UserId as clientId
            };

            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("111111111")).ReturnsAsync(targetAccount);

            // Act
            Func<Task> act = async () => await _service.AddBeneficiaryAsync(clientId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("No puedes agregarte a ti mismo como beneficiario.");

            _mockBeneficiaryRepo.Verify(r => r.AddAsync(It.IsAny<Beneficiary>()), Times.Never);
        }

        [Fact]
        public async Task AddBeneficiaryAsync_ShouldThrowException_WhenTargetAccountIsCancelled()
        {
            // Arrange
            int clientId = 1;
            var dto = new CreateBeneficiaryDto
            {
                AccountNumber = "987654321",
                Alias = "My Friend"
            };

            var targetAccount = new SavingsAccount
            {
                Id = 2,
                AccountNumber = "987654321",
                Status = AccountStatus.Cancelada, // Cancelada
                UserId = 2
            };

            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("987654321")).ReturnsAsync(targetAccount);

            // Act
            Func<Task> act = async () => await _service.AddBeneficiaryAsync(clientId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("La cuenta del beneficiario se encuentra cancelada.");
        }

        [Fact]
        public async Task AddBeneficiaryAsync_ShouldThrowException_WhenAlreadyExists()
        {
            // Arrange
            int clientId = 1;
            var dto = new CreateBeneficiaryDto
            {
                AccountNumber = "987654321",
                Alias = "My Friend"
            };

            var targetAccount = new SavingsAccount
            {
                Id = 2,
                AccountNumber = "987654321",
                Status = AccountStatus.Activa,
                UserId = 2
            };

            var existingBeneficiaries = new List<Beneficiary>
            {
                new Beneficiary { BeneficiaryAccountNumber = "987654321", ClientId = clientId }
            };

            _mockAccountRepo.Setup(r => r.GetByAccountNumberAsync("987654321")).ReturnsAsync(targetAccount);
            _mockBeneficiaryRepo.Setup(r => r.GetByClientAsync(clientId)).ReturnsAsync(existingBeneficiaries);

            // Act
            Func<Task> act = async () => await _service.AddBeneficiaryAsync(clientId, dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Ya tienes a este beneficiario registrado.");
        }
    }
}
