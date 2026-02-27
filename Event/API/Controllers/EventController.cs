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

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventController(
    IMediator mediator,
    ILogger<EventController> logger,
    IValidator<CreateEventRequestDto> createValidator,
    IValidator<UpdateEventRequestDto> updateValidator,
    IValidator<GetAllEventsRequestDto> getAllValidator,
    IValidator<GetEventByIdRequestDto> getByIdValidator)
    : BaseController<EventController>
{
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponseDto<IEnumerable<EventResponseDto>>), 200)]
    public async Task<IActionResult> GetAllEvents([FromQuery] string? customerId = null)
    {
        var request = new GetAllEventsRequestDto { CustomerId = customerId };

        return await this.TryExecuteAsync(
            request,
            getAllValidator,
            async () =>
            {
                var query = new GetAllEventsQuery { CustomerId = customerId };
                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<IEnumerable<EventResponseDto>>
                {
                    Result = result.Adapt<List<EventResponseDto>>()
                });
            }, logger);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<EventResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEventById([FromRoute] string id)
    {
        var request = new GetEventByIdRequestDto { Id = id };

        return await this.TryExecuteAsync(
            request,
            getByIdValidator,
            async () =>
            {
                var query = new GetEventQuery { Id = id };
                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<EventResponseDto>
                {
                    Result = result.Adapt<EventResponseDto>()
                });
            }, logger);
    }

    [HttpGet("by-customer/{customerId}")]
    [ProducesResponseType(typeof(BaseResponseDto<IEnumerable<EventResponseDto>>), 200)]
    public async Task<IActionResult> GetEventsByCustomerId([FromRoute] string customerId)
    {
        var request = new GetEventByIdRequestDto { Id = customerId };

        return await this.TryExecuteAsync(
            request,
            getByIdValidator,
            async () =>
            {
                var query = new GetEventsByCustomerIdQuery { CustomerId = customerId };
                var result = await mediator.Send(query);

                return this.Ok(new BaseResponseDto<IEnumerable<EventResponseDto>>
                {
                    Result = result.Adapt<List<EventResponseDto>>()
                });
            }, logger);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseResponseDto<EventResponseDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestDto request)
    {
        return await this.TryExecuteAsync(
            request,
            createValidator,
            async () =>
            {
                var command = request.Adapt<CreateEventCommand>();
                var result = await mediator.Send(command);

                return this.Created($"/api/Event/{result.Id}", new BaseResponseDto<EventResponseDto>
                {
                    Result = result.Adapt<EventResponseDto>()
                });
            }, logger);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateEvent([FromRoute] string id, [FromBody] UpdateEventRequestDto request)
    {
        request.Id = id;

        return await this.TryExecuteAsync(
            request,
            updateValidator,
            async () =>
            {
                var command = request.Adapt<UpdateEventCommand>();
                var result = await mediator.Send(command);

                return this.Ok(new BaseResponseDto<bool>
                {
                    Result = result
                });
            }, logger);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(BaseResponseDto<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteEvent([FromRoute] string id)
    {
        var request = new GetEventByIdRequestDto { Id = id };

        return await this.TryExecuteAsync(
            request,
            getByIdValidator,
            async () =>
            {
                var command = new DeleteEventCommand { Id = id };
                var result = await mediator.Send(command);

                return this.Ok(new BaseResponseDto<bool>
                {
                    Result = result
                });
            }, logger);
    }
}
