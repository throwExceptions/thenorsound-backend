using API.Test.Helpers;
using Application.Exceptions;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetEventQueryHandlerTests
{
    private readonly Mock<IEventRepository> _repoMock;
    private readonly GetEventQueryHandler _handler;

    public GetEventQueryHandlerTests()
    {
        _repoMock = new Mock<IEventRepository>();
        _handler = new GetEventQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnEvent_When_EventExists()
    {
        var expected = TestDataFactory.ValidEvent();
        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(new GetEventQuery { Id = TestDataFactory.ValidMongoId }, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_EventNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Event?)null);

        var act = () => _handler.Handle(new GetEventQuery { Id = "nonexistent" }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
