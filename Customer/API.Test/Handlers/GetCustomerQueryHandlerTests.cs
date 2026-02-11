using API.Test.Helpers;
using Application.Exceptions;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetCustomerQueryHandlerTests
{
    private readonly Mock<ICustomerRepository> _repoMock;
    private readonly GetCustomerQueryHandler _handler;

    public GetCustomerQueryHandlerTests()
    {
        _repoMock = new Mock<ICustomerRepository>();
        _handler = new GetCustomerQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnCustomer_When_CustomerExists()
    {
        var query = new GetCustomerQuery { Id = TestDataFactory.ValidMongoId };
        var expected = TestDataFactory.ValidCustomer();

        _repoMock.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CustomerNotFound()
    {
        var query = new GetCustomerQuery { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync((Customer?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectId_When_Invoked()
    {
        var query = new GetCustomerQuery { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync(TestDataFactory.ValidCustomer());

        await _handler.Handle(query, CancellationToken.None);

        _repoMock.Verify(r => r.GetByIdAsync(TestDataFactory.ValidMongoId), Times.Once);
    }
}
