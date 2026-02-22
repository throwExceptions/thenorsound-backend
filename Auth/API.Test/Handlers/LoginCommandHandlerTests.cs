using API.Test.Helpers;
using Application.Clients;
using Application.Commands;
using Application.Services;
using Domain.Repositories;
using FluentAssertions;
using Moq;
using System.Security.Authentication;

namespace API.Test.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<ICredentialRepository> _repoMock;
    private readonly Mock<IUserClient> _userClientMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _repoMock = new Mock<ICredentialRepository>();
        _userClientMock = new Mock<IUserClient>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new LoginCommandHandler(_repoMock.Object, _userClientMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnTokens_When_ValidCredentials()
    {
        var credential = TestDataFactory.ValidCredential();
        var user = TestDataFactory.ValidUser();
        var command = new LoginCommand { Email = "test@example.com", Password = "password123" };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(credential);
        _userClientMock.Setup(c => c.GetByEmailAsync(command.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(user)).Returns("access-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _repoMock.Setup(r => r.UpdateAsync(credential.Id, It.IsAny<Domain.Models.Credential>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.ExpiresMs.Should().Be(15 * 60 * 1000);
    }

    [Fact]
    public async Task Handle_Should_ThrowAuthenticationException_When_CredentialNotFound()
    {
        var command = new LoginCommand { Email = "unknown@example.com", Password = "password123" };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync((Domain.Models.Credential?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_Should_ThrowAuthenticationException_When_WrongPassword()
    {
        var credential = TestDataFactory.ValidCredential();
        var command = new LoginCommand { Email = "test@example.com", Password = "wrongpassword" };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(credential);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_Should_ThrowAuthenticationException_When_UserServiceReturnsNull()
    {
        var credential = TestDataFactory.ValidCredential();
        var command = new LoginCommand { Email = "test@example.com", Password = "password123" };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(credential);
        _userClientMock.Setup(c => c.GetByEmailAsync(command.Email)).ReturnsAsync((Application.Clients.DTOs.Response.UserClientResponseDto?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_Should_StoreRefreshToken_When_LoginSucceeds()
    {
        var credential = TestDataFactory.ValidCredential();
        var user = TestDataFactory.ValidUser();
        var command = new LoginCommand { Email = "test@example.com", Password = "password123" };

        _repoMock.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(credential);
        _userClientMock.Setup(c => c.GetByEmailAsync(command.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(user)).Returns("access-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _repoMock.Setup(r => r.UpdateAsync(credential.Id, It.IsAny<Domain.Models.Credential>())).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(
            credential.Id,
            It.Is<Domain.Models.Credential>(c =>
                c.RefreshToken == "refresh-token" &&
                c.RefreshTokenExpiry != null)),
            Times.Once);
    }
}
