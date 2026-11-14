---
paths:
  - "AGENTS.md"
  - "CLAUDE.md"
  - ".claude/settings*.json"
  - ".claude/rules/**"
  - ".claude/agents/**"
  - ".claude/skills/*/SKILL.md"
  - "knowledge/README.md"
---

# Editing the agent configuration

Every always-on line is read in every chat and followed literally. Test each line with one question: would the agent get this wrong without it? If not, cut it.

- `AGENTS.md` is the single source: under about 100 lines, only project facts and hard constraints. `CLAUDE.md` only imports it and gets no content of its own.
- State a rule once. Conventions for one file type go to `.claude/rules/*.md` with `paths:`. Facts about how the game works go to `knowledge/`.
- A skill earns its place with a repeatable procedure or an output format someone depends on. An agent earns its place with a read-only job that benefits from a fresh context.
- `.claude/settings.json` holds only what must hold mechanically: no hand edits of Unity YAML or level files, a question before any git write. Personal settings go to `.claude/settings.local.json`, which is git-ignored.
- This folder is part of the submission and may be read by the reviewer: plain English, no personal data, nothing copied from another project. The same holds for `knowledge/`: only what helps build the project. Nothing about the developer, who did which part, or where a piece of information came from in a chat.
