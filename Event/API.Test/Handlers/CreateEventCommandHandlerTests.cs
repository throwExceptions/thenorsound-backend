using API.Test.Helpers;
using Application.Commands;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

[Collection("Mapster")]
public class CreateEventCommandHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _handler = new CreateEventCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnCreatedEvent_When_CommandIsValid()
    {
        var command = new CreateEventCommand
        {
            CustomerId = TestDataFactory.ValidCustomerId,
            Project = "Test Event",
            Start = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc),
            ProjectNumber = "PRJ-001",
            Slots = new List<Slot>(),
        };

        var expected = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Event>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectData_When_ValidCommand()
    {
        var command = new CreateEventCommand
        {
            CustomerId = TestDataFactory.ValidCustomerId,
            Project = "Test Event",
            Start = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc),
            ProjectNumber = "PRJ-001",
            Slots = new List<Slot>(),
        };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Event>()))
            .ReturnsAsync(TestDataFactory.ValidEvent());

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.CreateAsync(
            It.Is<Event>(e => e.Project == "Test Event" && e.CustomerId == TestDataFactory.ValidCustomerId)),
            Times.Once);
    }
}
