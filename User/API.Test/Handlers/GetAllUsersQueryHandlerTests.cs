using API.Test.Helpers;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new GetAllUsersQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnAllUsers_When_NoFilterProvided()
    {
        var query = new GetAllUsersQuery();
        var expected = new List<User> { TestDataFactory.ValidUser() };

        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnCrewUsers_When_CrewUsersExist()
    {
        var query = new GetAllUsersQuery();
        var expected = new List<User> { TestDataFactory.ValidUser(isCrew: true) };

        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoUsersExist()
    {
        var query = new GetAllUsersQuery();

        _repoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
