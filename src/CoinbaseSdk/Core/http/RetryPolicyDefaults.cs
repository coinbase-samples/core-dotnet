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

    /// <summary>
    /// Default configuration values for Polly retry policy behavior.
    /// </summary>
    public static class RetryPolicyDefaults
    {
        /// <summary>
        /// Default maximum number of retry attempts (0 = no retries).
        /// </summary>
        public const int MaxRetries = 0;

        /// <summary>
        /// Default setting for whether to retry on specific HTTP status codes (false).
        /// </summary>
        public const bool ShouldRetryOnStatusCodes = false;

        /// <summary>
        /// Default median delay for the first retry attempt (500 milliseconds).
        /// Used as input to Polly's decorrelated jitter backoff strategy.
        /// </summary>
        public static readonly TimeSpan MedianFirstRetryDelay = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Default maximum delay cap for any retry attempt (1 second).
        /// Prevents jittered delays from growing unbounded.
        /// </summary>
        public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(1);
    }
}
