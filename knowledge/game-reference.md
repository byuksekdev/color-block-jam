# Game reference

How the reference game, Color Block Jam, plays in its first levels. The layouts below are a guide for building the levels; the level files saved by the editor are the authority.

## Rules

- A block is dragged freely in both directions, stops against walls and other blocks, and snaps to the grid on release. The held block shows an outline.
- A block leaves through a door of its colour when it reaches that door and fits inside its span. A door can be wider than the block. The door sinks while the block passes, and the block breaks into small cubes of its colour.
- A level is complete when no block is left. A "Level Complete" popup shows a "Continue" button with the reward, and continuing starts the next level directly, without the home screen.
- Each completed level adds 40 coins (400, 440, 480, 520, 560 over levels 1 to 5).
- The timer shows `mm:ss`.

## Gameplay HUD

Top: a level badge, the timer, a restart button and a pause button. Below them on the right: the coin count with a plus button. Bottom: four booster slots, locked, labelled Lv.6, Lv.8, Lv.12 and Lv.16.

## Levels 1 to 5

All five boards are rectangles without holes, and every timer starts at 05:00.

| Level | Board | Blocks | Doors |
|---|---|---|---|
| 1 | 4 x 5 | blue 2x2, pink 2x2 | blue on top (3 cells), pink on the bottom (3 cells) |
| 2 | 5 x 5 | green 1x3, orange 1x3, both upright | green on top (3 cells), orange on the bottom (3 cells) |
| 3 | 5 x 6 | blue L (4 cells), red 1x2, yellow L (4 cells) | blue on top, red on the left and on the right, yellow on the bottom |
| 4 | 5 x 6 | blue 2x2, orange 1x2, yellow 2x2 | yellow on top, orange on the left and on the right, blue on the bottom |
| 5 | 5 x 6 | light blue 1x3, green 2x2, pink 2x2, orange 3x1, red 3x1 | pink and green on top, orange on the left, red on the right, light blue on the bottom |

Level 1 in detail (columns from the left, rows from the top): the blue block covers columns 3 to 4 of rows 2 to 3; the pink block covers columns 1 to 2 of rows 3 to 4; the blue door spans columns 2 to 4 of the top side; the pink door spans columns 1 to 3 of the bottom side.

## Outside the required scope

From level 6 on: doors covered in ice that open after a number of blocks have left, boards with a hole, boards that are not rectangles, the Freeze Timer booster and the Rocket booster. Level 1 also shows a "Swipe to Gate" hand hint.
