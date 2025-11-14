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

namespace CoinbaseSdk.Core.Tests.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using CoinbaseSdk.Core.Client;
    using CoinbaseSdk.Core.Credentials;
    using CoinbaseSdk.Core.Error;
    using CoinbaseSdk.Core.Http;
    using CoinbaseSdk.Core.Serialization;
    using Moq;
    using Xunit;

    public class TestCoinbaseClient : CoinbaseClient
    {
        public TestCoinbaseClient(
            CoinbaseCredentials credentials,
            string apiBasePath,
            IJsonUtility jsonUtility = null,
            IHttpClient httpClient = null)
            : base(credentials, apiBasePath, jsonUtility, httpClient)
        {
        }
    }

    public class CoinbaseClientTests
    {
        private readonly Mock<IHttpClient> _mockHttpClient;
        private readonly Mock<IJsonUtility> _mockJsonUtility;
        private readonly CoinbaseCredentials _credentials;
        private readonly string _apiBasePath = "api.coinbase.com";

        public CoinbaseClientTests()
        {
            _mockHttpClient = new Mock<IHttpClient>();
            _mockJsonUtility = new Mock<IJsonUtility>();
            _credentials = new CoinbaseCredentials("accessKey", "passphrase", "signingKey");
        }

        [Fact]
        public void Constructor_NullCredentials_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new TestCoinbaseClient(null, _apiBasePath));
        }

        [Fact]
        public void Constructor_EmptyApiBasePath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new TestCoinbaseClient(_credentials, ""));
        }

        [Fact]
        public async Task SendRequestAsync_SuccessfulResponse_ReturnsDeserializedObject()
        {
            // Arrange
            var client = new TestCoinbaseClient(_credentials, _apiBasePath, _mockJsonUtility.Object, _mockHttpClient.Object);
            var expectedResponse = new TestModel { Category = TestEnum.VALUE_ONE };
            var jsonResponse = "{\"category\":\"VALUE_ONE\"}";
            var coinbaseResponse = new CoinbaseResponse(HttpStatusCode.OK, new HttpResponseMessage().Headers, jsonResponse);

            _mockHttpClient.Setup(x => x.SendAsyncRequest(It.IsAny<CoinbaseHttpRequest>(), It.IsAny<CallOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coinbaseResponse);

            _mockJsonUtility.Setup(x => x.Deserialize<TestModel>(jsonResponse))
                .Returns(expectedResponse);

            // Act
            var result = await client.SendRequestAsync<TestModel>(
                HttpMethod.Get,
                "/test",
                null,
                new[] { HttpStatusCode.OK },
                CancellationToken.None);

            // Assert
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task SendRequestAsync_UnexpectedStatusCode_ThrowsCoinbaseException()
        {
            // Arrange
            var client = new TestCoinbaseClient(_credentials, _apiBasePath, _mockJsonUtility.Object, _mockHttpClient.Object);
            var coinbaseResponse = new CoinbaseResponse(HttpStatusCode.BadRequest, new HttpResponseMessage().Headers, "Error");

            _mockHttpClient.Setup(x => x.SendAsyncRequest(It.IsAny<CoinbaseHttpRequest>(), It.IsAny<CallOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(coinbaseResponse);

            // Act & Assert
            await Assert.ThrowsAsync<CoinbaseException>(() => client.SendRequestAsync<TestModel>(
                HttpMethod.Get,
                "/test",
                null,
                new[] { HttpStatusCode.OK },
                CancellationToken.None));
        }

        [Fact]
        public async Task SendRequestAsync_HttpClientException_ThrowsCoinbaseClientException()
        {
            // Arrange
            var client = new TestCoinbaseClient(_credentials, _apiBasePath, _mockJsonUtility.Object, _mockHttpClient.Object);

            _mockHttpClient.Setup(x => x.SendAsyncRequest(It.IsAny<CoinbaseHttpRequest>(), It.IsAny<CallOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<CoinbaseClientException>(() => client.SendRequestAsync<TestModel>(
                HttpMethod.Get,
                "/test",
                null,
                new[] { HttpStatusCode.OK },
                CancellationToken.None));
        }
    }
}
