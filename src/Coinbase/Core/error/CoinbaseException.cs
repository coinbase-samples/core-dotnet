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

namespace Coinbase.Core.Error
{
  using System.Net;

  /// <summary>
  /// Exception thrown when an error occurs in the Coinbase API.
  /// </summary>
  public class CoinbaseException : System.Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseException"/> class from a message.
    /// </summary>
    /// <param name="message">Error message.</param>
    public CoinbaseException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseException"/> class from a cause.
    /// </summary>
    /// <param name="cause">Underlying exception.</param>
    public CoinbaseException(System.Exception cause)
        : base(cause.Message, cause)
    {
    }

    public CoinbaseException(string message, System.Exception cause)
        : base(message, cause)
    {
    }

    public CoinbaseException(HttpStatusCode httpStatusCode, string message)
        : base(message)
    {
        this.StatusCode = httpStatusCode;
    }

    public CoinbaseException(HttpStatusCode httpStatusCode, string message, System.Exception cause)
        : base(message, cause)
    {
        this.StatusCode = httpStatusCode;
    }

    public HttpStatusCode StatusCode { get; set; }

    public override string ToString()
    {
      return $"CoinbaseException{{StatusCode={this.StatusCode}, Message={this.Message}}}";
    }
  }
}
