using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Models;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class RegisterCredentialCommandHandlerTests
{
    private readonly Mock<ICredentialRepository> _repoMock;
    private readonly RegisterCredentialCommandHandler _handler;

    public RegisterCredentialCommandHandlerTests()
    {
        _repoMock = new Mock<ICredentialRepository>();
        _handler = new RegisterCredentialCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateCredential_When_ValidInput()
    {
        var command = new RegisterCredentialCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((Credential?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Credential>())).ReturnsAsync(new Credential());

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Credential>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowDuplicateException_When_CredentialAlreadyExists()
    {
        var existing = TestDataFactory.ValidCredential();
        var command = new RegisterCredentialCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existing);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DuplicateException>();
    }

    [Fact]
    public async Task Handle_Should_HashPassword_When_Creating()
    {
        var command = new RegisterCredentialCommand
        {
            Email = "test@example.com",
            Password = "password123"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((Credential?)null);

        Credential? capturedCredential = null;
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Credential>()))
            .Callback<Credential>(c => capturedCredential = c)
            .ReturnsAsync(new Credential());

        await _handler.Handle(command, CancellationToken.None);

        capturedCredential.Should().NotBeNull();
        capturedCredential!.PasswordHash.Should().NotBe("password123");
        BCrypt.Net.BCrypt.Verify("password123", capturedCredential.PasswordHash).Should().BeTrue();
    }
}
