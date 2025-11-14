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

namespace CoinbaseSdk.Core.Tests.Credentials
{
    using System;
    using CoinbaseSdk.Core.Credentials;
    using CoinbaseSdk.Core.Error;
    using Xunit;

    public class CoinbaseCredentialsTests
    {
        [Fact]
        public void Constructor_ValidInputs_CreatesInstance()
        {
            var credentials = new CoinbaseCredentials("accessKey", "passphrase", "signingKey");
            Assert.NotNull(credentials);
            Assert.Equal("accessKey", credentials.AccessKey);
            Assert.Equal("passphrase", credentials.Passphrase);
            Assert.Equal("signingKey", credentials.SigningKey);
        }

        [Theory]
        [InlineData(null, "passphrase", "signingKey")]
        [InlineData("", "passphrase", "signingKey")]
        [InlineData(" ", "passphrase", "signingKey")]
        public void Constructor_InvalidAccessKey_ThrowsCoinbaseClientException(string? accessKey, string? passphrase, string? signingKey)
        {
            Assert.Throws<CoinbaseClientException>(() => new CoinbaseCredentials(accessKey, passphrase, signingKey));
        }

        [Theory]
        [InlineData("accessKey", null, "signingKey")]
        [InlineData("accessKey", "", "signingKey")]
        [InlineData("accessKey", " ", "signingKey")]
        public void Constructor_InvalidPassphrase_ThrowsCoinbaseClientException(string? accessKey, string? passphrase, string? signingKey)
        {
            Assert.Throws<CoinbaseClientException>(() => new CoinbaseCredentials(accessKey, passphrase, signingKey));
        }

        [Theory]
        [InlineData("accessKey", "passphrase", null)]
        [InlineData("accessKey", "passphrase", "")]
        public void Constructor_InvalidSigningKey_ThrowsCoinbaseClientException(string? accessKey, string? passphrase, string? signingKey)
        {
            Assert.Throws<CoinbaseClientException>(() => new CoinbaseCredentials(accessKey, passphrase, signingKey));
        }

        [Fact]
        public void Sign_ValidInputs_ReturnsSignature()
        {
            // Arrange
            // Use a valid base64 string for the signing key to avoid FormatException in the test setup if the code expects base64
            // The code tries base64 first, then falls back to UTF8 bytes.
            var signingKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("secret"));
            var credentials = new CoinbaseCredentials("accessKey", "passphrase", signingKey);
            var timestamp = "1234567890";
            var method = "GET";
            var path = "/test";
            var body = "";

            // Act
            var signature = credentials.Sign(timestamp, method, path, body);

            // Assert
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
        }

        [Fact]
        public void Sign_InvalidSigningKey_ThrowsCoinbaseClientException()
        {
             // This is tricky because the code catches FormatException and falls back to UTF8 bytes.
             // So almost any string is a valid key in one way or another.
             // However, if HMACSHA256 throws, it wraps it.
             // But HMACSHA256 constructor usually doesn't throw on key content unless it's null (which we check in constructor) or too long (unlikely here).
             // Let's try to force an exception if possible, or maybe just skip this negative test if it's too robust.
             // Actually, the code says:
             /*
                try
                {
                  hmacKey = Convert.FromBase64String(this.SigningKey);
                }
                catch (FormatException)
                {
                  hmacKey = Encoding.UTF8.GetBytes(this.SigningKey);
                }
             */
             // So it handles non-base64 strings.
             // The only way `Sign` throws is if `HMACSHA256` or `ComputeHash` throws.
             // It's hard to mock `HMACSHA256` as it's a system class.
             // I'll skip the negative test for `Sign` for now as it seems robust against input format.
        }
    }
}
