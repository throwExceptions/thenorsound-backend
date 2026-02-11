using API.Test.Helpers;
using Application.Queries;
using Domain.Enums;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetAllCustomersQueryHandlerTests
{
    private readonly Mock<ICustomerRepository> _repoMock;
    private readonly GetAllCustomersQueryHandler _handler;

    public GetAllCustomersQueryHandlerTests()
    {
        _repoMock = new Mock<ICustomerRepository>();
        _handler = new GetAllCustomersQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnAllCustomers_When_NoFilterProvided()
    {
        var query = new GetAllCustomersQuery { CustomerType = null };
        var expected = new List<Customer> { TestDataFactory.ValidCustomer() };

        _repoMock.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFilteredCustomers_When_CustomerTypeProvided()
    {
        var query = new GetAllCustomersQuery { CustomerType = CustomerType.Crew };
        var expected = new List<Customer> { TestDataFactory.ValidCustomer(CustomerType.Crew) };

        _repoMock.Setup(r => r.GetAllAsync((int)CustomerType.Crew))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        _repoMock.Verify(r => r.GetAllAsync((int)CustomerType.Crew), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_When_NoCustomersExist()
    {
        var query = new GetAllCustomersQuery { CustomerType = null };

        _repoMock.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<Customer>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
