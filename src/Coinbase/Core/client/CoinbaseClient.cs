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
  using System.Net.Http;
  using System.Threading.Tasks;
  using Coinbase.Core.Credentials;
  using Coinbase.Core.Error;
  using Newtonsoft.Json;

  /// <summary>
  /// Interface that represents a Coinbase API Client.
  /// </summary>
  public class CoinbaseClient : ICoinbaseClient
  {
    private readonly IHttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseClient"/> class.
    /// </summary>
    /// <param name="httpClient">Http Client.</param>
    /// <param name="coinbaseCredentials">Api Credentials.</param>
    /// <param name="apiBasePath">Base url path for the API.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the Credentials are not valid or a base path is not provided.
    /// </exception>
    public CoinbaseClient(
      IHttpClient httpClient,
      string coinbaseCredentials = null,
      string apiBasePath = null)
    {
      if (coinbaseCredentials == null)
      {
        throw new ArgumentException("Credentials cannot be null", nameof(coinbaseCredentials));
      }

      if (string.IsNullOrEmpty(apiBasePath.Trim()))
      {
        throw new ArgumentException("API base path cannot be null or empty", nameof(apiBasePath));
      }

      this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public string ApiBasePath { get; }

    /// <inheritdoc/>
    public CoinbaseCredentials Credentials { get; }

    public IHttpClient HttpClient { get; }

    /// <inheritdoc/>
    public async Task<T> SendRequestAsync<T>(HttpMethod method, string requestUri, object options = null)
    {
      // Send the HTTP request
      HttpResponseMessage response = await httpClient.SendAsync(method, requestUri, options);

      if (response.IsSuccessStatusCode)
      {
        try
        {
          string responseBody = await response.Content.ReadAsStringAsync();
          return JsonConvert.DeserializeObject<T>(responseBody);
        }
        catch (Newtonsoft.Json.JsonException)
        {
          throw new CoinbaseException(
              "Error deserializing response body",
              response.StatusCode,
              response.ReasonPhrase);
        }
      }
      else
      {
        // Handle the error
        throw new Exception("Error");
      }
    }
  }
}
