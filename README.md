# Coinbase .NET Core Library

## Overview

The **Coinbase .NET Core Library** (`CoinbaseSdk.Core`) is a shared foundational library designed to power Coinbase .NET SDKs. It provides essential, common functionality required for building robust and consistent API clients.

This library is intended for internal use by Coinbase SDKs and advanced users building custom integrations who need a standardized base.

## Key Features

*   **Base Client Architecture**: Provides an abstract `CoinbaseClient` to handle the lifecycle of API requests.
*   **HTTP Communication**: Encapsulates `HttpClient` usage with standardized request/response handling.
*   **Serialization**: Includes `JsonUtility` wrapping `System.Text.Json` with custom converters for:
    *   Resilient Enum handling (`NullOnUnknownEnumConverter`).
    *   Standardized Date/Time formatting (`UtcIso8601DateTimeOffsetConverter`).
*   **Error Handling**: Defines a hierarchy of exceptions (`CoinbaseException`, `CoinbaseClientException`) for consistent error reporting.
*   **Authentication**: Structured credential management for API access.

## Installation

The library is available on NuGet:

```bash
dotnet add package CoinbaseSdk.Core
```

## License

This project is licensed under the [Apache License, Version 2.0](LICENSE).