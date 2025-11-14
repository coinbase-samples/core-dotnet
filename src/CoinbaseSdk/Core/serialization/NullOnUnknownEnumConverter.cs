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

        /// <summary>
        /// Generic JSON converter that converts unknown enum values to null.
        /// </summary>
        /// <typeparam name="TEnum">The enum type to convert.</typeparam>
        private class NullOnUnknownEnumConverterGeneric<TEnum> : JsonConverter<TEnum?>
          where TEnum : struct, Enum
        {
            /// <inheritdoc/>
            public override TEnum? Read(
              ref Utf8JsonReader reader,
              Type typeToConvert,
              JsonSerializerOptions options)
            {
                // "Red", "GREEN", etc
                if (reader.TokenType == JsonTokenType.String)
                {
                    var s = reader.GetString();

                    if (Enum.TryParse<TEnum>(s, ignoreCase: true, out var value))
                    {
                        return value;
                    }

                    // Unknown string -> null
                    return null;
                }

                // 0, 1, 2, ...
                if (reader.TokenType == JsonTokenType.Number)
                {
                    if (reader.TryGetInt32(out var i))
                    {
                        // Only accept defined numeric values
                        if (Enum.IsDefined(typeof(TEnum), i))
                        {
                            return (TEnum)Enum.ToObject(typeof(TEnum), i);
                        }

                        // Unknown underlying value -> null
                        return null;
                    }

                    // Not even a valid int → treat as bad input
                    throw new JsonException($"Cannot convert number to {typeof(TEnum).Name}");
                }

                // Anything else is just invalid JSON for an enum
                throw new JsonException($"Unexpected token {reader.TokenType} when parsing {typeof(TEnum).Name}");
            }

            /// <inheritdoc/>
            public override void Write(
              Utf8JsonWriter writer,
              TEnum? value,
              JsonSerializerOptions options)
            {
                if (value is null)
                {
                    writer.WriteNullValue();
                    return;
                }

                // Serialize as string name
                writer.WriteStringValue(value.Value.ToString());
            }
        }
    }
}
