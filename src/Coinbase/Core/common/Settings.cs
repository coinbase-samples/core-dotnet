namespace Coinbase.Core.Common
{
  using System.Text.Json;

  public static class Settings
  {
    public static JsonSerializerOptions BaseJsonSerializerOptions { get; } = new JsonSerializerOptions(JsonSerializerDefaults.Web);
  }
}