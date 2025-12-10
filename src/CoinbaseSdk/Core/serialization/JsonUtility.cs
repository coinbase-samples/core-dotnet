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

#nullable enable

namespace CoinbaseSdk.Core.Serialization
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.Json.Serialization.Metadata;

    public class JsonUtility : IJsonUtility
    {
        private static readonly object DefaultOptionsLock = new ();
        private static JsonSerializerOptions? defaultOptions;

        private readonly JsonSerializerOptions options;

        public JsonUtility()
            : this(DefaultOptions)
        {
        }

        public JsonUtility(JsonSerializerOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the shared default <see cref="JsonSerializerOptions"/> instance used by Coinbase clients.
        /// </summary>
        public static JsonSerializerOptions DefaultOptions => EnsureDefaultOptions();

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, this.options);
        }

        public T Deserialize<T>(string json)
        {
            T? result = JsonSerializer.Deserialize<T>(json, this.options);

            if (result == null)
            {
                throw new JsonException($"Unable to deserialize JSON payload into type {typeof(T).FullName}.");
            }

            return result;
        }

        internal static void ResetDefaultsForTesting()
        {
            lock (DefaultOptionsLock)
            {
                defaultOptions = null;
            }
        }

        private static JsonSerializerOptions EnsureDefaultOptions()
        {
            if (defaultOptions != null)
            {
                return defaultOptions;
            }

            lock (DefaultOptionsLock)
            {
                if (defaultOptions == null)
                {
                    defaultOptions = BuildDefaultOptions();
                }

                return defaultOptions;
            }
        }

        private static JsonSerializerOptions BuildDefaultOptions()
        {
            JsonSerializerOptions baseOptions = new (JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            };

            EnsureConverter<JsonStringEnumConverter>(baseOptions);
            EnsureConverter<NullOnUnknownEnumConverter>(baseOptions);
            EnsureConverter<UtcIso8601DateTimeOffsetConverter>(baseOptions);

            return baseOptions;
        }

        private static void EnsureConverter<TConverter>(JsonSerializerOptions options)
            where TConverter : JsonConverter, new()
        {
            if (!options.Converters.OfType<TConverter>().Any())
            {
                options.Converters.Add(new TConverter());
            }
        }
    }
}
