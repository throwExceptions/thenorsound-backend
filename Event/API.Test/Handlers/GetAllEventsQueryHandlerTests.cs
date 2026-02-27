using API.Test.Helpers;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetAllEventsQueryHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly GetAllEventsQueryHandler _handler;

    public GetAllEventsQueryHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _handler = new GetAllEventsQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnAllEvents_When_NoFilterProvided()
    {
        var query = new GetAllEventsQuery { CustomerId = null };
        var expected = new List<Event> { TestDataFactory.ValidEvent() };

        _repoMock.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFilteredEvents_When_CustomerIdProvided()
    {
        var customerId = TestDataFactory.ValidCustomerId;
        var query = new GetAllEventsQuery { CustomerId = customerId };
        var expected = new List<Event> { TestDataFactory.ValidEvent() };

        _repoMock.Setup(r => r.GetAllAsync(customerId))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoEventsExist()
    {
        var query = new GetAllEventsQuery { CustomerId = null };

        _repoMock.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Event>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
