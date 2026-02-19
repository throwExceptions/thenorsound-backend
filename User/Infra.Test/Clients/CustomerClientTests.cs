using Application.Clients.DTOs.Response;
using Domain.Enums;
using Infra.Clients;
using Infra.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Infra.Test.Clients;

public class CustomerClientTests
{
    private readonly Mock<ILogger<CustomerClient>> _loggerMock;
    private readonly Mock<IOptions<CustomerClientConfiguration>> _configMock;
    private readonly CustomerClientConfiguration _config;

    public CustomerClientTests()
    {
        _loggerMock = new Mock<ILogger<CustomerClient>>();
        _config = new CustomerClientConfiguration
        {
            BaseUrl = "https://api.example.com",
            GetByIdEndpoint = "api/Customer/{0}"
        };
        _configMock = new Mock<IOptions<CustomerClientConfiguration>>();
        _configMock.Setup(x => x.Value).Returns(_config);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnCustomer_When_RequestIsSuccessful()
    {
        var expectedResponse = new BaseClientResponseDto<CustomerClientResponseDto>
        {
            Result = new CustomerClientResponseDto { Id = "abc123", CustomerType = CustomerType.Customer },
            Success = true
        };

        var httpClient = CreateHttpClient(HttpStatusCode.OK, expectedResponse);
        var client = new CustomerClient(httpClient, _loggerMock.Object, _configMock.Object);

        var result = await client.GetByIdAsync("abc123");

        Assert.NotNull(result);
        Assert.Equal("abc123", result.Id);
        Assert.Equal(CustomerType.Customer, result.CustomerType);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_When_ResponseIsNotSuccessful()
    {
        var httpClient = CreateHttpClient(HttpStatusCode.NotFound);
        var client = new CustomerClient(httpClient, _loggerMock.Object, _configMock.Object);

        var result = await client.GetByIdAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_When_ResponseCannotBeDeserialized()
    {
        var httpClient = CreateHttpClient(HttpStatusCode.OK, "null");
        var client = new CustomerClient(httpClient, _loggerMock.Object, _configMock.Object);

        var result = await client.GetByIdAsync("abc123");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ThrowHttpRequestException_When_NetworkErrorOccurs()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };

        var client = new CustomerClient(httpClient, _loggerMock.Object, _configMock.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.GetByIdAsync("abc123"));
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnCrewCustomer_When_CustomerTypeIsCrew()
    {
        var expectedResponse = new BaseClientResponseDto<CustomerClientResponseDto>
        {
            Result = new CustomerClientResponseDto { Id = "crew123", CustomerType = CustomerType.Crew },
            Success = true
        };

        var httpClient = CreateHttpClient(HttpStatusCode.OK, expectedResponse);
        var client = new CustomerClient(httpClient, _loggerMock.Object, _configMock.Object);

        var result = await client.GetByIdAsync("crew123");

        Assert.NotNull(result);
        Assert.Equal(CustomerType.Crew, result.CustomerType);
    }

    private HttpClient CreateHttpClient<T>(HttpStatusCode statusCode, T content)
    {
        var json = JsonSerializer.Serialize(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return CreateHttpClient(statusCode, json);
    }

    private HttpClient CreateHttpClient(HttpStatusCode statusCode, string? content = null)
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
        };

        if (content != null)
        {
            response.Content = new StringContent(content);
        }

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_config.BaseUrl)
        };
    }
}
