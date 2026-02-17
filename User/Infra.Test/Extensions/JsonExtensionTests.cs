using Infra.Extension;

namespace Infra.Test.Extensions;

public class JsonExtensionTests
{
    [Fact]
    public void SerializeToLowercaseJson_Should_ReturnLowercaseJson_When_Called()
    {
        // Arrange
        var obj = GetTestDto();
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"firstName\":\"Test\",\"lastName\":\"Testsson\",\"email\":\"test@example.com\"}", json);
    }

    [Fact]
    public void SerializeToLowercaseJson_Should_SerializeNullValues_When_Called()
    {
        // Arrange
        var obj = GetTestDto();
        obj.Email = null;
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"firstName\":\"Test\",\"lastName\":\"Testsson\",\"email\":null}", json);
    }

    [Fact]
    public void SerializeToLowercaseJson_Should_HandleEmptyObject_When_Called()
    {
        // Arrange
        var obj = new TestDto();
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"firstName\":null,\"lastName\":null,\"email\":null}", json);
    }

    [Fact]
    public void SerializeToLowercaseJson_Should_HandleSpecialCharacters_When_Called()
    {
        // Arrange
        var obj = GetTestDto();
        obj.FirstName = "Test@Name#1!";
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Equal("{\"firstName\":\"Test@Name#1!\",\"lastName\":\"Testsson\",\"email\":\"test@example.com\"}", json);
    }

    [Fact]
    public void SerializeToLowerJson_Should_NotSerialize_When_NullObject()
    {
        // Arrange
        TestDto? obj = null;
        // Act
        var json = obj.SerializeToLowercaseJson();
        // Assert
        Assert.Empty(json);
    }

    private static TestDto GetTestDto()
    {
        return new TestDto
        {
            FirstName = "Test",
            LastName = "Testsson",
            Email = "test@example.com",
        };
    }

    private class TestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }
}
