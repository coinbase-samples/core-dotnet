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

namespace CoinbaseSdk.Core.Tests.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using CoinbaseSdk.Core.Serialization;

public enum TestEnum
{
    Red,
    Green,
    Blue
}

public class TestDto
{
    [JsonConverter(typeof(NullOnUnknownEnumConverterGeneric<TestEnum>))]
    public TestEnum? Status { get; set; }
}

public class NullOnUnknownEnumConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Read_KnownStringValue_ReturnsEnum()
    {
        var json = """{"status": "Red"}""";
        var result = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Equal(TestEnum.Red, result?.Status);
    }

    [Fact]
    public void Read_KnownStringValue_CaseInsensitive_ReturnsEnum()
    {
        var json = """{"status": "green"}""";
        var result = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Equal(TestEnum.Green, result?.Status);
    }

    [Fact]
    public void Read_UnknownStringValue_ReturnsNull()
    {
        var json = """{"status": "Yellow"}""";
        var result = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Null(result?.Status);
    }

    [Fact]
    public void Read_ExplicitNull_ReturnsNull()
    {
        var json = """{"status": null}""";
        var result = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Null(result?.Status);
    }

    [Fact]
    public void Read_KnownNumericValue_ReturnsEnum()
    {
        var json = """{"status": 1}""";
        var result = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Equal(TestEnum.Green, result?.Status);
    }

    [Fact]
    public void Read_UnknownNumericValue_ReturnsNull()
    {
        var json = """{"status": 999}""";
        var result = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Null(result?.Status);
    }

    [Fact]
    public void Read_InvalidToken_ThrowsJsonException()
    {
        var json = """{"status": true}""";
        var ex = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TestDto>(json, _options));
        Assert.Contains("Unexpected token", ex.Message);
    }

    [Fact]
    public void Read_NonIntNumber_ThrowsJsonException()
    {
        var json = """{"status": 1.5}""";
        var ex = Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TestDto>(json, _options));
        Assert.Contains("Cannot convert number", ex.Message);
    }

    [Fact]
    public void Write_EnumValue_WritesStringName()
    {
        var dto = new TestDto { Status = TestEnum.Blue };
        var json = JsonSerializer.Serialize(dto, _options);
        Assert.Contains("\"Blue\"", json);
    }

    [Fact]
    public void Write_NullValue_WritesNull()
    {
        var dto = new TestDto { Status = null };
        var json = JsonSerializer.Serialize(dto, _options);
        Assert.Contains("null", json);
    }

    [Fact]
    public void RoundTrip_KnownValue_Preserves()
    {
        var original = new TestDto { Status = TestEnum.Red };
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Equal(original.Status, deserialized?.Status);
    }

    [Fact]
    public void RoundTrip_NullValue_Preserves()
    {
        var original = new TestDto { Status = null };
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<TestDto>(json, _options);
        Assert.Null(deserialized?.Status);
    }
}
