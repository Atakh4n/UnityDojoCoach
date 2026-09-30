# Research: Dojo Core

## Command-line interface

- **Decision**: Use a .NET 10 console application with an exact switch on one command argument:
  `status`, `next`, or `hint`. Show usage and return a nonzero exit code for unknown arguments.
- **Rationale**: Three fixed commands need no parser package. C# supplies arguments through `Main`,
  and `dotnet run -- ...` forwards them to the program. This remains easy to study.
- **Alternatives considered**: `System.CommandLine` would help with nested commands and options,
  which v0.1 does not have. See [C# command-line arguments](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/main-command-line)
  and [dotnet run](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-run).

## Challenge and progress storage

- **Decision**: Use one authored JSON file per challenge under `curriculum/csharp/`, each with a
  stable ID and explicit order. Keep one local progress file at `.dojo/progress.json` in the
  repository root; add `.dojo/` to the root `.gitignore` during implementation.
- **Rationale**: The user requested JSON progress and a local C# curriculum. The same format
  for authored challenges keeps parsing simple. Explicit order avoids relying on file names or
  filesystem enumeration order. Local progress stays out of Git.
- **Alternatives considered**: A database is excluded by scope. A single curriculum JSON file
  would work but is harder to review as challenge content grows. User-profile storage would
  complicate locating and isolating progress for this single-checkout CLI.

## JSON validation and safe updates

- **Decision**: Use `System.Text.Json` with strict field and state validation after reading.
  Missing progress means a fresh state. Invalid existing progress produces an error and is not
  reset. On state changes, write a temporary file beside progress, then move it over the old
  file; remove the temporary file on failure.
- **Rationale**: Built-in JSON support avoids a runtime dependency. Validation is still needed
  because deserialization alone does not enforce distinct IDs, required text, hint limits, or
  references to curriculum entries. A temporary file reduces the chance of a partial write;
  it does not promise crash-proof persistence on every filesystem.
- **Alternatives considered**: Direct overwrite is shorter but can truncate progress on a
  failed write. SQLite is outside scope. See [System.Text.Json deserialization](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization)
  and [File.Move overwrite behavior](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.move).

## Test approach

- **Decision**: Use a small .NET 10 MSTest project with test-only packages and `dotnet test`.
  Test the command behavior against temporary curriculum and progress directories, including
  real CLI invocation for the main paths. Keep the production application package-free.
- **Rationale**: MSTest is a mature, actively maintained open-source testing solution supported
  by Microsoft. It gives readable assertions and repeatable regression checks without building
  a custom test runner.
- **Alternatives considered**: A handwritten assertion executable avoids packages but
  duplicates commodity test-runner functionality. See [dotnet test](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
  and [MSTest project guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-mstest).

## Scope resolution

- **Decision**: `dojo next` is the only completion action. With no active challenge it activates
  the first available one; otherwise it marks the active challenge complete and activates the
  next. Hints stop after the fourth teaching step; no solution is stored or revealed.
- **Rationale**: This directly follows the clarified feature specification and the project
  constitution. Automated assessment, extra commands, Unity, web services, and AI are deferred.
- **Alternatives considered**: A separate `complete` command or source-code evaluator would
  expand v0.1 beyond the requested three commands.
