using API.Test.Helpers;
using Application.Queries;
using Domain.Enums;
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
        var query = new GetAllUsersQuery { UserType = null };
        var expected = new List<User> { TestDataFactory.ValidUser() };

        _repoMock.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFilteredUsers_When_UserTypeProvided()
    {
        var query = new GetAllUsersQuery { UserType = CustomerType.Crew };
        var expected = new List<User> { TestDataFactory.ValidUser(CustomerType.Crew) };

        _repoMock.Setup(r => r.GetAllAsync((int)CustomerType.Crew))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync((int)CustomerType.Crew), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoUsersExist()
    {
        var query = new GetAllUsersQuery { UserType = null };

        _repoMock.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<User>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
