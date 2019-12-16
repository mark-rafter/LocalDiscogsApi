using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using LocalDiscogsApi.Config;
using LocalDiscogsApi.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LocalDiscogsApi.Middleware
{
    public class PreventRateLimiterHandler : DelegatingHandler
    {
        private readonly IMemoryCache memoryCache;
        private readonly ITimerService timerService;

        // from DiscogsApiOptions
        private readonly TimeSpan ratelimitTimeout;
        private readonly string ratelimitRemainingHeaderName;

        public PreventRateLimiterHandler(IMemoryCache memoryCache, ITimerService timerService, IDiscogsApiOptions discogsApiOptions)
        {
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));

            this.ratelimitTimeout = TimeSpan.FromSeconds(discogsApiOptions.RatelimitTimeout);
            this.ratelimitRemainingHeaderName = discogsApiOptions.RatelimitRemainingHeaderName;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await CheckRateLimiterAndWait();

            HttpResponseMessage httpResponse = await base.SendAsync(request, cancellationToken);

            UpdateRateLimiter(httpResponse?.Headers);

            return httpResponse;
        }

        private async Task CheckRateLimiterAndWait()
        {
            Ratelimit ratelimit = memoryCache.Get<Ratelimit>(nameof(Ratelimit));

            if (ratelimit?.Remaining < 1)
            {
                /// wait until <see cref="ratelimitTimeout" /> has elapsed since the last request: 
                /// RatelimitTimeout - (now - lastRequestTime)
                DateTimeOffset nowUtc = DateTimeOffset.Now.ToUniversalTime();

                TimeSpan timeToWait = ratelimitTimeout - (nowUtc - ratelimit.LastRequestTimeUtc);

                if (timeToWait.TotalMilliseconds > 0)
                {
                    await timerService.Delay(timeToWait);
                }
            }
        }

        private void UpdateRateLimiter(HttpResponseHeaders headers)
        {
            IEnumerable<string> ratelimitRemainingHeader = null;
            headers?.TryGetValues(ratelimitRemainingHeaderName, out ratelimitRemainingHeader);

            if (int.TryParse(ratelimitRemainingHeader?.FirstOrDefault(), out int ratelimitRemaining))
            {
                DateTimeOffset requestTime = headers?.Date ?? DateTimeOffset.UtcNow;

                memoryCache.Set(
                        nameof(Ratelimit),
                        new Ratelimit(ratelimitRemaining, requestTime),
                        ratelimitTimeout
                        );
            }
        }

        public class Ratelimit
        {
            public Ratelimit(int remaining, DateTimeOffset lastRequestTime)
            {
                Remaining = remaining;
                LastRequestTimeUtc = lastRequestTime.ToUniversalTime();
            }

            public int Remaining { get; private set; }
            public DateTimeOffset LastRequestTimeUtc { get; private set; }
        }
    }
}
