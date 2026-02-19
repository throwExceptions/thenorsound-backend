using API.Test.Helpers;
using Application.Clients;
using Application.Clients.DTOs.Response;
using Application.Commands;
using Application.Exceptions;
using Domain.Enums;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

[Collection("Mapster")]
public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<ICustomerClient> _customerClientMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _customerClientMock = new Mock<ICustomerClient>();
        _handler = new CreateUserCommandHandler(_repoMock.Object, _customerClientMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnCreatedUser_When_ValidCustomerUser()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            Role = Role.CustomerUser,
            CustomerId = TestDataFactory.ValidMongoId2,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _customerClientMock.Setup(c => c.GetByIdAsync(command.CustomerId))
            .ReturnsAsync(new CustomerClientResponseDto { Id = command.CustomerId, CustomerType = CustomerType.Customer });

        var expected = TestDataFactory.ValidUser();
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _customerClientMock.Verify(c => c.GetByIdAsync(command.CustomerId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowDuplicateException_When_EmailAlreadyExists()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(TestDataFactory.ValidUser());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateException>();
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_UserLinkedToCrewCustomer()
    {
        var command = new CreateUserCommand
        {
            Email = "crew@example.com",
            FirstName = "Crew",
            LastName = "Member",
            Role = Role.User,
            CustomerId = TestDataFactory.ValidMongoId2,
            Occupation = "Ljudtekniker",
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _customerClientMock.Setup(c => c.GetByIdAsync(command.CustomerId))
            .ReturnsAsync(new CustomerClientResponseDto { Id = command.CustomerId, CustomerType = CustomerType.Crew });

        var expected = TestDataFactory.ValidUser(isCrew: true);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
        _customerClientMock.Verify(c => c.GetByIdAsync(command.CustomerId), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CustomerNotFound()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            Role = Role.CustomerUser,
            CustomerId = TestDataFactory.ValidMongoId2,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _customerClientMock.Setup(c => c.GetByIdAsync(command.CustomerId))
            .ReturnsAsync((CustomerClientResponseDto?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_Should_SkipCustomerValidation_When_NoCustomerIdProvided()
    {
        var command = new CreateUserCommand
        {
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "Adminsson",
            Role = Role.Superuser,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        var expected = TestDataFactory.ValidUser();
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
        _customerClientMock.Verify(c => c.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectData_When_ValidCommand()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            CustomerId = TestDataFactory.ValidMongoId2,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _customerClientMock.Setup(c => c.GetByIdAsync(command.CustomerId))
            .ReturnsAsync(new CustomerClientResponseDto { Id = command.CustomerId, CustomerType = CustomerType.Customer });

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(TestDataFactory.ValidUser());

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.Email == "test@example.com" && u.FirstName == "Test")),
            Times.Once);
    }
}
