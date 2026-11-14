# AGENTS.md

<!-- Read in every chat and every subagent. Keep it under ~100 lines: only facts an agent cannot
infer from the code, and hard constraints. How to edit this setup: .claude/rules/agent-config.md. -->

## Priority

**The user's instruction in the chat outranks everything in this file.** Scope, order of work and how much to research are the user's call. Challenge a weak idea in one line, then do what was asked.

## Project

A vertical slice of the puzzle game Color Block Jam, built as a take-home case with a 7-day limit. A reviewer reads the code, uses the level editor and plays an Android build. Art is not evaluated.

- Unity `2022.3.62f2`, URP, uGUI + TextMesh Pro. Target: Android, portrait only, reference resolution 1080 x 1920; the layout must also hold on a 20:9 device.
- `Assets/[ColorBlockJam]/` is the only folder for our work: `Scripts/`, `Data/`, `Levels/`, `Scenes/` (`GameScene.unity` is the one build scene), `VisualAssets/` (supplied art: do not add, buy or import other art).
- `Scripts/Core/` is data and logic and has no `using UnityEngine` or `using UnityEditor`. `Scripts/Game/` is views, UI, input, level serialization and storage. `Scripts/Editor/` is the level editor and the EditMode tests. Our code has no assembly definitions.
- `Assets/Plugins/` (DOTween, Lean Pool, simulator devices) and `Assets/TextMesh Pro/` are third-party: never edit them.
- `knowledge/` holds what the code cannot say: the case requirements, the reference game, the decision log, known pitfalls. Index: `knowledge/README.md`.

## What the reviewer weighs, in this order

1. **Architecture**: data, logic and view are separate; low coupling; easy to extend.
2. **Editor usability**: a designer builds, saves and plays a level without help, code or hand-edited files.
3. **Code quality**: clear names, no dead code, one style.
4. **Correctness**: every acceptance criterion in `knowledge/case-brief.md`.
5. **Performance**: no avoidable allocation per frame, stable frame rate on a mid-range phone.
6. **Scope control**: small and complete beats large and unfinished.

When two options tie, pick the one that scores higher up this list. Optional scope (level validation, working boosters, ice blocker, arrow block) starts only when the user says the required scope is done.

## Architecture

- **Data** is plain serializable types with no behaviour. **Logic** is plain C# that takes and returns data and never touches `UnityEngine.Object`, scenes or time APIs directly. **View** is MonoBehaviours that render state and forward input; a view holds no game rule.
- Logic never references a view. Views reach logic through events or an interface, not through singletons or `FindObjectOfType`.
- **One level format.** The level editor writes it and the game reads it, through the same serialization code. The five shipped levels are files saved by the editor, never typed by hand and never built in code.
- Restart rebuilds the level from its data. Nothing survives from the previous attempt: no static state, no reused runtime object that still carries values.
- Persisted player state (coins, current level, settings toggles) sits behind one small storage interface.
- A structural choice that the code does not make obvious goes into `knowledge/decisions.md` in the same change. The case README is written from that file.

## Rules

- Before a non-trivial edit, state your reading of the request and your assumptions in two lines. When a requirement is unclear, decide, continue, and record the decision in `knowledge/decisions.md`; ask first only when a wrong guess is expensive to undo.
- Every changed line traces to the request. No `TODO`, `FIXME`, stubs, commented-out code or drive-by refactors.
- No new package, plugin or asset without the user's yes: every dependency has to be justified in the README.
- Pure rules (timer, win and fail conditions, block movement, level validation, save and load) get EditMode tests: `.claude/rules/tests.md`.
- Report a code change in at most 8 lines: changed / verified (with evidence) / not verified / Editor steps left for the user.

## Unity

- Never hand-edit `.unity`, `.prefab`, `.asset` or `.meta` files, with an edit tool or from the shell. They change only through the Editor.
- When the session has Unity Editor tools (an MCP server connected to the open Editor), use them: build scene, prefab and asset content, read the Console, run the EditMode tests, enter Play Mode. Do not save a scene the request did not ask to change, and remove any object made only for a check.
- Without Editor tools, print the exact Editor steps (menu path, object, component, field, value) and stop. The user compiles, tests and pastes the result.
- `Scripts/Core/` is engine-free, so an agent can also build it and run its tests with `dotnet` in a scratch folder outside the repository (`.claude/rules/tests.md`).
- New `.cs` files get their `.meta` from Unity on the next import; do not write one.

## Evidence

Claim only what was run. Code that was only read or written is "written, not compiled". "Compiles" needs a clean Console. "Tests pass" needs the Test Runner summary. "Works" needs Play Mode or a device. Each comes from the Editor tools in this session or from what the user pasted. For `Scripts/Core/`, "builds and its tests pass with `dotnet`" is a valid claim when that was run; it is not a Unity compile. Saying less is a complete answer.

## Git

- Stay on the checked-out branch. **Never commit, push, stash, switch branch or rewrite history unless the user asks for it in that message.**
- When work is ready, list the changed files and propose how to split them into commits, with a message for each (`/commit-message`). The user reviews and commits.
- Message format, as the history uses it: `Type(Scope): Summary`, for example `Chore(Setup): Unity project initialized`. Types: `Feat`, `Fix`, `Refactor`, `Perf`, `Test`, `Docs`, `Chore`.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/` and builds never enter the repository. The APK goes to a GitHub release.
