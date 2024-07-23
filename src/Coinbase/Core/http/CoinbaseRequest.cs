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
  using System;
  using System.Collections.Generic;
  using System.Text;
  using Coinbase.Core.Credentials;

  public class CoinbaseRequest
  {
    private readonly string method;
    private readonly string body;
    private Dictionary<string, string> headers;

    public CoinbaseRequest(string path, string method, string body, Dictionary<string, string> headers)
    {
      this.Uri = BuildUri(path, requestParms);
      this.method = method;
      this.body = body;
      this.headers = headers;
    }

    public CoinbaseRequest(string path, string method, CoinbaseCredentials credentials, object params)
    {
      this.Uri = BuildUri(path, requestParms);
      this.method = method;
      this.body = body;
      this.headers = BuildHeaders(path, method, credentials, params);
    }

    public Uri Uri { get; }

    private Uri BuildUri(string baseUri, Dictionary<string, string> queryParams)
    {
      var uriBuilder = new UriBuilder(baseUri);
      var query = new StringBuilder();

      foreach (var param in queryParams)
      {
          if (query.Length > 0)
              query.Append('&');

          query.Append(Uri.EscapeDataString(param.Key))
               .Append('=')
               .Append(Uri.EscapeDataString(param.Value));
      }

      return uriBuilder.Uri;    
    }

    private Dictionary<string, string> BuildHeaders(string path, string method, CoinbaseCredentials credentials, object options)
    {
      var headers = new Dictionary<string, string>();
      headers.Add("Content-Type", "application/json");
      headers.Add("User-Agent", "Coinbase Pro .NET Client");

      var credentials = options as CoinbaseCredentials;
      headers.Add("CB-ACCESS-KEY", credentials.ApiKey);
      headers.Add("CB-ACCESS-SIGN", credentials.Signature);
      headers.Add("CB-ACCESS-TIMESTAMP", credentials.Timestamp);
      headers.Add("CB-ACCESS-PASSPHRASE", credentials.Passphrase);

      return headers;
    }
  }
 }
