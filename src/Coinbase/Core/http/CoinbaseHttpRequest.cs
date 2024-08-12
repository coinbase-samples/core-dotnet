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
  using System.Linq;
  using System.Net.Http;
  using System.Text.Json;
  using Coinbase.Core.Credentials;
  using Coinbase.Core.Serialization;

  public class CoinbaseHttpRequest
  {
    private readonly string body;
    private readonly IJsonUtility jsonUtility;
    private Dictionary<string, string> headers;

    public CoinbaseHttpRequest(
      string path,
      string method,
      CoinbaseCredentials credentials,
      object request,
      IJsonUtility jsonUtility)
    {
      this.jsonUtility = jsonUtility;
      this.Method = new HttpMethod(method);
      if (this.Method == HttpMethod.Post || this.Method == HttpMethod.Put)
      {
        this.body = jsonUtility.Serialize(request);
        this.Uri = this.BuildUri(path);
      }
      else
      {
        this.body = string.Empty;
        this.Uri = this.BuildUri(path, request);
      }

      this.headers = this.BuildHeaders(this.Uri.AbsolutePath, method, credentials);
    }

    public Uri Uri { get; }

    public HttpMethod Method { get; }

    public Dictionary<string, string> Headers => this.headers;

    public string Content => this.body;

    private Uri BuildUri(string baseUri, object request = null)
    {
      var uriBuilder = new UriBuilder($"https://{baseUri}")
      {
        Query = this.ToQueryString(request),
      };

      return uriBuilder.Uri;
    }

    protected virtual Dictionary<string, string> BuildHeaders(string path, string method, CoinbaseCredentials credentials)
    {
      var headers = new Dictionary<string, string>();

      // generate a timestamp and use that in both sign and the timestamp header
      var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

      headers.Add("X-CB-ACCESS-KEY", credentials.AccessKey);
      headers.Add("X-CB-ACCESS-SIGNATURE", credentials.Sign(timestamp, method, path, this.body));
      headers.Add("X-CB-ACCESS-TIMESTAMP", timestamp);
      headers.Add("X-CB-ACCESS-PASSPHRASE", credentials.Passphrase);

      return headers;
    }

    private string ToQueryString(object obj)
    {
      if (obj == null)
      {
        return string.Empty;
      }

      var jsonString = this.jsonUtility.Serialize(obj);
      var dictionary = this.jsonUtility.Deserialize<Dictionary<string, object>>(jsonString);

      return string.Join("&", dictionary
          .Where(kvp => kvp.Value != null)
          .SelectMany(kvp =>
          {
            if (kvp.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
              return jsonElement.EnumerateArray().Select(item => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(item.ToString())}");
            }
            else
            {
              return new[] { $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}" };
            }
          }));
    }
  }
}
