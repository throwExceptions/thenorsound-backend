using API.Test.Helpers;
using Application.Commands;
using Application.Exceptions;
using Domain.Models;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace API.Test.Handlers;

public class UpdateCredentialPasswordCommandHandlerTests
{
    private readonly Mock<ICredentialRepository> _repoMock;
    private readonly UpdateCredentialPasswordCommandHandler _handler;

    public UpdateCredentialPasswordCommandHandlerTests()
    {
        _repoMock = new Mock<ICredentialRepository>();
        _handler = new UpdateCredentialPasswordCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_CredentialNotFound()
    {
        var command = new UpdateCredentialPasswordCommand
        {
            Email = "unknown@example.com",
            NewPassword = "NewPassword1!"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((Credential?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_CallUpdateAsync_When_CredentialFound()
    {
        var existing = TestDataFactory.ValidCredential();
        var command = new UpdateCredentialPasswordCommand
        {
            Email = existing.Email,
            NewPassword = "NewPassword1!"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(existing.Id, It.IsAny<Credential>())).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(existing.Id, It.IsAny<Credential>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_HashNewPassword_When_Updating()
    {
        var existing = TestDataFactory.ValidCredential();
        var command = new UpdateCredentialPasswordCommand
        {
            Email = existing.Email,
            NewPassword = "NewPassword1!"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existing);

        Credential? captured = null;
        _repoMock.Setup(r => r.UpdateAsync(existing.Id, It.IsAny<Credential>()))
            .Callback<string, Credential>((_, c) => captured = c)
            .ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.PasswordHash.Should().NotBe("NewPassword1!");
        BCrypt.Net.BCrypt.Verify("NewPassword1!", captured.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_NotMatchOldPassword_When_Updated()
    {
        var existing = TestDataFactory.ValidCredential();
        var command = new UpdateCredentialPasswordCommand
        {
            Email = existing.Email,
            NewPassword = "NewPassword1!"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existing);

        Credential? captured = null;
        _repoMock.Setup(r => r.UpdateAsync(existing.Id, It.IsAny<Credential>()))
            .Callback<string, Credential>((_, c) => captured = c)
            .ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        BCrypt.Net.BCrypt.Verify("password123", captured!.PasswordHash).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_SetUpdatedAt_When_PasswordChanged()
    {
        var existing = TestDataFactory.ValidCredential();
        var before = DateTime.UtcNow;
        var command = new UpdateCredentialPasswordCommand
        {
            Email = existing.Email,
            NewPassword = "NewPassword1!"
        };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(existing.Id, It.IsAny<Credential>())).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        existing.UpdatedAt.Should().NotBeNull();
        existing.UpdatedAt.Should().BeOnOrAfter(before);
    }
}
