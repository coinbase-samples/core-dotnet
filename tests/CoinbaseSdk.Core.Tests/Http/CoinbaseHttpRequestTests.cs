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

namespace CoinbaseSdk.Core.Tests.Http
{
    using System.Collections.Generic;
    using System.Net.Http;
    using CoinbaseSdk.Core.Credentials;
    using CoinbaseSdk.Core.Http;
    using CoinbaseSdk.Core.Serialization;
    using Xunit;

    public class CoinbaseHttpRequestTests
    {
        private readonly CoinbaseCredentials _credentials;
        private readonly IJsonUtility _jsonUtility;

        public CoinbaseHttpRequestTests()
        {
            _credentials = new CoinbaseCredentials("accessKey", "passphrase", "signingKey");
            _jsonUtility = new JsonUtility();
        }

        [Fact]
        public void Constructor_GetRequest_BuildsUriWithQueryString()
        {
            var path = "api.coinbase.com/v2/user";
            var method = "GET";
            var request = new { id = "123" };

            var httpRequest = new CoinbaseHttpRequest(path, method, _credentials, request, _jsonUtility);

            Assert.Equal(HttpMethod.Get, httpRequest.Method);
            Assert.Contains("id=123", httpRequest.Uri.Query);
            Assert.Empty(httpRequest.Content);
        }

        [Fact]
        public void Constructor_PostRequest_BuildsBody()
        {
            var path = "api.coinbase.com/v2/user";
            var method = "POST";
            var request = new { name = "New User" };

            var httpRequest = new CoinbaseHttpRequest(path, method, _credentials, request, _jsonUtility);

            Assert.Equal(HttpMethod.Post, httpRequest.Method);
            Assert.Contains(@"""name"":""New User""", httpRequest.Content);
            Assert.DoesNotContain("name=New User", httpRequest.Uri.Query);
        }

        [Fact]
        public void Constructor_BuildsHeaders()
        {
            var path = "api.coinbase.com/v2/user";
            var method = "GET";

            var httpRequest = new CoinbaseHttpRequest(path, method, _credentials, null, _jsonUtility);

            Assert.Contains("X-CB-ACCESS-KEY", httpRequest.Headers.Keys);
            Assert.Contains("X-CB-ACCESS-SIGNATURE", httpRequest.Headers.Keys);
            Assert.Contains("X-CB-ACCESS-TIMESTAMP", httpRequest.Headers.Keys);
            Assert.Contains("X-CB-ACCESS-PASSPHRASE", httpRequest.Headers.Keys);

            Assert.Equal(_credentials.AccessKey, httpRequest.Headers["X-CB-ACCESS-KEY"]);
            Assert.Equal(_credentials.Passphrase, httpRequest.Headers["X-CB-ACCESS-PASSPHRASE"]);
        }
    }
}
