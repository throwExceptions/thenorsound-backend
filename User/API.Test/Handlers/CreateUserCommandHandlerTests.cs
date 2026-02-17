using API.Test.Helpers;
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
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new CreateUserCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnCreatedUser_When_ValidCommand()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            Role = Role.CustomerUser,
            UserType = UserType.Customer,
            CustomerId = TestDataFactory.ValidMongoId2,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        var expected = TestDataFactory.ValidUser();
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowDuplicateException_When_EmailAlreadyExists()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            UserType = UserType.Customer,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(TestDataFactory.ValidUser());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateException>();
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_CrewUser()
    {
        var command = new CreateUserCommand
        {
            Email = "crew@example.com",
            FirstName = "Crew",
            LastName = "Member",
            Role = Role.User,
            UserType = UserType.Crew,
            CustomerId = TestDataFactory.ValidMongoId2,
            Occupation = "Ljudtekniker",
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        var expected = TestDataFactory.ValidUser(UserType.Crew);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_CallRepositoryWithCorrectData_When_ValidCommand()
    {
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            UserType = UserType.Customer,
            CustomerId = TestDataFactory.ValidMongoId2,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(TestDataFactory.ValidUser());

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.Email == "test@example.com" && u.FirstName == "Test")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_AdminUser()
    {
        var command = new CreateUserCommand
        {
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "Adminsson",
            Role = Role.Superuser,
            UserType = UserType.Admin,
            CustomerId = TestDataFactory.ValidMongoId2,
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        var expected = TestDataFactory.ValidUser(UserType.Admin);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }
}
