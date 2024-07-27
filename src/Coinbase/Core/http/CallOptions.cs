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

namespace Coinbase.Core.Http
{
  using System;
  using System.Collections.Generic;
  using System.Net;

  public class CallOptions
  {
    public bool ShouldRetryOnStatusCodes { get; set; } = false; // Default retry

    public int MaxRetries { get; set; } = 3; // Default max retries

    public TimeSpan MinNetworkRetriesDelay { get; set; } = TimeSpan.FromSeconds(1); // Default minimum delay

    public TimeSpan MaxNetworkRetriesDelay { get; set; } = TimeSpan.FromSeconds(30); // Default maximum delay

    public HashSet<HttpStatusCode> RetryableStatusCodes { get; set; } = new HashSet<HttpStatusCode> { }; // Default retryable status codes
  }
}