---
paths:
  - "Assets/**/*Tests.cs"
---

# EditMode tests

A test earns its place when it would have gone red for a real defect. The cases come from the acceptance criteria in `knowledge/case-brief.md` and from the table below, not from the happy path alone.

## What gets tested

Pure logic only: the timer, win and fail decisions, block movement and door rules, coin inventory, level validation, level save and load. Views, prefab wiring, input and animation are checked in Play Mode by the user. Do not extract a class only to have something to test.

## What a test must prove

- **Expected values come from the case brief or from `knowledge/decisions.md`**, never from running the code. No source for a value: decide, record the decision, and name it in the test.
- **A bug fix starts red.** Name the mechanism, write the test, and have the user confirm it fails on the unfixed code for that reason.
- **Every condition decides once.** For each operand of a rule, one case where only that operand flips the result.
- **The rule under test is the one the game runs.** After extracting logic, search for its caller outside the tests. No caller means the MonoBehaviour still has its own copy.

## Cases this project needs

| The code does | Cases |
|---|---|
| Counts a timer down | reaches exactly zero; a step larger than the time left (clamped at zero, fail raised once); paused time does not count; zero or negative start value |
| Decides win or fail | the last block leaves on the same tick the timer reaches zero (one outcome, and which one is a recorded decision); the win or fail event raised twice has one effect |
| Restarts a level | state after restart equals the state of a fresh load: board, timer, any counters |
| Changes the coin count | survives save then load; leaving a level and returning does not add or lose coins; a reward is granted once per win |
| Saves and loads a level | save then load gives an equal level; width or height of 0 or 1; a block or door outside the board; an empty board; an unknown or missing field in the file; the same round trip with the thread culture set to `tr-TR` |
| Validates a level | a block overlapping another; a block colour with no matching door; a door that runs past the end of its side |
| Uses an enum as a key (colours, block shapes) | one test over `Enum.GetValues`: every member has an entry |

Culture case: set `CultureInfo.CurrentCulture = new CultureInfo("tr-TR")` in the test and restore it in `[TearDown]`.

## Tools

- Unity Test Framework 1.1.33 with NUnit 3.5: `[TestCase]`, `[TestCaseSource]`, `[Values]`, `[Combinatorial]` exist; `Assert.Multiple` does not.
- Names: `Method_Condition_ExpectedResult`. One behaviour per test.
- No mocking package. A fake is hand-written, returns canned data and never recomputes the rule under test.
- No `Time.deltaTime`, `DateTime.Now` or unseeded random inside the rule under test: the caller passes the elapsed time and the seed.
- Never a bare `[Ignore]`. A deliberate skip names its reason and gets a line under "Known problems" for the README.
- Tests live in `Scripts/Editor/Tests/`. Our code has no assembly definitions, so the `Editor` folder is what makes them EditMode tests.
- Tests of `Scripts/Core/` use no `UnityEngine` type, so an agent can run them: a scratch project outside the repository (`net9.0`, `LangVersion` 9.0, NUnit 3.14, NUnit3TestAdapter, Microsoft.NET.Test.Sdk) that includes `Scripts/Core/**/*.cs` and those test files, then `dotnet test`. Report the summary line. That NUnit is newer than Unity's 3.5: use only what 3.5 has.
- Every other test needs Unity. Run the EditMode tests through the Editor tools when the session has them. Otherwise hand the user the Test Runner path (Window > General > Test Runner > EditMode) and report "written, not run" until they paste the summary. A `dotnet` pass does not replace the Test Runner pass before a milestone commit.
