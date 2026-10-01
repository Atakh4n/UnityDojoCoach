# UnityDojoCoach

Dojo Core v0.1 is a small .NET 10 CLI for practicing C#. It reads authored challenges and
stores local progress. It never reads, edits, builds, or repairs `StudentWork/` exercises.

## Run locally

Install the .NET 10 SDK for development. From the repository root:

```powershell
dotnet run --project src/Dojo/Dojo.csproj -- status
dotnet run --project src/Dojo/Dojo.csproj -- next
dotnet run --project src/Dojo/Dojo.csproj -- hint
```

For normal local Windows use:

```powershell
dotnet build src/Dojo/Dojo.csproj --configuration Release
.\src\Dojo\bin\Release\net10.0\dojo.exe status
.\src\Dojo\bin\Release\net10.0\dojo.exe next
.\src\Dojo\bin\Release\net10.0\dojo.exe hint
```

The executable is framework-dependent and requires the .NET 10 runtime. Run from the repository
root so it finds `curriculum/csharp/` and `.dojo/progress.json`; no PATH installation is needed.

## Three commands

- `status` shows the current challenge, completed count, and total hints used without saving.
- `next` first activates a challenge. Later calls declare the active challenge complete and
  advance; the final completion leaves no active challenge. There is no automated grading.
- `hint` reveals one new prompt: question, small hint, concept, then pseudocode. After four
  prompts it reports exhaustion. No command reveals a complete solution.

Progress is saved in ignored `.dojo/progress.json`. Empty curriculum starts with no active
challenge and zero counts when no progress exists. Missing definitions never reset saved
progress: status reports unavailable IDs and keeps totals; next/hint fail if the active
definition is unavailable. Failed saves preserve the previous valid file and disclose no new
hint. Malformed or unreadable data is reported rather than silently reset.

Usage errors exit `2`; data/I/O errors exit `1`; success and normal empty states exit `0`.
All output is UTF-8. This version has no UI, database, auth, network/AI, Unity integration,
adaptive learning, or additional commands.

## Build and validate

```powershell
dotnet restore tests/Dojo.Tests/Dojo.Tests.csproj
dotnet build src/Dojo/Dojo.csproj
dotnet test tests/Dojo.Tests/Dojo.Tests.csproj
```

The application has no runtime packages; MSTest is test-only. Tests use isolated temporary
curriculum/progress directories and never student files. See
[the validation guide](specs/001-dojo-core/quickstart.md) for story-specific filters, walkthroughs,
Windows launch checks, performance checks, and the manual first-time readability check.
