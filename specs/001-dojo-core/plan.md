# Implementation Plan: UnityDojoCoach v0.1 — Dojo Core

**Branch**: `main` | **Date**: 2026-09-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-dojo-core/spec.md`

## Summary

Build a small .NET 10 command-line application with `status`, `next`, and `hint`. Authored C#
challenges live in `curriculum/csharp/`; an untracked local JSON file stores the active challenge,
self-declared completions, and revealed hints. Use standard library argument handling, file I/O,
and JSON serialization. Keep command logic separate from file loading only where that makes it
easier to read and test. Add one MSTest project for the state transitions and CLI behavior. The
[research](research.md), [data model](data-model.md), [CLI contract](contracts/cli.md), and
[quickstart](quickstart.md) define the design and validation.

## Technical Context

**Language/Version**: C# on .NET 10; SDK 10.0.401 is installed locally

**Primary Dependencies**: .NET standard library for the application; MSTest packages for tests only

**Storage**: Authored challenge JSON in `curriculum/csharp/`; local `.dojo/progress.json`

**Testing**: `dotnet test` with MSTest, plus end-to-end command checks from an isolated directory

**Target Platform**: Local .NET 10 command line on Windows, Linux, or macOS

**Project Type**: Single console application and one test project

**Performance Goals**: Find and display the next challenge in under 10 seconds with 100 challenges

**Constraints**: Offline and single student; no web UI, database, auth, Unity, AI, adaptive learning,
automated grading, separate completion command, or complete solution disclosure

**Scale/Scope**: Three commands, one C# curriculum, one local progress file, initial sample challenges

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Gate | Pre-design | Post-design |
|---|---|---|---|
| Learning through practice | Challenges and CLI must support student action and readable feedback | Pass | Pass: prompts, status, and explicit self-declared advancement |
| Protected student work | No planned write or repair under `StudentWork/` | Pass | Pass: reads authored curriculum and writes only local progress |
| Progressive teaching | Hints must reveal one stage in order and no full solution | Pass | Pass: four ordered prompts and a hard stop after the fourth |
| Simple infrastructure | No unnecessary layers or runtime dependencies | Pass | Pass: one console project, direct file handling, no DI framework |
| Reuse before reinvention | Evaluate mature tools for commodity needs | Pass | Pass: built-in JSON/CLI and Microsoft-maintained MSTest; rationale in research |
| Scoped and validated | Cover requested commands and run build plus relevant tests | Pass | Pass: explicit test and manual validation scenarios below |
| Git hygiene and private data | Keep progress and build output out of Git; review staged scope | Pass | Pass: implementation will add ignore rules for `.dojo/`, `bin/`, and `obj/` |

There are no justified constitution violations and no unresolved clarifications.

## Project Structure

### Documentation (this feature)

```text
specs/001-dojo-core/
├── spec.md
├── checklists/requirements.md
├── plan.md
├── research.md
├── data-model.md
├── contracts/cli.md
├── quickstart.md
└── tasks.md                 # Dependency-ordered implementation and validation work
```

### Source Code (repository root)

```text
.gitignore                   # Ignore local progress and .NET build output
src/Dojo/
├── Dojo.csproj
├── Program.cs               # Parse the three commands and present results
├── Challenge.cs             # Small challenge data type
├── ChallengeCatalog.cs      # Load, order, and validate authored challenges
├── Progress.cs              # Small progress data type and state checks
├── ProgressStore.cs         # Read/write local JSON without silent reset
└── DojoApp.cs               # Status, next, and hint transitions
curriculum/csharp/
├── 001-*.json               # Authored prompts, no solutions
└── 002-*.json
tests/Dojo.Tests/
├── Dojo.Tests.csproj
├── DojoWorkspace.cs          # Isolated curriculum/progress fixtures
├── NextCliTests.cs
├── NextStateTests.cs
├── StatusCliTests.cs
├── StatusStateTests.cs
├── HintCliTests.cs
├── HintStateTests.cs
├── ErrorTests.cs             # Shared errors and argument validation
└── ProgressSaveTests.cs      # Failed-write and replacement preservation
```

**Structure Decision**: One production project and one test project are enough. Keep files named
for the responsibility a junior developer will look for. Use direct constructors and simple
methods; add no generic repository, command framework, dependency injection container, or
persistence abstraction. Tests may run the app against temporary curriculum and progress data.

Set `<AssemblyName>dojo</AssemblyName>` and `<UseAppHost>true</UseAppHost>` in `Dojo.csproj`.
Development uses `dotnet run --project src/Dojo/Dojo.csproj -- <command>`; local Windows use
builds Release and invokes `.\src\Dojo\bin\Release\net10.0\dojo.exe <command>` from the
repository root. See the quickstart for both invocation paths.

Treat missing catalog references as unavailable definitions, not corrupt progress. Status
retains saved totals and reports unavailable IDs. Guard `next` and `hint` before mutation when
the active definition is unavailable. Preserve historical IDs and counts during later valid
saves. Write a temporary file before replacement and publish success/hint output only after
the save succeeds; test that both write and replacement failures preserve the original bytes.

## Build and Validation Strategy

1. Implement and review the three CLI flows against the [contract](contracts/cli.md), with
   state rules from the [data model](data-model.md). Include a small authored curriculum with
   at least two challenges, four progressive prompts each, and no solutions.
2. Add `.dojo/`, `**/bin/`, and `**/obj/` to the root `.gitignore` before exercising local progress.
   Do not edit `StudentWork/` or include local progress in a commit.
3. Restore and build at setup and each story checkpoint. Categorize story tests as `US1`,
   `US2`, and `US3`. Run only `TestCategory=US1` at the next checkpoint and
   `TestCategory=US1|TestCategory=US2` at the status checkpoint; use `TestCategory=US3`
   for hint's initial failing tests. Run the full suite only after all three commands exist.
   Tests cover fresh status, first
   activation, advancement and final completion, persistence across invocations, hint order and
   exhaustion, duplicate prevention, missing/invalid curriculum, malformed progress, and error
   exit codes. Check preserved counts for unavailable definitions and byte-for-byte preservation
   after temporary-write/replacement failures. Argument errors are covered once in ErrorTests.
4. Follow [quickstart.md](quickstart.md) from an isolated temporary working directory for the
   story-specific walkthroughs, then the full end-to-end sequence after all commands exist.
   Verify status values, prompt order, and absence of solution text. Include manual SC-004:
   a first-time student identifies the current challenge and completed count from status
   without additional instructions; record the observation and resolve unclear labels.
5. Review the final diff for scope, secrets, generated files, and any change under `StudentWork/`.
   Report any build or test check that could not run.
