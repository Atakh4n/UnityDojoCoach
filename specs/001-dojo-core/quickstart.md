# Quickstart and Validation: Dojo Core

This guide describes checks to run **after implementation**. It does not change the student's
work. Run commands from the repository root unless a step changes directory.

## Prerequisites

- .NET 10 SDK (`dotnet --version` begins with `10.`).
- At least two authored challenge files under `curriculum/csharp/`, each with four prompts and
  no complete solution.
- The [CLI contract](contracts/cli.md) and [data model](data-model.md) are the expected behavior.

## Build and automated checks

```powershell
dotnet restore src/Dojo/Dojo.csproj
dotnet build src/Dojo/Dojo.csproj --no-restore
dotnet test tests/Dojo.Tests/Dojo.Tests.csproj
```

Expect all commands to exit successfully. The tests should cover state transitions, saved
progress, hint order and exhaustion, invalid data, and error handling. Inspect the test output
for failures; a successful build alone does not validate behavior.

## Isolated command walkthrough

Use a temporary working directory so the walkthrough does not alter real progress. In
PowerShell, from the repository root:

```powershell
$dojoRepo = (Get-Location).Path
$dojoTrial = Join-Path ([System.IO.Path]::GetTempPath()) ("dojo-check-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path (Join-Path $dojoTrial 'curriculum') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $dojoRepo 'curriculum/csharp') -Destination (Join-Path $dojoTrial 'curriculum/csharp') -Recurse
Push-Location $dojoTrial
dotnet run --project (Join-Path $dojoRepo 'src/Dojo/Dojo.csproj') -- status
dotnet run --project (Join-Path $dojoRepo 'src/Dojo/Dojo.csproj') -- next
dotnet run --project (Join-Path $dojoRepo 'src/Dojo/Dojo.csproj') -- hint
dotnet run --project (Join-Path $dojoRepo 'src/Dojo/Dojo.csproj') -- status
dotnet run --project (Join-Path $dojoRepo 'src/Dojo/Dojo.csproj') -- next
dotnet run --project (Join-Path $dojoRepo 'src/Dojo/Dojo.csproj') -- status
Pop-Location
```

Expected sequence: initial status shows no active challenge and zero counts; first `next`
activates the first challenge without completing one; `hint` reveals only the guiding question;
the following status reports one hint; second `next` completes the first challenge and
activates the second; final status reports one completed challenge and one hint used. The
temporary `.dojo/progress.json` should reflect the same state.

Repeat `hint` four times on a fresh active challenge to check ordered prompts. The fifth call
must report `No more hints available.` without increasing usage. Continue `next` until the last
challenge is completed; status must then show no active challenge and the correct completed
count. A further `next` must not change that count.

Run the app with an unknown command and with deliberately malformed progress in the temporary
directory. Confirm a nonzero exit code, an informative error, and no overwrite of malformed
progress. Restore the test directory by using a new temporary path for each run. Verify that
`StudentWork/` and any real `.dojo/progress.json` in the repository remain untouched.
