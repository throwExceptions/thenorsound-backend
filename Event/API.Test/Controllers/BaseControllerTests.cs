namespace API.Test.Controllers;

using API.Controllers;
using API.DTOs.Response;
using Application.Exceptions;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Security.Authentication;
using Xunit;
using Error = Domain.Models.Error;

public class BaseControllerTests
{
    private readonly Mock<ILogger<BaseControllerTests>> _loggerMock;
    private readonly BaseController<BaseControllerTests> _controller;
    private readonly IValidator<TestRequestDto> _alwaysPassValidator;

    public BaseControllerTests()
    {
        _loggerMock = new Mock<ILogger<BaseControllerTests>>();
        _controller = new BaseController<BaseControllerTests>();
        _alwaysPassValidator = new AlwaysPassValidator();
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnOkResult_When_NoExceptionsAreThrown()
    {
        var request = new TestRequestDto();
        Func<Task<IActionResult>> function = async () => await Task.FromResult(new OkResult());

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnErrorResponse_When_BadRequestExceptionIsThrown()
    {
        var request = new TestRequestDto();
        var formValidationError = new List<KeyValuePair<string, object>>();
        Func<Task<IActionResult>> function = async () => throw new BadRequestException("Test exception", formValidationError);

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnErrorResponse_WhenHttpRequestExceptionIsThrown()
    {
        var request = new TestRequestDto();
        Func<Task<IActionResult>> function = async () => throw new HttpRequestException();

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnErrorResponse_WhenDuplicateExceptionIsThrown()
    {
        var request = new TestRequestDto();
        var formValidationError = new List<KeyValuePair<string, object>>();
        Func<Task<IActionResult>> function = async () => throw new DuplicateException("Test exception", formValidationError);

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.Conflict, objectResult.StatusCode);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnErrorResponse_When_FormValidationExceptionIsThrown()
    {
        var request = new TestRequestDto();
        var formValidationError = new List<KeyValuePair<string, object>>();
        Func<Task<IActionResult>> function = async () => throw new FormValidationException("Test exception", formValidationError);

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnErrorResponse_When_AuthenticationExceptionIsThrown()
    {
        var request = new TestRequestDto();
        Func<Task<IActionResult>> function = async () => throw new AuthenticationException();

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnErrorResponse_WhenExceptionIsThrown()
    {
        var request = new TestRequestDto();
        Func<Task<IActionResult>> function = async () => throw new Exception();

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnErrorResponseWithCorrectContent_When_ExceptionIsThrown()
    {
        var request = new TestRequestDto();
        var exceptionMessage = "Test exception";
        Func<Task<IActionResult>> function = async () => throw new Exception(exceptionMessage);

        var result = await _controller.TryExecuteAsync(request, _alwaysPassValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<BaseResponseDto<Error>>(objectResult.Value);
        Assert.Equal(ErrorType.UnknownError, response.Error.Type);
        Assert.Equal(exceptionMessage, response.Error.ErrorMessage);
    }

    [Fact]
    public async Task TryExecuteAsync_Should_ReturnBadRequest_When_ValidationFails()
    {
        var request = new TestRequestDto();
        var alwaysFailValidator = new AlwaysFailValidator();
        Func<Task<IActionResult>> function = async () => await Task.FromResult(new OkResult());

        var result = await _controller.TryExecuteAsync(request, alwaysFailValidator, function, _loggerMock.Object);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        var response = Assert.IsType<BaseResponseDto<Error>>(objectResult.Value);
        Assert.Equal(ErrorType.ValidationError, response.Error.Type);
    }

    private class TestRequestDto
    {
        public string TestProperty { get; set; } = string.Empty;
    }

    private class AlwaysPassValidator : AbstractValidator<TestRequestDto>
    {
        public AlwaysPassValidator()
        {
        }
    }

    private class AlwaysFailValidator : AbstractValidator<TestRequestDto>
    {
        public AlwaysFailValidator()
        {
            RuleFor(x => x.TestProperty)
                .NotEmpty()
                .WithMessage("Test validation failed");
        }
    }
}
