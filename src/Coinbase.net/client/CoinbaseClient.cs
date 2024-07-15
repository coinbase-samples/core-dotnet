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

namespace Coinbase
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Net;
  using System.Net.Http;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using Coinbase.Core.Credentials;

  /// <summary>
  /// Interface that represents a Coinbase API Client.
  /// </summary>
  public class CoinbaseClient : ICoinbaseClient
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseClient"/> class.
    /// </summary>
    /// <param name="coinbaseCredentials">Api Credentials.</param>
    /// <param name="apiBasePath">Base url path for the API.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the Credentials are not valid or a base path is not provided.
    /// </exception>
    public CoinbaseClient(
      string coinbaseCredentials = null,
      string apiBasePath = null)
    {
      if (coinbaseCredentials == null)
      {
        throw new ArgumentException("Credentials cannot be null", nameof(coinbaseCredentials));
      }

      if (string.IsNullOrEmpty(apiBasePath.Trim()))
      {
        throw new ArgumentException("API base path cannot be null or empty", nameof(apiBasePath));
      }
    }

    /// <inheritdoc/>
    public string ApiBasePath { get; }

    /// <inheritdoc/>
    public CoinbaseCredentials Credentials { get; }

    /// <inheritdoc/>
    public Task<T> SendRequestAsync<T>(HttpMethod method, string path, object options = null)
    {
        throw new NotImplementedException();
    }
  }
}