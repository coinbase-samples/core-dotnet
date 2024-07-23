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
  using System.Net.Http;

  public class CoinbaseResponse
  {
    private readonly int statusCode;
    private readonly string body;
    private readonly string contentType;

    public CoinbaseResponse(int statusCode, string body, string contentType)
    {
      this.statusCode = statusCode;
      this.body = body;
      this.contentType = contentType;
    }

    /// <summary>
    /// Pulls the status code and response payload from the HttpResponseMessage object.
    /// Handles the error object that can be returned by the API.
    /// </summary>
    public CoinbaseResponse(HttpResponseMessage response)
    {
      if (response == null)
      {
        this.statusCode = 0;
        this.body = string.Empty;
        this.contentType = string.Empty;
        return;
      }

      this.statusCode = (int)response.StatusCode;

      if (response.IsSuccessStatusCode) {
        string body = await response.Content.ReadAsStringAsync();

      }

      this.body = response.Content.ReadAsStringAsync().Result;
      this.contentType = response.Content.Headers.ContentType.MediaType;
    }
  }
}
