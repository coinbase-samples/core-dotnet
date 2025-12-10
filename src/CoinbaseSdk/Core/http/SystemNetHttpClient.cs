/*
 * Copyright 2024-present Coinbase Global, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace CoinbaseSdk.Core.Http
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Standard HTTP client implementation for Coinbase API requests.
    /// Supports optional Polly-based retries via <see cref="CallOptions"/>.
    /// See the core-dotnet README for retry configuration details.
    /// </summary>
    public class SystemNetHttpClient : IHttpClient
    {
        private static readonly Lazy<HttpClient> LazyDefaultHttpClient
            = new Lazy<HttpClient>(BuildDefaultSystemNetHttpClient);

        private readonly HttpClient httpClient;
        private readonly CallOptions defaultCallOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemNetHttpClient"/> class.
        /// </summary>
        /// <param name="httpClient">
        /// The <see cref="HttpClient"/> client to use. If <c>null</c>, an HTTP
        /// client will be created with default parameters.
        /// </param>
        /// <param name="timeout">
        /// The timespan before the request times out.
        /// </param>
        /// <param name="defaultCallOptions">
        /// Optional default retry configuration applied to every request unless
        /// a per-request <see cref="CallOptions"/> override is supplied.
        /// </param>
        public SystemNetHttpClient(
            HttpClient httpClient = null,
            TimeSpan? timeout = null,
            CallOptions defaultCallOptions = null)
        {
            this.httpClient = httpClient ?? LazyDefaultHttpClient.Value;
            this.defaultCallOptions = defaultCallOptions;

            if (timeout.HasValue)
            {
                this.httpClient.Timeout = timeout.Value;
            }
        }

        /// <summary>Default timespan before the request times out.</summary>
        public static TimeSpan DefaultHttpTimeout => HttpClientDefaults.RequestTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClient"/> class
        /// with default parameters.
        /// </summary>
        /// <returns>The new instance of the <see cref="HttpClient"/> class.</returns>
        public static HttpClient BuildDefaultSystemNetHttpClient()
        {
            return new HttpClient
            {
                Timeout = DefaultHttpTimeout,
            };
        }

        /// <summary>Sends a request to Coinbase API as an asynchronous operation.</summary>
        /// <param name="request">The request to send.</param>
        /// <param name="callOptions">
        /// Optional retry configuration. If null or MaxRetries = 0, the request executes once.
        /// See <see cref="CallOptions"/> for retry settings.
        /// </param>
        /// <param name="cancellationToken">
        /// Cancellation token. Canceling stops any pending retry attempts immediately.
        /// </param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<CoinbaseResponse> SendAsyncRequest(
            CoinbaseHttpRequest request,
            CallOptions callOptions = null,
            CancellationToken cancellationToken = default)
        {
            var effectiveCallOptions = callOptions ?? this.defaultCallOptions;
            var response = await this.SendHttpRequest(request, effectiveCallOptions, cancellationToken).ConfigureAwait(false);

            using (response)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var reader = new StreamReader(responseStream);
                var responseBody = await reader.ReadToEndAsync().ConfigureAwait(false);

                return new CoinbaseResponse(
                    response.StatusCode,
                    response.Headers,
                    responseBody);
            }
        }

        private async Task<HttpResponseMessage> SendHttpRequest(
            CoinbaseHttpRequest request,
            CallOptions callOptions,
            CancellationToken cancellationToken)
        {
            var policy = PollyRetryPolicyProvider.Instance.BuildPolicy(callOptions, cancellationToken);

            return await policy.ExecuteAsync(
                async ct =>
                {
                    using var httpRequest = this.BuildRequestMessage(request);
                    return await this.httpClient.SendAsync(httpRequest, ct).ConfigureAwait(false);
                },
                cancellationToken).ConfigureAwait(false);
        }

        private HttpRequestMessage BuildRequestMessage(CoinbaseHttpRequest request)
        {
            var requestMessage = new HttpRequestMessage(request.Method, request.Uri);

            foreach (var header in request.Headers)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }

            if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
            {
                requestMessage.Content = new StringContent(request.Content, Encoding.UTF8, "application/json");
            }

            return requestMessage;
        }
    }
}
