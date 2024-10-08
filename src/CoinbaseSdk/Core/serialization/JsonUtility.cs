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

namespace CoinbaseSdk.Core.Serialization
{
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class JsonUtility : IJsonUtility
  {
    private readonly JsonSerializerOptions options;

    public JsonUtility()
    {
      this.options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
      {
        Converters =
        {
          new JsonStringEnumConverter(),
          new UtcIso8601DateTimeOffsetConverter(),
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
      };
    }

    public JsonUtility(JsonSerializerOptions options)
    {
      this.options = options;
    }

    public string Serialize(object obj)
    {
      return JsonSerializer.Serialize(obj, this.options);
    }

    public T Deserialize<T>(string json)
    {
      return JsonSerializer.Deserialize<T>(json, this.options);
    }
  }
}
