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

namespace CoinbaseSdk.Core.Tests
{
  using System.Text.Json;
  using CoinbaseSdk.Core.Serialization;
  using Xunit;

  public enum TestEnum
  {
    VALUE_ONE,
    VALUE_TWO,
  }

  public class TestModel
  {
    public TestEnum? Category { get; set; }
  }

  public class NonNullableTestModel
  {
    public TestEnum Category { get; set; }
  }

  public class EnumSerializationTest
  {
    [Fact]
    public void TestUnknownEnumValueConvertsToNull()
    {
      var jsonUtility = new JsonUtility();
      var json = @"{""category"":""UNKNOWN_VALUE""}";

      var result = jsonUtility.Deserialize<TestModel>(json);

      Assert.Null(result.Category);
    }

    [Fact]
    public void TestKnownEnumValueDeserializes()
    {
      var jsonUtility = new JsonUtility();
      var json = @"{""category"":""VALUE_ONE""}";

      var result = jsonUtility.Deserialize<TestModel>(json);

      Assert.Equal(TestEnum.VALUE_ONE, result.Category);
    }

    [Fact]
    public void TestKnownEnumValueDeserializesForNonNullable()
    {
      var jsonUtility = new JsonUtility();
      var json = @"{""category"":""VALUE_TWO""}";

      var result = jsonUtility.Deserialize<NonNullableTestModel>(json);

      Assert.Equal(TestEnum.VALUE_TWO, result.Category);
    }

    [Fact]
    public void TestUnknownEnumValueThrowsForNonNullable()
    {
      var jsonUtility = new JsonUtility();
      var json = @"{""category"":""UNKNOWN_VALUE""}";

      Assert.Throws<JsonException>(() => jsonUtility.Deserialize<NonNullableTestModel>(json));
    }
  }
}
