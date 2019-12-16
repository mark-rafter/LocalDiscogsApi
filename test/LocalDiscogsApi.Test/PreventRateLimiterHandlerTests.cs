using FluentAssertions;
using LocalDiscogsApi.Middleware;
using LocalDiscogsApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static LocalDiscogsApi.Middleware.PreventRateLimiterHandler;

namespace LocalDiscogsApi.Test
{
    public class PreventRateLimiterHandlerTests : IDisposable
    {
        private readonly IMemoryCache memoryCache;
        private readonly Mock<ITimerService> timerServiceMock;
        private readonly Config.DiscogsApiOptions discogsApiOptions;

        private readonly Mock<HttpMessageHandler> mockInnerHandler;
        private readonly HttpClient fakeHttpClient;

        private readonly PreventRateLimiterHandler handler;

        public PreventRateLimiterHandlerTests()
        {
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            timerServiceMock = new Mock<ITimerService>();
            mockInnerHandler = new Mock<HttpMessageHandler>();

            discogsApiOptions = new Config.DiscogsApiOptions
            {
                Url = "https://api.discogs.fake",
                UserAgent = "UserAgent",
                RatelimitRemainingHeaderName = "RatelimitRemainingHeaderName",
                RatelimitTimeout = 60
            };

            handler = new PreventRateLimiterHandler(memoryCache, timerServiceMock.Object, discogsApiOptions)
            {
                InnerHandler = mockInnerHandler.Object
            };

            fakeHttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(discogsApiOptions.Url)
            };
        }

        public void Dispose()
        {
            fakeHttpClient.Dispose();
        }

        [Fact]
        public async Task SendAsync_ReturnsWithNoHeaders_DoesNotCache()
        {
            // Arrange
            SetupInnerHandlerHttpResponseHeaders(null, null);

            // Act
            HttpResponseMessage result = await fakeHttpClient.SendAsync(new HttpRequestMessage());

            Ratelimit cacheObject = memoryCache.Get<Ratelimit>(nameof(Ratelimit));

            // Assert
            cacheObject.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_ReturnsWithHeaders_StoresRemainingInCache()
        {
            // Arrange
            const int expectedRemaining = 45;

            // Arrange
            SetupInnerHandlerHttpResponseHeaders(expectedRemaining, null);

            // Act
            await fakeHttpClient.SendAsync(new HttpRequestMessage());

            Ratelimit cacheObject = memoryCache.Get<Ratelimit>(nameof(Ratelimit));

            // Assert
            cacheObject.Remaining.Should().Be(expectedRemaining);
        }

        [Fact]
        public async Task SendAsync_RateLimitNotHit_DoesNotDelay()
        {
            // Arrange
            SetupInnerHandlerHttpResponseHeaders(50, null);

            // simulate a previous http request in order to populate the cache
            await fakeHttpClient.SendAsync(new HttpRequestMessage());

            // Act
            await fakeHttpClient.SendAsync(new HttpRequestMessage());

            // Assert
            timerServiceMock.Verify(x => x.Delay(It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_RateLimitHitMoreThan60sAgo_DoesNotDelay()
        {
            // Arrange
            DateTimeOffset lastRequestTime = DateTimeOffset.Now.AddSeconds(-discogsApiOptions.RatelimitTimeout);

            SetupInnerHandlerHttpResponseHeaders(0, lastRequestTime);

            // simulate a previous http request in order to populate the cache
            await fakeHttpClient.SendAsync(new HttpRequestMessage());

            // Act
            await fakeHttpClient.SendAsync(new HttpRequestMessage());

            // Assert
            timerServiceMock.Verify(x => x.Delay(It.IsAny<TimeSpan>()), Times.Never);
        }

        /// <summary>
        /// Warning: test assumes that <see cref="discogsApiOptions.RateLimitTimeout"/> is 60 seconds.
        /// Warning: test is measuring elapsed time between the two SendAsync calls, therefore
        /// a sufficient delay (due to debugging or unperformant code) could cause unintentional failure.
        /// </summary>
        [Theory]
        [InlineData(0, 60)]
        [InlineData(10, 50)]
        [InlineData(30, 30)]
        [InlineData(59, 1)]
        public async Task SendAsync_RateLimitHitLessThan60sAgo_DelaysForCorrectTime(int lastCallSecondsAgo, int expectedDelay)
        {
            // Arrange
            DateTimeOffset lastRequestTime = DateTimeOffset.Now.AddSeconds(-lastCallSecondsAgo);
            TimeSpan expectedDelayLowerLimit = TimeSpan.FromSeconds(expectedDelay - 2);
            TimeSpan expectedDelayUpperLimit = TimeSpan.FromSeconds(expectedDelay);

            SetupInnerHandlerHttpResponseHeaders(0, lastRequestTime);

            // simulate a previous http request in order to populate the cache
            await fakeHttpClient.SendAsync(new HttpRequestMessage());

            // Act
            await fakeHttpClient.SendAsync(new HttpRequestMessage());

            // Assert
            timerServiceMock
                .Verify(x => x.Delay(It.IsInRange(expectedDelayLowerLimit, expectedDelayUpperLimit, Moq.Range.Exclusive)),
                    Times.Once);
        }

        private void SetupInnerHandlerHttpResponseHeaders(int? ratelimitRemaining, DateTimeOffset? lastRequestTime)
        {
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            if (ratelimitRemaining.HasValue)
                httpResponse.Headers.Add(discogsApiOptions.RatelimitRemainingHeaderName, ratelimitRemaining.ToString());

            if (lastRequestTime.HasValue)
                httpResponse.Headers.Date = lastRequestTime;

            mockInnerHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    nameof(HttpClient.SendAsync),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);
        }
    }
}
