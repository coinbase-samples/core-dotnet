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

namespace CoinbaseSdk.Core.Client
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using CoinbaseSdk.Core.Credentials;
    using CoinbaseSdk.Core.Error;
    using CoinbaseSdk.Core.Http;
    using CoinbaseSdk.Core.Serialization;

    /// <summary>
    /// Interface that represents a Coinbase API Client.
    /// </summary>
    public abstract class CoinbaseClient : ICoinbaseClient
    {
        private readonly IHttpClient httpClient;
        private readonly IJsonUtility jsonUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoinbaseClient"/> class.
        /// </summary>
        /// <param name="coinbaseCredentials">Api Credentials.</param>
        /// <param name="apiBasePath">Base url path for the API.</param>
        /// <param name="jsonUtility">A Utility to modify how Json is serialized/deserialized.</param>
        /// <param name="httpClient">Http Client, will default to <see cref="SystemNetHttpClient"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the Credentials are not valid or a base path is not provided.
        /// </exception>
        public CoinbaseClient(
          CoinbaseCredentials coinbaseCredentials,
          string apiBasePath,
          IJsonUtility jsonUtility = null,
          IHttpClient httpClient = null)
        {
            this.Credentials = coinbaseCredentials ?? throw new ArgumentException("Credentials cannot be null", nameof(coinbaseCredentials));

            if (string.IsNullOrWhiteSpace(apiBasePath))
            {
                throw new ArgumentException("API base path cannot be null or empty", nameof(apiBasePath));
            }

            this.ApiBasePath = apiBasePath;

            this.httpClient = httpClient ?? new SystemNetHttpClient();
            this.jsonUtility = jsonUtility ?? new JsonUtility();
        }

        /// <inheritdoc/>
        public string ApiBasePath { get; }

        /// <inheritdoc/>
        public CoinbaseCredentials Credentials { get; }

        /// <summary>
        /// Gets the HTTP client used to send requests. Protected for use by derived classes.
        /// </summary>
        protected IHttpClient HttpClient => this.httpClient;

        /// <summary>
        /// Gets the JSON utility used for serialization. Protected for use by derived classes.
        /// </summary>
        protected IJsonUtility JsonUtility => this.jsonUtility;

        /// <inheritdoc/>
        public virtual async Task<T> SendRequestAsync<T>(
          HttpMethod method,
          string path,
          object options,
          HttpStatusCode[] expectedStatusCodes,
          CancellationToken cancellationToken,
#nullable enable
          CallOptions? callOptions = null)
#nullable disable
        {
            CoinbaseHttpRequest request = this.BuildRequest(method, path, options);
            this.ConfigureRequest(request);

            CoinbaseResponse response = await this.SendHttpRequestAsync(request, callOptions, cancellationToken);

            this.ValidateResponse(response, expectedStatusCodes);

            return this.jsonUtility.Deserialize<T>(response.Content);
        }

        /// <summary>
        /// Builds the initial <see cref="CoinbaseHttpRequest"/> from the provided parameters.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="path">API path.</param>
        /// <param name="options">Request parameters.</param>
        /// <returns>The configured request.</returns>
        protected virtual CoinbaseHttpRequest BuildRequest(HttpMethod method, string path, object options)
        {
            return new CoinbaseHttpRequest(
              $"{this.ApiBasePath}{path}",
              method.Method,
              this.Credentials,
              options,
              this.jsonUtility);
        }

        /// <summary>
        /// Configures the request before sending (e.g., adding custom headers).
        /// Override this method to add client-specific headers or request modifications.
        /// </summary>
        /// <param name="request">The request to configure.</param>
        protected virtual void ConfigureRequest(CoinbaseHttpRequest request)
        {
            // Default implementation does nothing; subclasses can override
        }

        /// <summary>
        /// Sends the HTTP request and handles exceptions.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="callOptions">Retry options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The response.</returns>
        /// <exception cref="CoinbaseClientException">Thrown when the HTTP request fails.</exception>
        protected virtual async Task<CoinbaseResponse> SendHttpRequestAsync(
            CoinbaseHttpRequest request,
            CallOptions callOptions,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.SendAsyncRequest(request, callOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CoinbaseClientException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Validates the response status code and throws an exception if it's not expected.
        /// Override this method to customize error handling or deserialization.
        /// </summary>
        /// <param name="response">The response to validate.</param>
        /// <param name="expectedStatusCodes">Expected status codes.</param>
        /// <exception cref="CoinbaseException">Thrown when the response status is not expected.</exception>
        protected virtual void ValidateResponse(CoinbaseResponse response, HttpStatusCode[] expectedStatusCodes)
        {
            if (!expectedStatusCodes.Contains(response.StatusCode))
            {
                throw new CoinbaseException(response.StatusCode, response.Content);
            }
        }
    }
}
