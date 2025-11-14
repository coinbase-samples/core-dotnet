# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-11-14

### Added
- `NullOnUnknownEnumConverter` and supporting generic converter to treat unknown enum values as `null`.
- `CoinbaseSdk.Core.Tests` xUnit project validating serialization behavior.
- Comprehensive unit tests for `CoinbaseClient`, `CoinbaseCredentials`, `CoinbaseException`, `CoinbaseHttpRequest`, `JsonUtility`, and `UtcIso8601DateTimeOffsetConverter`.

### Changed
- `JsonUtility` now wires the new enum converter, retains string enum serialization, and opts into `DefaultJsonTypeInfoResolver`.
- Solution and packaging updated for version `0.1.0`, including test project references.

### Fixed
- `NullReferenceException` in `CoinbaseCredentials` constructor when validating null input parameters.
