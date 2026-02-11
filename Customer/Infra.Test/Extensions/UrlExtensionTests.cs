using Application.Clients.DTOs.Request;
using Infra.Extension;

namespace Infra.Test.Extensions
{
    public class UrlExtensionTests
    {
        [Fact]
        public void AppendQueryString_Should_ReturnCorrectUrl_When_GivenValidParameters()
        {
            // Arrange
            var baseUrl = "https://api.example.com/resource";
            var requestDto = GetRequestDto();
            // Act
            var resultUrl = baseUrl.AppendQueryString(requestDto);
            // Assert
            var expectedUrl = "https://api.example.com/resource?id=id&name=TestName&description=TestDescription";
            Assert.Equal(expectedUrl, resultUrl);
        }

        [Fact]
        public void AppendQueryString_Should_ReturnNoQueryString_When_EmptyDto()
        {
            // Arrange
            var baseUrl = "https://api.example.com/resource";
            var requestDto = new SampleRequestDto();
            // Act
            var resultUrl = baseUrl.AppendQueryString(requestDto);
            // Assert
            var expectedUrl = "https://api.example.com/resource";
            Assert.Equal(expectedUrl, resultUrl);
        }

        [Fact]
        public void AppendQueryString_ReturnNoQueryString_When_NullDto()
        {
            // Arrange
            var baseUrl = "https://api.example.com/resource";
            SampleRequestDto? requestDto = null;
            // Act
            var resultUrl = baseUrl.AppendQueryString(requestDto);
            // Assert
            var expectedUrl = "https://api.example.com/resource";
            Assert.Equal(expectedUrl, resultUrl);
        }

        private static SampleRequestDto GetRequestDto()
        {
            return new SampleRequestDto
            {
                Name = "TestName",
                Description = "TestDescription",
                Id = "id",
            };
        }
    }
}
