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
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.Json.Serialization.Metadata;

    public class JsonUtility : IJsonUtility
    {
        private static readonly object ConfigurationLock = new ();
        private static readonly List<Action<JsonSerializerOptions>> PendingConfigurationActions = new ();
        private static Func<JsonSerializerOptions> optionsFactory = BuildBaseOptions;
        private static JsonSerializerOptions? defaultOptions;
        private static bool defaultsFrozen;

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

        /// <summary>
        /// Allows SDKs to adjust the default serializer options before they are frozen.
        /// Must be called before any <see cref="JsonUtility"/> instance is created (i.e. before <see cref="DefaultOptions"/> is accessed).
        /// </summary>
        public static void ConfigureDefaults(Action<JsonSerializerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            AddConfigurationAction(configure);
        }

        /// <summary>
        /// Replaces the factory used to build the default serializer options.
        /// Must be called before any <see cref="JsonUtility"/> instance is created.
        /// </summary>
        public static void UseCustomDefaultFactory(Func<JsonSerializerOptions> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            SetFactory(factory);
        }

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
            lock (ConfigurationLock)
            {
                defaultOptions = null;
                defaultsFrozen = false;
                optionsFactory = BuildBaseOptions;
                PendingConfigurationActions.Clear();
            }
        }

        private static void AddConfigurationAction(Action<JsonSerializerOptions> configure)
        {
            lock (ConfigurationLock)
            {
                if (defaultsFrozen)
                {
                    throw new InvalidOperationException("JsonUtility defaults have already been created and cannot be reconfigured.");
                }

                PendingConfigurationActions.Add(configure);
            }
        }

        private static void SetFactory(Func<JsonSerializerOptions> factory)
        {
            lock (ConfigurationLock)
            {
                if (defaultsFrozen)
                {
                    throw new InvalidOperationException("JsonUtility defaults have already been created and cannot be reconfigured.");
                }

                optionsFactory = factory;
            }
        }

        private static JsonSerializerOptions EnsureDefaultOptions()
        {
            lock (ConfigurationLock)
            {
                if (defaultOptions == null)
                {
                    defaultOptions = BuildOptions();
                    defaultsFrozen = true;
                }

                return defaultOptions;
            }
        }

        private static JsonSerializerOptions BuildBaseOptions()
        {
            JsonSerializerOptions baseOptions = new (JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            };

            baseOptions.Converters.Add(new NullOnUnknownEnumConverter());
            baseOptions.Converters.Add(new JsonStringEnumConverter());
            baseOptions.Converters.Add(new UtcIso8601DateTimeOffsetConverter());

            return baseOptions;
        }

        private static JsonSerializerOptions BuildOptions()
        {
            JsonSerializerOptions options = optionsFactory();

            foreach (Action<JsonSerializerOptions> configure in PendingConfigurationActions)
            {
                configure(options);
            }

            PendingConfigurationActions.Clear();

            return options;
        }
    }
}
