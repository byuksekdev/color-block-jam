# Asset notes

What the supplied art contains and what is missing. The brief welcomes feedback on the assets; the README's asset feedback is written from the "Feedback" list. Add a line when a problem shows up during work.

## Models (`VisualAssets/Models/`)

| File | Meshes | Use |
|---|---|---|
| `BlockParts.fbx` | `ModularBlock_Center`, `ModularBlock_Edge`, `ModularBlock_InnerCorner`, `ModularBlock_OuterCorner` | A block is assembled per cell from these parts, so any shape made of cells can be built. |
| `WallAndDoor.fbx` | `Wall`, `Corner_1`, `corner_3`, `corner_3.002`, `corner_4`, `corner_5`, `Door_1x1`, `Door_Arrow` | The board frame. A door is a run of `Door_1x1` pieces. |
| `GroundGrid.fbx` | `Grid` | One ground cell. |
| `Arrows.fbx` | `Arrow_1`, `Arrow_2`, `Arrow_3` | The arrow on an arrow block: one mesh for a block of one, two and three cells along its arrow. |

The FBX files carry four material slots: `ModularBlock_Base`, `Base_Side`, `Gate_Orange`, `Arrow_Gate_Orange`.

## Textures and UI

- `VisualAssets/Textures/icetexture.jpg`: ice. The game draws frozen blocks and doors with `Shaders/FrostedIce.mat`, made from this texture; the supplied `VisualAssets/Materials/Ice.mat` is not used.
- `VisualAssets/UI/`: sprites for the home screen, the tab bar, the gameplay HUD, popups, the settings rows, coins and booster icons.

## UI sprites in use

- Nine UI sprites have slice borders set in their import settings so that one sprite serves different sizes: `bg_popup`, `bg_popup_info_holder`, `Btn_popup_green`, `Btn_popup_blue`, `Btn_level_green`, `bg_home_coin_holder`, `bg_home_resource_holder`, `bg_gameplay_time_holder`, `SquareRoundedMask`.
- `Btn_levelBg_normal` is the level badge and timer of the HUD, `Btn_square_retry` the restart button and `Btn_settings_normal` the pause button. The home screen's settings button is `Settings_Icon` alone.
- `VisualAssets/Fonts/UiText.mat` is a material of the TextMesh Pro default font with an outline; all UI text uses it.

## Feedback

- `btn_toggle_on.png` has no matching "off" sprite. The off state of a settings toggle has to be made from the same sprite.
- No material assets and no block colour textures are supplied; block, wall and door materials have to be created.
- The UVs of the block parts cover a small patch of the texture (`ModularBlock_Center`: 0.23–0.43 by 0–0.37) and repeat in every quarter, so a texture laid on by UV, such as the ice, shows as a small repeating spot.
- No audio is supplied, so the sound and music toggles can only store and show their state.
- No font is supplied.
- The home screen's level path has no sprites (level nodes, rails); the nodes are made from `SquareRoundedMask`.
- No avatar is supplied for `profile_box_blue`, and no art for a fail popup.
- Mesh names in `WallAndDoor.fbx` mix cases and carry export leftovers (`Corner_1`, `corner_3`, `corner_3.002`).
- The models do not share an up axis: block parts have -Z up, and the door and the frame corner lie upside down. Each needs its own correction in code.
- The ground tile is slightly smaller than a cell (1.953 against 2 units), which leaves gaps between tiles.

## Measured in the project

- One board cell is 2 units. `Grid` is 1.953 units wide, so neighbouring ground tiles leave a thin gap; a flat base under the tiles fills it.
- The block parts are a quarter of a cell (1 x 1 unit) and are authored with -Z up, so they are turned 90 degrees around X. Upright, a block reaches from 0.655 below its origin to 0.963 above it.
- `ModularBlock_Edge` has its wall on +Z, `ModularBlock_OuterCorner` its corner on (+X, +Z). `ModularBlock_InnerCorner` has its pivot on the concave corner, covers the three other quarters around it, and its rounding reaches 0.16 units into the empty quarter.
- `Wall` is a 1 x 3.58 x 1 box. `Door_1x1` is 2 units long, so one piece spans one cell; `Corner_1` is 1 x 1. Door and corner are authored with the rounded end down and are turned over; the frame top sits 1.24 units above the ground.
- `Door_Arrow` points to +Z and is used as authored.
- `BlockParts.fbx` and `GroundGrid.fbx` are imported with `Read/Write` on, because block and ground meshes are combined at run time.
