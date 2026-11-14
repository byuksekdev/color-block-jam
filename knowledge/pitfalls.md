# Pitfalls

Failure modes that fit this project's stack (DOTween, Lean Pool, uGUI, a timer, restart). Each one maps to an acceptance criterion it can break. Check the relevant ones before calling a feature done.

## Restart and leftover state

*Breaks: "Restart sets the level back to its start state. No data stays from the previous attempt."*

- Static fields and static events survive a restart and a scene reload. Keep game state in instances that restart throws away.
- A tween started in the old attempt keeps running and writes position, scale or colour onto the new board. Kill the tweens of the board before rebuilding it (`DOTween.Kill(target)` or a shared tween id).
- A coroutine or delayed callback from the old attempt fires into the new one: a late "level failed" after the player already restarted.

## Pooled objects

- An object returned to Lean Pool is disabled, not destroyed: `this == null` stays false and `OnDestroy` does not run. A callback that captured it still sees a live object, now possibly reused as a different block.
- Reset everything in `OnEnable` or the spawn hook: colour, scale, rotation, parent, running tweens, event subscriptions. `Awake` and `Start` run only the first time.
- Despawn while a tween runs on the object: kill the tween first.

## Tweens

- A killed tween that someone awaits or chains completes instead of failing, so the "after the animation" code runs anyway. Guard the continuation with the state it depends on.
- A tween callback that changes game state (remove the block, grant the win) makes the rule depend on the animation. Change the state first, then animate the view to match it.
- `Time.timeScale = 0` freezes DOTween as well. A pause menu that animates needs `SetUpdate(true)` on its tweens.

## Timer and pause

*Breaks: "The timer counts down and stops the level at zero."*

- One frame can skip past zero: clamp at zero and raise the fail once, guarded by a state check rather than `== 0f`.
- Decide what happens when the last block leaves on the frame the timer ends, and record it in `decisions.md`.
- The timer must not run while paused, while a popup is open or after win or fail. A timer driven by the game state stops correctly in all three; one driven only by `timeScale` does not survive a popup that animates.
- An app sent to the background: `Time.deltaTime` does not jump on resume, but a timer built on `DateTime` or `Time.realtimeSinceStartup` does.
- Rewrite the timer label when the shown second changes, not every frame: a string per frame is garbage on the update loop.

## Input

- A second tap while the first is still handled: restart tapped twice, next-level tapped twice (skips a level), a reward granted twice. Latch the action and release the latch on every exit path.
- Input must stop when the level is won, failed or paused, including a drag that began before the popup opened.
- Multi-touch: a second finger grabbing another block mid-drag. Decide whether one drag at a time is the rule and enforce it.

## Saved data

*Breaks: "The coin count stays correct after the player leaves the level and returns to it."*

- Grant coins once per win, at the state change, not when a popup button is pressed: the player can leave before pressing it, or press it twice.
- Write through one storage type. Two scripts that each cache the coin count drift apart.
- A device can run a Turkish locale. `float.Parse("1.5")` reads 15 there, `1.5f.ToString()` writes `1,5`, and `"I".ToLower()` is not `i`. Level files and saved values use `CultureInfo.InvariantCulture`. `JsonUtility` is already culture-invariant; hand-built strings are not.
- A level file with a missing or unknown field (an older save, a hand-made test file) must load with defaults or fail with a clear message in the editor, never with a null reference in play.

## Layout

*Breaks: "The layout is correct at 1080 x 1920 and stays correct on a 20:9 device."*

- Canvas Scaler: Scale With Screen Size, reference 1080 x 1920. Check the match value at 16:9 and at 20:9 (1080 x 2400) in the Game view before calling a screen done.
- Anchor to edges and corners, not to the centre with fixed offsets. Top and bottom bars respect `Screen.safeArea` on phones with a notch or a gesture bar.
- The 3D board has to fit as well: frame it from the board size and the camera aspect, not from a fixed camera position.

## Unity project hygiene

- A script moved or renamed outside the Editor loses its `.meta` and every scene and prefab reference to it. Move files inside Unity.
- Unity rewrites `ProjectSettings/` and `Packages/packages-lock.json` on its own; look at the diff before committing so unrelated changes do not ride along.
- An empty folder is not tracked by git, but its `.meta` is. Unity then deletes the orphan `.meta` on the reviewer's machine and shows a warning. Commit a folder only once it has content.

## Rendering

- The "Balanced" and "High Fidelity" renderers of the project template carry a screen space ambient occlusion feature. It adds a depth-normals pass, so every board mesh is drawn twice. The Editor plays at "High Fidelity"; Android is set to "Performant", which has no such feature, so the device looks slightly flatter than the Editor.
- A UI sprite that is not in `UiAtlas` breaks the batch of its screen. Add new UI sprites to the atlas; sprites larger than 1024 pixels stay out of it.
