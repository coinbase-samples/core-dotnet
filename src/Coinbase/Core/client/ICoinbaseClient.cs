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

namespace Coinbase.Core.Client
{
  using System.Net;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using Coinbase.Core.Credentials;

  /// <summary>
  /// Interface that represents a Coinbase API Client.
  /// </summary>
  public interface ICoinbaseClient
  {
    /// <summary>
    /// Gets the base path for the Coinbase API.
    /// </summary>
    string ApiBasePath { get; }

    /// <summary>
    /// Gets the credentials used by the client to authenticate requests.
    /// </summary>
    CoinbaseCredentials Credentials { get; }

    /// <summary>
    /// Send a synchronous request to the Coinbase Service Endpoint.
    /// </summary>
    /// <typeparam name="T">Return type of the Request.</typeparam>
    /// <param name="method">HTTP Method for the Request.</param>
    /// <param name="path">API Path.</param>
    /// <param name="request">Request parameters.</param>
    /// <param name="expectedStatusCodes">Set of expected Status Code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task object representing the async operation.</returns>
    Task<T> SendRequestAsync<T>(
      HttpMethod method,
      string path,
      object request,
      HttpStatusCode[] expectedStatusCodes,
      CancellationToken cancellationToken);
  }
}
