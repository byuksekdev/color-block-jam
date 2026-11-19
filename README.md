# Color Block Jam: Vertical Slice

A vertical slice of Color Block Jam in Unity: home screen, settings, the first five levels of the reference game plus a bonus level, and a level editor for designers.

Everything in the required scope is implemented, as are three of the four optional items: level validation with a solver, the ice blocker and the arrow block. Boosters are placeholders, which the brief allows.

- **Unity:** 2022.3.62f2, Universal Render Pipeline, portrait, reference resolution 1080 x 1920
- **Build:** the Android APK is on the [latest release](../../releases/latest); the [recording below](#gameplay) shows the full flow
- **Checked on Android:** the APK was installed and played on an Android 16 emulator at 1080 x 2400 (20:9)
- **Tests:** 233 EditMode tests on the board rules, the timer, the game flow and coins, the level format, validation and the editing rules, all passing

## Gameplay

The home screen and settings, levels 1 to 6 with the ice and arrow blocks, then a time-out on level 7 as the levels start again from the first. The wait before the time-out is sped up 16x.

https://github.com/user-attachments/assets/5e95f8b0-eca5-4243-ae56-3156d968c579

## Contents

1. [Open and run the project](#open-and-run-the-project)
2. [Make a level with the editor](#make-a-level-with-the-editor)
3. [Architecture](#architecture)
4. [Decisions on unclear requirements](#decisions-on-unclear-requirements)
5. [Performance](#performance)
6. [Known problems and incomplete parts](#known-problems-and-incomplete-parts)
7. [Third-party code and packages](#third-party-code-and-packages)
8. [Feedback on the supplied assets](#feedback-on-the-supplied-assets)
9. [LLM tools](#llm-tools)
10. [Work time](#work-time)

## Open and run the project

1. Add the project folder in Unity Hub and open it with Unity **2022.3.62f2**. The project's own files are in `Assets/[ColorBlockJam]/`.
2. Open `Assets/[ColorBlockJam]/Scenes/HomeScene.unity` and press **Play**. Use the Game view at 1080 x 1920 or the Device Simulator.
3. `GameScene` can also be played on its own; it starts the level the player has reached.
4. **Tests:** Window > General > Test Runner > EditMode > Run All.
5. **Android build:** File > Build Settings, select Android and press **Switch Platform** if it is not active, then **Build**. Both scenes are already in the build list.

The player's progress (coins, reached level, settings) is saved in PlayerPrefs. Edit > Clear All PlayerPrefs starts again as a new player.

## Make a level with the editor

The level editor is a Unity Editor window: a level is drawn on a grid, saved and played from the window, with no code and no hand-edited file.

https://github.com/user-attachments/assets/cb9ac763-a1ae-480b-8ec5-b6bded130bc1

Open **Tools > Color Block Jam > Level Editor**.

1. Press **New Level**.
2. Set the board **Width** and **Height** with the + and - buttons, and the **Timer** in minutes and seconds.
3. **Block** tool: pick a colour and a shape, then click a cell to place it. **R** turns the shape and **F** mirrors it.
4. **Door** tool: drag along the strip beside the board to place a door of the chosen colour.
5. Optional: the **Ice** tool freezes a block or a door until a number of blocks have left, and the **Arrow** tool lets a block move along one line only.
6. Read the **Level check**. It lists what is wrong, such as a block colour without a door. Point at a line to see that block or door outlined on the board. When nothing is wrong, **Check if it can be solved** runs the solver.
7. Press **Save**, then **Play**. The game starts directly on this level.

Also in the editor:

- **Move** drags a block to another place. **Erase**, or a right click with any tool, removes a block or a door.
- **Ctrl+Z** undoes every change on the board.
- Drag a level in the list to change the play order. **Duplicate** copies a level, and **Delete** asks before it removes one.
- Saving warns before an unsolvable level is saved, and making the board smaller asks first when blocks or doors would be lost.
- A level that still has a problem is saved as a draft. It stays out of the game until it is fixed, so unfinished work cannot break the build.
- **Play** starts a preview: winning it does not change the player's coins or progress.

Levels are saved as JSON files in `Assets/[ColorBlockJam]/Levels/`, and `Data/LevelCatalog.asset` lists them in play order. All six shipped levels were made with this editor.

## Architecture

The code is split into three layers, by folder and namespace:

| Folder | Holds | Depends on |
|---|---|---|
| `Scripts/Core/` | Data and logic: level data, board rules, drag, exit, timer, session, game flow, player profile, editing rules, validation, solver | Nothing. No `UnityEngine` or `UnityEditor` |
| `Scripts/Game/` | Views and wiring: board views, UI screens, input, level loading, PlayerPrefs storage | Core |
| `Scripts/Editor/` | The level editor and the EditMode tests | Core, Game |

```text
Level JSON ──► LevelSession (Core) ──events──► BoardView, HUD (Game)
                    ▲                                │
                    └──── begin, move, end drag ◄────┘ BoardInput
```

The main decisions, with the reason for each:

- **Logic is plain C#; views only show and forward input.** The session owns the board and hands the views a read-only interface. Views ask it to move a block and learn the result from its events. Every rule runs in one place and is tested without a scene. The rules work on grid cells, not on transforms or physics, so they are exact and repeatable.
- **Each scene has one composition root.** `HomeRoot` and `GameRoot` create the logic objects and pass them to their views. There are no singletons, no static state and no `FindObjectOfType` at run time. The two scenes share data only through storage.
- **Restart builds a new session from the level data.** Nothing is reset field by field, so nothing from the previous attempt can remain. The board's parts come back from Lean Pool pools, so a restart creates no game objects.
- **One level format.** A level is a JSON file with logical data only: board size, timer, blocks as sets of cells, doors as runs of cells. The editor and the game use the same serializer. Walls, corners and positions are derived, so a file cannot contradict itself, and any block shape needs no new code or prefab.
- **The editor follows the same split.** Editing rules (place, move, resize) live in `Core/Editing/` and are tested. The window only shows the level and forwards input. Each board tool is one class behind a small interface. Every change goes through one method, which records the undo step.
- **Ice and arrows are fields, not new block types.** Ice is a count on a block or door; what remains is computed from the blocks that have left. An arrow is an axis on a block. Both reuse the existing drag and exit rules.
- **The solver uses the game's own rules.** It shares the board and the exit rule with the game, so it cannot disagree with it. A block that leaves never makes the rest harder, so the solver lets out whichever block it can and starts again from there. It never has to try the order of the exits, which keeps the check quick enough to run on every save.
- **Player data sits behind one storage interface.** `PlayerProfile` is the only code that changes coins, the reached level and the settings. It saves through `IPlayerStorage` after every change, so tests use a fake storage.
- **Views and screens are prefabs, and tuning lives in assets.** Board parts, screens and popups are prefabs with their look and timing on them. Drag feel is set in `DragTuning`, colours in `BlockPalette` and UI motion in `UiMotion`. A visual or feel change needs no code change.
- **Folders, not assembly definitions.** The layers are kept apart by folder and namespace. `Scripts/Core/` builds and passes its tests with `dotnet` outside Unity, which shows it has no engine dependency. Assembly definitions would enforce the same split at compile time, at the cost of more setup than a project of this size needs.

Every decision, with its reasons and the alternatives that were rejected, is in [`knowledge/decisions.md`](knowledge/decisions.md).

## Decisions on unclear requirements

| Question | Decision |
|---|---|
| When does the timer start? | When the player first touches a block, so no time is lost while reading the board. |
| The last block leaves as the timer reaches zero | The level is won. |
| Where do coins come from? | A new player has 400 coins. The first win of a level grants 40; replaying a level grants nothing. These values match the reference game. |
| What is the pause screen? | The settings screen with an extra Home button, as in the reference game. Restart is its own button on the HUD. |
| How long are the timers? | One minute on every level instead of the reference game's five, so the timeout and the fail popup can be reviewed quickly. Each level's timer is set in the editor. |
| Which levels? | Levels 1 to 5 follow the boards of the reference game's first five levels. Level 6 is a bonus level that shows the arrow block, the ice block and the ice door. |
| After the last level | The level number keeps counting up, and the levels start again from the first one. |
| The level button | Starts the level directly. The reference game's pre-level booster popup is left out. |

## Performance

- Board parts come from pools. A restart creates no game objects.
- No per-frame allocation in the gameplay loop. The timer label is rewritten only when the shown second changes, and the exit animation reuses its vertex buffers.
- Android builds with IL2CPP for ARM64 and ARMv7, targets 60 fps and uses the lightest quality level: no HDR, no shadows, no ambient occlusion pass.
- UI sprites share one sprite atlas, so the images of a screen are drawn together.
- Measured in the Editor: starting a level takes about 1 ms, and cutting a block at the door costs about 0.04 ms per frame.

## Known problems and incomplete parts

- **Boosters** are shown and react to a touch, but have no function. The brief allows this.
- **Lives, the profile box, the level path and the home tabs** are placeholders that react to a touch.
- **Sound, music and vibration toggles** save and show their state, but nothing plays or vibrates. No audio is supplied.
- **Solver limit:** the solver stops after a set number of positions. For very open levels it may not give an answer; the editor then saves the level and shows a note to try it by playing.
- **Tutorial:** level 1's "Swipe to Gate" hand hint from the reference game is not implemented.

## Third-party code and packages

| Name | Where | Why |
|---|---|---|
| DOTween 1.3.045 (free, Demigiant) | `Assets/Plugins/Demigiant/` | Animation of the board and the UI |
| Lean Pool (free, CW) | `Assets/Plugins/CW/` | Object pools for the board's parts |
| 2D Sprite package | Unity Package Manager | The Sprite Editor, to set nine-slice borders on the UI sprites |
| Device Simulator Devices package | Unity Package Manager | Phone profiles, to check the layout on 20:9 screens and safe areas |
| TextMesh Pro, URP, Test Framework | Unity Package Manager | Text, rendering and the EditMode tests |

## Feedback on the supplied assets

- The block parts' UVs cover only a small patch of a texture and repeat in every quarter of a cell. A texture laid on by UV, such as the ice texture, shows as a small repeating spot. The ice is therefore drawn with a small custom shader that maps the texture in world space.
- The models do not share an up axis. The block parts are authored with -Z up, and the door and frame corner lie upside down, so each needs its own rotation.
- The ground tile is 1.953 units wide against a 2-unit cell, which leaves thin gaps between tiles.
- `btn_toggle_on.png` has no matching "off" sprite.
- No block materials or colour textures, no audio, no font, no level-path sprites, no avatar and no fail-popup art are supplied.

## LLM tools

I used **Claude Code** (Anthropic) as a coding assistant throughout the project. I designed the architecture, the level format and how the level editor works, and worked in small steps: Claude Code wrote most of the implementation, and I reviewed each change before committing it.

- **Parts it was used for:** the C# code of the game and the level editor, the EditMode tests, the two shaders and drafts of the documentation.
- **Unity Editor access:** through the MCP for Unity bridge it could compile, run the tests, enter Play Mode and build prefabs and scenes.
- **Working rules:** the conventions it followed and the decision log are in the repository: `AGENTS.md`, `.claude/` and `knowledge/`.

## Work time

About 20 hours, spread over six days.
