using API.Test.Helpers;
using Application.Exceptions;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly GetUserByEmailQueryHandler _handler;

    public GetUserByEmailQueryHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new GetUserByEmailQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnUser_When_UserExists()
    {
        var query = new GetUserByEmailQuery { Email = "test@example.com" };
        var expected = TestDataFactory.ValidUser();

        _repoMock.Setup(r => r.GetByEmailAsync(query.Email))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserNotFound()
    {
        var query = new GetUserByEmailQuery { Email = "missing@example.com" };

        _repoMock.Setup(r => r.GetByEmailAsync(query.Email))
            .ReturnsAsync((User?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectEmail_When_Invoked()
    {
        var query = new GetUserByEmailQuery { Email = "test@example.com" };

        _repoMock.Setup(r => r.GetByEmailAsync(query.Email))
            .ReturnsAsync(TestDataFactory.ValidUser());

        await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetByEmailAsync("test@example.com"), Times.Once);
    }
}
