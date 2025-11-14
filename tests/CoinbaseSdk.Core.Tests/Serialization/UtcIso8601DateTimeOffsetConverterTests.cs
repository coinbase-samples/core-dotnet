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

namespace CoinbaseSdk.Core.Tests.Serialization
{
    using System;
    using System.Text.Json;
    using CoinbaseSdk.Core.Serialization;
    using Xunit;

    public class UtcIso8601DateTimeOffsetConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public UtcIso8601DateTimeOffsetConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new UtcIso8601DateTimeOffsetConverter() }
            };
        }

        [Fact]
        public void Serialize_DateTimeOffset_ReturnsIso8601String()
        {
            var date = new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.Zero);
            var json = JsonSerializer.Serialize(date, _options);
            Assert.Equal("\"2023-10-27T12:00:00.0000000Z\"", json);
        }

        [Fact]
        public void Deserialize_Iso8601String_ReturnsDateTimeOffset()
        {
            var json = "\"2023-10-27T12:00:00.0000000Z\"";
            var date = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.Zero), date);
        }
    }
}
