# Quickstart and Validation: Dojo Core

This guide describes checks to run **after implementation**. It does not change the student's
work. Run commands from the repository root unless a step changes directory.

## Prerequisites

- .NET 10 SDK (`dotnet --version` begins with `10.`).
- At least two authored challenge files under `curriculum/csharp/`, each with four prompts and
  no complete solution.
- The [CLI contract](contracts/cli.md) and [data model](data-model.md) are the expected behavior.

## Build and automated checks

Configure `<AssemblyName>dojo</AssemblyName>` and `<UseAppHost>true</UseAppHost>` in
`src/Dojo/Dojo.csproj`. Development invocation is
`dotnet run --project src/Dojo/Dojo.csproj -- <command>`. For normal local Windows invocation,
run `dotnet build src/Dojo/Dojo.csproj --configuration Release`, then
`.\src\Dojo\bin\Release\net10.0\dojo.exe <command>` from the repository root.
The framework-dependent executable requires the .NET 10 runtime; no PATH installation is needed.

Restore and build at setup and each story checkpoint. Categorize story tests as `US1`, `US2`,
and `US3`; initial failing runs filter to the story being added. At T015 run only
`dotnet test tests/Dojo.Tests/Dojo.Tests.csproj --filter "TestCategory=US1"`.
At T020 run only
`dotnet test tests/Dojo.Tests/Dojo.Tests.csproj --filter "TestCategory=US1|TestCategory=US2"`.
Do not run the full suite until all three commands exist at T025. The following full checks
also run after cross-cutting error/save tests and any later fixes:

```powershell
dotnet restore src/Dojo/Dojo.csproj
dotnet build src/Dojo/Dojo.csproj --no-restore
dotnet test tests/Dojo.Tests/Dojo.Tests.csproj
```

Expect all commands to exit successfully. The tests should cover state transitions, saved
progress, hint order and exhaustion, invalid data, and error handling. Inspect the test output
for failures; a successful build alone does not validate behavior.

## Story-specific CLI walkthroughs

Use the temporary setup below for each independent story. Capture the absolute project path
before changing directory. Always finish with `Pop-Location`; use `try/finally` when automating.
Keep deliberately changed fixtures inside the temporary directory only.

- **US1 / T015**: With exactly two copied challenges and no progress, run only `next` four
  times in separate processes. Inspect temporary progress JSON after each call: first active
  with zero completions, second active with one completion, none active with two completions,
  then no change. Do not use unimplemented status/hint. Repeat with fresh empty curriculum:
  no challenges available and no progress file created.
- **US2 / T020**: Seed temporary progress directly with the first copied challenge completed,
  the second active, and hint counts of one and two respectively. Run only status: expect the
  second challenge, one completion, three hints. Compare progress hashes before/after; they
  must match. With fresh empty curriculum and no progress, expect none/zero/zero and no file.
- **US3 / T025**: Seed temporary progress with the first copied challenge active, an empty
  completion list and empty hint map. Run hint five times in separate processes: question,
  small hint, concept, pseudocode, exhaustion. Inspect JSON: count four, no fifth increment.
  No next/status invocation is needed for this story's independent proof.

Seed JSON using the fields in [data-model.md](data-model.md) and actual copied challenge IDs;
do not assume filenames are IDs. Review prompts for absence of complete solutions.

## Final isolated command walkthrough (T031)

Run this combined walkthrough only after all three commands are implemented.

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

## Unavailable definitions and failed saves

In a new temporary workspace, seed valid progress with one completed challenge, another active,
and hint counts one and two. Capture the progress hash; remove only temporary copied definitions.
Status must show the active saved ID as unavailable, list distinct unavailable IDs, and retain
one completion and three hints. Next and hint must exit `1` without modifying the hash or
revealing a hint. Check `$LASTEXITCODE` immediately after each command.

Repeat with no active ID and historical completions/hints: status retains totals, next reports
no challenges available without saving, and hint reports no active challenge. With fresh empty
curriculum and no saved data, status shows none/zero/zero. Missing historical IDs alone must not
block next/hint when their required active definition is present; later saves retain history.

Automated `ProgressSaveTests.cs` must force temporary-write and destination-replacement failures
using deterministic isolated fixtures without elevated permissions. Assert the previous valid
JSON remains byte-for-byte identical and parseable, temporary files are cleaned up where possible,
and failed next/hint emit no successful change or newly revealed hint.

## Final acceptance checks

- **SC-001**: Use 100 synthetic safe challenge definitions and fresh temporary progress; time
  one next call with the built executable (exclude compilation). It must finish under ten seconds.
- **SC-002**: With ten synthetic safe definitions, use separate next processes and status after
  each restart. First activation has zero completions; each later call adds one; after ten
  completions no challenge is active. Another next leaves the count unchanged.
- Verify the Release Windows executable exists as `dojo.exe`; invoke its absolute path with
  status, next, and hint from a temporary workspace. Its output/exit contract must match development
  invocation, and progress must remain inside the trial directory.
- **SC-004 / T032**: Show seeded status output to a first-time student without additional
  instructions. Ask them to identify the current challenge and completed count. Record observed
  answers and pass/fail here after the check; do not claim a pass before it occurs. Resolve unclear
  labels and rerun affected tests and the usability check when needed.

Synthetic fixtures must contain generic task text and four safe teaching stages, never student
solutions. T033 reviews scoped Git changes and creates a checkpoint only during future
implementation, after validation. No implementation or commit is part of this planning revision.
