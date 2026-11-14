# Case brief

The requirements of the take-home case, restated as a checklist. The original brief is the authority; when this file and the brief disagree, fix this file.

## Goal

A small vertical slice of Color Block Jam in Unity: a playable level flow and a level editor. Code quality, architecture and editor usability are evaluated. Art is not.

## Constraints

- Unity 2022.3.62f2. Reference resolution 1080 x 1920, portrait.
- Use the supplied assets. No other art unless absolutely necessary.
- Any LLM tool is allowed; the README says which tools were used and for which parts.
- Time limit: 7 days.

## Deliverables

One link (Git repository or archive) with:

1. The Unity project: `Assets`, `Packages`, `ProjectSettings`. Not `Library`, `Temp` or `Build`.
2. `README.md` (see below).
3. An Android build (`.apk`) or a screen recording of at most 3 minutes that shows the full flow.

## Required scope and acceptance criteria

### Home screen

- A tab navigation bar; the tabs are placeholders and the other sections are not implemented.
- The level button and the top UI are shown.
- Buttons other than level and settings give visual feedback on touch and need no function.

Acceptance:

- [ ] The level button starts the current level.
- [ ] The settings button opens the settings screen.
- [ ] The layout is correct at 1080 x 1920 and stays correct on a 20:9 device.

### Settings

- All buttons are placeholders, but each gives visual feedback.
- The vibration, sound and music toggles show their on/off state.

Acceptance:

- [ ] Each toggle shows a different visual state for on and for off.
- [ ] The player can close the screen and return to the home screen.

### Gameplay

- The first 5 levels, built from the supplied assets.
- Working: timer, coin inventory, fail popup, pause, restart, home navigation.
- Boosters need no function. The life system can be a placeholder.

Acceptance:

- [ ] The player can complete a level and continue to the next level.
- [ ] The timer counts down and stops the level at zero.
- [ ] The fail popup opens when the level fails, and offers restart and return to home.
- [ ] Restart sets the level back to its start state. No data stays from the previous attempt.
- [ ] The coin count stays correct after the player leaves the level and returns to it.

### Level editor

For non-technical users. It controls board width and height, door placement, block placement and the timer value.

Acceptance:

- [ ] A designer can make a new level, save it and play it, without writing code or editing a file by hand.
- [ ] The editor shows the level in a visual form, not only numeric fields.
- [ ] The saved data has one format, and the game reads the same format.
- [ ] The 5 gameplay levels come from this editor.

## Optional scope

Only after the required scope is complete: level validation (a warning before saving an unsolvable level), functional boosters, ice blocker, arrow block.

## Evaluation, in order of weight

1. Architecture: separation of data, logic and view; low coupling; easy to extend.
2. Editor usability: a designer can use the editor without help.
3. Code quality: clear names, no dead code, consistent style.
4. Correctness: the acceptance criteria above are satisfied.
5. Performance: no unnecessary allocation in the update loop; stable frame rate on a mid-range device.
6. Scope control: a small and complete result is better than a large and incomplete one.

## README must contain

- How to open and run the project.
- How to open the editor and make a level.
- The architecture decisions and the reason for each.
- Known problems and incomplete parts.
- The LLM tools used and the parts they were used for.
- Approximate work time.

## Notes from the brief

- Follow industry best practices.
- Problems found in the supplied assets are worth reporting; feedback on them is welcome.
- An unclear requirement: decide, continue, and write the decision in the README.
