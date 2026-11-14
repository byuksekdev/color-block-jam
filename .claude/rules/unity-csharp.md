---
paths:
  - "Assets/**/*.cs"
---

# Unity C# conventions

Formatting and most naming are enforced by `.editorconfig`. This file holds what an IDE cannot check.

## Naming

- Types, methods, properties, events, constants and public members: `PascalCase`. Interfaces: `IName`.
- Private fields: `_camelCase`, serialized ones included (`[SerializeField] private Button _playButton;`). Locals and parameters: `camelCase`.
- No public fields. Expose a property, or serialize a private field. The one exception is the serialized data types in `Scripts/Core/` (the level file, saved player state): they use public fields, because `[SerializeField]` would bring `UnityEngine` into engine-free code.
- A name says what the thing is in the game (`DoorView`, `LevelTimer`, `BoardState`), not what pattern it uses. No `Manager` or `Helper` when a more exact word exists. A bool reads as a question: `IsPaused`, `HasWon`.
- Everything sits in a namespace under `ColorBlockJam`. One type per file, file name equals type name.

## Unity objects

- Never `?.`, `??`, `??=` or `is null` on a `UnityEngine.Object`: they skip Unity's destroyed-object check. Use `== null` or the implicit bool.
- `TryGetComponent` instead of `GetComponent` plus a null check.
- References come from serialized fields or are passed in. No `Find`, `FindObjectOfType`, `GameObject.Find`, `SendMessage` or tag lookups at runtime.
- Cache what a per-frame method needs. No `Camera.main`, `GetComponent`, LINQ, string building, `new` of a reference type, lambda capture or boxing inside `Update`, `LateUpdate`, `FixedUpdate` or a per-frame coroutine loop.
- Prefer an event to a per-frame check. A script with nothing to do each frame has no `Update`.

## Events

- Subscribe in `OnEnable`, unsubscribe in `OnDisable`. Read the current state right after subscribing: an event raised earlier is not replayed.
- No static events and no static mutable state: they outlive a restart and break the "no data stays from the previous attempt" criterion.

## Control flow

- Guard clauses first. An `if` body always sits on its own line, in braces.
- No magic numbers: a tunable value is a serialized field or lives in a data asset, a fixed one is a named constant.
- Number and date text that is saved or parsed uses `CultureInfo.InvariantCulture`; case changes use `ToLowerInvariant` / `ToUpperInvariant`. A device with a Turkish locale writes `1,5` by default and folds `i` differently.

## Comments

- Default: none. Names, small methods and structure carry the meaning.
- Allowed: a short `///` on a public API that is not obvious; one line of *why* for a real invariant or an external constraint.
- Forbidden: comments that restate the code, section banners, commented-out code, `TODO` / `FIXME`.

## Size

- Soft cap of about 300 lines per file. A class that grows past it usually has two jobs: say so and propose the split before adding a third.
- `#region` is not used. `#if UNITY_EDITOR` blocks sit flush-left; editor-only types live in an `Editor/` folder instead where possible.
