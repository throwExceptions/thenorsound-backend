using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new DeleteUserCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserNotFound()
    {
        var command = new DeleteUserCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnTrue_When_UserExists()
    {
        var command = new DeleteUserCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(TestDataFactory.ValidUser());
        _repoMock.Setup(r => r.DeleteAsync(command.Id))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_CallDeleteWithCorrectId_When_UserExists()
    {
        var command = new DeleteUserCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync(TestDataFactory.ValidUser());
        _repoMock.Setup(r => r.DeleteAsync(command.Id))
            .ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.DeleteAsync(TestDataFactory.ValidMongoId), Times.Once);
    }
}
