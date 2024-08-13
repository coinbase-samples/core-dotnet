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
  using System.Collections.Generic;
  using System.Net;

  /// <summary>
  /// Options for the HttpClient to use when making requests. Most of the options
  /// are related to retry logic.
  /// </summary>
  public class CallOptions
  {
    /// <summary>
    /// Defaults to false. If true the client will retry on any status code
    /// provided in <see cref="RetryableStatusCodes"/>.
    /// </summary>
    public bool ShouldRetryOnStatusCodes { get; set; } = false; // Default retry

    /// <summary>
    /// Defaults to 3. The maximum number of retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3; // Default max retries

    /// <summary>
    /// Defaults to 500 milliseconds. The minimum delay between calls.
    /// </summary>
    public TimeSpan MinNetworkRetriesDelay { get; set; } = TimeSpan.FromSeconds(0.5); // Default minimum delay

    /// <summary>
    /// Defaults to 1 second. The maximum delay between calls.
    /// </summary>
    public TimeSpan MaxNetworkRetriesDelay { get; set; } = TimeSpan.FromSeconds(1); // Default maximum delay

    /// <summary>
    /// Defaults to an empty set. The status codes to retry on when
    /// <see cref="ShouldRetryOnStatusCodes"/> is true.
    /// </summary>
    public HashSet<HttpStatusCode> RetryableStatusCodes { get; set; } = new HashSet<HttpStatusCode> { }; // Default retryable status codes
  }
}