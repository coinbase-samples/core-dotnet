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

namespace Coinbase.Core.Service
{
  using System.Net;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using Coinbase.Core.Client;

  /// <summary>
  /// Abstract class that represents any Coinbase API Service.
  /// </summary>
  public abstract class CoinbaseService
  {
    private ICoinbaseClient client;

    protected CoinbaseService()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseService"/> class with a
    /// custom <see cref="ICoinbaseClient"/>.
    /// </summary>
    /// <param name="client">The client used by the service to send requests.</param>
    protected CoinbaseService(ICoinbaseClient client)
    {
      this.client = client;
    }

    public ICoinbaseClient Client
    {
      get => this.client;
      set => this.client = value;
    }

    /// <summary>
    /// Send a synchronous request to the Coinbase Service Endpoint.
    /// </summary>
    /// <typeparam name="T">Return type of the Request.</typeparam>
    /// <param name="method">HTTP Method for the Request.</param>
    /// <param name="path">API Path.</param>
    /// <param name="request">Request Object.</param>
    /// <param name="expectedStatusCodes">Set of expected Status Code.</param>
    /// <returns></returns>
    protected T Request<T>(
        HttpMethod method,
        string path,
        object request,
        HttpStatusCode[] expectedStatusCodes)
    {
      return this.RequestAsync<T>(method, path, request, expectedStatusCodes, default)
          .ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected async Task<T> RequestAsync<T>(
        HttpMethod method,
        string path,
        object request,
        HttpStatusCode[] expectedStatusCodes,
        CancellationToken cancellationToken = default)
    {
      return await this.Client.SendRequestAsync<T>(
          method,
          path,
          request,
          expectedStatusCodes,
          cancellationToken).ConfigureAwait(false);
    }
  }
}
