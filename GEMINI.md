# Coinbase .NET Core

## Project Overview

**Coinbase .NET Core** (`CoinbaseSdk.Core`) is a foundational library designed to support other .NET samples and SDKs for Coinbase. It provides essential infrastructure for interacting with Coinbase APIs, including HTTP client management, authentication, and JSON serialization.

### Key Technologies
*   **Language:** C# 11
*   **Framework:** Targets .NET 8.0 (supports .NET Standard 2.0+, .NET Core 2.0+, .NET Framework 5+)
*   **Serialization:** `System.Text.Json`
*   **Testing:** xUnit

### Architecture
The library is organized into logical components within `src/CoinbaseSdk/Core/`:
*   **`client/`**: Defines the `ICoinbaseClient` interface and implementation for making API requests.
*   **`serialization/`**: Handles JSON serialization using `System.Text.Json`, featuring custom converters like `NullOnUnknownEnumConverter` and `UtcIso8601DateTimeOffsetConverter`.
*   **`http/`**: Contains HTTP-related abstractions (`IHttpClient`, `CoinbaseHttpRequest`, `CoinbaseResponse`).
*   **`credentials/`**: Manages API credentials.
*   **`error/`**: Defines custom exceptions and error messages.

## Building and Running

This project uses the standard .NET CLI.

### Prerequisites
*   .NET SDK (version 8.0 or compatible)

### Build
To build the project:
```bash
dotnet build
```

### Test
To run the unit tests:
```bash
dotnet test
```

## Development Conventions

### Coding Style
*   **StyleCop:** The project enforces coding standards using `Stylecop.Analyzers` and a custom rule set (`StyleCopRules.ruleset`). Ensure your code adheres to these rules to pass the build.
*   **Async/Await:** Asynchronous programming patterns are used extensively for I/O-bound operations (HTTP requests).

### Serialization
*   **Engine:** `System.Text.Json` is the default serialization engine.
*   **Configuration:** The `JsonUtility` class configures the serializer with `JsonSerializerDefaults.Web`, ignores null values when writing, and uses custom converters for enums and dates.

### Testing
*   **Framework:** Tests are written using **xUnit**.
*   **Location:** Tests are located in `tests/CoinbaseSdk.Core.Tests/`.
*   **Coverage:** `coverlet.collector` is included for code coverage analysis.
