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

namespace CoinbaseSdk.Core.Tests.Error
{
    using System;
    using System.Net;
    using CoinbaseSdk.Core.Error;
    using Xunit;

    public class CoinbaseExceptionTests
    {
        [Fact]
        public void CoinbaseException_Constructor_SetsProperties()
        {
            var statusCode = HttpStatusCode.BadRequest;
            var responseBody = "Error message";
            var exception = new CoinbaseException(statusCode, responseBody);

            Assert.Equal(statusCode, exception.StatusCode);
            Assert.Equal(responseBody, exception.Message);
            Assert.Contains(statusCode.ToString(), exception.ToString());
        }

        [Fact]
        public void CoinbaseClientException_Constructor_SetsMessage()
        {
            var message = "Client error";
            var exception = new CoinbaseClientException(message);

            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void CoinbaseClientException_Constructor_SetsMessageAndInnerException()
        {
            var message = "Client error";
            var innerException = new Exception("Inner error");
            var exception = new CoinbaseClientException(message, innerException);

            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }
    }
}
