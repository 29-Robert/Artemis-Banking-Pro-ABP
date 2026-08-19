using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.Interfaces.Repositories;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Services
{
    public class CreditCardServiceTests
    {
        private readonly Mock<ICreditCardRepository> _mockCreditCardRepository;
        private readonly Mock<IGenericRepository<User>> _mockUserRepository;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILoanRepository> _mockLoanRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CreditCardService _creditCardService;

        public CreditCardServiceTests()
        {
            _mockCreditCardRepository = new Mock<ICreditCardRepository>();
            _mockUserRepository = new Mock<IGenericRepository<User>>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLoanRepository = new Mock<ILoanRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _creditCardService = new CreditCardService(
                _mockCreditCardRepository.Object,
                _mockUserRepository.Object,
                 _mockLoanRepository.Object,
                _mockEmailService.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                Mock.Of<ILogger<CreditCardService>>());
        }

        #region GetCreditCardByIdAsync Tests

        [Fact]
        public async Task GetCreditCardByIdAsync_ShouldReturnCard_WhenCardExists()
        {
            // Arrange
            var cardId = 1;
            var creditCard = new CreditCard { Id = cardId };
            var responseDto = new CreditCardResponseDto { Id = cardId, Consumptions = new List<CreditCardConsumptionDto>() };

            _mockCreditCardRepository.Setup(r => r.GetByIdWithDetailsAsync(cardId))
                .ReturnsAsync(creditCard);
            _mockMapper.Setup(m => m.Map<CreditCardResponseDto>(creditCard))
                .Returns(responseDto);

            // Act
            var result = await _creditCardService.GetCreditCardByIdAsync(cardId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cardId, result.Id);
        }

        [Fact]
        public async Task GetCreditCardByIdAsync_ShouldThrowKeyNotFoundException_WhenCardDoesNotExist()
        {
            // Arrange
            _mockCreditCardRepository.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((CreditCard)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _creditCardService.GetCreditCardByIdAsync(1));

            Assert.Equal("La tarjeta seleccionada no existe.", exception.Message);
        }

        #endregion

        #region AssignCreditCardAsync Tests

        [Fact]
        public async Task AssignCreditCardAsync_ShouldAssignCard_WhenDataIsValid()
        {
            // Arrange
            var request = new CreateCreditCardRequestDto { ClientId = "1", CreditLimit = 50000 };
            var client = new User { Id = 1, IsActive = true, Email = "test@test.com" };
            var cardResponse = new CreditCardCreatedResponseDto { CreditLimit = 50000 };

            _mockUserRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);
            _mockCreditCardRepository.Setup(r => r.GetByCardNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((CreditCard)null); // Simula que el número generado es único

            _mockMapper.Setup(m => m.Map<CreditCardCreatedResponseDto>(It.IsAny<CreditCard>()))
                .Returns(cardResponse);

            // Act
            var result = await _creditCardService.AssignCreditCardAsync(request, adminId: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50000m, result.CreditLimit); // 'm' indica que es decimal
            _mockCreditCardRepository.Verify(r => r.AddAsync(It.IsAny<CreditCard>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockEmailService.Verify(e => e.SendNotificationEmailAsync(client.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AssignCreditCardAsync_ShouldThrowInvalidOperationException_WhenClientIsInactive()
        {
            // Arrange
            var request = new CreateCreditCardRequestDto { ClientId = "1", CreditLimit = 50000 };
            var client = new User { Id = 1, IsActive = false }; // Inactivo

            _mockUserRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _creditCardService.AssignCreditCardAsync(request, 2));

            Assert.Equal("Solo se puede asignar tarjetas de crédito a clientes activos.", exception.Message);
        }

        [Fact]
        public async Task AssignCreditCardAsync_ShouldThrowArgumentException_WhenLimitIsZeroOrLess()
        {
            // Arrange
            var request = new CreateCreditCardRequestDto { ClientId = "1", CreditLimit = 0 }; // Límite inválido
            var client = new User { Id = 1, IsActive = true };

            _mockUserRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(client);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _creditCardService.AssignCreditCardAsync(request, 2));

            Assert.Equal("El límite de crédito debe ser mayor que cero.", exception.Message);
        }

        #endregion

        #region UpdateCreditLimitAsync Tests

        [Fact]
        public async Task UpdateCreditLimitAsync_ShouldUpdateLimit_WhenValid()
        {
            // Arrange
            var card = new CreditCard { Id = 1, Status = "Activa", CurrentDebt = 10000, Client = new User { Email = "test@test.com" } };
            var newLimit = 50000m;
            var responseDto = new CreditCardResponseDto { CreditLimit = newLimit };

            _mockCreditCardRepository.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(card);
            _mockMapper.Setup(m => m.Map<CreditCardResponseDto>(card)).Returns(responseDto);

            // Act
            var result = await _creditCardService.UpdateCreditLimitAsync(1, newLimit);

            // Assert
            Assert.Equal(newLimit, result.CreditLimit);
            _mockCreditCardRepository.Verify(r => r.UpdateAsync(card), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCreditLimitAsync_ShouldThrowInvalidOperationException_WhenNewLimitIsLowerThanDebt()
        {
            // Arrange
            var card = new CreditCard { Id = 1, Status = "Activa", CurrentDebt = 20000 };
            var newLimit = 15000m; // Menor que la deuda

            _mockCreditCardRepository.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(card);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _creditCardService.UpdateCreditLimitAsync(1, newLimit));

            Assert.Equal("El límite de la tarjeta no puede ser inferior al monto adeudado actualmente.", exception.Message);
        }

        #endregion

        #region CancelCreditCardAsync Tests

        [Fact]
        public async Task CancelCreditCardAsync_ShouldCancelCard_WhenDebtIsZero()
        {
            // Arrange
            var card = new CreditCard { Id = 1, Status = "Activa", CurrentDebt = 0 };
            _mockCreditCardRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(card);

            // Act
            await _creditCardService.CancelCreditCardAsync(1);

            // Assert
            Assert.Equal("Cancelada", card.Status);
            _mockCreditCardRepository.Verify(r => r.UpdateAsync(card), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelCreditCardAsync_ShouldThrowInvalidOperationException_WhenCardHasDebt()
        {
            // Arrange
            var card = new CreditCard { Id = 1, Status = "Activa", CurrentDebt = 500 }; // Tiene deuda pendiente
            _mockCreditCardRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(card);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _creditCardService.CancelCreditCardAsync(1));

            Assert.Equal("Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.", exception.Message);
        }

        #endregion
    }
}