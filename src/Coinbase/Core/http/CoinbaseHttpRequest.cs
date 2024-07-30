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
  using System.Collections;
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Reflection;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using Coinbase.Core.Credentials;

  public class CoinbaseHttpRequest
  {
    private readonly string body;
    private Dictionary<string, string> headers;

    public CoinbaseHttpRequest(
      string path,
      string method,
      CoinbaseCredentials credentials,
      object request,
      IJsonUtility jsonUtility = null)
    {
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

    private Dictionary<string, string> BuildHeaders(string path, string method, CoinbaseCredentials credentials)
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

      var properties = obj.GetType().GetProperties();
      var keyValuePairs = new List<string>();

      foreach (var property in properties)
      {
        var jsonProperty = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        var propertyName = jsonProperty?.Name ?? property.Name;
        var value = property.GetValue(obj);

        if (value is IEnumerable enumerable && !(value is string))
        {
          foreach (var item in enumerable)
          {
            keyValuePairs.Add($"{propertyName}={Uri.EscapeDataString(item?.ToString() ?? string.Empty)}");
          }
        }
        else
        {
          keyValuePairs.Add($"{propertyName}={Uri.EscapeDataString(value?.ToString() ?? string.Empty)}");
        }
      }

      return string.Join("&", keyValuePairs);
    }
  }
}
