using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.Application.Interfaces.Services;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArtemisBankingPro.Tests.Features.Commerces.Handlers
{
    public class UpdateCommerceCommandHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly UpdateCommerceCommandHandler _handler;

        public UpdateCommerceCommandHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new UpdateCommerceCommandHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateCommerceAndReturnUnit()
        {
            var command = new UpdateCommerceCommand
            {
                Id = 1,
                BusinessName = "Supermercado XYZ Renovado",
                RNC = "131456789",
                Email = "contacto@xyz.com",
                Phone = "8095550192",
                Address = "Av. Winston Churchill 200"
            };

            _commerceServiceMock.Setup(s => s.UpdateCommerceAsync(command.Id, It.Is<UpdateCommerceDto>(d =>
                d.BusinessName == command.BusinessName &&
                d.RNC == command.RNC &&
                d.Email == command.Email &&
                d.Phone == command.Phone &&
                d.Address == command.Address)))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
            _commerceServiceMock.Verify(s => s.UpdateCommerceAsync(command.Id, It.IsAny<UpdateCommerceDto>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentCommerceId_ShouldThrowKeyNotFoundException()
        {
            var command = new UpdateCommerceCommand
            {
                Id = 999,
                BusinessName = "Inexistente",
                RNC = "123",
                Email = "inexistente@xyz.com",
                Phone = "123",
                Address = "Desconocida"
            };

            _commerceServiceMock.Setup(s => s.UpdateCommerceAsync(command.Id, It.IsAny<UpdateCommerceDto>()))
                .ThrowsAsync(new KeyNotFoundException($"No se encontró el comercio con el ID {command.Id}."));

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"No se encontró el comercio con el ID {command.Id}.");
        }
    }
}
