# Data Model: Dojo Core

## Challenge

An authored JSON document stored under `curriculum/csharp/`; one file represents one challenge.

| Field | Meaning | Rules |
|---|---|---|
| `id` | Stable progress key | Required, nonempty, unique across the curriculum |
| `order` | Position in the curriculum | Required positive integer, unique across the curriculum |
| `title` | Short challenge name | Required, nonempty text |
| `description` | Student task | Required, nonempty text; no complete solution |
| `hints` | Teaching prompts | Exactly four nonempty strings in constitution order |

The four `hints` entries are a guiding question, small hint, conceptual explanation, and
pseudocode. The catalog sorts by `order`. Its loader rejects duplicate IDs or orders, missing
fields, malformed JSON, and hints that are blank or not exactly four entries. A human review
must check that prose and pseudocode do not amount to a complete answer; structural validation
cannot prove that.

Example shape, with no exercise solution:

```json
{
  "id": "csharp-001",
  "order": 1,
  "title": "First exercise",
  "description": "Describe the student's task here.",
  "hints": [
    "What information does the task give you?",
    "Start with the smallest piece of the task.",
    "Explain the relevant concept without solving the exercise.",
    "Outline the steps in pseudocode without supplying complete code."
  ]
}
```

## Student progress

The local, untracked `.dojo/progress.json` stores one student's state for this checkout.

| Field | Meaning | Rules |
|---|---|---|
| `activeChallengeId` | Challenge currently being attempted | String or `null`; cannot also be completed |
| `completedChallengeIds` | Self-declared completions | Array of distinct nonempty IDs in recorded completion order; preserve existing order |
| `hintCounts` | Revealed hints per challenge | Object keyed by challenge ID; integer values from 0 to 4 |

Missing progress means `null`, an empty completion list, and an empty hint-count map in memory.
`dojo status` does not need to create a file. A command that changes state saves the complete
record. Duplicate completions, blank IDs, negative or excessive hint counts, an
active ID also marked completed, or malformed JSON are errors; the file remains untouched.
An ID absent from the current catalog is unavailable, not invalid progress. Preserve every
saved ID, count, and completion order even when the entire curriculum is missing or empty.
Status reports unavailable IDs, including completed and hint-count keys, and retains saved totals.
The total shown by `dojo status` is the sum of all `hintCounts`, including completed challenges.

Example after completing the first challenge and revealing one hint on the second:

```json
{
  "activeChallengeId": "csharp-002",
  "completedChallengeIds": ["csharp-001"],
  "hintCounts": {
    "csharp-001": 0,
    "csharp-002": 1
  }
}
```

## State transitions

| Command and starting state | Result |
|---|---|
| `status`, any valid state | Display active ID/title, saved active ID marked unavailable, or none; saved completion count and hint total; no mutation |
| `next` or `hint`, unavailable active ID | Error; no mutation, completion, or hint disclosure |
| `next`, no active ID and an available challenge | Activate first challenge not completed; save |
| `next`, active ID and a later challenge | Add active ID to completed IDs, activate next; save |
| `next`, active ID and no later challenge | Add active ID to completed IDs, clear active ID; save |
| `next`, no active ID and none available | Show empty/complete result; no mutation |
| `hint`, available active ID with fewer than four revealed hints | Increment count and save successfully before showing the hint at the previous count |
| `hint`, no active ID or all four hints revealed | Explain state; no mutation |

The next challenge is the lowest ordered uncompleted challenge after the active one. If new
curriculum entries are added after all existing entries are complete, a later `dojo next` can
activate the first new entry. The CLI does not inspect or edit `StudentWork/`.

Unavailable historical IDs alone do not block a command whose required definitions are present.
An empty catalog with no active ID reports no challenges available without saving, regardless
of historical completions. Fresh empty status has zero counts; existing empty status retains
saved counts. Missing definitions are never interpreted as proof that an active challenge is complete.

## Save failure safety

Serialize changed state to a temporary file in the progress directory and replace the destination
only after the write finishes successfully. Never truncate the previous file first. On a write
or replacement failure, return an error, preserve the previous valid file byte-for-byte, and
clean up the temporary file where possible. Do not report successful changes or reveal a new
hint before the save succeeds. Test both temporary-write failure and replacement failure using
isolated fixtures; do not use real student progress or require elevated permissions.
