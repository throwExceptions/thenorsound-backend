using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class DeleteEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly DeleteEventCommandHandler _handler;

    public DeleteEventCommandHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _handler = new DeleteEventCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnTrue_When_EventDeleted()
    {
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(TestDataFactory.ValidEvent());
        _repoMock.Setup(r => r.DeleteAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(true);

        var result = await _handler.Handle(
            new DeleteEventCommand { Id = TestDataFactory.ValidMongoId },
            CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(TestDataFactory.ValidMongoId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_EventNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Event?)null);

        var act = () => _handler.Handle(
            new DeleteEventCommand { Id = "nonexistent" },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
