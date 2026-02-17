using API.DTOs.Request;
using API.DTOs.Response;
using Application.Commands;
using Application.Queries;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// User management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController(
    IMediator mediator,
    ILogger<UserController> logger,
    IValidator<CreateUserRequestDto> createValidator,
    IValidator<UpdateUserRequestDto> updateValidator,
    IValidator<GetAllUsersRequestDto> getAllValidator,
    IValidator<GetUserByIdRequestDto> getByIdValidator,
    IValidator<GetUsersByCustomerIdRequestDto> getByCustomerIdValidator,
    IValidator<GetUserByEmailRequestDto> getByEmailValidator)
    : BaseController<UserController>
{
    /// <summary>
    /// Get all users (optional filter by userType)
    /// </summary>
    /// <param name="userType">Filter: 1=Admin, 2=Customer, 3=Crew</param>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponseDto<IEnumerable<UserResponseDto>>), 200)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int? userType = null)
    {
        var request = new GetAllUsersRequestDto { UserType = userType };

        return await this.TryExecuteAsync(
            request,
            getAllValidator,
            async () =>
            {
                var query = new GetAllUsersQuery
                {
                    UserType = userType.HasValue
                        ? (Domain.Enums.UserType)userType.Value
                        : null
                };

                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<IEnumerable<UserResponseDto>>
                {
                    Result = result.Adapt<List<UserResponseDto>>()
                });
            }, logger);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<UserResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById([FromRoute] string id)
    {
        var request = new GetUserByIdRequestDto { Id = id };

        return await this.TryExecuteAsync(
            request,
            getByIdValidator,
            async () =>
            {
                var query = new GetUserQuery { Id = id };
                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<UserResponseDto>
                {
                    Result = result.Adapt<UserResponseDto>()
                });
            }, logger);
    }

    /// <summary>
    /// Get all users for a customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(BaseResponseDto<IEnumerable<UserResponseDto>>), 200)]
    public async Task<IActionResult> GetUsersByCustomerId([FromRoute] string customerId)
    {
        var request = new GetUsersByCustomerIdRequestDto { CustomerId = customerId };

        return await this.TryExecuteAsync(
            request,
            getByCustomerIdValidator,
            async () =>
            {
                var query = new GetUsersByCustomerIdQuery { CustomerId = customerId };
                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<IEnumerable<UserResponseDto>>
                {
                    Result = result.Adapt<List<UserResponseDto>>()
                });
            }, logger);
    }

    /// <summary>
    /// Get user by email (for Auth service)
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(BaseResponseDto<UserResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserByEmail([FromRoute] string email)
    {
        var request = new GetUserByEmailRequestDto { Email = email };

        return await this.TryExecuteAsync(
            request,
            getByEmailValidator,
            async () =>
            {
                var query = new GetUserByEmailQuery { Email = email };
                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<UserResponseDto>
                {
                    Result = result?.Adapt<UserResponseDto>()
                });
            }, logger);
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponseDto<UserResponseDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        return await this.TryExecuteAsync(
            request,
            createValidator,
            async () =>
            {
                var command = request.Adapt<CreateUserCommand>();
                var result = await mediator.Send(command);

                return this.Created($"/api/user/{result.Id}", new BaseResponseDto<UserResponseDto>
                {
                    Result = result.Adapt<UserResponseDto>()
                });
            }, logger);
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserRequestDto request)
    {
        request.Id = id;

        return await this.TryExecuteAsync(
            request,
            updateValidator,
            async () =>
            {
                var command = request.Adapt<UpdateUserCommand>();
                var result = await mediator.Send(command);

                return this.Ok(new BaseResponseDto<bool>
                {
                    Result = result
                });
            }, logger);
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUser([FromRoute] string id)
    {
        var request = new GetUserByIdRequestDto { Id = id };

        return await this.TryExecuteAsync(
            request,
            getByIdValidator,
            async () =>
            {
                var command = new DeleteUserCommand { Id = id };
                var result = await mediator.Send(command);

                return this.Ok(new BaseResponseDto<bool>
                {
                    Result = result
                });
            }, logger);
    }
}
