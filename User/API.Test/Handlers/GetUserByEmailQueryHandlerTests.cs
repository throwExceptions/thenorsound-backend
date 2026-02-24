using API.Test.Helpers;
using Application.Clients;
using Application.Clients.DTOs.Response;
using Application.Exceptions;
using Application.Queries;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<ICustomerClient> _customerClientMock;
    private readonly GetUserByEmailQueryHandler _handler;

    public GetUserByEmailQueryHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _customerClientMock = new Mock<ICustomerClient>();
        _handler = new GetUserByEmailQueryHandler(_repoMock.Object, _customerClientMock.Object);
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

    [Fact]
    public async Task Handle_Should_MigrateCustomerType_When_CustomerTypeIsZeroAndCustomerIdIsSet()
    {
        var query = new GetUserByEmailQuery { Email = "test@example.com" };
        var user = TestDataFactory.ValidUser(customerType: 0);
        var customer = new CustomerClientResponseDto { CustomerType = Domain.Enums.CustomerType.Crew };

        _repoMock.Setup(r => r.GetByEmailAsync(query.Email)).ReturnsAsync(user);
        _repoMock.Setup(r => r.UpdateAsync(user.Id, user)).ReturnsAsync(true);
        _customerClientMock.Setup(c => c.GetByIdAsync(user.CustomerId)).ReturnsAsync(customer);

        var result = await _handler.Handle(query, CancellationToken.None);

        result!.CustomerType.Should().Be((int)Domain.Enums.CustomerType.Crew);
        _repoMock.Verify(r => r.UpdateAsync(user.Id, user), Times.Once);
    }
}
