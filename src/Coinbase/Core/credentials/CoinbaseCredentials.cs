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

  /// <summary>
  /// Class that represents the credentials used to authenticate with the Coinbase API.
  /// </summary>
  public class CoinbaseCredentials
  {
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
      if (string.IsNullOrWhiteSpace(accessKey.Trim()))
      {
        throw new CoinbaseClientException("Access key is required");
      }

      this.AccessKey = accessKey;

      if (string.IsNullOrWhiteSpace(passphrase.Trim()))
      {
        throw new CoinbaseClientException("Passphrase is required");
      }

      this.Passphrase = passphrase;

      if (string.IsNullOrWhiteSpace(signingKey))
      {
        throw new CoinbaseClientException("Signing key is required");
      }

      this.SigningKey = signingKey;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoinbaseCredentials"/> class.
    /// </summary>
    public CoinbaseCredentials()
    {
    }

    required public string AccessKey { get; set; }

    required public string Passphrase { get; set; }

    required public string SigningKey { get; set; }

    public string Sign(string timestamp, string method, string path, string body)
    {
      try
      {
        string message = $"{timestamp}{method}{path}{body}";

        byte[] hmacKey;
        try
        {
          hmacKey = Convert.FromBase64String(this.SigningKey);
        }
        catch (FormatException)
        {
          hmacKey = Encoding.UTF8.GetBytes(this.SigningKey);
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
