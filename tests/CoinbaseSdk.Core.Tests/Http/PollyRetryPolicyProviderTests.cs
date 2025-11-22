using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CoinbaseSdk.Core.Http;
using Xunit;

namespace CoinbaseSdk.Core.Tests.Http
{
    public class PollyRetryPolicyProviderTests
    {
        [Fact]
        public async Task BuildPolicy_NoRetryConfiguration_ExecutesOnce()
        {
            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions: null, CancellationToken.None);
            var attempts = 0;

            using var response = await policy.ExecuteAsync(ct =>
            {
                attempts++;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }, CancellationToken.None);

            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task BuildPolicy_RetriesOnHttpRequestException()
        {
            var callOptions = new CallOptions
            {
                MaxRetries = 2,
            };

            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions, CancellationToken.None);
            var attempts = 0;

            using var response = await policy.ExecuteAsync(async ct =>
            {
                attempts++;
                if (attempts <= 2)
                {
                    throw new HttpRequestException("boom");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }, CancellationToken.None);

            Assert.Equal(3, attempts);
        }

        [Fact]
        public async Task BuildPolicy_RetriesOnConfiguredStatusCodes()
        {
            var callOptions = new CallOptions
            {
                MaxRetries = 1,
                ShouldRetryOnStatusCodes = true,
                RetryableStatusCodes = new HashSet<HttpStatusCode>
                {
                    HttpStatusCode.ServiceUnavailable,
                },
            };

            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions, CancellationToken.None);
            var attempts = 0;

            using var response = await policy.ExecuteAsync(ct =>
            {
                attempts++;
                if (attempts == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }, CancellationToken.None);

            Assert.Equal(2, attempts);
        }

        [Fact]
        public async Task BuildPolicy_DoesNotRetryWhenOuterTokenCanceled()
        {
            var callOptions = new CallOptions
            {
                MaxRetries = 3,
            };

            using var cts = new CancellationTokenSource();
            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions, cts.Token);
            var attempts = 0;

            await Assert.ThrowsAsync<OperationCanceledException>(() => policy.ExecuteAsync(ct =>
            {
                attempts++;
                cts.Cancel();
                throw new OperationCanceledException(ct);
            }, cts.Token));

            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task BuildPolicy_RetriesOnTimeout_WhenOuterTokenNotCanceled()
        {
            var callOptions = new CallOptions
            {
                MaxRetries = 2,
                MedianFirstRetryDelay = TimeSpan.Zero,
                MaxRetryDelay = TimeSpan.Zero,
            };

            // Simulate a timeout by throwing OCE with a canceled token that is NOT the outer token
            var timeoutCts = new CancellationTokenSource();
            timeoutCts.Cancel();

            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions, CancellationToken.None);
            var attempts = 0;

            using var response = await policy.ExecuteAsync(ct =>
            {
                attempts++;
                if (attempts <= 2)
                {
                    throw new OperationCanceledException(timeoutCts.Token);
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }, CancellationToken.None);

            Assert.Equal(3, attempts);
        }

        [Fact]
        public async Task BuildPolicy_RetriesOnOperationCanceledWhenInnerTokenNotCanceled()
        {
            var callOptions = new CallOptions
            {
                MaxRetries = 1,
                MedianFirstRetryDelay = TimeSpan.Zero,
                MaxRetryDelay = TimeSpan.Zero,
            };

            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions, CancellationToken.None);
            var attempts = 0;

            using var response = await policy.ExecuteAsync(ct =>
            {
                attempts++;
                if (attempts == 1)
                {
                    throw new OperationCanceledException(CancellationToken.None);
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }, CancellationToken.None);

            Assert.Equal(2, attempts);
        }

        [Fact]
        public async Task BuildPolicy_RespectsMaxDelayBounds()
        {
            var callOptions = new CallOptions
            {
                MaxRetries = 3,
                MedianFirstRetryDelay = TimeSpan.FromMilliseconds(10),
                MaxRetryDelay = TimeSpan.FromMilliseconds(50),
            };

            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions, CancellationToken.None);
            var attempts = 0;
            var delays = new List<TimeSpan>();
            var lastAttemptTime = DateTime.UtcNow;

            await Assert.ThrowsAsync<HttpRequestException>(() => policy.ExecuteAsync(ct =>
            {
                var now = DateTime.UtcNow;
                if (attempts > 0)
                {
                    delays.Add(now - lastAttemptTime);
                }

                lastAttemptTime = now;
                attempts++;
                throw new HttpRequestException("boom");
            }, CancellationToken.None));

            Assert.Equal(4, attempts);
            Assert.Equal(3, delays.Count);

            foreach (var delay in delays)
            {
                Assert.True(delay <= callOptions.MaxRetryDelay.Add(TimeSpan.FromMilliseconds(50)),
                    $"Delay {delay.TotalMilliseconds}ms should not exceed max of {callOptions.MaxRetryDelay.TotalMilliseconds}ms (plus tolerance)");
            }
        }
    }
}

