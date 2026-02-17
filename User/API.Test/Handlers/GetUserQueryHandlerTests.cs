using API.Test.Helpers;
using Application.Exceptions;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new GetUserQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnUser_When_UserExists()
    {
        var query = new GetUserQuery { Id = TestDataFactory.ValidMongoId };
        var expected = TestDataFactory.ValidUser();

        _repoMock.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserNotFound()
    {
        var query = new GetUserQuery { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync((User?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectId_When_Invoked()
    {
        var query = new GetUserQuery { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync(TestDataFactory.ValidUser());

        await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetByIdAsync(TestDataFactory.ValidMongoId), Times.Once);
    }
}
