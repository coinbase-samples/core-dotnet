using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoinbaseSdk.Core.Credentials;
using CoinbaseSdk.Core.Http;
using CoinbaseSdk.Core.Serialization;
using CoinbaseSdk.Core.Tests.TestUtilities;
using Xunit;

namespace CoinbaseSdk.Core.Tests.Http
{
    public class SystemNetHttpClientTests
    {
        private static readonly CoinbaseCredentials DefaultCredentials = new("access", "passphrase", "signingKey");
        private static readonly IJsonUtility JsonUtility = new JsonUtility();

        [Fact]
        public async Task SendAsyncRequest_NoRetryConfigured_DoesNotRetryOnHttpException()
        {
            var handler = new StubHttpMessageHandler((_, _, _) => throw new HttpRequestException("boom"));
            var httpClient = new HttpClient(handler);
            var client = new SystemNetHttpClient(httpClient);

            await Assert.ThrowsAsync<HttpRequestException>(
                () => client.SendAsyncRequest(this.CreateRequest(), callOptions: null, cancellationToken: CancellationToken.None));

            Assert.Equal(1, handler.CallCount);
        }

        [Fact]
        public async Task SendAsyncRequest_RetryConfigured_RetriesOnHttpException()
        {
            var handler = new StubHttpMessageHandler((attempt, _, _) =>
            {
                if (attempt < 2)
                {
                    throw new HttpRequestException("boom");
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"status\":\"ok\"}", Encoding.UTF8, "application/json"),
                });
            });

            var httpClient = new HttpClient(handler);
            var client = new SystemNetHttpClient(httpClient);
            var callOptions = new CallOptions
            {
                MaxRetries = 2,
            };

            var response = await client.SendAsyncRequest(
                this.CreateRequest(),
                callOptions,
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, handler.CallCount);
        }

        [Fact]
        public async Task SendAsyncRequest_RetriesOnConfiguredStatusCodes()
        {
            var handler = new StubHttpMessageHandler((attempt, _, _) =>
            {
                if (attempt == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"status\":\"ok\"}", Encoding.UTF8, "application/json"),
                });
            });

            var httpClient = new HttpClient(handler);
            var client = new SystemNetHttpClient(httpClient);
            var callOptions = new CallOptions
            {
                MaxRetries = 2,
                ShouldRetryOnStatusCodes = true,
                RetryableStatusCodes = new HashSet<HttpStatusCode> { HttpStatusCode.ServiceUnavailable },
            };

            var response = await client.SendAsyncRequest(
                this.CreateRequest(),
                callOptions,
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, handler.CallCount);
        }

        [Fact]
        public async Task SendAsyncRequest_UsesDefaultCallOptionsWhenNoneProvided()
        {
            var handler = new StubHttpMessageHandler((attempt, _, _) =>
            {
                if (attempt < 2)
                {
                    throw new HttpRequestException("boom");
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"status\":\"ok\"}", Encoding.UTF8, "application/json"),
                });
            });

            var httpClient = new HttpClient(handler);
            var defaultOptions = new CallOptions
            {
                MaxRetries = 1,
            };

            var client = new SystemNetHttpClient(httpClient, defaultCallOptions: defaultOptions);

            var response = await client.SendAsyncRequest(
                this.CreateRequest(),
                callOptions: null,
                CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, handler.CallCount);
        }

        [Fact]
        public async Task SendAsyncRequest_PerRequestOverridesDefaultCallOptions()
        {
            var handler = new StubHttpMessageHandler((attempt, _, _) =>
            {
                throw new HttpRequestException($"boom-{attempt}");
            });

            var httpClient = new HttpClient(handler);
            var defaultOptions = new CallOptions
            {
                MaxRetries = 2,
            };

            var client = new SystemNetHttpClient(httpClient, defaultCallOptions: defaultOptions);

            var perRequestOptions = new CallOptions
            {
                MaxRetries = 0,
            };

            await Assert.ThrowsAsync<HttpRequestException>(
                () => client.SendAsyncRequest(this.CreateRequest(), perRequestOptions, CancellationToken.None));

            Assert.Equal(1, handler.CallCount);
        }

        private CoinbaseHttpRequest CreateRequest()
        {
            return new CoinbaseHttpRequest(
                "api.coinbase.com/v2/user",
                HttpMethod.Get.Method,
                DefaultCredentials,
                request: null,
                jsonUtility: JsonUtility);
        }
    }
}
