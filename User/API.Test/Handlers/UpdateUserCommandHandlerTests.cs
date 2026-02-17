using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Enums;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _handler = new UpdateUserCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UserNotFound()
    {
        var command = new UpdateUserCommand { Id = TestDataFactory.ValidMongoId };

        _repoMock.Setup(r => r.GetByIdAsync(command.Id))
            .ReturnsAsync((User?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_UpdateOnlyProvidedField_When_SingleFieldGiven()
    {
        var existing = TestDataFactory.ValidUser();
        var originalEmail = existing.Email;

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<User>()))
            .ReturnsAsync(true);

        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            FirstName = "New Name",
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.FirstName.Should().Be("New Name");
        existing.Email.Should().Be(originalEmail);
    }

    [Fact]
    public async Task Handle_Should_UpdateAllFields_When_AllFieldsProvided()
    {
        var existing = TestDataFactory.ValidUser();

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<User>()))
            .ReturnsAsync(true);

        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Email = "new@example.com",
            FirstName = "Updated",
            LastName = "Updatedsson",
            Role = (int)Role.Admin,
            Phone = "+46709999999",
            Occupation = "Ljustekniker",
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Email.Should().Be("new@example.com");
        existing.FirstName.Should().Be("Updated");
        existing.LastName.Should().Be("Updatedsson");
        existing.Role.Should().Be(Role.Admin);
        existing.Phone.Should().Be("+46709999999");
        existing.Occupation.Should().Be("Ljustekniker");
    }

    [Fact]
    public async Task Handle_Should_NotOverwriteFields_When_FieldsAreNull()
    {
        var existing = TestDataFactory.ValidUser();
        var originalFirstName = existing.FirstName;

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<User>()))
            .ReturnsAsync(true);

        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            FirstName = null,
            Email = null,
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.FirstName.Should().Be(originalFirstName);
    }

    [Fact]
    public async Task Handle_Should_ThrowDuplicateException_When_EmailAlreadyTaken()
    {
        var existing = TestDataFactory.ValidUser();

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.GetByEmailAsync("taken@example.com"))
            .ReturnsAsync(TestDataFactory.ValidUser());

        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Email = "taken@example.com",
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateException>();
    }

    [Fact]
    public async Task Handle_Should_AllowSameEmail_When_EmailUnchanged()
    {
        var existing = TestDataFactory.ValidUser();

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<User>()))
            .ReturnsAsync(true);

        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Email = existing.Email,
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_UpdateSkills_When_SkillsProvided()
    {
        var existing = TestDataFactory.ValidUser(UserType.Crew);

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<User>()))
            .ReturnsAsync(true);

        var newSkills = new List<Skill> { TestDataFactory.ValidSkill() };
        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            Skills = newSkills,
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.Skills.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Should_SetUpdatedAt_When_UserUpdated()
    {
        var existing = TestDataFactory.ValidUser();
        var before = DateTime.UtcNow;

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<User>()))
            .ReturnsAsync(true);

        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            FirstName = "Updated",
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.UpdatedAt.Should().NotBeNull();
        existing.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Handle_Should_UpdateIsEmployee_When_ValueProvided()
    {
        var existing = TestDataFactory.ValidUser(UserType.Crew);

        _repoMock.Setup(r => r.GetByIdAsync(TestDataFactory.ValidMongoId))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(TestDataFactory.ValidMongoId, It.IsAny<User>()))
            .ReturnsAsync(true);

        var command = new UpdateUserCommand
        {
            Id = TestDataFactory.ValidMongoId,
            IsEmployee = true,
        };

        await _handler.Handle(command, CancellationToken.None);

        existing.IsEmployee.Should().BeTrue();
    }
}
