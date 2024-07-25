namespace Coinbase.Core.Http
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Net;
  using System.Net.Http;
  using System.Net.Http.Headers;
  using System.Threading;
  using System.Threading.Tasks;

  /// <summary>
  /// Standard client to make requests to Coinbase's API, using
  /// <see cref="HttpClient"/> to send HTTP requests. It can
  /// automatically retry failed requests when it's safe to do so.
  /// </summary>
  public class SystemNetHttpClient : IHttpClient
  {
    /// <summary>Default maximum number of retries made by the client.</summary>
    public const int DefaultMaxNumberRetries = 2;

    private static readonly Lazy<HttpClient> LazyDefaultHttpClient
        = new Lazy<HttpClient>(BuildDefaultSystemNetHttpClient);

    private readonly HttpClient httpClient;

    private readonly object randLock = new object();

    private readonly Random rand = new Random();

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemNetHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// The <see cref="HttpClient"/> client to use. If <c>null</c>, an HTTP
    /// client will be created with default parameters.
    /// </param>
    /// <param name="maxNetworkRetries">
    /// The maximum number of times the client will retry requests that fail due to an
    /// intermittent problem.
    /// </param>
    /// <param name="timeout">
    /// The timespan before the request times out.
    /// </param>
    public SystemNetHttpClient(
        HttpClient httpClient = null,
        int maxNetworkRetries = DefaultMaxNumberRetries,
        TimeSpan? timeout = null)
    {
      this.httpClient = httpClient ?? LazyDefaultHttpClient.Value;
      this.MaxNetworkRetries = maxNetworkRetries;

      if (timeout.HasValue)
      {
        this.httpClient.Timeout = timeout.Value;
      }
    }

    /// <summary>Default timespan before the request times out.</summary>
    public static TimeSpan DefaultHttpTimeout => TimeSpan.FromSeconds(80);

    /// <summary>
    /// Maximum sleep time between tries to send HTTP requests after network failure.
    /// </summary>
    public static TimeSpan MaxNetworkRetriesDelay => TimeSpan.FromSeconds(5);

    /// <summary>
    /// Minimum sleep time between tries to send HTTP requests after network failure.
    /// </summary>
    public static TimeSpan MinNetworkRetriesDelay => TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets how many network retries were configured for this client.
    /// </summary>
    public int MaxNetworkRetries { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the client should sleep between automatic
    /// request retries.
    /// </summary>
    /// <remarks>This is an internal property meant to be used in tests only.</remarks>
    internal bool NetworkRetriesSleep { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="System.Net.Http.HttpClient"/> class
    /// with default parameters.
    /// </summary>
    /// <returns>The new instance of the <see cref="System.Net.Http.HttpClient"/> class.</returns>
    public static System.Net.Http.HttpClient BuildDefaultSystemNetHttpClient()
    {
      return new System.Net.Http.HttpClient
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
      var (response, retries) = await this.SendHttpRequest(request, cancellationToken).ConfigureAwait(false);

      var reader = new StreamReader(
          await response.Content.ReadAsStreamAsync().ConfigureAwait(false));

      return new CoinbaseResponse(
          response.StatusCode,
          response.Headers,
          await reader.ReadToEndAsync().ConfigureAwait(false));
    }

    private async Task<(HttpResponseMessage responseMessage, int retries)> SendHttpRequest(
        CoinbaseHttpRequest request,
        CancellationToken cancellationToken)
    {
      TimeSpan duration;
      Exception requestException;
      HttpResponseMessage response = null;
      int retry = 0;

      while (true)
      {
        requestException = null;

        var httpRequest = this.BuildRequestMessage(request);

        var stopwatch = Stopwatch.StartNew();

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

        stopwatch.Stop();

        duration = stopwatch.Elapsed;

        if (!this.ShouldRetry(
            retry,
            requestException != null,
            response?.StatusCode,
            response?.Headers))
        {
          break;
        }

        retry += 1;
        await Task.Delay(this.SleepTime(retry)).ConfigureAwait(false);
      }

      if (requestException != null)
      {
        throw requestException;
      }

      return (response, retry);
    }

    private bool ShouldRetry(
        int numRetries,
        bool error,
        HttpStatusCode? statusCode,
        HttpHeaders headers)
    {
      // Do not retry if we are out of retries.
      if (numRetries >= this.MaxNetworkRetries)
      {
        return false;
      }

      // Retry on connection error.
      if (error == true)
      {
        return true;
      }

      // Retry on conflict errors.
      if (statusCode == HttpStatusCode.Conflict)
      {
        return true;
      }

      // Retry on 500, 503, and other internal errors.
      if (statusCode.HasValue && ((int)statusCode.Value >= 500))
      {
        return true;
      }

      return false;
    }

    private System.Net.Http.HttpRequestMessage BuildRequestMessage(CoinbaseHttpRequest request)
    {
      var requestMessage = new System.Net.Http.HttpRequestMessage(request.Method, request.Uri);

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

    private TimeSpan SleepTime(int numRetries)
    {
      // We disable sleeping in some cases for tests.
      if (!this.NetworkRetriesSleep)
      {
        return TimeSpan.Zero;
      }

      // Apply exponential backoff with MinNetworkRetriesDelay on the number of numRetries
      // so far as inputs.
      var delay = TimeSpan.FromTicks((long)(MinNetworkRetriesDelay.Ticks
          * Math.Pow(2, numRetries - 1)));

      // Do not allow the number to exceed MaxNetworkRetriesDelay
      if (delay > MaxNetworkRetriesDelay)
      {
        delay = MaxNetworkRetriesDelay;
      }

      // Apply some jitter by randomizing the value in the range of 75%-100%.
      var jitter = 1.0;
      lock (this.randLock)
      {
        jitter = (3.0 + this.rand.NextDouble()) / 4.0;
      }

      delay = TimeSpan.FromTicks((long)(delay.Ticks * jitter));

      // But never sleep less than the base sleep seconds.
      if (delay < MinNetworkRetriesDelay)
      {
        delay = MinNetworkRetriesDelay;
      }

      return delay;
    }
  }
}
