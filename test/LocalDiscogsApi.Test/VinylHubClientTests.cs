using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LocalDiscogsApi.Clients;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Moq.Protected;
using Xunit;

namespace LocalDiscogsApi.Test
{
    public class VinylHubClientTests
    {
        private readonly Mock<HttpMessageHandler> mockHandler;

        private readonly IVinylHubClient client;

        public VinylHubClientTests()
        {
            mockHandler = new Mock<HttpMessageHandler>();

            HttpClient fakeHttpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri(TestData.ApiBaseAddress)
            };

            Mock<IWebHostEnvironment> envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(x => x.ContentRootPath).Returns("fake/path");

            client = new VinylHubClient(fakeHttpClient, envMock.Object);
        }

        [Fact]
        public async Task GetSellerNameByDocId_CallsVinylHubWithDocId()
        {
            // Arrange
            int docid = 1234;

            var expectedUri = new Uri($"{TestData.ApiBaseAddress}/shop/{docid}");

            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("dummy content", Encoding.UTF8)
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            string result = await client.GetSellerNameByDocId(docid);

            // Assert
            mockHandler.Protected().Verify(
                nameof(HttpClient.SendAsync),
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData("... \"https://www.discogs.com/seller/name.of-the_seller123/profile/\"}], \"user\": false}'/>", "name.of-the_seller123")]
        [InlineData("... \"https://www.discogs.com/seller/name.of-the_seller123/profile\"}], \"user\": false}'/>", "name.of-the_seller123")]
        [InlineData("... \"https://www.discogs.com/seller/name.of-the_seller123/\"}], \"user\": false}'/>", "name.of-the_seller123")]
        [InlineData("... \"https://www.discogs.com/seller/name.of-the_seller123\"}], \"user\": false}'/>", "name.of-the_seller123")]
        [InlineData("... \"https://www.discogs.com/seller/\"}], \"user\": false}'/>", null)]
        [InlineData("... \"\"}], \"user\": false}'/>", null)]
        public async Task GetSellerNameByDocId_ReturnsSellerNameIfFound(string content, string expectedSellerName)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content, Encoding.UTF8)
            };

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            string result = await client.GetSellerNameByDocId(1234);

            // Assert
            result.Should().Be(expectedSellerName);
        }

        private static class TestData
        {
            public const string ApiBaseAddress = "https://api.vinylhub.fake";
        }
    }
}