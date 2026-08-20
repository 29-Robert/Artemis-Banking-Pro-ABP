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
    public class ChangeCommerceStatusCommandHandlerTests
    {
        private readonly Mock<ICommerceService> _commerceServiceMock;
        private readonly ChangeCommerceStatusCommandHandler _handler;

        public ChangeCommerceStatusCommandHandlerTests()
        {
            _commerceServiceMock = new Mock<ICommerceService>();
            _handler = new ChangeCommerceStatusCommandHandler(_commerceServiceMock.Object);
        }

        [Fact]
        public async Task Handle_Deactivation_ShouldPassIsActiveFalseToService()
        {
            var command = new ChangeCommerceStatusCommand { Id = 1, IsActive = false };

            _commerceServiceMock.Setup(s => s.ChangeStatusAsync(command.Id, command.IsActive))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
            _commerceServiceMock.Verify(s => s.ChangeStatusAsync(command.Id, false), Times.Once);
        }

        [Fact]
        public async Task Handle_Reactivation_ShouldPassIsActiveTrueToService()
            {
            var command = new ChangeCommerceStatusCommand { Id = 1, IsActive = true };

            _commerceServiceMock.Setup(s => s.ChangeStatusAsync(command.Id, command.IsActive))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
            _commerceServiceMock.Verify(s => s.ChangeStatusAsync(command.Id, true), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentCommerceId_ShouldPropagateKeyNotFoundException()
        {
            var command = new ChangeCommerceStatusCommand { Id = 999, IsActive = true };

            _commerceServiceMock.Setup(s => s.ChangeStatusAsync(command.Id, command.IsActive))
                .ThrowsAsync(new KeyNotFoundException($"No se encontró el comercio con el ID {command.Id}."));

            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
            
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"No se encontró el comercio con el ID {command.Id}.");
        }
    }
}
