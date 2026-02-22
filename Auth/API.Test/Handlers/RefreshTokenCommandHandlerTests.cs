using API.Test.Helpers;
using Application.Clients;
using Application.Commands;
using Application.Services;
using Domain.Repositories;
using FluentAssertions;
using Moq;
using System.Security.Authentication;

namespace API.Test.Handlers;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<ICredentialRepository> _repoMock;
    private readonly Mock<IUserClient> _userClientMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _repoMock = new Mock<ICredentialRepository>();
        _userClientMock = new Mock<IUserClient>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new RefreshTokenCommandHandler(_repoMock.Object, _userClientMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnNewTokens_When_ValidRefreshToken()
    {
        var credential = TestDataFactory.ValidCredential(
            refreshToken: TestDataFactory.ValidRefreshToken,
            refreshTokenExpiry: DateTime.UtcNow.AddDays(7));
        var user = TestDataFactory.ValidUser();
        var command = new RefreshTokenCommand { RefreshToken = TestDataFactory.ValidRefreshToken };

        _repoMock.Setup(r => r.GetByRefreshTokenAsync(command.RefreshToken)).ReturnsAsync(credential);
        _userClientMock.Setup(c => c.GetByEmailAsync(credential.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(user)).Returns("new-access-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");
        _repoMock.Setup(r => r.UpdateAsync(credential.Id, It.IsAny<Domain.Models.Credential>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_Should_ThrowAuthenticationException_When_TokenNotFound()
    {
        var command = new RefreshTokenCommand { RefreshToken = "invalid-token" };

        _repoMock.Setup(r => r.GetByRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((Domain.Models.Credential?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_Should_ThrowAuthenticationException_When_TokenExpired()
    {
        var credential = TestDataFactory.ValidCredential(
            refreshToken: TestDataFactory.ValidRefreshToken,
            refreshTokenExpiry: DateTime.UtcNow.AddDays(-1));
        var command = new RefreshTokenCommand { RefreshToken = TestDataFactory.ValidRefreshToken };

        _repoMock.Setup(r => r.GetByRefreshTokenAsync(command.RefreshToken)).ReturnsAsync(credential);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("Token expired");
    }

    [Fact]
    public async Task Handle_Should_RotateRefreshToken_When_RefreshSucceeds()
    {
        var credential = TestDataFactory.ValidCredential(
            refreshToken: TestDataFactory.ValidRefreshToken,
            refreshTokenExpiry: DateTime.UtcNow.AddDays(7));
        var user = TestDataFactory.ValidUser();
        var command = new RefreshTokenCommand { RefreshToken = TestDataFactory.ValidRefreshToken };

        _repoMock.Setup(r => r.GetByRefreshTokenAsync(command.RefreshToken)).ReturnsAsync(credential);
        _userClientMock.Setup(c => c.GetByEmailAsync(credential.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(user)).Returns("new-access-token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh-token");
        _repoMock.Setup(r => r.UpdateAsync(credential.Id, It.IsAny<Domain.Models.Credential>())).ReturnsAsync(true);

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(r => r.UpdateAsync(
            credential.Id,
            It.Is<Domain.Models.Credential>(c =>
                c.RefreshToken == "new-refresh-token" &&
                c.RefreshTokenExpiry > DateTime.UtcNow)),
            Times.Once);
    }
}
