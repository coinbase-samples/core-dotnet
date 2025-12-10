# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-11-24

### Added
- **Centralized Configuration**: Added `HttpClientDefaults` and `RetryPolicyDefaults` for consistent default values.
- **Extensibility Hooks**: Added protected virtual methods in `CoinbaseClient` (`BuildRequest`, `ConfigureRequest`, `SendHttpRequestAsync`, `ValidateResponse`) to simplify subclass customization.
- **Robust Serialization**: Added `NullOnUnknownEnumConverter` to gracefully handle unknown enum values as `null` during deserialization.
- **Testing Infrastructure**: Added `StubHttpMessageHandler` and comprehensive unit tests covering client logic, credentials, error handling, and serialization.

### Changed
- **Code Refactoring**: Consolidated request logic and exception handling in `CoinbaseClient`, significantly reducing code duplication in subclasses like `CoinbasePrimeClient`.
- **Retry Logic Simplified**: Removed redundant `IRetryPolicyProvider` abstraction; `SystemNetHttpClient` now integrates directly with the Polly policy provider.
- **Thread Safety**: Improved `JsonUtility` initialization using `Lazy<T>` to replace manual locking.
- **API Visibility**: Restricted visibility of internal properties (e.g., `CoinbaseClient.HttpClient`, `CoinbaseService.Client`) to `protected` to better encapsulate implementation details.
- **Documentation**: Unified retry policy and cancellation documentation across XML comments and README.

### Fixed
- **Parameter Validation**: Fixed potential `NullReferenceException` in `CoinbaseCredentials` constructor.
