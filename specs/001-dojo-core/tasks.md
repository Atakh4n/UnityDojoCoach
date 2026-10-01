---
description: "Dependency-ordered implementation tasks for Dojo Core v0.1"
---

# Tasks: UnityDojoCoach v0.1 — Dojo Core

**Input**: Design documents in `specs/001-dojo-core/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md),
[data-model.md](data-model.md), [CLI contract](contracts/cli.md), and [quickstart.md](quickstart.md)

**Tests**: Requested for each story and for final validation. Write story tests before its
implementation and confirm they fail for the expected missing behavior.
Assign MSTest categories `US1`, `US2`, and `US3` to the corresponding story files. Initial
failing runs use only the story category. Checkpoints build and run the implemented categories
only; the full suite is permitted only after all three commands are implemented.

**Organization**: Setup and shared foundations precede three user-story phases. Tasks target
product infrastructure outside `StudentWork/`; no task may create, edit, or repair a student
solution. Keep challenge prompts educational and exclude complete solutions.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Different files and no dependency on another unfinished task in the same set.
- **[Story]**: The user story in [spec.md](spec.md); omitted for shared and final tasks.
- Paths are relative to the repository root unless they link to this feature's documents.

## Path Conventions

- Production console app: `src/Dojo/`; authored challenges: `curriculum/csharp/`.
- Tests: `tests/Dojo.Tests/`; local progress: `.dojo/progress.json` (ignored by Git).
- Feature documents: `specs/001-dojo-core/`.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the smallest buildable .NET 10 CLI and test project without product behavior.

- [ ] T001 Add `.dojo/`, `**/bin/`, and `**/obj/` ignore rules to root `.gitignore`; verify `StudentWork/` source remains visible and untouched.
- [ ] T002 Create `src/Dojo/Dojo.csproj` targeting .NET 10 with `AssemblyName` set to `dojo` and `UseAppHost` set to `true`, and a minimal `src/Dojo/Program.cs` entry point that returns usage for unsupported commands; use no runtime packages (depends on T001).
- [ ] T003 Create `tests/Dojo.Tests/Dojo.Tests.csproj` targeting .NET 10 with MSTest and a project reference to `src/Dojo/Dojo.csproj`; verify restore and initial build (depends on T002).

**Checkpoint**: Both projects restore and build; no challenge flow has been implemented.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the data and file handling shared by all three commands.

- [ ] T004 Define challenge fields in `src/Dojo/Challenge.cs`: `id` and `title` and `description` are required nonempty text; `order` is a required positive integer; `hints` contains exactly four nonempty strings in guiding-question, small-hint, concept, pseudocode order (depends on T003).
- [ ] T005 Define progress fields and validation in `src/Dojo/Progress.cs`: IDs are nonempty strings; `activeChallengeId` may be null and cannot also be completed; `completedChallengeIds` contains distinct IDs in recorded completion order; each `hintCounts` value is an integer from 0 to 4. Preserve unavailable IDs and existing order (depends on T004).
- [ ] T006 Load and sort `curriculum/csharp/*.json` in `src/Dojo/ChallengeCatalog.cs`; reject malformed files, duplicate IDs, duplicate or nonpositive orders, blank required text, and hint arrays other than four nonempty strings (depends on T004).
- [ ] T007 Read and save `.dojo/progress.json` in `src/Dojo/ProgressStore.cs`; missing file means fresh state; validate structure without rejecting unavailable catalog references or resetting saved data. Write changed state to a same-directory temporary file before replacement, preserve prior bytes on failure, and clean up temporary files where possible (depends on T005 and T006).
- [ ] T008 Create isolated temporary-curriculum and progress fixtures in `tests/Dojo.Tests/DojoWorkspace.cs`; tests must never read or write `StudentWork/` or real `.dojo/progress.json` (depends on T003 and T007).

**Checkpoint**: Shared files compile and reject invalid challenge/progress state; no command
advancement, status, or hint behavior is required yet.

---

## Phase 3: User Story 1 — Find the next challenge (Priority: P1) 🎯 MVP

**Goal**: `dojo next` activates the first challenge, then self-declares and advances exactly
one challenge per later call, including the final completion.

**Independent Test**: In a fresh temporary workspace with two ordered challenges, invoke
`dojo next` three times across separate processes. Verify first activation, one completion
and advancement, then two completions and no active challenge; another call changes nothing.

### Tests for User Story 1

- [ ] T009 [P] [US1] Add CLI contract tests in `tests/Dojo.Tests/NextCliTests.cs` for first activation, title/description output, empty curriculum, final completion, and unavailable-active error output; run the US1 category to confirm the expected failure before implementation. Shared argument errors belong only to T026 (depends on T008).
- [ ] T010 [P] [US1] Add state and persistence tests in `tests/Dojo.Tests/NextStateTests.cs` for explicit order, no completion on first call, one completion on each later call, distinct completed IDs, restart persistence, no duplicate after exhaustion, and unavailable active definitions leaving progress byte-for-byte unchanged. Include empty catalog with historical progress and preservation of unavailable historical IDs during valid advancement; run US1 to confirm expected failure (depends on T008).

### Implementation for User Story 1

- [ ] T011 [P] [US1] Author `curriculum/csharp/001-first-steps.json` with a unique nonempty `id`, positive `order`, title, task description, and exactly four nonempty progressive prompts; do not include a complete solution (depends on T009 and T010).
- [ ] T012 [P] [US1] Author `curriculum/csharp/002-next-steps.json` with a distinct `id` and `order` and the same required title, description, and four safe prompts; do not include a complete solution (depends on T009 and T010).
- [ ] T013 [US1] Implement `next` transitions in `src/Dojo/DojoApp.cs`: reject an unavailable active definition before mutation; activate first when none is active, otherwise add active ID once to completions and activate the next ordered uncompleted challenge, or clear active ID at the end. Preserve saved history; empty catalog never resets it. Persist only changed state and report success only after saving (depends on T007, T011, and T012).
- [ ] T014 [US1] Route `next` and exact one-argument usage through `src/Dojo/Program.cs`; match `specs/001-dojo-core/contracts/cli.md` for stdout, stderr, and exit codes (depends on T013).
- [ ] T015 [US1] Build and run `dotnet test tests/Dojo.Tests/Dojo.Tests.csproj --filter "TestCategory=US1"` and the isolated US1 next-only walkthrough from `specs/001-dojo-core/quickstart.md`; resolve relevant failures without touching `StudentWork/` (depends on T014).

**Checkpoint**: User Story 1 works and is independently testable. This is the smallest MVP.

---

## Phase 4: User Story 2 — Review learning progress (Priority: P2)

**Goal**: `dojo status` displays active challenge or none, distinct completed count, and total
hints revealed without changing the progress file.

**Independent Test**: Seed a temporary `.dojo/progress.json` directly with valid active,
completed, and hint-count fields. Run `dojo status` in a new process, compare all output values,
and verify the file is unchanged; no `dojo next` call is needed.

### Tests for User Story 2

- [ ] T016 [P] [US2] Add stdout contract tests in `tests/Dojo.Tests/StatusCliTests.cs` for status labels with fresh, active, fresh-empty, and saved-empty curriculum states, unavailable active output, and distinct unavailable-ID diagnostics; run US2 to confirm expected failure (depends on T008).
- [ ] T017 [P] [US2] Add read-only state tests in `tests/Dojo.Tests/StatusStateTests.cs` for saved completion counting, hint totals including unavailable/completed IDs, fresh-empty zero counts, and no progress-file creation or mutation when definitions disappear; run US2 to confirm expected failure (depends on T008).

### Implementation for User Story 2

- [ ] T018 [US2] Implement read-only status in `src/Dojo/DojoApp.cs`: no-active or unavailable-active output, distinct unavailable-ID diagnostics, saved completed count, and total hints across all IDs, without creating or rewriting progress even with an empty catalog (depends on T015, T016, and T017).
- [ ] T019 [US2] Route `status` through `src/Dojo/Program.cs` and render exact labels and error behavior from `specs/001-dojo-core/contracts/cli.md` (depends on T018).
- [ ] T020 [US2] Build and run `dotnet test tests/Dojo.Tests/Dojo.Tests.csproj --filter "TestCategory=US1|TestCategory=US2"` and the isolated US2 seeded-status walkthrough in `specs/001-dojo-core/quickstart.md`; confirm progress remains unchanged (depends on T019).

**Checkpoint**: User Story 2 can be demonstrated from seeded progress without relying on
another story's command flow.

---

## Phase 5: User Story 3 — Request guided help (Priority: P3)

**Goal**: `dojo hint` reveals one new guidance prompt in order, records usage, and stops after
four prompts without revealing a complete solution.

**Independent Test**: Seed a temporary active challenge and zero hint count. Invoke `dojo hint`
five times across separate processes. Expect four ordered prompts, then exhaustion with no
fifth increment; no `dojo next` or `dojo status` call is needed.

### Tests for User Story 3

- [ ] T021 [P] [US3] Add CLI contract tests in `tests/Dojo.Tests/HintCliTests.cs` for `Hint 1/4` through `Hint 4/4`, `No more hints available.`, and no-active output; confirm expected failure (depends on T008).
- [ ] T022 [P] [US3] Add state tests in `tests/Dojo.Tests/HintStateTests.cs` for exactly one new hint per call, persisted count from 0 to 4, no increment on exhaustion/no active challenge, unavailable active definitions failing without mutation or disclosure even at count four, preservation of historical IDs, and no solution text; run US3 to confirm expected failure (depends on T008).

### Implementation for User Story 3

- [ ] T023 [US3] Implement the hint transition in `src/Dojo/DojoApp.cs`: reject unavailable active definitions before mutation/disclosure; use the current hint count as the next index, increment once, save successfully before revealing only that prompt, and stop at four (depends on T020, T021, and T022).
- [ ] T024 [US3] Route `hint` through `src/Dojo/Program.cs` and match `specs/001-dojo-core/contracts/cli.md` for hint numbering, empty states, and exit codes (depends on T023).
- [ ] T025 [US3] Build and run the full `dotnet test tests/Dojo.Tests/Dojo.Tests.csproj` now that all three commands exist, and the isolated US3 five-call hint walkthrough in `specs/001-dojo-core/quickstart.md`; inspect prompts for teaching progression and absence of complete answers (depends on T024).

**Checkpoint**: All three commands work and each story has its own seeded-state tests.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Check errors, documentation, complete validation, and a scoped Git checkpoint.

- [ ] T026 Add cross-command error tests in `tests/Dojo.Tests/ErrorTests.cs` for malformed/unreadable progress, malformed challenge files, nonzero error exits, and unchanged progress. Cover missing/unknown/extra arguments here only; unavailable-definition behavior is already covered by story tests (depends on T025).
- [ ] T027 Add `tests/Dojo.Tests/ProgressSaveTests.cs` with deterministic isolated temporary-write and destination-replacement failures; assert prior valid JSON is byte-for-byte identical, remains parseable, and no success output or new hint is disclosed for failed `next`/`hint`. Verify temporary-file cleanup where possible without elevated permissions (depends on T026).
- [ ] T028 Resolve only failures found by T026–T027 in `src/Dojo/ChallengeCatalog.cs`, `src/Dojo/ProgressStore.cs`, `src/Dojo/DojoApp.cs`, and `src/Dojo/Program.cs`; avoid new commands or layers (depends on T027).
- [ ] T029 Document commands, self-declared completion, hint limits, preserved unavailable progress, local storage, .NET 10 prerequisites, development invocation, and normal Windows `dojo.exe` invocation in `README.md`; align `specs/001-dojo-core/quickstart.md` (depends on T028).
- [ ] T030 Run restore, Debug and Release builds of `src/Dojo/Dojo.csproj`, and the full `dotnet test tests/Dojo.Tests/Dojo.Tests.csproj`; confirm the Release Windows apphost is named `dojo.exe`, record and resolve failures (depends on T029).
- [ ] T031 Run the final isolated CLI validations in `specs/001-dojo-core/quickstart.md`, including normal Windows invocation, preserved unavailable progress, ten-challenge restart accuracy (SC-002), and the under-10-second target for 100 challenges (SC-001), without reading/writing `StudentWork/` (depends on T030).
- [ ] T032 Perform manual SC-004 first-time readability validation: show status output to a first-time student without extra instructions, ask them to identify the current challenge and completed count, record the observation in `specs/001-dojo-core/quickstart.md`, and resolve unclear labels with relevant tests if needed (depends on T031).
- [ ] T033 Review `git status` and the staged diff for `.gitignore`, `src/Dojo/`, `curriculum/csharp/`, `tests/Dojo.Tests/`, `README.md`, and `specs/001-dojo-core/quickstart.md`; exclude progress, build output, secrets, and all `StudentWork/` files, then create a logically scoped Git checkpoint (depends on T032).

---

## Dependencies & Execution Order

### Phase Dependencies

1. **Setup (T001–T003)** establishes buildable production and test projects.
2. **Foundational (T004–T008)** depends on Setup and blocks all story phases.
3. **US1 (T009–T015)**, **US2 (T016–T020)**, and **US3 (T021–T025)** proceed in priority
   order because they extend the same `DojoApp.cs` and `Program.cs` files. Each story has
   isolated tests that can be run from seeded state.
4. **Polish (T026–T033)** depends on all three story checkpoints.

### User Story Dependencies

| Story | Starts after | Independent proof |
|---|---|---|
| US1 (P1) | T008 | Fresh and repeated `next` calls in a temporary workspace |
| US2 (P2) | T015 for shared-file editing; T008 for test authoring | Seeded progress, then `status` without `next` |
| US3 (P3) | T020 for shared-file editing; T008 for test authoring | Seeded active state, then five `hint` calls |

### Within Each User Story

- Write the two story test files first and confirm the expected red result.
- For US1, author the two challenge files before implementing `next`.
- Implement the state operation before wiring the CLI output, then run the story checkpoint.
- Do not advance to the next story while its checkpoint fails.

### Parallel Opportunities

- T009 and T010 can be written together; after both exist, T011 and T012 can be authored
  together because each touches a different challenge file.
- T016 and T017 can be written together; T021 and T022 can be written together.
- All shared `DojoApp.cs` and `Program.cs` edits and the final validation remain sequential.

## Parallel Example: User Story 1

```text
After T008: T009 NextCliTests.cs || T010 NextStateTests.cs
After both tests fail as expected: T011 001-first-steps.json || T012 002-next-steps.json
Then: T013 -> T014 -> T015
```

## Parallel Example: User Story 2

```text
After T008: T016 StatusCliTests.cs || T017 StatusStateTests.cs
After T015 and both tests fail as expected: T018 -> T019 -> T020
```

## Parallel Example: User Story 3

```text
After T008: T021 HintCliTests.cs || T022 HintStateTests.cs
After T020 and both tests fail as expected: T023 -> T024 -> T025
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational tasks through T008.
2. Write the US1 tests, author safe challenge prompts, and implement `next` through T014.
3. Complete T015 and stop if the independent next-command tests fail.

### Incremental Delivery

1. Add `status` with seeded-state tests and validate at T020.
2. Add `hint` with seeded-state tests and validate at T025.
3. Complete cross-command error checks, documentation, build/test/CLI validation, and the
   manual readability check and scoped checkpoint in T026–T033.

## Notes

- The production app uses .NET standard library features; MSTest is test-only.
- `dojo next` self-declares completion; do not add automated grading or `dojo complete`.
- Preserve the student's authorship and all files under `StudentWork/`.
