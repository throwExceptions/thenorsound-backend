using API.DTOs.Request;
using API.DTOs.Response;
using Application.Commands;
using Application.Queries;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Customer management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController(
    IMediator mediator,
    ILogger<CustomerController> logger,
    IValidator<CreateCustomerRequestDto> createValidator,
    IValidator<UpdateCustomerRequestDto> updateValidator,
    IValidator<GetAllCustomersRequestDto> getAllValidator,
    IValidator<GetCustomerByIdRequestDto> getByIdValidator)
    : BaseController<CustomerController>
{
    /// <summary>
    /// Get all customers (optional filter by customerType)
    /// </summary>
    /// <param name="customerType">Filter: 1=Customer, 2=Crew</param>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponseDto<IEnumerable<CustomerResponseDto>>), 200)]
    public async Task<IActionResult> GetAllCustomers([FromQuery] int? customerType = null)
    {
        var request = new GetAllCustomersRequestDto { CustomerType = customerType };

        return await this.TryExecuteAsync(
            request,
            getAllValidator,
            async () =>
            {
                var query = new GetAllCustomersQuery
                {
                    CustomerType = customerType.HasValue
                        ? (Domain.Enums.CustomerType)customerType.Value
                        : null
                };

                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<IEnumerable<CustomerResponseDto>>
                {
                    Result = result.Adapt<List<CustomerResponseDto>>()
                });
            }, logger);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<CustomerResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCustomerById([FromRoute] string id)
    {
        var request = new GetCustomerByIdRequestDto { Id = id };

        return await this.TryExecuteAsync(
            request,
            getByIdValidator,
            async () =>
            {
                var query = new GetCustomerQuery { Id = id };
                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<CustomerResponseDto>
                {
                    Result = result.Adapt<CustomerResponseDto>()
                });
            }, logger);
    }

    /// <summary>
    /// Create new customer
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponseDto<CustomerResponseDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequestDto request)
    {
        return await this.TryExecuteAsync(
            request,
            createValidator,
            async () =>
            {
                var command = request.Adapt<CreateCustomerCommand>();
                var result = await mediator.Send(command);

                return this.Created($"/api/customers/{result.Id}", new BaseResponseDto<CustomerResponseDto>
                {
                    Result = result.Adapt<CustomerResponseDto>()
                });
            }, logger);
    }

    /// <summary>
    /// Update customer
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCustomer([FromRoute] string id, [FromBody] UpdateCustomerRequestDto request)
    {
        request.Id = id;

        return await this.TryExecuteAsync(
            request,
            updateValidator,
            async () =>
            {
                var command = request.Adapt<UpdateCustomerCommand>();
                var result = await mediator.Send(command);

                return this.Ok(new BaseResponseDto<bool>
                {
                    Result = result
                });
            }, logger);
    }

    /// <summary>
    /// Delete customer (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCustomer([FromRoute] string id)
    {
        var request = new GetCustomerByIdRequestDto { Id = id };

        return await this.TryExecuteAsync(
            request,
            getByIdValidator,
            async () =>
            {
                var command = new DeleteCustomerCommand { Id = id };
                var result = await mediator.Send(command);

                return this.Ok(new BaseResponseDto<bool>
                {
                    Result = result
                });
            }, logger);
    }
}
