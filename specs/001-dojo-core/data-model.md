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
| `completedChallengeIds` | Self-declared completions | Array of distinct IDs in curriculum order |
| `hintCounts` | Revealed hints per challenge | Object keyed by challenge ID; integer values from 0 to 4 |

Missing progress means `null`, an empty completion list, and an empty hint-count map in memory.
`dojo status` does not need to create a file. A command that changes state saves the complete
record. Unknown challenge IDs, duplicate completions, negative or excessive hint counts, an
active ID also marked completed, or malformed JSON are errors; the file remains untouched.
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
| `status`, any valid state | Display active ID/title or none, completion count, hint total; no mutation |
| `next`, no active ID and an available challenge | Activate first challenge not completed; save |
| `next`, active ID and a later challenge | Add active ID to completed IDs, activate next; save |
| `next`, active ID and no later challenge | Add active ID to completed IDs, clear active ID; save |
| `next`, no active ID and none available | Show empty/complete result; no mutation |
| `hint`, active ID with fewer than four revealed hints | Show hint at current count, increment that count, save |
| `hint`, no active ID or all four hints revealed | Explain state; no mutation |

The next challenge is the lowest ordered uncompleted challenge after the active one. If new
curriculum entries are added after all existing entries are complete, a later `dojo next` can
activate the first new entry. The CLI does not inspect or edit `StudentWork/`.
