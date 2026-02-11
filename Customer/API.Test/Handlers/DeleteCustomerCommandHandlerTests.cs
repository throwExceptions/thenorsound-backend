using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class DeleteCustomerCommandHandlerTests
{
    private readonly Mock<ICustomerRepository> _repoMock;
    private readonly DeleteCustomerCommandHandler _handler;

    public DeleteCustomerCommandHandlerTests()
    {
        _repoMock = new Mock<ICustomerRepository>();
        _handler = new DeleteCustomerCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CustomerNotFound()
    {
        var command = new DeleteCustomerCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((Customer?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnTrue_When_CustomerExists()
    {
        var command = new DeleteCustomerCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(TestDataFactory.ValidCustomer());
        _repoMock.Setup(r => r.DeleteAsync(command.Id))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_CallDeleteWithCorrectId_When_CustomerExists()
    {
        var command = new DeleteCustomerCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(TestDataFactory.ValidCustomer());
        _repoMock.Setup(r => r.DeleteAsync(command.Id))
            .ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.DeleteAsync(TestDataFactory.ValidMongoId), Times.Once);
    }
}
