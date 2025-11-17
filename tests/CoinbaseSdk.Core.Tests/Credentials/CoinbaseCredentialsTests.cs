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

namespace CoinbaseSdk.Core.Tests.Credentials
{
    using System;
    using CoinbaseSdk.Core.Credentials;
    using CoinbaseSdk.Core.Error;
    using Xunit;

    public class CoinbaseCredentialsTests
    {
        [Fact]
        public void Constructor_WithParameters_CreatesInstance()
        {
            var credentials = new CoinbaseCredentials("accessKey", "passphrase", "signingKey");
            Assert.NotNull(credentials);
            Assert.Equal("accessKey", credentials.AccessKey);
            Assert.Equal("passphrase", credentials.Passphrase);
            Assert.Equal("signingKey", credentials.SigningKey);
        }

        [Fact]
        public void Constructor_NullAccessKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CoinbaseCredentials(null, "passphrase", "signingKey"));
        }

        [Fact]
        public void Constructor_NullPassphrase_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CoinbaseCredentials("accessKey", null, "signingKey"));
        }

        [Fact]
        public void Constructor_NullSigningKey_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CoinbaseCredentials("accessKey", "passphrase", null));
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
        public void Sign_NonBase64SigningKey_ReturnsSignature()
        {
            // Arrange - use non-base64 string, should fallback to UTF8
            var credentials = new CoinbaseCredentials("accessKey", "passphrase", "plaintext-key");
            var timestamp = "1234567890";
            var method = "POST";
            var path = "/api/test";
            var body = "{\"data\":\"value\"}";

            // Act
            var signature = credentials.Sign(timestamp, method, path, body);

            // Assert
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
        }

        [Fact]
        public void Sign_DifferentInputs_GeneratesDifferentSignatures()
        {
            // Arrange
            var signingKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("secret"));
            var credentials = new CoinbaseCredentials("accessKey", "passphrase", signingKey);

            // Act
            var sig1 = credentials.Sign("1234567890", "GET", "/path1", "");
            var sig2 = credentials.Sign("1234567890", "GET", "/path2", "");

            // Assert
            Assert.NotEqual(sig1, sig2);
        }
    }
}
