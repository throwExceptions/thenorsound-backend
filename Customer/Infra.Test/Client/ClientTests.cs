//using Application.Clients.DTOs.Response;
//using Domain.Entities;
//using Infra.Clients;
//using Infra.Settings;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Moq;
//using Moq.Protected;
//using System.Net;
//using System.Text.Json;

//namespace Infra.Test.Client;

//public class ClientTests
//{
//    private readonly Mock<ILogger<SampleClient>> _loggerMock;
//    private readonly Mock<IOptions<SampleConfiguration>> _configMock;
//    private readonly SampleConfiguration _sampleConfig;

//    public ClientTests()
//    {
//        _loggerMock = new Mock<ILogger<SampleClient>>();
//        _sampleConfig = new SampleConfiguration
//        {
//            BaseUrl = "https://api.example.com",
//            ApiKey = "test-api-key",
//            GetByIdEndpoint = "samples/{0}",
//            CreateEndpoint = "samples"
//        };
//        _configMock = new Mock<IOptions<SampleConfiguration>>();
//        _configMock.Setup(x => x.Value).Returns(_sampleConfig);
//    }

//    [Fact]
//    public async Task GetAsync_Should_ReturnResponse_When_RequestIsSuccessful()
//    {
//        // Arrange
//        var expectedResponse = new SampleRespnseDto
//        {
//            Id = "id",
//            Name = "Test Sample",
//            Description = "Test Description"
//        };

//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.IsAny<HttpRequestMessage>(),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.OK,
//                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
//            });

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act
//        var result = await client.GetAsync<SampleRespnseDto>("samples/1");

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(expectedResponse.Id, result.Id);
//        Assert.Equal(expectedResponse.Name, result.Name);
//        Assert.Equal(expectedResponse.Description, result.Description);
//    }

//    [Fact]
//    public async Task GetAsync_Should_ReturnDefault_When_ResponseIsNotSuccessful()
//    {
//        // Arrange
//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.IsAny<HttpRequestMessage>(),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.NotFound,
//                ReasonPhrase = "Not Found"
//            });

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act
//        var result = await client.GetAsync<SampleRespnseDto>("samples/999");

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetAsync_Should_ReturnDefault_When_ResponseCannotBeDeserialized()
//    {
//        // Arrange
//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.IsAny<HttpRequestMessage>(),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.OK,
//                Content = new StringContent("null")
//            });

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act
//        var result = await client.GetAsync<SampleRespnseDto>("samples/1");

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task GetAsync_Should_ThrowHttpRequestException_WhenNetworkErrorOccurs()
//    {
//        // Arrange
//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.IsAny<HttpRequestMessage>(),
//                ItExpr.IsAny<CancellationToken>())
//            .ThrowsAsync(new HttpRequestException("Network error"));

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act & Assert
//        await Assert.ThrowsAsync<HttpRequestException>(() =>
//            client.GetAsync<SampleRespnseDto>("samples/1"));
//    }

//    [Fact]
//    public async Task PostAsync_Should_ReturnResponse_When_RequestIsSuccessful()
//    {
//        // Arrange
//        var sample = new Customer
//        {
//            Id = "id",
//            Name = "New Sample",
//            Description = "New Description"
//        };

//        var expectedResponse = new SampleRespnseDto
//        {
//            Id = "id",
//            Name = "New Sample",
//            Description = "New Description"
//        };

//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.Created,
//                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
//            });

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act
//        var result = await client.PostAsync<SampleRespnseDto>("samples", sample);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(expectedResponse.Id, result.Id);
//        Assert.Equal(expectedResponse.Name, result.Name);
//        Assert.Equal(expectedResponse.Description, result.Description);
//    }

//    [Fact]
//    public async Task PostAsync_Should_ReturnDefault_When_ResponseIsNotSuccessful()
//    {
//        // Arrange
//        var sample = new Customer
//        {
//            Name = "New Sample",
//            Description = "New Description"
//        };

//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.BadRequest,
//                ReasonPhrase = "Bad Request"
//            });

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act
//        var result = await client.PostAsync<SampleRespnseDto>("samples", sample);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task PostAsync_Should_ReturnDefault_When_ResponseCannotBeDeserialized()
//    {
//        // Arrange
//        var sample = new Customer
//        {
//            Name = "New Sample",
//            Description = "New Description"
//        };

//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
//                ItExpr.IsAny<CancellationToken>())
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.Created,
//                Content = new StringContent("null")
//            });

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act
//        var result = await client.PostAsync<SampleRespnseDto>("samples", sample);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task PostAsync_Should_ThrowHttpRequestException_WhenNetworkErrorOccurs()
//    {
//        // Arrange
//        var sample = new Customer
//        {
//            Name = "New Sample",
//            Description = "New Description"
//        };

//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
//                ItExpr.IsAny<CancellationToken>())
//            .ThrowsAsync(new HttpRequestException("Network error"));

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act & Assert
//        await Assert.ThrowsAsync<HttpRequestException>(() =>
//            client.PostAsync<SampleRespnseDto>("samples", sample));
//    }

//    [Fact]
//    public async Task PostAsync_Should_SendCorrectContentTypeHeader_When_Succesful()
//    {
//        // Arrange
//        var sample = new Customer
//        {
//            Name = "New Sample",
//            Description = "New Description"
//        };

//        HttpRequestMessage? capturedRequest = null;

//        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
//        httpMessageHandlerMock
//            .Protected()
//            .Setup<Task<HttpResponseMessage>>(
//                "SendAsync",
//                ItExpr.IsAny<HttpRequestMessage>(),
//                ItExpr.IsAny<CancellationToken>())
//            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
//            .ReturnsAsync(new HttpResponseMessage
//            {
//                StatusCode = HttpStatusCode.Created,
//                Content = new StringContent(JsonSerializer.Serialize(new SampleRespnseDto()))
//            });

//        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
//        {
//            BaseAddress = new Uri(_sampleConfig.BaseUrl)
//        };

//        var client = new SampleClient(httpClient, _loggerMock.Object, _configMock.Object);

//        // Act
//        await client.PostAsync<SampleRespnseDto>("samples", sample);

//        // Assert
//        Assert.NotNull(capturedRequest);
//        Assert.Equal("application/json", capturedRequest.Content?.Headers.ContentType?.MediaType);
//        Assert.Equal("utf-8", capturedRequest.Content?.Headers.ContentType?.CharSet);
//    }
//}
