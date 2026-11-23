# Input validation and speed-cap verification

Status: verified and accepted after independent adversarial review, 9.3/10.

Contract: [numeric input validation](../../design/input-validation.md#approved-implementation-contract).

## Implemented behavior

Decimal input commits on Enter or focus loss. Invalid commits restore the previous value with labeled red feedback beneath the equation and a red outline on the offending white input. Whole-paste validation runs before Unity appends characters. Shared mass and repeated values synchronize after successful commits. Invalid Active or shared-mass drafts block execution; unused invalid palette drafts do not. Placement and Run reject an action that ends an invalid edit.

Inputs support exaggerated values up to a literal magnitude of 1000000. Calculations retain double precision and validate signs, units, divisors, finite results, world bounds, and program duration. Actual velocity caps independently, default 100 m/s, configurable in Workspace settings. The red warning reads "Speed limit reached. Movement capped at {limit} m/s." Play continues. Reset clears the warning; otherwise it holds for one second after the latest cap event and fades over one second.

## Native evidence

- 52 EditMode tests passed, including decimal grammar, normalization, signed values, division by zero, extreme inputs, nested arithmetic, precision, and whole-program constraints.
- 43 PlayMode tests passed, including existing mechanics/UI and new commit/restoration, whole-paste handling, shared-mass commit timing, invalid draft gating, invalid-template placement, immediate velocity/kick clamping, red warning/fade/reset, pre-integration displacement bounds, and extreme opposing gravity/acceleration cancellation.
- Layout verification passed: 64 migrated asset identities, 139 unique asset GUIDs, four scenes, five assemblies, and local documentation links.

Results: [EditMode](../results/editmode-results.json), [PlayMode](../results/playmode-results.json), [layout](../results/layout-results.json).

## Reproduced issues corrected during implementation

The initial engine-only speed constraint allowed a high-acceleration ball to travel 5.596 m to a wall during a 0.02 s step with a configured 20 m/s cap. The final code bounds predicted velocity before integration. The regression now verifies at most 0.401 m movement for that step. Extreme opposing contributions use the same pre-integration path without emitting a warning when their net effect needs no cap.

A stale UI field could overwrite a model value changed directly by code when Run committed all fields. Editors now track actual user edits rather than inferring a draft from differences against the model.

Range feedback initially wrapped inside a narrow operand column. It now uses the equation width and names the variable. Equation continuation indentation also accounts for the actual output/equal-sign width, keeping inputs on the right.

A final browser Retry check found that feedback creation skipped inactive parent cards when both sidebars were collapsed. The lookup now includes inactive parents, and the collapsed-panel native regression rebuilds a complete three-command program with both panels closed.

The browser helper previously pressed Tab, which leaves Unity's default input focused. Verification now uses the approved Enter commit interaction and a real pointer click to test cancellation of Run after invalid focus loss.

## Web and visual evidence

Final Web build `379f78e8894c4579a85172cf34c39602` succeeded. All 27 browser check groups passed with no application errors. The suite uses real keyboard and clipboard input, pointer-based invalid-commit cancellation, block dragging, scene navigation, collapsed-panel Retry, paused long-program scrolling, and desktop layouts at 1280×720 and 1920×1080.

The speed-warning capture records an unchanged acceleration input of 1000000, finite physical state, and actual speed below the 100 m/s cap. The exact red warning is readable in the play area. Native coverage additionally verifies the one-second hold, one-second fade, and Reset clearing.

- [Web build result](../results/web-build-result.json)
- [Browser results](../results/web-results.json)
- [Red speed warning](../screenshots/speed-cap-warning.png) and [captured state](../results/speed-cap-warning.json)
- [Nested field feedback](../screenshots/pass2-positive-error.png)

The interrupted earlier browser process and pre-fix Retry failure were not counted as passing runs. The final clean run includes the corrected Retry sequence. Chromium emitted a software-renderer cleanup message after the passing suite and exited with code zero; the application error list is empty.

No approved validation behavior is deferred. Mobile and IME input were not part of this desktop verification matrix.

## Independent review

The independent reviewer rated the implementation **9.3/10**, with no blocking defects and no substantive missing approved features from pass 1 as revised by later contracts or the numeric-validation contract. See the [adversarial review](input-validation-adversarial-review.md).
