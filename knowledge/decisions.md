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

## Only well-formed levels are in the catalog
**Decision:** the editor writes a level file into the catalog only while the level check finds no problem in it. A file that is not well formed yet, such as a new and still empty level, is a draft: the editor lists it after the playable levels and marks it, and it enters the catalog the first time it is saved well formed. A playable level that is saved with a problem leaves the catalog again. An EditMode test loads every level of the catalog and fails on one that is not well formed or cannot be solved.
**Why:** the game refuses a level that is not well formed, so one such file in the catalog would stop the player at that level. A designer can leave work unfinished without breaking the build.
**Alternatives:** letting the game skip a level it cannot play, which hides the mistake from the designer. Drafts keep no place of their own in the play order: the order is the catalog's, and a draft is not in it.

## A level file holds logical data only
**Decision:** a level file stores the board width and height, the timer in seconds, the blocks (colour, cells, the axis of an arrow block, an ice count), the doors (colour, side, start, length, an ice count) and a format version. Walls, corners, ground cells and world positions are not stored; the game builds them from the board size and the doors.
**Why:** the supplied block and door models are modular per cell, so a block is a set of cells and a door is a run of cells. A file that stores no derived data cannot contradict itself, and a new block shape needs no new code and no new prefab.
**Alternatives:** a shape id plus a rotation per block, which ties every level file to a shape table.
The axis and the ice counts were added later with zero as "none", so a file written before them reads the same and the format version did not change. The editor compares the open level with its file after reading and writing the file again, so such a file does not show as changed.

## The level editor is a Unity Editor window
**Decision:** the editor is an `EditorWindow` built with UI Toolkit. It lists the levels, edits board size, timer, blocks and doors on a visual grid, checks the level structure while the designer edits, saves, and starts Play Mode on the edited level.
**Why:** the designer works where the level files live: save writes into the project and play is one button, with no code and no file editing.
**Alternatives:** an editor scene inside the game. Files written on a device do not reach the project, and it needs a second set of runtime UI.

## The editor window is a view; editing rules are logic
**Decision:** what the editor may do to a level (place and move a block, place a door, resize the board) lives in `Scripts/Core/Editing/` and is tested there. The window only shows the level and forwards the designer's input. Each board tool is one class behind a small interface, and every change goes through one method that records an undo step. Opening, creating, deleting, saving and playing a level is a class of its own beside the window, the level switcher, which asks before anything is lost.
**Why:** the editor follows the same separation as the game, so an editing rule cannot differ between two tools, and a new tool is one new class.

## Play starts from the level editor
**Decision:** the Play button of the editor saves the open level, enters Play Mode in the game scene whichever scene is open, and starts that level directly. In Play Mode the button switches the running game to the open level. The button is off while the level check lists a problem.
**Why:** the designer tries a level with one click and never plays a level the game would refuse. The request is kept in the editor session because entering Play Mode reloads the editor's code; the game itself knows nothing about the editor.

## One palette for block colours
**Decision:** a `BlockPalette` asset maps each block colour to its material. The game draws blocks and doors with that material, and the level editor draws them flat in the material's colour.
**Why:** the designer sees in the editor the colours the player sees in the game, and a colour is changed in one place.

## Rules run on grid cells; the view moves smoothly
**Decision:** the logic owns every block position as grid cells. During a drag the view moves the block continuously and asks the logic how far it may go; collision is arithmetic on cell occupancy. On release the block snaps to the nearest cell. There is no physics and there are no colliders. The dragged block may sink a set fraction of a cell into another block: a cell it overlaps by less than that does not count as occupied by it. The board edge stays hard.
**Why:** the rules are deterministic and testable without a scene, and no game state lives in transforms. The overlap is what makes a one-cell gap usable with a finger: without it the block passes only when it is lined up exactly, and a block that is boxed in does not move at all, which reads as a frozen touch.
**Alternatives:** transform positions and physics overlap as the source of truth, which brings float tolerances into the rules.

## When a block leaves the board
**Decision:** a block leaves through a door of its colour when it touches that door's side of the board, all of its cells lie inside the door's span, and no other block stands between any of its cells and the door. The door takes the block while it is being dragged, once it has come within a set reach of the door's side or is pressed against it; a block that only rests at its door stays. Every door the block may leave through is asked, and one it is pressed against goes first, so the order of the doors in the level file makes no difference. The logic removes the block first and reports the cell it left from; the view then lines the block up on that cell and moves it into the door by its own length, cutting off what passes the door line, while cubes of its colour fly out on the other side. Ice on the block or on the door keeps the block in, and an arrow block leaves only through a door that lies along its arrow.
**Why:** this matches the reference game, where a door may be wider than the block and a block is caught slightly before it reaches the door. Changing the state before animating keeps the rule independent of the animation, and a removed block cannot be grabbed again while it leaves. Lining up first keeps a block that was caught off its cell from sliding through the wall beside the door. The cut is made on the block's own mesh: every vertex past the door line is flattened onto it each frame of the exit. Each block has a mesh of its own, so no other block is touched, and the supplied materials need no clipping shader.

## Only the session changes the board
**Decision:** the session owns the board state and hands it out as a read-only interface. A view asks the session to begin, move and end a drag, and learns about results from the session's events: state changed, block left, shown second changed.
**Why:** a view cannot move or remove a block by itself, so every rule runs in one place and the views stay replaceable.

## Each attempt is a new session
**Decision:** starting or restarting a level builds a new session object from the level data, and the board view is rebuilt from that session.
**Why:** the brief requires that no data stays from the previous attempt. Nothing is reset field by field, so nothing can be forgotten.

## The board is built from the session, out of pooled prefabs
**Decision:** the board view shows everything of a level (ground, frame, doors, blocks) when a session starts, from the session's read-only board. Each kind of part is a prefab in `Prefabs/Board/` (`Block`, `Door`, `Wall`, `WallCorner`, `Ground`, `ExitEffect`), and `Board` is the prefab that holds them together with one Lean Pool pool per part. A restart gives every part back to its pool and takes it out again. No level has scene content or a prefab of its own.
**Why:** a level file is enough to show a level, so a level made in the editor plays without any other step, and a board of any size uses the same code path. What a part looks like, where its mesh sits and how it moves is set on its prefab, so a change to the art or the feel needs no code. Restart, which a player presses often, creates no objects.
**Alternatives:** building the parts in code with `new GameObject`, which was the first version: it put mesh offsets and animation times into the scripts and made every visual change a code change. The pools are components in the scene and are passed to the views, so the pool plugin's static entry point is not used.

## Ice is a count in the level; what is left is worked out
**Decision:** a block or a door under ice stores one number: how many blocks have to leave the level before it melts. The board keeps no counter for it: what is left is that number minus the blocks that have left. A block under ice cannot be dragged and cannot leave; a door under ice lets nothing out. The two are independent, and each block and door has its own number.
**Why:** one number per block and door is all a designer sets, and a value that is worked out cannot drift from the board, so a restart needs nothing reset. The views read the counts again after each exit, from the event they already listen to.

## An arrow block is an axis on the block
**Decision:** a block has an axis: any, horizontal or vertical. The drag ignores the finger across the axis, and the exit rule lets the block out only through a door on a side its axis points to. The view takes the supplied arrow mesh that matches the block's length.
**Why:** it is one field and two checks in rules that already exist, so every block shape can be an arrow block and no second kind of block is needed.

## The solver lets out whichever block it can, then starts again
**Decision:** `LevelSolver` answers solvable, unsolvable or unknown. It first rejects what can be seen without a search: a block that reaches no door of its colour even alone on the board, and ice that can never melt because too many blocks wait for each other. Then it searches the positions the blocks can be moved into, nearest first, until some block can be walked to its door; that block leaves and the search starts again from there. The search has a limit of positions; past it the answer is unknown.
**Why:** a block that leaves never makes the rest harder: it frees cells, melts ice, and every move made to let it out can be taken back. So the order of exits never has to be tried, only whether a next exit exists, which keeps the search small on real levels. The solver uses the same board and exit rule as the game, so it cannot disagree with it.
**Alternatives:** a search over every order of moves and exits, which grows too fast to run on save. The editor warns only on "unsolvable"; "unknown" saves without a warning, because a wrong warning teaches the designer to ignore it.

## A tool brings its own options
**Decision:** a board tool of the editor can hand over an element with its own choices (the ice count of the Ice tool, the direction of the Arrow tool), which the tool panel shows while that tool is chosen.
**Why:** a new tool stays one class and one entry in the tool list, and the panel shows only what the chosen tool uses.

## Drag feel is tuned in one asset
**Decision:** how far the dragged block may sink into another block, and from how far a door takes a block, are two values in `Data/DragTuning.asset`. The game scene's root turns them into the logic's `DragRules` and hands them to every session. The logic clamps both: the overlap stays below half a cell and the reach within half a cell.
**Why:** these values are found by playing, not by reasoning, so they have to be changeable without code. They are rules, not view settings, so they are passed into the logic instead of being applied by the input, and the tests can run the same rules with exact values.

## The outline of a held block is a second mesh
**Decision:** the `Block` prefab has a child that takes the same mesh as the block and draws it with `Shaders/BlockOutline.shader`: the mesh is widened along its normals and only its back faces are drawn. The child is active only while a finger holds the block. Colour and width are set on the material.
**Why:** it follows any block shape without extra art or a render feature, and it costs one draw call for the one block that is held.
**Alternatives:** a full-screen outline pass, which costs every frame for one block, or outline meshes built per shape in code.

## A block is one mesh made of quarter parts
**Decision:** each cell of a block is four quarters. A quarter gets the supplied part that fits its neighbours (flat centre, straight edge, outer corner, inner corner), and the parts of a block are combined into one mesh when the board is built.
**Why:** any shape made of cells gets rounded outer corners and correct inner corners without new art, and a block is one renderer instead of up to four per cell. The source models are readable at run time for this (`Read/Write` is on in their import settings).
**Alternatives:** one prefab per shape, which needs new art for every new shape; one object per part, which multiplies the renderers.

## The camera frames the board
**Decision:** the camera keeps a fixed tilt and moves back until the board with its frame fits a rectangle of the screen that leaves room for the HUD above and the boosters below. The touch position is projected onto the plane of the block tops.
**Why:** boards of different sizes and screens of different aspect ratios need no per-level or per-device camera values.

## Two scenes, each with its own composition root
**Decision:** `HomeScene` is the first scene and holds the home screen and the settings; `GameScene` holds the board, the HUD and the popups of a level. Each scene has one root object (`HomeRoot`, `GameRoot`) that creates the logic objects and passes them to the views of that scene. Nothing is carried from one scene to the other in memory: each root loads the player's state from the storage. `GameScene` starts the level the player has reached, so it also plays when it is opened alone in the Editor.
**Why:** the menu and the game do not share objects, so each scene stays small, loads only what it shows and can be worked on alone. Passing state through the storage needs no singleton, no static and no object that survives a scene load.
**Alternatives:** one scene with screens switched on and off, which was the first version: no loading between screens, but every screen and the board sat in one file. A persistent boot scene with additive loading: one composition root for the whole game, at the cost of a third scene and a rule for finding each scene's views.

## Screens and repeated UI parts are prefabs
**Decision:** every screen and popup is a prefab in `Prefabs/UI/` with its own canvas, and a part that repeats (a setting row, a booster button, a level node) is a prefab nested in its screen. The settings popup is one prefab used by both scenes. The values that set how the UI moves (button press, popup opening) are in one `UiMotion` asset.
**Why:** a screen can be opened and changed alone, a change to a repeated part is made once, and a value that changes on one screen (the timer) does not rebuild the others.

## The flow between screens is logic
**Decision:** `GameFlow` in `Scripts/Core/Flow/` starts, restarts and leaves levels and decides what a win is worth. The screens only show values and report button presses; one view class, `GameScreens`, shows the screen that fits the flow's events and passes each press on.
**Why:** the acceptance criteria about restart, the next level and the coin count are rules, so they are tested without a scene. A screen can be redrawn or replaced without touching them.

## Player data sits behind one storage interface
**Decision:** coins, the reached level and the settings are one `PlayerData` object. `PlayerProfile` is the only code that changes it and saves it through `IPlayerStorage` after every change. The game stores it as one JSON text in PlayerPrefs. Settings are stored as the list of those switched off.
**Why:** quitting at any moment loses nothing, a test replaces the storage with a fake, and moving to a file or a server is one new class. A setting added later starts switched on without a migration.

## Pause is a session state
**Decision:** pause, win and fail are states of the session. The timer advances only in the playing state. `Time.timeScale` is not changed.
**Why:** popups can animate while the game is paused, and the timer cannot run behind a popup.

## The pause screen is the settings screen
**Decision:** the pause button of a level opens the settings screen with an extra Home button. Closing it resumes the level. Restart is a button of its own on the HUD. The game also pauses when the app goes to the background.
**Why:** this is how the reference game does it, and one screen serves both places.

## A touch on the UI does not reach the board
**Decision:** before a touch can pick up a block, the board input asks the event system whether a UI element lies under it.
**Why:** a press on a HUD button or on a popup must not move the block behind it.

## The timer starts on the first touch
**Decision:** the countdown begins when the player first touches the board, not when the level appears.
**Why:** the brief does not say when the timer starts. The player loses no time while reading the board.

## Win beats timeout
**Decision:** when the last block leaves on the tick in which the timer reaches zero, the level is won.
**Why:** the brief does not cover this case. The player did complete the level.

## Coin rules
**Decision:** a new player starts with 400 coins. Winning the level the player has reached grants 40 coins, once, at the moment the level is won, and moves the player to the next level. "Continue" works only on a won level, so pressing it twice cannot skip a level. A level that is played again grants nothing and does not move the player. A level started from the level editor is a preview: the flow is told so when the level starts, and restart and the next level stay in it, so nothing the player has changes even when the previewed level is the one the player has reached. Nothing spends coins.
**Why:** the brief requires a working coin inventory but names no source and no price. The values match the reference game. Granting at the state change keeps the count correct when the player leaves before pressing a button.

## Level 1 and the tutorial hint
**Decision:** the five levels reproduce levels 1 to 5 of the reference game. The "Swipe to Gate" hand hint of level 1 is not implemented.
**Why:** a tutorial is not in the required scope.

## Home screen and popups
**Decision:** the level button starts the level directly; the reference game's popup before a level, with its booster choice, is left out. The level path above the button, the profile box, the lives and the tabs are placeholders that react to a touch. The fail popup is our own design: "Time's Up!" with Try Again and Home.
**Why:** the brief asks for a level button that starts the current level, and boosters and lives are outside the required scope.

## UI layout
**Decision:** the canvas scales with the screen width from a 1080 x 1920 reference. Each screen keeps its content inside a panel that follows the device's safe area, and the bars are anchored to the top and the bottom of that panel. The camera leaves the top 22 percent and the bottom 20 percent of the screen to the HUD.
**Why:** a taller screen only adds room between the bars, so the layout holds at 16:9 and at 20:9, and a notch covers nothing.

## Android runs the lightest render settings
**Decision:** on Android the game uses the "Performant" quality level: no HDR, no shadows, no ambient occlusion pass. All UI sprites that fit are packed into one sprite atlas.
**Why:** the two heavier quality levels of the project template draw the board twice for ambient occlusion, which the flat look does not need. The atlas lets a screen's images share draw calls: the home screen went from 29 batches to 14.

## After the last level
**Decision:** the level number keeps counting up and the level content starts again from the first file.
**Why:** "continue to the next level" must always have a next level, and five levels are shipped.

## Android build: IL2CPP with ARM64 and ARMv7
**Decision:** the Android build uses the IL2CPP scripting backend and targets ARM64 and ARMv7.
**Why:** a phone that runs only 64-bit apps cannot install an ARMv7-only build.

---

## Open

- **Third-party plugins**: DOTween (UI and board animation) and Lean Pool (the board's pools) are in `Assets/Plugins/`. Record where they came from for the README. The exit effect adds no art: it draws Unity's built-in cube with the block's material.
- **Large open levels are not checked**: the solver stops after a set number of positions and answers "unknown", and the editor then saves without a warning.
- **Settings have no effect yet**: the vibration, sound and music switches are saved and shown, but nothing in the game listens to them. There are no audio assets in the supplied art.
