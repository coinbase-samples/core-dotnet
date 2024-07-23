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
  using Coinbase.Core.Client;
  using Coinbase.Core.Credentials;
  using Newtonsoft.Json;

  public class CoinbaseHttpRequest
  {
    private readonly string body;
    private Dictionary<string, string> headers;

    public CoinbaseHttpRequest(string path, string method, CoinbaseCredentials credentials, ICoinbaseRequest request)
    {
      this.Method = new HttpMethod(method);
      if (this.Method == HttpMethod.Post || this.Method == HttpMethod.Put)
      {
        this.body = JsonConvert.SerializeObject(request);
        this.Uri = BuildUri(path);
      }
      else
      {
        this.body = "";
        this.Uri = BuildUri(path, request);
      }
      this.headers = BuildHeaders(path, method, credentials);
    }

    public Uri Uri { get; }

    public HttpMethod Method { get; }

    public Dictionary<string, string> Headers => this.headers;

    public string Content => this.body;

    private Uri BuildUri(string baseUri, ICoinbaseRequest request = null)
    {
      var uriBuilder = new UriBuilder(baseUri);
      uriBuilder.Query = ToQueryString(request);

      return uriBuilder.Uri;
    }

    private Dictionary<string, string> BuildHeaders(string path, string method, CoinbaseCredentials credentials)
    {
      var headers = new Dictionary<string, string>();
      headers.Add("Content-Type", "application/json");
      headers.Add("User-Agent", "Coinbase Pro .NET Client");

      // generate a timestamp and use that in both sign and the timestamp header
      var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

      headers.Add("CB-ACCESS-KEY", credentials.AccessKey);
      headers.Add("CB-ACCESS-SIGN", credentials.Sign(timestamp, method, path, body));
      headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
      headers.Add("CB-ACCESS-PASSPHRASE", credentials.Passphrase);

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
          var jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>();
          var propertyName = jsonProperty?.PropertyName ?? property.Name;
          var value = property.GetValue(obj);

          if (value is IEnumerable enumerable && !(value is string))
          {
              foreach (var item in enumerable)
              {
                  keyValuePairs.Add($"{propertyName}={Uri.EscapeDataString(item.ToString())}");
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
