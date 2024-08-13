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

namespace CoinbaseSdk.Core.Error
{
    /// <summary>
    /// Exception thrown when an error occurs in the Coinbase API Client.
    /// </summary>
    public class CoinbaseClientException : CoinbaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoinbaseClientException"/> class from a message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public CoinbaseClientException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoinbaseClientException"/> class from a message and a cause.
        /// </summary>
        /// <param name="message">Error Message.</param>
        /// <param name="cause">Underlying exception.</param>
        public CoinbaseClientException(string message, System.Exception cause)
            : base(message, cause)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoinbaseClientException"/> class from a cause.
        /// </summary>
        /// <param name="cause">Underlying exception.</param>
        public CoinbaseClientException(System.Exception cause)
            : base(cause.Message, cause)
        {
        }
    }
}
