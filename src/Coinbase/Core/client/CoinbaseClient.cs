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

namespace Coinbase.Core.Client
{
  using System;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using Coinbase.Core.Credentials;
  using Coinbase.Core.Error;
  using Coinbase.Core.Http;
  using Newtonsoft.Json;

  /// <summary>
  /// Interface that represents a Coinbase API Client.
  /// </summary>
  public abstract class CoinbaseClient : ICoinbaseClient
  {
    private readonly IHttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseClient"/> class.
    /// </summary>
    /// <param name="httpClient">Http Client, will default to <see cref="SystemNetHttpClient"/>.</param>
    /// <param name="coinbaseCredentials">Api Credentials.</param>
    /// <param name="apiBasePath">Base url path for the API.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the Credentials are not valid or a base path is not provided.
    /// </exception>
    public CoinbaseClient(
      CoinbaseCredentials coinbaseCredentials,
      string apiBasePath,
      IHttpClient httpClient = null)
    {
      this.Credentials = coinbaseCredentials ?? throw new ArgumentException("Credentials cannot be null", nameof(coinbaseCredentials));

      if (string.IsNullOrWhiteSpace(apiBasePath.Trim()))
      {
        throw new ArgumentException("API base path cannot be null or empty", nameof(apiBasePath));
      }

      this.ApiBasePath = apiBasePath;

      this.httpClient = httpClient ?? new SystemNetHttpClient();
    }

    /// <inheritdoc/>
    public string ApiBasePath { get; }

    /// <inheritdoc/>
    public CoinbaseCredentials Credentials { get; }

    public IHttpClient HttpClient { get; }

    /// <inheritdoc/>
    public async Task<T> SendRequestAsync<T>(
      HttpMethod method,
      string path,
      object options,
      HttpStatusCode[] expectedStatusCodes,
      CancellationToken cancellationToken)
    {
      CoinbaseHttpRequest request = new CoinbaseHttpRequest($"{this.ApiBasePath}{path}", method.Method, this.Credentials, options);

      // Send the HTTP request
      CoinbaseResponse response = await this.httpClient.SendAsyncRequest(request, cancellationToken);

      // If the response is successful return the content as type T
      if (!expectedStatusCodes.Contains(response.StatusCode))
      {
        try
        {
          var error = JsonConvert.DeserializeObject<CoinbaseErrorMessage>(response.Content);
          throw new CoinbaseClientException(error.Message);
        }
        catch (JsonReaderException)
        {
          throw new CoinbaseException(response.Content);
        }
      }

      return JsonConvert.DeserializeObject<T>(response.Content);
    }
  }
}
