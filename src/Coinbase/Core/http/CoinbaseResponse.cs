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
  using System.Net;
  using System.Net.Http.Headers;

  /// <summary>Represents the response from a Coinbase API request.</summary>
  public class CoinbaseResponse
  {
    /// <summary>Initializes a new instance of the <see cref="CoinbaseResponse"/> class.</summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="headers">The HTTP headers of the response.</param>
    /// <param name="content">The body of the response.</param>
    public CoinbaseResponse(HttpStatusCode statusCode, HttpResponseHeaders headers, string content)
    {
        this.StatusCode = statusCode;
        this.Headers = headers;
        this.Content = content;
    }

    /// <summary>Gets the HTTP status code of the response.</summary>
    /// <value>The HTTP status code of the response.</value>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Gets the HTTP headers of the response.</summary>
    /// <value>The HTTP headers of the response.</value>
    public HttpResponseHeaders Headers { get; }

    /// <summary>Gets the body of the response.</summary>
    /// <value>The body of the response.</value>
    public string Content { get; }
  }
}
