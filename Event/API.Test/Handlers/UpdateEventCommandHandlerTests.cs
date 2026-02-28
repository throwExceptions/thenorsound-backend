using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class UpdateEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly UpdateEventCommandHandler _handler;

    public UpdateEventCommandHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _handler = new UpdateEventCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnEvent_When_EventUpdated()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(ev);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Event>()))
            .ReturnsAsync(true);

        var command = new UpdateEventCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Project = "Updated Event",
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(TestDataFactory.ValidMongoId);
        result.Project.Should().Be("Updated Event");
        _repoMock.Verify(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_EventNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Event?)null);

        var command = new UpdateEventCommand { Id = "nonexistent", Project = "Updated" };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_UpdateProjectName_When_ProjectProvided()
    {
        var ev = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(ev);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<Event>()))
            .ReturnsAsync(true);

        var command = new UpdateEventCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Project = "New Project Name",
        };

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(
            TestDataFactory.ValidMongoId,
            It.Is<Event>(e => e.Project == "New Project Name")),
            Times.Once);
    }
}
