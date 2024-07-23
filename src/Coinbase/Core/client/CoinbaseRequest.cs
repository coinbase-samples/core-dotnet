namespace Coinbase.Core.Client
{
  public interface ICoinbaseRequest
  {
    string GetRequestPath();
    string GetQueryParameters();
    string GetRequstBody();
  }
}
