---
name: grill-me
description: >
  Questions the user about a plan or design, one decision at a time, until every branch is
  settled. Use before building a system (architecture, level format, editor design) or when the
  user asks to be grilled or to have a plan questioned.
---

# Grill me

Interview the user about the plan until both sides understand it the same way. Walk the decision tree branch by branch and settle a decision before moving to the ones that depend on it.

- Ask **one question at a time**.
- With each question give your **recommended answer** and the reason. Push back when the user's answer scores worse against the evaluation order in `AGENTS.md`.
- A question the code or `knowledge/` can answer is not asked: look it up.
- Do not write code during the interview. Stay in it until the user ends it.
- At the end, list the settled decisions in the format of `knowledge/decisions.md` and offer to add them there.
