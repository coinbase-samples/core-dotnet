namespace Coinbase.Core.Http
{
  using System;
  using System.IO;
  using System.Net.Http;
  using System.Net.Http.Headers;
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
    public static TimeSpan DefaultHttpTimeout => TimeSpan.FromSeconds(80);

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
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<CoinbaseResponse> SendAsyncRequest(
        CoinbaseHttpRequest request,
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
        CancellationToken cancellationToken)
    {
      Exception requestException;
      HttpResponseMessage response = null;
      requestException = null;

      var httpRequest = this.BuildRequestMessage(request);

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
        requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      }

      // Request body
      requestMessage.Content = new StringContent(request.Content);

      return requestMessage;
    }
  }
}
