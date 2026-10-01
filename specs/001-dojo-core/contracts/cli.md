# CLI Contract: Dojo Core

The executable is named `dojo`. It accepts exactly one of three commands and is run from the
repository root, where `curriculum/csharp/` and `.dojo/` are resolved. During development,
`dotnet run --project src/Dojo/Dojo.csproj -- <command>` invokes the same contract.
Configure `AssemblyName` as `dojo` and `UseAppHost` as `true` in `Dojo.csproj`.
For normal local Windows use, build Release and run
`.\src\Dojo\bin\Release\net10.0\dojo.exe <command>` from the repository root.
This framework-dependent executable requires the .NET 10 runtime; no PATH installation is required.

| Command | Standard output | State change |
|---|---|---|
| `dojo status` | `Current challenge: <id> — <title>` or `Current challenge: none`; `Completed challenges: <count>`; `Hints used: <count>` | None |
| `dojo next` | `Challenge: <id> — <title>` followed by the task description, or `Curriculum complete.` / `No challenges available.` | Activates first; otherwise completes active and advances |
| `dojo hint` | `Hint <number>/4: <text>`, `No more hints available.`, or `No active challenge. Run dojo next.` | Reveals and records one hint only when available |

`dojo next` is the student's self-declaration that the active challenge is complete. The tool
does not check code. `dojo hint` never displays a full solution. Each invocation reveals one
new prompt in the constitution's teaching order; after the fourth, it reports exhaustion.

With fresh progress and an empty/missing catalog, status shows none and zero counts. With
saved progress, status preserves all data and totals. If the active definition is missing,
print `Current challenge: <id> (unavailable)`. For any missing definitions referenced by the
active ID, completions, or hint counts, also print `Unavailable challenges: <ids>` (distinct
IDs in ordinal order). Structurally valid saved progress remains readable with exit code `0`.
An empty catalog with no active ID makes `next` print `No challenges available.` without saving.

## Errors and exit status

- Success and normal empty states use exit code `0` and write the result to standard output.
- An unknown command, missing command, or extra argument shows `Usage: dojo <status|next|hint>`
  on standard error and returns exit code `2`.
- Invalid challenge data, malformed or unreadable progress, and write failures show a concise
  `Error: ...` message on standard error and return exit code `1`.
- On an error, existing progress remains unchanged. The CLI does not silently reset it.
- An unavailable active definition makes `next` and `hint` return `1` with
  `Error: Challenge <id> is unavailable.` on standard error, even if hints were exhausted.
  Unavailable historical IDs alone do not block operations on available definitions.
- Failed writes or replacements preserve the previous valid file byte-for-byte; no success
  result or newly revealed hint is printed before persistence succeeds.

No other command, option, automated grading flow, or solution-unlock interface belongs to v0.1.
See [data-model.md](../data-model.md) for the challenge and progress fields.
