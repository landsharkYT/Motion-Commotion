# Equation actions and block-return verification

Status: complete. The [independent review](equation-actions-adversarial-review.md) rates the implementation 9.2/10 with no substantive missing features.

The [current equation contract](../../design/standalone-equation-actions.md) records the user's revised standalone behaviors and takes precedence over earlier nested-only restrictions.

## Corrections

Three deterministic interaction tests failed before the fix: Blocks had no return/drop target, primary equations selected an inner formula instead of the owning command, and the first three palette equations lacked connectors.

The Blocks panel now accepts returned commands and nested formulas. Returning a command removes it and its attached formulas. Returning a nested formula clears only its owning input. Dragging the primary equation selects and previews the whole connected command.

All five equation tiles have connectors and can snap into the main stack. Velocity sets velocity, Acceleration configures ongoing acceleration, Position places the orb instantly, Force applies one kick, and Weight configures downward gravity. Nested expressions remain pure values.

Velocity, Acceleration, Position, and Force each require a fresh Direction. Signed velocity, acceleration, and position outputs reverse that direction. Position uses the captured starting point as origin and preserves motion settings. Commanded acceleration acts alongside gravity, survives other actions, and participates in Reset.

Destination penetration is rejected without leaving the orb at the rejected point. The check permits Unity's configured contact margin; an initial test exposed the engine's -0.01 m distance at a visibly touching surface. Placement clears old landing-approach history, and only a later Wait physics step can establish a valid landing.

## Native evidence

- 36 EditMode tests passed, including action types, units, direction consumption, signed outputs, range rejection, and zero-time execution.
- 38 PlayMode tests passed, including all-five stack snapping, command/formula return deletion, whole-command dragging, ongoing acceleration, fixed-origin placement, state preservation, overlap rejection, reset, and landing after placement.
- Existing compact layout, nested-expression, equation-pencil, session, physics, and outcome regressions remain active.

The new runtime telemetry exposes commanded acceleration and runtime errors for read-only browser assertions. Browser checks drive actual pointer and keyboard input rather than injecting commands.

## Final verification

| Check | Result | Evidence |
| --- | --- | --- |
| EditMode | 36 passed | [Results](../results/editmode-results.json) |
| PlayMode | 38 passed | [Results](../results/playmode-results.json) |
| Browser interactions | 27 groups passed; no captured page or console errors | [Results](../results/web-results.json) |
| WebGL build | Completed | [Build result](../results/web-build-result.json) |
| Layout and asset identity checks | Passed | [Results](../results/layout-results.json) |
| Unity console | No errors or compilation failure | [Results](../results/equation-actions-console-results.json) |
| Independent review | 9.2/10; no substantive missing features | [Review](equation-actions-adversarial-review.md) |

Actual player captures:

- [Returned command deletion](../screenshots/block-return-delete.png)
- [Velocity snapped into Active](../screenshots/equation-stack-velocity.png)
- [Acceleration snapped into Active](../screenshots/equation-stack-acceleration.png)
- [Position snapped into Active](../screenshots/equation-stack-position.png)

The browser verifies the three new actions at time zero, acceleration through Wait, Reset, overlap rejection, and the repaired drag/delete interaction. It also repeats the existing compact layout, typed nesting, mass, validation, modal, outcome, and navigation checks.

The README, domain glossary, architecture notes, and earlier specification authority links point to the current equation contract. The pass 2 palette table and heading now state the current actions rather than the superseded value-only behavior.
