# Decisions

Every structural choice and every resolved ambiguity, newest last. The README's "architecture decisions" and "unclear requirements" sections are written from this file, so each entry must stand on its own.

Entry format:

```markdown
## Short title
**Decision:** what was chosen.
**Why:** the reason, tied to a requirement or an evaluation point where possible.
**Alternatives:** what was rejected and why (omit when there was none).
```

---

## Own application identifier
**Decision:** company name `byuksekdev`, product name `Color Block Jam`, Android package `com.byuksekdev.colorblockjam`.
**Why:** Android refuses to install an APK whose package name matches an installed app signed with another key. A reviewer who has the shipped game on the test phone could not install this build under the original identifier.

## Portrait only
**Decision:** default orientation is Portrait; auto-rotation is off.
**Why:** the brief sets a portrait reference resolution of 1080 x 1920 and asks for a correct layout on 20:9, which is a taller portrait screen, not a landscape one.

## The APK is not committed
**Decision:** builds stay out of the repository (`*.apk` is git-ignored) and are attached to a GitHub release.
**Why:** the brief excludes the `Build` folder, and binaries bloat history.

## Three script folders, no assembly definitions
**Decision:** our code stays in Unity's default assemblies. `Scripts/Core/` holds data and logic and uses no `UnityEngine` or `UnityEditor` type. `Scripts/Game/` holds views, UI, input, level serialization and storage. `Scripts/Editor/` holds the level editor and the EditMode tests.
**Why:** folders and namespaces carry the separation of data, logic and view, and the project setup stays plain. The engine-free rule for `Scripts/Core/` is checked by building that folder and running its tests with `dotnet` outside Unity.
**Alternatives:** one assembly definition per layer, so that Unity itself rejects a wrong reference. Rejected for now: it adds setup (DOTween's modules need their own definition, references are kept by hand) that a project of this size does not need.

## Levels are JSON files listed in a catalog
**Decision:** a level is one JSON text file under `Levels/`. The level editor writes it and the game reads it through the same serializer. A `LevelCatalog` asset lists the level files in play order, and the game loads levels from that list.
**Why:** "one format, and the game reads the same format" becomes one code path. Text files are easy to compare and the save and load round trip is testable. An ordered list needs no file-name convention and no `Resources` path strings.
**Alternatives:** one ScriptableObject per level (Unity YAML, and the round trip is harder to test); `StreamingAssets` (needs a web request to read on Android).

## A level file holds logical data only
**Decision:** a level file stores the board width and height, the timer in seconds, the blocks (colour and cells), the doors (colour, side, start, length) and a format version. Walls, corners, ground cells and world positions are not stored; the game builds them from the board size and the doors.
**Why:** the supplied block and door models are modular per cell, so a block is a set of cells and a door is a run of cells. A file that stores no derived data cannot contradict itself, and a new block shape needs no new code and no new prefab.
**Alternatives:** a shape id plus a rotation per block, which ties every level file to a shape table.

## The level editor is a Unity Editor window
**Decision:** the editor is an `EditorWindow` built with UI Toolkit. It lists the levels, edits board size, timer, blocks and doors on a visual grid, checks the level structure while the designer edits, saves, and starts Play Mode on the edited level.
**Why:** the designer works where the level files live: save writes into the project and play is one button, with no code and no file editing.
**Alternatives:** an editor scene inside the game. Files written on a device do not reach the project, and it needs a second set of runtime UI.

## Rules run on grid cells; the view moves smoothly
**Decision:** the logic owns every block position as grid cells. During a drag the view moves the block continuously and asks the logic how far it may go; collision is arithmetic on cell occupancy. On release the block snaps to the nearest cell. There is no physics and there are no colliders.
**Why:** the rules are deterministic and testable without a scene, and no game state lives in transforms.
**Alternatives:** transform positions and physics overlap as the source of truth, which brings float tolerances into the rules.

## When a block leaves the board
**Decision:** a block leaves through a door of its colour when it touches that door's side of the board, all of its cells lie inside the door's span, and no other block stands between any of its cells and the door. The logic removes the block first; the exit animation follows.
**Why:** this matches the reference game, where a door may be wider than the block. Changing the state before animating keeps the rule independent of the animation.

## Each attempt is a new session
**Decision:** starting or restarting a level builds a new session object from the level data, and the board view is rebuilt from that session.
**Why:** the brief requires that no data stays from the previous attempt. Nothing is reset field by field, so nothing can be forgotten.

## One scene, one composition root
**Decision:** `GameScene` is the only scene; home, gameplay and the popups are screens switched on and off. One bootstrap object constructs the logic objects and passes them to the views.
**Why:** no scene loading between screens, and every dependency is visible in one place. There is no singleton and no service locator.

## Pause is a session state
**Decision:** pause, win and fail are states of the session. The timer advances only in the playing state. `Time.timeScale` is not changed.
**Why:** popups can animate while the game is paused, and the timer cannot run behind a popup.

## The timer starts on the first touch
**Decision:** the countdown begins when the player first touches the board, not when the level appears.
**Why:** the brief does not say when the timer starts. The player loses no time while reading the board.

## Win beats timeout
**Decision:** when the last block leaves on the tick in which the timer reaches zero, the level is won.
**Why:** the brief does not cover this case. The player did complete the level.

## Coin rules
**Decision:** a new player starts with 400 coins. Each completed level grants 40 coins, once, at the moment the level is won. Nothing spends coins.
**Why:** the brief requires a working coin inventory but names no source and no price. The values match the reference game. Granting at the state change keeps the count correct when the player leaves before pressing a button.

## Level 1 and the tutorial hint
**Decision:** the five levels reproduce levels 1 to 5 of the reference game. The "Swipe to Gate" hand hint of level 1 is not implemented.
**Why:** a tutorial is not in the required scope.

## After the last level
**Decision:** the level number keeps counting up and the level content starts again from the first file.
**Why:** "continue to the next level" must always have a next level, and five levels are shipped.

## Android build: IL2CPP with ARM64 and ARMv7
**Decision:** the Android build uses the IL2CPP scripting backend and targets ARM64 and ARMv7.
**Why:** a phone that runs only 64-bit apps cannot install an ARMv7-only build.

---

## Open

- **Unused template packages**: whether to remove `com.unity.collab-proxy`, `com.unity.visualscripting` and `com.unity.ide.vscode` from `Packages/manifest.json`.
- **Third-party plugins**: DOTween and Lean Pool are in `Assets/Plugins/`. Record where they came from and what each is used for, or remove the one that ends up unused.
