using API.Controllers;
using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.DTOs.Response;
using API.Test.Helpers;
using Application.Commands;
using Application.Queries;
using Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Error = Domain.Models.Error;

namespace API.Test.Controllers;

[Collection("Mapster")]
public class UserControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<UserController>>();

        _controller = new UserController(
            _mediatorMock.Object,
            loggerMock.Object,
            new CreateUserRequestDtoValidator(),
            new UpdateUserRequestDtoValidator(),
            new GetAllUsersRequestDtoValidator(),
            new GetUserByIdRequestDtoValidator(),
            new GetUsersByCustomerIdRequestDtoValidator(),
            new GetUserByEmailRequestDtoValidator());
    }

    // --- GetAllUsers ---

    [Fact]
    public async Task GetAllUsers_Should_ReturnOkWithUsers_When_ValidRequest()
    {
        var users = new List<User> { TestDataFactory.ValidUser() };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _controller.GetAllUsers();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<IEnumerable<UserResponseDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllUsers_Should_ReturnBadRequest_When_InvalidFilter()
    {
        var result = await _controller.GetAllUsers(userType: 99);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    // --- GetUserById ---

    [Fact]
    public async Task GetUserById_Should_ReturnOkWithUser_When_ValidId()
    {
        var user = TestDataFactory.ValidUser();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.GetUserById(TestDataFactory.ValidMongoId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<UserResponseDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserById_Should_ReturnBadRequest_When_InvalidId()
    {
        var result = await _controller.GetUserById("invalid");

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    // --- GetUsersByCustomerId ---

    [Fact]
    public async Task GetUsersByCustomerId_Should_ReturnOkWithUsers_When_ValidCustomerId()
    {
        var users = new List<User> { TestDataFactory.ValidUser() };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersByCustomerIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _controller.GetUsersByCustomerId(TestDataFactory.ValidMongoId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<IEnumerable<UserResponseDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUsersByCustomerId_Should_ReturnBadRequest_When_InvalidCustomerId()
    {
        var result = await _controller.GetUsersByCustomerId("bad-id");

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    // --- GetUserByEmail ---

    [Fact]
    public async Task GetUserByEmail_Should_ReturnOkWithUser_When_ValidEmail()
    {
        var user = TestDataFactory.ValidUser();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByEmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _controller.GetUserByEmail("test@example.com");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<UserResponseDto>>().Subject;
        response.Success.Should().BeTrue();
    }

    // --- CreateUser ---

    [Fact]
    public async Task CreateUser_Should_Return201WithUser_When_ValidRequest()
    {
        var request = TestDataFactory.ValidCreateRequest();
        var created = TestDataFactory.ValidUser();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _controller.CreateUser(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<BaseResponseDto<UserResponseDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Result.Email.Should().Be(created.Email);
    }

    [Fact]
    public async Task CreateUser_Should_ReturnBadRequest_When_InvalidRequest()
    {
        var request = TestDataFactory.ValidCreateRequest();
        request.Email = "bad";
        request.FirstName = string.Empty;

        var result = await _controller.CreateUser(request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateUser_Should_MapResponseCorrectly_When_CrewWithSkills()
    {
        var skill = TestDataFactory.ValidSkill();
        var user = TestDataFactory.ValidUser(UserType.Crew, skills: new List<Skill> { skill });

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var request = TestDataFactory.ValidCreateRequest(userType: 3);
        request.Occupation = "Ljudtekniker";

        var result = await _controller.CreateUser(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<BaseResponseDto<UserResponseDto>>().Subject;
        response.Result.UserType.Should().Be((int)UserType.Crew);
        response.Result.Skills.Should().HaveCount(1);
    }

    // --- UpdateUser ---

    [Fact]
    public async Task UpdateUser_Should_ReturnOkWithTrue_When_ValidRequest()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new UpdateUserRequestDto { FirstName = "Updated" };

        var result = await _controller.UpdateUser(TestDataFactory.ValidMongoId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<bool>>().Subject;
        response.Result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUser_Should_ReturnBadRequest_When_InvalidId()
    {
        var request = new UpdateUserRequestDto();

        var result = await _controller.UpdateUser("bad-id", request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    // --- DeleteUser ---

    [Fact]
    public async Task DeleteUser_Should_ReturnOkWithTrue_When_ValidId()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.DeleteUser(TestDataFactory.ValidMongoId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<bool>>().Subject;
        response.Result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUser_Should_ReturnBadRequest_When_InvalidId()
    {
        var result = await _controller.DeleteUser("not-hex");

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }
}
