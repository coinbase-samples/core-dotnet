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
            Assert.Equal(@"""2023-10-27T12:00:00.0000000Z""", json);
        }

        [Fact]
        public void Deserialize_Iso8601String_ReturnsDateTimeOffset()
        {
            var json = @"""2023-10-27T12:00:00.0000000Z""";
            var date = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.Zero), date);
        }

        [Fact]
        public void Deserialize_InvalidDateString_ThrowsJsonException()
        {
            var json = @"""invalid-date""";
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<DateTimeOffset>(json, _options));
        }

        [Fact]
        public void Deserialize_Number_ThrowsJsonException()
        {
            var json = "12345";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTimeOffset>(json, _options));
        }

        [Fact]
        public void Serialize_NonUtcOffset_ConvertsToUtc()
        {
            var date = new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.FromHours(5));
            var json = JsonSerializer.Serialize(date, _options);
            Assert.Equal(@"""2023-10-27T07:00:00.0000000Z""", json);
        }

        [Fact]
        public void Serialize_NegativeOffset_ConvertsToUtc()
        {
            var date = new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.FromHours(-8));
            var json = JsonSerializer.Serialize(date, _options);
            Assert.Equal(@"""2023-10-27T20:00:00.0000000Z""", json);
        }

        [Fact]
        public void Serialize_WithMilliseconds_PreservesPrecision()
        {
            var date = new DateTimeOffset(2023, 10, 27, 12, 0, 0, 123, TimeSpan.Zero);
            var json = JsonSerializer.Serialize(date, _options);
            Assert.Equal(@"""2023-10-27T12:00:00.1230000Z""", json);
        }

        [Fact]
        public void Serialize_MinValue_ReturnsValidIso8601()
        {
            var date = DateTimeOffset.MinValue;
            var json = JsonSerializer.Serialize(date, _options);
            Assert.Equal(@"""0001-01-01T00:00:00.0000000Z""", json);
        }

        [Fact]
        public void Serialize_MaxValue_ReturnsValidIso8601()
        {
            var date = DateTimeOffset.MaxValue;
            var json = JsonSerializer.Serialize(date, _options);
            Assert.Equal(@"""9999-12-31T23:59:59.9999999Z""", json);
        }

        [Fact]
        public void Deserialize_Iso8601WithOffset_ParsesCorrectly()
        {
            var json = @"""2023-10-27T12:00:00+05:00""";
            var date = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.FromHours(5)), date);
        }

        [Fact]
        public void Deserialize_Iso8601WithNegativeOffset_ParsesCorrectly()
        {
            var json = @"""2023-10-27T12:00:00-08:00""";
            var date = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.FromHours(-8)), date);
        }

        [Fact]
        public void Deserialize_Iso8601WithoutFractionalSeconds_ParsesCorrectly()
        {
            var json = @"""2023-10-27T12:00:00Z""";
            var date = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.Zero), date);
        }

        [Fact]
        public void Deserialize_Iso8601WithMilliseconds_ParsesCorrectly()
        {
            var json = @"""2023-10-27T12:00:00.123Z""";
            var date = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(new DateTimeOffset(2023, 10, 27, 12, 0, 0, 123, TimeSpan.Zero), date);
        }

        [Fact]
        public void Deserialize_EmptyString_ThrowsException()
        {
            var json = @"""""";
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<DateTimeOffset>(json, _options));
        }

        [Fact]
        public void Deserialize_WhitespaceString_ThrowsException()
        {
            var json = @"""   """;
            Assert.Throws<FormatException>(() => JsonSerializer.Deserialize<DateTimeOffset>(json, _options));
        }

        [Fact]
        public void Deserialize_Null_ThrowsException()
        {
            var json = "null";
            Assert.Throws<ArgumentNullException>(() => JsonSerializer.Deserialize<DateTimeOffset>(json, _options));
        }

        [Fact]
        public void Deserialize_Boolean_ThrowsJsonException()
        {
            var json = "true";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTimeOffset>(json, _options));
        }

        [Fact]
        public void RoundTrip_UtcDateTime_PreservesValue()
        {
            var original = new DateTimeOffset(2023, 10, 27, 12, 30, 45, 678, TimeSpan.Zero);
            var json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void RoundTrip_NonUtcDateTime_PreservesUtcEquivalent()
        {
            var original = new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.FromHours(5));
            var json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(original.UtcDateTime, deserialized.UtcDateTime);
            Assert.Equal(TimeSpan.Zero, deserialized.Offset);
        }

        [Fact]
        public void RoundTrip_WithPrecision_PreservesFractionalSeconds()
        {
            var original = new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.Zero).AddTicks(1234567);
            var json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<DateTimeOffset>(json, _options);
            Assert.Equal(original, deserialized);
        }

        [Fact]
        public void Serialize_ObjectWithDateTimeOffsetProperty_Works()
        {
            var obj = new TestObject { Timestamp = new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.FromHours(3)) };
            var json = JsonSerializer.Serialize(obj, _options);
            Assert.Contains(@"""2023-10-27T09:00:00.0000000Z""", json);
        }

        [Fact]
        public void Deserialize_ObjectWithDateTimeOffsetProperty_Works()
        {
            var json = @"{""Timestamp"":""2023-10-27T12:00:00.0000000Z""}";
            var obj = JsonSerializer.Deserialize<TestObject>(json, _options);
            Assert.NotNull(obj);
            Assert.Equal(new DateTimeOffset(2023, 10, 27, 12, 0, 0, TimeSpan.Zero), obj.Timestamp);
        }

        private class TestObject
        {
            public DateTimeOffset Timestamp { get; set; }
        }
    }
}
