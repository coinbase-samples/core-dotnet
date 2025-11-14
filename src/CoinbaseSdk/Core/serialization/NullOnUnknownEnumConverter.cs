/*
 * Copyright 2025-present Coinbase Global, Inc.
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
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  /// <summary>
  /// JSON converter factory that converts unknown enum values to null.
  /// </summary>
  public class NullOnUnknownEnumConverter : JsonConverterFactory
  {
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
      return Nullable.GetUnderlyingType(typeToConvert)?.IsEnum ?? false;
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(
      Type typeToConvert,
      JsonSerializerOptions options)
    {
      var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
      var converterType = typeof(NullOnUnknownEnumConverterGeneric<>).MakeGenericType(enumType);
      return (JsonConverter)Activator.CreateInstance(converterType);
    }
  }
}
