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
    using System.Threading;

    /// <summary>
    /// Configuration options for retry behavior using Polly's decorrelated jitter backoff.
    /// Retries are disabled by default (<see cref="MaxRetries"/> = 0).
    /// See the core-dotnet README for configuration examples.
    /// </summary>
    public class CallOptions
    {
        /// <summary>
        /// Defaults to 0 (retries disabled). The maximum number of retry attempts
        /// (in addition to the initial request). Set to a positive value to enable retries.
        /// </summary>
        public int MaxRetries { get; set; } = RetryPolicyDefaults.MaxRetries;

        /// <summary>
        /// Defaults to 500 milliseconds. The target median delay for the first retry
        /// when using decorrelated jitter backoff. Actual delays may vary above or below
        /// this value due to jitter, which helps prevent retry storms.
        /// Only used when <see cref="MaxRetries"/> is greater than zero.
        /// </summary>
        public TimeSpan MedianFirstRetryDelay { get; set; } = RetryPolicyDefaults.MedianFirstRetryDelay;

        /// <summary>
        /// Defaults to 1 second. The maximum delay cap for any retry attempt.
        /// Prevents delays from growing unbounded on later retry attempts.
        /// Only used when <see cref="MaxRetries"/> is greater than zero.
        /// </summary>
        public TimeSpan MaxRetryDelay { get; set; } = RetryPolicyDefaults.MaxRetryDelay;

        /// <summary>
        /// Defaults to false. When true, the client evaluates HTTP responses against
        /// <see cref="RetryableStatusCodes"/> in addition to standard transient failures
        /// (network exceptions, timeouts, etc.). Only applies when <see cref="MaxRetries"/>
        /// is greater than zero.
        /// </summary>
        public bool ShouldRetryOnStatusCodes { get; set; } = RetryPolicyDefaults.ShouldRetryOnStatusCodes;

        /// <summary>
        /// Defaults to an empty set. The HTTP status codes to retry on when
        /// <see cref="ShouldRetryOnStatusCodes"/> is true. Should contain a bounded set
        /// of retryable responses (e.g., 429 Too Many Requests, 503 Service Unavailable).
        /// Only used when both <see cref="ShouldRetryOnStatusCodes"/> is true and
        /// <see cref="MaxRetries"/> is greater than zero.
        /// </summary>
        public HashSet<HttpStatusCode> RetryableStatusCodes { get; set; } = new HashSet<HttpStatusCode>();

        /// <summary>
        /// Returns true when retries are enabled and a Polly policy should be created.
        /// </summary>
        internal bool HasRetryConfiguration()
        {
            return this.MaxRetries > 0;
        }
    }
}