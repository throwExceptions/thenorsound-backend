using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.DTOs.Response;
using Application.Commands;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IMediator mediator,
    IValidator<LoginRequestDto> loginValidator,
    IValidator<RegisterCredentialRequestDto> registerValidator,
    ILogger<AuthController> logger) : BaseController<AuthController>
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        return await TryExecuteAsync(request, loginValidator, async () =>
        {
            var result = await mediator.Send(new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            });

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new BaseResponseDto<LoginResponseDto>
            {
                Result = new LoginResponseDto
                {
                    Token = result.AccessToken,
                    Expires = result.ExpiresMs
                }
            });
        }, logger);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new AuthenticationException("Missing refresh token");
            }

            var result = await mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken });

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new BaseResponseDto<LoginResponseDto>
            {
                Result = new LoginResponseDto
                {
                    Token = result.AccessToken,
                    Expires = result.ExpiresMs
                }
            });
        }
        catch (AuthenticationException ex)
        {
            logger.LogInformation("Authentication exception in AuthController. {ex}", ex.ToString());
            return Unauthorized(new BaseResponseDto<Domain.Models.Error>
            {
                Error = new Domain.Models.Error
                {
                    Type = Domain.Enums.ErrorType.AuthenticationFailed,
                    ErrorMessage = ex.Message,
                    FormValidationError = new List<KeyValuePair<string, object>>()
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError("Exception in AuthController.", ex);
            return StatusCode(500, new BaseResponseDto<Domain.Models.Error>
            {
                Error = new Domain.Models.Error
                {
                    Type = Domain.Enums.ErrorType.UnknownError,
                    ErrorMessage = ex.Message,
                    FormValidationError = new List<KeyValuePair<string, object>>()
                }
            });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await mediator.Send(new LogoutCommand { RefreshToken = refreshToken });
        }

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/Auth/refresh"
        });

        return Ok(new BaseResponseDto<bool> { Result = true });
    }

    [HttpPost("credentials")]
    public async Task<IActionResult> RegisterCredential([FromBody] RegisterCredentialRequestDto request)
    {
        return await TryExecuteAsync(request, registerValidator, async () =>
        {
            await mediator.Send(new RegisterCredentialCommand
            {
                Email = request.Email,
                Password = request.Password
            });

            return Ok(new BaseResponseDto<bool> { Result = true });
        }, logger);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api/Auth/refresh"
        });
    }
}
