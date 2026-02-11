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
public class CustomerControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<CustomerController>>();

        _controller = new CustomerController(
            _mediatorMock.Object,
            loggerMock.Object,
            new CreateCustomerRequestDtoValidator(),
            new UpdateCustomerRequestDtoValidator(),
            new GetAllCustomersRequestDtoValidator(),
            new GetCustomerByIdRequestDtoValidator());
    }

    // --- GetAllCustomers ---

    [Fact]
    public async Task GetAllCustomers_Should_ReturnOkWithCustomers_When_ValidRequest()
    {
        var customers = new List<Customer> { TestDataFactory.ValidCustomer() };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllCustomersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var result = await _controller.GetAllCustomers();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<IEnumerable<CustomerResponseDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllCustomers_Should_ReturnBadRequest_When_InvalidFilter()
    {
        var result = await _controller.GetAllCustomers(customerType: 99);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    // --- GetCustomerById ---

    [Fact]
    public async Task GetCustomerById_Should_ReturnOkWithCustomer_When_ValidId()
    {
        var customer = TestDataFactory.ValidCustomer();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCustomerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _controller.GetCustomerById(TestDataFactory.ValidMongoId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<CustomerResponseDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Result.Name.Should().Be(customer.Name);
    }

    [Fact]
    public async Task GetCustomerById_Should_ReturnBadRequest_When_InvalidId()
    {
        var result = await _controller.GetCustomerById("invalid");

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    // --- CreateCustomer ---

    [Fact]
    public async Task CreateCustomer_Should_Return201WithCustomer_When_ValidRequest()
    {
        var request = TestDataFactory.ValidCreateRequest();
        var created = TestDataFactory.ValidCustomer();

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _controller.CreateCustomer(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<BaseResponseDto<CustomerResponseDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Result.Name.Should().Be(created.Name);
    }

    [Fact]
    public async Task CreateCustomer_Should_ReturnBadRequest_When_InvalidRequest()
    {
        var request = TestDataFactory.ValidCreateRequest();
        request.Name = string.Empty;
        request.Mail = "bad";

        var result = await _controller.CreateCustomer(request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateCustomer_Should_MapResponseCorrectly_When_CustomerHasTariffs()
    {
        var tariff = TestDataFactory.ValidTariff();
        var customer = TestDataFactory.ValidCustomer(tariffs: new List<Tariff> { tariff });

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var request = TestDataFactory.ValidCreateRequest();

        var result = await _controller.CreateCustomer(request);

        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        var response = createdResult.Value.Should().BeOfType<BaseResponseDto<CustomerResponseDto>>().Subject;
        response.Result.CustomerType.Should().Be((int)CustomerType.Customer);
        response.Result.Tariffs.Should().HaveCount(1);
        response.Result.Tariffs[0].Tariff.Should().Be(tariff.TariffValue);
    }

    // --- UpdateCustomer ---

    [Fact]
    public async Task UpdateCustomer_Should_ReturnOkWithTrue_When_ValidRequest()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new UpdateCustomerRequestDto { Name = "Updated AB" };

        var result = await _controller.UpdateCustomer(TestDataFactory.ValidMongoId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<bool>>().Subject;
        response.Result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCustomer_Should_ReturnBadRequest_When_InvalidId()
    {
        var request = new UpdateCustomerRequestDto();

        var result = await _controller.UpdateCustomer("bad-id", request);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    // --- DeleteCustomer ---

    [Fact]
    public async Task DeleteCustomer_Should_ReturnOkWithTrue_When_ValidId()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCustomerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.DeleteCustomer(TestDataFactory.ValidMongoId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BaseResponseDto<bool>>().Subject;
        response.Result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCustomer_Should_ReturnBadRequest_When_InvalidId()
    {
        var result = await _controller.DeleteCustomer("not-hex");

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }
}
