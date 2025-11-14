# Repository Guidelines

## Project Structure & Module Organization
The solution `core-dotnet.sln` wires together the library under `src/CoinbaseSdk/Core` and its xUnit test suite in `tests/CoinbaseSdk.Core.Tests`. Production code is grouped by concern (for example `client/`, `credentials/`, `error/`, `http/`, `serialization/`, `service/`). Keep new runtime assets alongside the feature folder that uses them, and expose public surface area through the `CoinbaseSdk.Core` namespace. Test fixtures mirror the library layout under `tests/.../Serialization` or sibling files such as `EnumSerializationTest.cs`; follow this pattern for new scenarios.

## Build, Test, and Development Commands
Run `dotnet restore core-dotnet.sln` before the first build to hydrate all packages. Use `dotnet build core-dotnet.sln -c Debug` for local validation; CI treats warnings as errors, so resolve StyleCop and compiler feedback immediately. Execute `dotnet test tests/CoinbaseSdk.Core.Tests/CoinbaseSdk.Core.Tests.csproj --collect:"XPlat Code Coverage"` to run the full suite and capture coverlet coverage artifacts. When iterating, scope to a single test via `dotnet test ... --filter FullyQualifiedName~Namespace.Class`.

## Coding Style & Naming Conventions
We compile against `net8.0` with C# 11 features enabled. Indent with four spaces; tabs are blocked. StyleCop analyzers run via `StyleCopRules.ruleset` and must pass cleanly—never disable rules without discussion. Follow standard .NET naming: PascalCase for types and methods, camelCase for locals and parameters, and `I`-prefixed interfaces. Place new files in namespaces that match their directory and keep each type in its own file. Prefer `readonly` for immutable fields and favor expressions over mutable state where possible.

## Testing Guidelines
Author unit tests with xUnit attributes (`[Fact]` and `[Theory]`). Name test classes after the type under test plus the concern, e.g. `JsonUtilitySerializeTests`. Keep assertions specific and deterministic; avoid real network or clock dependencies. Coverage is collected via coverlet, so ensure new code paths include positive and negative tests. If a bug is fixed, add a regression test in the matching directory.

## Commit & Pull Request Guidelines
Commit messages follow a short, imperative style (`add serialization guard`). Group related changes together and keep diff noise low. Every pull request should include context in the description, reference the tracking issue, and list validation steps (`dotnet test`). Link screenshots only when UI-affecting. Highlight any breaking changes and call out new public APIs for reviewer attention.

## Security & Configuration Tips
Never commit API keys or customer data; prefer test fixtures under `tests/.../Resources` for sample payloads. Configuration defaults live in code—document overrides in the PR if you introduce new environment variables.
