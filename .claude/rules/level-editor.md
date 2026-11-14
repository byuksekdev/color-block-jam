---
paths:
  - "Assets/**/Editor/LevelEditor/**"
  - "Assets/**/Core/Editing/**"
---

# Level editor

The user of this tool is a designer who does not read code. A feature is done when they can use it without being told how.

- The window is a view. What may happen to a level is decided in `Scripts/Core/Editing/` and `Scripts/Core/Validation/`; the window and the tools call those and hold no rule of their own. A new rule gets a test in `Scripts/Editor/Tests/`.
- Every change to the open level goes through `LevelEditorWindow.Edit`, which records the undo step and refreshes the views. Nothing else writes to the level.
- A board tool is one class that implements `IBoardTool` and is added to the window's tool list. Do not grow a tool with a second job.
- Level files and the level catalog are written only by `LevelLibrary`, through `LevelSerializer`.
- Text the designer reads (labels, hints, problems) is plain and says what to do. No field names, no jargon.
- A destructive action asks first (delete a level) or can be undone (everything on the board).
- To check the window, read its visual tree and send it pointer events through the Editor tools. Do not capture the screen: a capture shows whatever is in front, which may not be Unity.
