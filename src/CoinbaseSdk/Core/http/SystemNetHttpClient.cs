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
  using System.Net;
  using System.Net.Http;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;

  /// <summary>
  /// Standard client to make requests to Coinbase's API, using
  /// <see cref="HttpClient"/> to send HTTP requests. This is a simple client
  /// that does NOT include any retry logic.
  /// </summary>
  public class SystemNetHttpClient : IHttpClient
  {
    private static readonly Lazy<HttpClient> LazyDefaultHttpClient
        = new Lazy<HttpClient>(BuildDefaultSystemNetHttpClient);

    private readonly HttpClient httpClient;

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
    public SystemNetHttpClient(
        HttpClient httpClient = null,
        TimeSpan? timeout = null)
    {
      this.httpClient = httpClient ?? LazyDefaultHttpClient.Value;

      if (timeout.HasValue)
      {
        this.httpClient.Timeout = timeout.Value;
      }
    }

    /// <summary>Default timespan before the request times out.</summary>
    public static TimeSpan DefaultHttpTimeout => TimeSpan.FromSeconds(15);

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
    /// <param name="request">The parameters of the request to send.</param>
    /// <param name="callOptions">The configured <see cref="CallOptions"/>.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<CoinbaseResponse> SendAsyncRequest(
        CoinbaseHttpRequest request,
        CallOptions callOptions = null,
        CancellationToken cancellationToken = default)
    {
      var response = await this.SendHttpRequest(request, cancellationToken).ConfigureAwait(false);

      var reader = new StreamReader(
          await response.Content.ReadAsStreamAsync().ConfigureAwait(false));

      return new CoinbaseResponse(
          response.StatusCode,
          response.Headers,
          await reader.ReadToEndAsync().ConfigureAwait(false));
    }

    private async Task<HttpResponseMessage> SendHttpRequest(
        CoinbaseHttpRequest request,
        CancellationToken cancellationToken,
        CallOptions callOptions = null)
    {
      Exception requestException;
      HttpResponseMessage response = null;
      requestException = null;
      callOptions ??= new CallOptions();
      var retry = 0;

      var httpRequest = this.BuildRequestMessage(request);

      while (true)
      {
        try
        {
          response = await this.httpClient.SendAsync(httpRequest, cancellationToken)
              .ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
          requestException = exception;
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
          requestException = exception;
        }

        if (!this.ShouldRetry(
                    callOptions,
                    retry,
                    requestException != null,
                    response?.StatusCode))
        {
          break;
        }

        retry += 1;

        // Calculate the exponential backoff delay
        var delay = TimeSpan.FromTicks(callOptions.MinNetworkRetriesDelay.Ticks * (1L << (retry - 1)));

        // Clamp the delay between the minimum and maximum delay values
        if (delay < callOptions.MinNetworkRetriesDelay)
        {
          delay = callOptions.MinNetworkRetriesDelay;
        }
        else if (delay > callOptions.MaxNetworkRetriesDelay)
        {
          delay = callOptions.MaxNetworkRetriesDelay;
        }

        await Task.Delay(delay, cancellationToken);
      }

      if (requestException != null)
      {
        throw requestException;
      }

      return response;
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

    private bool ShouldRetry(
        CallOptions callOptions,
        int numRetries,
        bool requestException,
        HttpStatusCode? statusCode)
    {
      if (numRetries >= callOptions.MaxRetries)
      {
        return false;
      }

      if (requestException)
      {
        return true;
      }

      if (!callOptions.ShouldRetryOnStatusCodes)
      {
        return false;
      }

      if (statusCode.HasValue && callOptions.RetryableStatusCodes.Contains(statusCode.Value))
      {
        return true;
      }

      return false;
    }
  }
}
