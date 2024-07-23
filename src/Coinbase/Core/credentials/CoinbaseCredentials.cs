/*
 * Copyright 2024-present Coinbase Global, Inc.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

namespace Coinbase.Core.Credentials
{
  using System;
  using System.Security.Cryptography;
  using System.Text;
  using Coinbase.Core.Error;
  using Newtonsoft.Json;

  /// <summary>
  /// Class that represents the credentials used to authenticate with the Coinbase API.
  /// </summary>
  public class CoinbaseCredentials
  {
    [JsonProperty(Required = Required.Always)]
    private string accessKey;
    [JsonProperty(Required = Required.Always)]
    private string passphrase;
    [JsonProperty(Required = Required.Always)]
    private string signingKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseCredentials"/> class.
    /// </summary>
    /// <param name="credentialsJson">Json blob with credentials payload.</param>
    /// <exception cref="CoinbaseClientException">
    /// if <c>credentialsJson</c> is not a valid JSON object.
    /// </exception>
    public CoinbaseCredentials(string credentialsJson)
    {
      try
      {
        var credentials = JsonConvert.DeserializeObject<CoinbaseCredentials>(credentialsJson);
        this.accessKey = credentials.AccessKey;
        this.passphrase = credentials.Passphrase;
        this.signingKey = credentials.SigningKey;
      }
      catch (Exception e)
      {
        throw new CoinbaseClientException("Failed to parse credentials", e);
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseCredentials"/> class.
    /// </summary>
    /// <param name="accessKey">Coinbase API AccessKey.</param>
    /// <param name="passphrase">Coinbase API Secret Key.</param>
    /// <param name="signingKey">Coinbase API Signing Key.</param>
    public CoinbaseCredentials(
      string accessKey = null,
      string passphrase = null,
      string signingKey = null)
    {
      if (string.IsNullOrEmpty(accessKey.Trim()))
      {
        throw new CoinbaseClientException("Access key is required");
      }

      this.accessKey = accessKey;

      if (string.IsNullOrEmpty(passphrase.Trim()))
      {
        throw new CoinbaseClientException("Passphrase is required");
      }

      this.passphrase = passphrase;

      if (string.IsNullOrEmpty(signingKey))
      {
        throw new CoinbaseClientException("Signing key is required");
      }

      this.signingKey = signingKey;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseCredentials"/> class.
    /// </summary>
    public CoinbaseCredentials()
    {
    }

    public string AccessKey
    {
      get { return this.accessKey; }
      set { this.accessKey = value; }
    }

    public string Passphrase
    {
      get { return this.passphrase; }
      set { this.passphrase = value; }
    }

    public string SigningKey
    {
      get { return this.signingKey; }
      set { this.signingKey = value; }
    }

    public string Sign(long timestamp, string method, string path, string body)
    {
      try
      {
        string message = $"{timestamp}{method}{path}{body}";

        byte[] hmacKey;
        try
        {
          hmacKey = Convert.FromBase64String(this.signingKey);
        }
        catch (FormatException)
        {
          hmacKey = Encoding.UTF8.GetBytes(this.signingKey);
        }

        using var hmac = new HMACSHA256(hmacKey);
        byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToBase64String(signature);
      }
      catch (Exception e)
      {
        throw new CoinbaseClientException("Failed to generate signature", e);
      }
    }
  }
}