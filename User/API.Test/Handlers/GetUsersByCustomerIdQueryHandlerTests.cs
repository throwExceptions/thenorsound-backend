using API.Test.Helpers;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetUsersByCustomerIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly GetUsersByCustomerIdQueryHandler _handler;

    public GetUsersByCustomerIdQueryHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new GetUsersByCustomerIdQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnUsers_When_UsersExistForCustomer()
    {
        var query = new GetUsersByCustomerIdQuery { CustomerId = TestDataFactory.ValidMongoId2 };
        var expected = new List<User> { TestDataFactory.ValidUser() };

        _repoMock.Setup(r => r.GetByCustomerIdAsync(query.CustomerId, null))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetByCustomerIdAsync(TestDataFactory.ValidMongoId2, null), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoUsersForCustomer()
    {
        var query = new GetUsersByCustomerIdQuery { CustomerId = TestDataFactory.ValidMongoId2 };

        _repoMock.Setup(r => r.GetByCustomerIdAsync(query.CustomerId, null))
            .ReturnsAsync(new List<User>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectCustomerId_When_Invoked()
    {
        var query = new GetUsersByCustomerIdQuery { CustomerId = TestDataFactory.ValidMongoId2 };

        _repoMock.Setup(r => r.GetByCustomerIdAsync(query.CustomerId, null))
            .ReturnsAsync(new List<User>());

        await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetByCustomerIdAsync(TestDataFactory.ValidMongoId2, null), Times.Once);
    }
}
