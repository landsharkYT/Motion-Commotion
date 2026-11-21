# Pass 1 implementation evidence

This report describes the original pass 1 implementation. Paths and links were updated during the project-layout migration; canonical result files now contain the latest rerun. Historical review ratings remain unchanged.

## Implemented foundation

The project contains the four specified scenes, shared gameplay and block prefabs, five explicit formulas, typed nested inputs, command validation, manual physics stepping, editable physical mass, gravity commands, execution controls, and a development Web build.

No authored puzzle levels, functional editor, rearrangement UI, derivatives, scoring, narrative, persistence, or public deployment were added.

## Observed results

| Check | Result | Evidence |
| --- | --- | --- |
| Unity Edit Mode | 26 passed, zero failed | [Detailed results](../results/editmode-results.json) |
| Unity Play Mode | 10 passed, zero failed | [Detailed results](../results/playmode-results.json) |
| Browser interactions | 18 checks passed, no captured browser errors | [Detailed results](../results/web-results.json) |
| Development Web build | Completed | [Build result](../results/web-build-result.json) |

The Web player is in `Builds/Web`. [README.md](../../../README.md) contains the build, local server, and repeatable verification commands. [Coverage map](coverage.md) connects requirements to source and checks.

## Visual evidence

- [Title](../screenshots/web-title.png)
- [Empty workspace](../screenshots/web-empty.png)
- [Nested formula program](../screenshots/web-program.png)
- [Paused experiment](../screenshots/web-paused.png)
- [Completed experiment](../screenshots/web-completed.png)
- [Reset experiment](../screenshots/web-reset.png)
- [Weight command](../screenshots/web-weight.png)
- [Resized desktop window](../screenshots/web-resized.png)
- [Rejected extreme numeric draft](../screenshots/web-invalid-extreme.png)
- [Rejected cumulative kick budget](../screenshots/web-invalid-cumulative.png)

Reset checks include both the physics position and rendered Transform. The restored ball is visibly at its starting position. The first independent review did not reproduce the suspected visual corruption; it identified locked scrolling instead. The expanded browser suite verifies that paused users can reveal the offscreen Wait without changing program content or physical state.

Earlier failure and focused-drag captures are retained as diagnostic history. The canonical current results are the three result files linked above.

## Review gate

The [final independent review](adversarial-review.md) scores the current implementation **9.4/10**, marks the gate **PASS**, and explicitly concludes **No substantive missing features from docs/design/pass1spec.md.** No findings remain open.

The [first review](adversarial-review-round1.md) scored 8.0 and identified unsafe extreme numeric inputs and locked workspace scrolling. Both findings have targeted regression checks and implementation fixes. The [second review](adversarial-review-round2.md) passed at 9.2 with one minor numeric-display finding. The final build corrects that display issue, adds native and browser assertions, and passed the reviewer's focused actual-player recheck.
