---
name: commit-message
description: >
  Proposes how to split the current changes into commits and writes a message for each in this
  repository's `Type(Scope): Summary` format. Use for /commit-message, a request for a commit
  message or for what to commit, or when work is ready to be committed. It never commits or pushes by itself.
---

# Commit messages

The user reviews and commits. This skill prepares the plan and stops.

## Steps

1. Read `git status` and the diff. Name every changed file, and flag any the current work did not mean to touch (Unity often rewrites `ProjectSettings/`, `Packages/packages-lock.json` and scene files on its own).
2. Check that every new file under `Assets/` has its `.meta` and every deleted one lost it. A missing or orphaned `.meta` breaks references for the next person who opens the project.
3. Group the changes into commits: one purpose per commit, and the project still compiles after each. Setup, a feature, its tests and a documentation change are separate commits.
4. For each commit give the file list and one message, then the exact commands in separate code blocks (`git add <paths>`, `git commit -m "..."`). Write the messages in English; explain in the language of the conversation.

## Format

```text
Type(Scope): Summary
```

- **Type**: `Feat` (new behaviour), `Fix` (a defect), `Refactor` (same behaviour, new structure), `Perf`, `Test`, `Docs`, `Chore` (setup, settings, tooling).
- **Scope**: the area, one word in PascalCase: `Setup`, `Home`, `Settings`, `Gameplay`, `Board`, `Timer`, `Coins`, `LevelEditor`, `Levels`, `UI`, `Agents`.
- **Summary**: what the change does, in the past tense as the history has it, no trailing period: `Chore(Setup): Unity project initialized`.
- Describe what the diff does, not the hoped-for effect. "Fixed" is not proof that the bug is gone: say what was verified in the reply, not in the subject.
- Add a body only for what the diff cannot say: why this approach, a limit that remains.

| Weak | Useful |
|---|---|
| `Feat(Game): Updates` | `Feat(Timer): Countdown added that fails the level at zero` |
| `Fix(UI): Fixed bug` | `Fix(Home): Tab bar kept inside the safe area on 20:9 screens` |

## Limits

- Commit only when the user says so in that message, and then only the files of the agreed scope. A request to commit is not a request to push.
- Never `git add -A` or `git add .`: stage the listed paths.
