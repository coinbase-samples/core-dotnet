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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Polly;
    using Polly.Contrib.WaitAndRetry;

    /// <summary>
    /// Internal provider that builds Polly retry policies from <see cref="CallOptions"/>.
    /// Uses Polly's decorrelated jitter backoff strategy for optimal retry distribution.
    /// </summary>
    internal sealed class PollyRetryPolicyProvider
    {
        public static PollyRetryPolicyProvider Instance { get; } = new PollyRetryPolicyProvider();

        /// <summary>
        /// Creates a Polly <see cref="IAsyncPolicy{HttpResponseMessage}"/> that honors the supplied <see cref="CallOptions"/>.
        /// When retries are disabled this returns <see cref="Policy.NoOpAsync{TResult}"/>.
        /// </summary>
        public IAsyncPolicy<HttpResponseMessage> BuildPolicy(CallOptions callOptions, CancellationToken cancellationToken)
        {
            if (callOptions == null || !callOptions.HasRetryConfiguration())
            {
                return Policy.NoOpAsync<HttpResponseMessage>();
            }

            var builder = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<OperationCanceledException>(ex => ShouldHandleOperationCanceled(ex, cancellationToken));

            if (callOptions.ShouldRetryOnStatusCodes && callOptions.RetryableStatusCodes.Count > 0)
            {
                builder = builder.OrResult(response => ShouldRetryResponse(callOptions, response.StatusCode));
            }

            var delays = BuildDelaySequence(callOptions);

            return builder.WaitAndRetryAsync(
                sleepDurations: delays,
                onRetryAsync: (outcome, _, _, _) =>
                {
                    outcome.Result?.Dispose();
                    return Task.CompletedTask;
                });
        }

        private static bool ShouldRetryResponse(CallOptions callOptions, HttpStatusCode statusCode)
        {
            return callOptions.RetryableStatusCodes.Contains(statusCode);
        }

        private static bool ShouldHandleOperationCanceled(OperationCanceledException exception, CancellationToken outerToken)
        {
            // If the outer token (user-supplied cancellation) requested cancellation,
            // we should not retry.
            if (outerToken.IsCancellationRequested)
            {
                return false;
            }

            // If the outer token was not canceled, the OperationCanceledException
            // must have come from an internal source (like a timeout).
            // We generally want to retry these transient failures.
            return true;
        }

        /// <summary>
        /// Builds a delay sequence using Polly's decorrelated jitter backoff strategy.
        /// Decorrelated jitter prevents retry storms by introducing randomization that
        /// spreads out concurrent retry attempts, improving overall system stability.
        /// Delays are capped at <see cref="CallOptions.MaxRetryDelay"/>.
        /// </summary>
        private static IEnumerable<TimeSpan> BuildDelaySequence(CallOptions callOptions)
        {
            var delays = Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: callOptions.MedianFirstRetryDelay,
                retryCount: callOptions.MaxRetries);

            return delays.Select(delay =>
                delay > callOptions.MaxRetryDelay
                    ? callOptions.MaxRetryDelay
                    : delay);
        }
    }
}
