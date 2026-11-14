# Asset notes

What the supplied art contains and what is missing. The brief welcomes feedback on the assets; the README's asset feedback is written from the "Feedback" list. Add a line when a problem shows up during work.

## Models (`VisualAssets/Models/`)

| File | Meshes | Use |
|---|---|---|
| `BlockParts.fbx` | `ModularBlock_Center`, `ModularBlock_Edge`, `ModularBlock_InnerCorner`, `ModularBlock_OuterCorner` | A block is assembled per cell from these parts, so any shape made of cells can be built. |
| `WallAndDoor.fbx` | `Wall`, `Corner_1`, `corner_3`, `corner_3.002`, `corner_4`, `corner_5`, `Door_1x1`, `Door_Arrow` | The board frame. A door is a run of `Door_1x1` pieces. |
| `GroundGrid.fbx` | `Grid` | One ground cell. |
| `Arrows.fbx` | `Arrow_1`, `Arrow_2`, `Arrow_3` | Arrow block (optional scope). |

The FBX files carry four material slots: `ModularBlock_Base`, `Base_Side`, `Gate_Orange`, `Arrow_Gate_Orange`.

## Textures and UI

- `VisualAssets/Textures/icetexture.jpg`: ice (optional scope).
- `VisualAssets/UI/`: sprites for the home screen, the tab bar, the gameplay HUD, popups, the settings rows, coins and booster icons.

## Feedback

- `btn_toggle_on.png` has no matching "off" sprite. The off state of a settings toggle has to be made from the same sprite.
- No material assets and no block colour textures are supplied; block, wall and door materials have to be created.
- No audio is supplied, so the sound and music toggles can only store and show their state.
- No font is supplied.
- Mesh names in `WallAndDoor.fbx` mix cases and carry export leftovers (`Corner_1`, `corner_3`, `corner_3.002`).

## To check

- Which corner mesh is the outer corner and which the inner corner, and the pivot and size of each part, once they are placed in a scene.
