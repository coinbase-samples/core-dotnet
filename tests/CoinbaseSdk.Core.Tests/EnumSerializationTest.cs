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
      var json = "{\"category\":\"UNKNOWN_VALUE\"}";

      var result = jsonUtility.Deserialize<TestModel>(json);

      Assert.Null(result.Category);
    }

    [Fact]
    public void TestKnownEnumValueDeserializes()
    {
      var jsonUtility = new JsonUtility();
      var json = "{\"category\":\"VALUE_ONE\"}";

      var result = jsonUtility.Deserialize<TestModel>(json);

      Assert.Equal(TestEnum.VALUE_ONE, result.Category);
    }

    [Fact]
    public void TestKnownEnumValueDeserializesForNonNullable()
    {
      var jsonUtility = new JsonUtility();
      var json = "{\"category\":\"VALUE_TWO\"}";

      var result = jsonUtility.Deserialize<NonNullableTestModel>(json);

      Assert.Equal(TestEnum.VALUE_TWO, result.Category);
    }

    [Fact]
    public void TestUnknownEnumValueThrowsForNonNullable()
    {
      var jsonUtility = new JsonUtility();
      var json = "{\"category\":\"UNKNOWN_VALUE\"}";

      Assert.Throws<JsonException>(() => jsonUtility.Deserialize<NonNullableTestModel>(json));
    }
  }
}
