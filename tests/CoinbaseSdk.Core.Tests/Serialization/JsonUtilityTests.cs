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

namespace CoinbaseSdk.Core.Tests.Serialization
{
    using System;
    using System.Text.Json;
    using CoinbaseSdk.Core.Serialization;
    using Xunit;

    public class JsonUtilityTests : IDisposable
    {
        public JsonUtilityTests()
        {
            JsonUtility.ResetDefaultsForTesting();
        }

        public void Dispose()
        {
            JsonUtility.ResetDefaultsForTesting();
        }

        [Fact]
        public void Serialize_Object_ReturnsJsonString()
        {
            var jsonUtility = new JsonUtility();
            var obj = new { name = "Test" };
            var json = jsonUtility.Serialize(obj);
            Assert.Equal(@"{""name"":""Test""}", json);
        }

        [Fact]
        public void Deserialize_JsonString_ReturnsObject()
        {
            var jsonUtility = new JsonUtility();
            var json = @"{""name"":""Test""}";
            var obj = jsonUtility.Deserialize<TestObject>(json);
            Assert.Equal("Test", obj.Name);
        }

        [Fact]
        public void Constructor_WithOptions_UsesOptions()
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var utility = new JsonUtility(options);
            var obj = new { Name = "Test" };
            var json = utility.Serialize(obj);
            Assert.Equal(@"{""name"":""Test""}", json);
        }

        [Fact]
        public void ConfigureDefaults_BeforeCreation_ModifiesOptions()
        {
            JsonUtility.ConfigureDefaults(options =>
            {
                options.PropertyNamingPolicy = null;
                options.DictionaryKeyPolicy = null;
            });

            var jsonUtility = new JsonUtility();
            var obj = new { Name = "Test" };
            var json = jsonUtility.Serialize(obj);
            Assert.Equal(@"{""Name"":""Test""}", json);
        }

        [Fact]
        public void UseCustomDefaultFactory_ReplacesOptions()
        {
            JsonUtility.UseCustomDefaultFactory(() => new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DictionaryKeyPolicy = null,
            });

            var jsonUtility = new JsonUtility();
            var obj = new { Name = "Factory" };
            var json = jsonUtility.Serialize(obj);
            Assert.Equal(@"{""Name"":""Factory""}", json);
        }

        private class TestObject
        {
            public string? Name { get; set; }
        }
    }
}
