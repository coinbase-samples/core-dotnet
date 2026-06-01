# Coinbase .NET Core Library

The canonical source repository is **[coinbase/core-dotnet](https://github.com/coinbase/core-dotnet)**. The former [coinbase-samples/core-dotnet](https://github.com/coinbase-samples/core-dotnet) repository is deprecated. New work and releases continue on `coinbase/core-dotnet` starting with **0.2.0**. The NuGet package ID is unchanged (`CoinbaseSdk.Core`).

## Overview

The **Coinbase .NET Core Library** (`CoinbaseSdk.Core`) is the foundational building block for Coinbase .NET SDKs. It provides a standardized, robust, and thread-safe infrastructure for building API clients, handling authentication, HTTP communication, and serialization.

This library is primarily intended for internal use by Coinbase SDKs but is also available for advanced users building custom integrations requiring a standardized base.

## Key Features

*   **Base Client Architecture**: Abstract `CoinbaseClient` managing request lifecycles with extensible hooks (`BuildRequest`, `ConfigureRequest`, `ValidateResponse`).
*   **Resilient HTTP Communication**: Wrapper around `System.Net.Http.HttpClient` with built-in Polly retry policies for transient failure handling.
*   **Robust Serialization**: `JsonUtility` wrapper around `System.Text.Json` featuring:
    *   `NullOnUnknownEnumConverter`: Graceful handling of new/unknown enum members.
    *   `UtcIso8601DateTimeOffsetConverter`: Standardized ISO-8601 date/time processing.
    *   Thread-safe initialization using `Lazy<T>`.
*   **Centralized Configuration**: `HttpClientDefaults` and `RetryPolicyDefaults` ensuring consistent default behavior across SDKs.
*   **Standardized Error Handling**: Hierarchy of exceptions (`CoinbaseException`, `CoinbaseClientException`) for uniform error reporting.

## HTTP Client & Retries

The library uses `SystemNetHttpClient` to execute requests. By default, it performs a single attempt. You can enable retries using `CallOptions`, which leverages Polly's decorrelated jitter backoff strategy to handle transient failures (like rate limits or 5xx errors) without causing retry storms.

Retries can be configured at two levels:

1.  **Per-Client**: Set default behavior for all requests made by a client instance.
2.  **Per-Request**: Override behavior for a specific API call.

### Configuration Examples

**Per-Request Configuration:**

```csharp
var callOptions = new CallOptions
{
    MaxRetries = 2,
    ShouldRetryOnStatusCodes = true,
    RetryableStatusCodes = new HashSet<HttpStatusCode> { HttpStatusCode.TooManyRequests }
};

// options are passed to the SendRequestAsync method
var response = await client.SendRequestAsync<MyResponse>(..., callOptions);
```

**Per-Client Configuration:**

```csharp
var httpClient = new SystemNetHttpClient(
    defaultCallOptions: new CallOptions
    {
        MaxRetries = 3,
        ShouldRetryOnStatusCodes = true,
        RetryableStatusCodes = new HashSet<HttpStatusCode> { HttpStatusCode.TooManyRequests }
    });

var client = new MyCoinbaseClient(credentials, httpClient: httpClient);
```

### Retry Configuration Options

| Option | Default | Description |
| --- | --- | --- |
| `MaxRetries` | `0` | Maximum number of retry attempts (0 disables retries). |
| `MedianFirstRetryDelay` | 500ms | Target median delay for the first retry (subject to jitter). |
| `MaxRetryDelay` | 1s | Maximum delay cap for any retry attempt. |
| `ShouldRetryOnStatusCodes` | `false` | Whether to retry on specific HTTP status codes. |
| `RetryableStatusCodes` | empty | Set of status codes to retry (e.g., 429, 503). |

> **Note:** Retry attempts honor the `CancellationToken` provided to the request. If canceled, pending retries are aborted.

## Installation

```bash
dotnet add package CoinbaseSdk.Core
```

## License

This project is licensed under the [Apache License, Version 2.0](LICENSE).
