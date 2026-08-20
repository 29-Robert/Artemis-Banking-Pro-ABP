using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Handlers
{
    public class CreateCommerceCommandHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly CreateCommerceCommandHandler _handler;

        public CreateCommerceCommandHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new CreateCommerceCommandHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateCommerceAndReturnDto()
        {
            var command = new CreateCommerceCommand
            {
                BusinessName = "Supermercado XYZ",
                RNC = "131456789",
                Email = "contacto@xyz.com",
                Password = "SecurePassword123!",
                Phone = "8095550192",
                Address = "Av. Winston Churchill"
            };

            var expectedDto = new CommerceListItemDto
            {
                Id = 1,
                BusinessName = "Supermercado XYZ",
                RNC = "131456789",
                IsActive = true
            };

            _commerceServiceMock.Setup(s => s.CreateCommerceAsync(It.Is<CreateCommerceDto>(d =>
                d.BusinessName == command.BusinessName &&
                d.RNC == command.RNC &&
                d.Email == command.Email &&
                d.Password == command.Password &&
                d.Phone == command.Phone &&
                d.Address == command.Address)))
                .ReturnsAsync(expectedDto);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.BusinessName.Should().Be("Supermercado XYZ");
            result.IsActive.Should().BeTrue();
            _commerceServiceMock.Verify(s => s.CreateCommerceAsync(It.IsAny<CreateCommerceDto>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateRnc_ShouldThrowInvalidOperationException()
        {
            var command = new CreateCommerceCommand
            {
                BusinessName = "Supermercado XYZ",
                RNC = "131456789",
                Email = "contacto@xyz.com",
                Password = "SecurePassword123!",
                Phone = "8095550192",
                Address = "Av. Winston Churchill"
            };

            _commerceServiceMock.Setup(s => s.CreateCommerceAsync(It.IsAny<CreateCommerceDto>()))
                .ThrowsAsync(new InvalidOperationException("El RNC '131456789' ya está registrado."));

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El RNC '131456789' ya está registrado.");
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ShouldThrowInvalidOperationException()
        {
            var command = new CreateCommerceCommand
            {
                BusinessName = "Supermercado XYZ",
                RNC = "131456789",
                Email = "contacto@xyz.com",
                Password = "SecurePassword123!",
                Phone = "8095550192",
                Address = "Av. Winston Churchill"
            };

            _commerceServiceMock.Setup(s => s.CreateCommerceAsync(It.IsAny<CreateCommerceDto>()))
                .ThrowsAsync(new InvalidOperationException("El correo electrónico 'contacto@xyz.com' ya está registrado."));

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El correo electrónico 'contacto@xyz.com' ya está registrado.");
        }
    }
}
