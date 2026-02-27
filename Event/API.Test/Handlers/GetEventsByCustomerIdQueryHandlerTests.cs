using API.Test.Helpers;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetEventsByCustomerIdQueryHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly GetEventsByCustomerIdQueryHandler _handler;

    public GetEventsByCustomerIdQueryHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _handler = new GetEventsByCustomerIdQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnEvents_When_CustomerHasEvents()
    {
        var customerId = TestDataFactory.ValidCustomerId;
        var expected = new List<Event> { TestDataFactory.ValidEvent() };

        _repoMock.Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(
            new GetEventsByCustomerIdQuery { CustomerId = customerId },
            CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetByCustomerIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_CustomerHasNoEvents()
    {
        var customerId = TestDataFactory.ValidCustomerId;

        _repoMock.Setup(r => r.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<Event>());

        var result = await _handler.Handle(
            new GetEventsByCustomerIdQuery { CustomerId = customerId },
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}
