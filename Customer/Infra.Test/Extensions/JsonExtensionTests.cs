using Application.Clients.DTOs.Request;
using Infra.Extension;

namespace Infra.Test.Extensions;

public class JsonExtensionTests
{
    [Fact]
    public void SerializeToLowercaseJson_Should_ReturnLowercaseJson_When_Called()
    {
        // Arrange
        var obj = GetRequestDto();
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"id\":\"id\",\"name\":\"TestName\",\"description\":\"TestDescription\"}", json);
    }

    [Fact]
    public void SerializeToLowercaseJson_Should_SerializeNullValues_When_Called()
    {
        // Arrange
        var obj = GetRequestDto();
        obj.Description = null;
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"id\":\"id\",\"name\":\"TestName\",\"description\":null}", json);
    }

    [Fact]
    public void SerializeToLowercaseJson_Should_HandleEmptyObject_When_Called()
    {
        // Arrange
        var obj = new SampleRequestDto();
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"id\":null,\"name\":null,\"description\":null}", json);
    }

    [Fact]
    public void SerializeToLowercaseJson_Should_HandleSpecialCharacters_When_Called()
    {
        // Arrange
        var obj = GetRequestDto();
        obj.Name = "Test@Name#1!";
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"id\":\"id\",\"name\":\"Test@Name#1!\",\"description\":\"TestDescription\"}", json);
    }

    [Fact]
    public void SerializeToLowerJson_Should_NotSerialize_When_NullObject()
    {
        // Arrange
        SampleRequestDto? obj = null;
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Empty(json);
    }

    private static SampleRequestDto GetRequestDto()
    {
        return new SampleRequestDto
        {
            Id = "id",
            Name = "TestName",
            Description = "TestDescription",
        };
    }
}
