# Feature Specification: UnityDojoCoach v0.1 — Dojo Core

**Feature Branch**: `main`
**Created**: 2026-09-30
**Status**: Draft
**Input**: User description: "Build a minimal C#/.NET CLI challenge manager with `dojo status`, `dojo next`, and `dojo hint`; challenges under `curriculum/csharp/`; local JSON progress; status shows current challenge, completed count, and hints used. `dojo next` self-declares completion of the active challenge; `dojo hint` reveals one ordered hint at a time and never shows a full solution."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Find the next challenge (Priority: P1)

As a student, I want to see the next available C# challenge so I can begin a practical exercise.

**Why this priority**: Challenge discovery is the smallest useful learning flow.

**Independent Test**: Given an ordered curriculum and fresh progress, run `dojo next` and verify that the first available challenge is shown with its title and task description.

**Acceptance Scenarios**:

1. **Given** no active challenge and at least one available challenge, **When** I run `dojo next`, **Then** I see the first challenge, it becomes active, and no challenge is marked completed.
2. **Given** an active challenge and another available challenge, **When** I run `dojo next`, **Then** the active challenge is marked completed and the next challenge becomes active and is shown.
3. **Given** the last challenge is active, **When** I run `dojo next`, **Then** it is marked completed, no challenge remains active, and I see that the curriculum is complete.
4. **Given** no active or available challenge, **When** I run `dojo next`, **Then** I see that no challenge is available and completion data does not change.

---

### User Story 2 - Review learning progress (Priority: P2)

As a student, I want to see my current challenge, completed challenge count, and hints used so I can understand my progress.

**Why this priority**: A brief summary makes the challenge flow understandable across sessions.

**Independent Test**: Given progress with a selected challenge, completed challenges, and recorded hints, run `dojo status` and compare all three displayed values with the saved progress.

**Acceptance Scenarios**:

1. **Given** an existing progress record, **When** I run `dojo status`, **Then** it shows the current challenge, completed challenge count, and total hints used.
2. **Given** no progress record, **When** I run `dojo status`, **Then** it shows no active challenge, zero completed challenges, and zero hints used.
3. **Given** an empty curriculum, **When** I run `dojo status`, **Then** it shows no current challenge and zero counts.
4. **Given** I have advanced from one challenge to another with `dojo next`, **When** I run `dojo status` in a new session, **Then** it shows the new active challenge and a completed count of one.

---

### User Story 3 - Request guided help (Priority: P3)

As a student, I want help with the current challenge in small steps so I can keep working out the answer myself.

**Why this priority**: Guided help supports learning without taking over the exercise.

**Independent Test**: Select a challenge, run `dojo hint`, and verify that the displayed guidance is safe for the student's current stage and that the usage count survives a new session.

**Acceptance Scenarios**:

1. **Given** an active challenge with unrevealed hints, **When** I run `dojo hint`, **Then** I see only its next hint and its usage is recorded once.
2. **Given** I request another hint for the same challenge, **When** I run `dojo hint`, **Then** I see only the next unrevealed hint in order and the usage count increases once.
3. **Given** no active challenge, **When** I run `dojo hint`, **Then** I see a clear message and the usage count does not change.
4. **Given** all hints for the active challenge are revealed, **When** I run `dojo hint`, **Then** I see that no more hints are available and the usage count does not increase.

### Edge Cases

- Missing progress is treated as a fresh start and saved when a command first changes progress.
- Malformed progress or challenge content produces a clear error; existing student data is not silently overwritten.
- Missing or empty curriculum produces an informative result rather than a crash.
- Calling `dojo next` with an active challenge self-declares it complete; no code validation is performed.
- Calling `dojo next` after the curriculum is complete does not duplicate a completed ID.
- Guidance exhausted for the current challenge reports that no more hints are available and does not reveal a complete solution.
- No command in this feature changes files under `StudentWork/`.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The application MUST accept `dojo status`, `dojo next`, and `dojo hint` as student-facing commands.
- **FR-002**: The application MUST find C# challenges stored under `curriculum/csharp/` and present them in a stable, explicit curriculum order.
- **FR-003**: Each challenge MUST provide a stable identifier, title, task description, and four ordered guidance prompts: a guiding question, small hint, conceptual explanation, and pseudocode. It MUST NOT include a complete solution in those prompts.
- **FR-004**: The application MUST maintain local student progress in JSON, including the active challenge, completed challenge identifiers, and per-challenge hint usage.
- **FR-005**: `dojo status` MUST display the active challenge or explicitly show that none is active, the count of distinct completed challenges, and the total hints revealed.
- **FR-006**: If no challenge is active, `dojo next` MUST activate and show the first available challenge without marking a challenge completed.
- **FR-007**: If a challenge is active, `dojo next` MUST mark it completed, persist its ID, and activate and show the next challenge; if no challenge remains, it MUST show that the curriculum is complete and leave none active.
- **FR-008**: `dojo hint` MUST reveal only the next unrevealed hint for the active challenge, in authored order, and increment persisted hint usage once per newly revealed hint.
- **FR-009**: When no hints remain or no challenge is active, `dojo hint` MUST report that state without changing hint usage.
- **FR-010**: `dojo hint` MUST reveal guidance in the order guiding question, small hint, conceptual explanation, then pseudocode. It MUST never reveal a complete solution. This version MUST NOT offer a solution-unlock command.
- **FR-011**: The application MUST persist changed progress so a later invocation reports the same active challenge, completed IDs, and hint usage.
- **FR-012**: The application MUST present clear results for missing challenges, exhausted hints, malformed challenge data, and unreadable progress without silently resetting valid progress.
- **FR-013**: The application MUST NOT create, edit, or repair challenge solutions in `StudentWork/`.
- **FR-014**: Challenge completion MUST be self-declared through `dojo next`; this version MUST NOT add automated code validation or a separate completion command.

### Key Entities *(include if feature involves data)*

- **Challenge**: An ordered C# exercise with a stable identifier, title, task description, and ordered hints.
- **Student progress**: Local state identifying the active challenge, distinct completed challenge identifiers, and revealed hint counts for each challenge.
- **Hint**: A challenge-specific teaching prompt with a defined order that does not disclose a complete solution.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A student can find the first available challenge with one command in under 10 seconds in a curriculum of 100 challenges.
- **SC-002**: Across a 10-challenge curriculum, each `dojo next` invocation activates exactly one challenge or completes the active challenge and advances exactly once; completed counts remain accurate after restarting the application.
- **SC-003**: In 100% of tested hint sequences, each request reveals no more than one new hint, exhausted hints do not increment usage, and no complete solution appears.
- **SC-004**: In a usability check, a first-time student can identify the current challenge and completed count from `dojo status` without additional instructions.
- **SC-005**: In 100% of tested normal and error paths, files under `StudentWork/` remain unchanged.

## Assumptions

- The v0.1 experience is for one student using a local command-line environment; accounts, sync, and network access are outside scope.
- The student's `dojo next` action is an intentional declaration that the current exercise is complete; no solution check is required.
- Challenges have a stable authored order; the initial curriculum may contain only a small set of exercises.
- Hint usage counts newly revealed hints, rather than failed or exhausted requests.
- The only v0.1 commands are `status`, `next`, and `hint`; automated validation and a separate completion command are outside scope.
