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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Abstract class that represents any Coinbase API Service.
    /// </summary>
    public abstract class CoinbaseService
    {
      private readonly ICoinbaseClient client;
      private readonly string basePath;

      /// <summary>
      /// Initializes a new instance of the <see cref="CoinbaseService"/> class with a
      /// custom <see cref="ICoinbaseClient"/>.
      /// </summary>
      /// <param name="client">The client used by the service to send requests.</param>
      /// <param name="basePath">The base path for the service.</param>
      protected CoinbaseService(ICoinbaseClient client, string basePath)
      {
          this.client = client;
          this.basePath = basePath;
      }

      public ICoinbaseClient Client
      {
          get => this.client;
      }

      public string BasePath
      {
          get => this.basePath;
      }

      /// <summary>
      /// Send a synchronous request to the Coinbase Service Endpoint.
      /// </summary>
      /// <typeparam name="T">Return type of the Request.</typeparam>
      /// <param name="method">HTTP Method for the Request.</param>
      /// <param name="path">API Path.</param>
      /// <returns></returns>
      protected T Request<T>(
          HttpMethod method,
          string path)
      {
          return this.RequestAsync<T>(method, path)
              .ConfigureAwait(false).GetAwaiter().GetResult();
      }

      protected async Task<T> RequestAsync<T>(
          HttpMethod method,
          string path,
          CancellationToken cancellationToken = default)
      {
          return await this.Client.SendRequestAsync<T>(
              method,
              path,
              cancellationToken).ConfigureAwait(false);
      }


      protected RequestOptions SetupRequestOptions(RequestOptions requestOptions)
      {
          if (requestOptions == null)
          {
              requestOptions = new RequestOptions();
          }

          requestOptions.BaseUrl = requestOptions.BaseUrl ?? this.BaseUrl;

          return requestOptions;
      }
    }
}
