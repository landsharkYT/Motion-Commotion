# Numeric input validation audit

Status: design approved; implemented and verified with 52 EditMode tests, 43 PlayMode tests, and 27 browser checks. The approved contract below supersedes the baseline audit and rejected proposals.

## Request

Prevent invalid numeric values, division by zero, negative mass or weight, unreasonable ranges, empty committed fields, and nonnumeric text. Provide concise feedback. This revisits the existing policy that preserves invalid text drafts.

## Baseline audit before this change

| Area | Behavior before this change |
| --- | --- |
| Literal parsing | Finite invariant-culture numbers required. Empty text, letters, NaN, infinity, and overflow are rejected before execution. |
| Editing | Standard InputField accepts arbitrary typed or pasted text. No numeric character filter or length limit. Invalid drafts remain in the field. |
| Numeric format | NumberStyles.Float allows decimal points, signs, scientific notation, and surrounding whitespace. Commas are not accepted. |
| Divisors | Both Δt denominator fields must be strictly positive. |
| Time | Position t and Wait allow zero. Total Wait duration is at most 3600 seconds. |
| Mass | Shared physical mass is constrained to 0.0001–1000000 kg. Invalid text does not change the last valid physical mass. |
| Gravity and weight | g cannot be negative; standalone g is at most 10000 m/s². Positive mass and nonnegative g make valid W nonnegative. |
| Force | Standalone force magnitude cannot be negative. Individual and combined kicks have a 10000 m/s velocity-change budget. |
| New motion actions | Signed velocity and acceleration outputs are at most 10000 in magnitude; position is at most 1000000 m, with destination-coordinate checks. |
| Nested expressions | Finite numbers, compatible units, signs, and cycles are checked. Standalone output caps are not automatically applied to intermediate values. |
| Palette templates | Unused nonmass templates can remain invalid without numeric feedback. Their text is copied into Active on placement, where it is validated. Mass is global and validates immediately. |
| Feedback | Active commands show short errors and block Run. Most nested failures identify the enclosing command rather than the specific offending field. Range messages do not identify the allowed bounds. |

The number 1000 is not universally invalid under the current rules. Its unit and use matter. Current operating limits are broader than the proposed learner-facing limits.

## Existing decisions to preserve

The [equation action contract](standalone-equation-actions.md) allows signed velocity, acceleration, and position values. Negative values reverse Direction. Zero commanded acceleration and zero gravity are valid. Mass and denominator Δt must be greater than zero. Formula time inputs do not advance the clock; Wait alone advances time.

Numeric validation must cover typed text, pasted text, formula inputs, nested results, palette templates, and pre-execution program validation. A keyboard filter alone cannot replace validation at the domain boundary.

## First decision frontier

- Numeric grammar, including decimal points, meaningful minus signs, scientific notation, separators, whitespace, and units.
- Temporary incomplete text while editing versus empty or invalid committed fields.
- Per-quantity limits and checks on computed results and combined physical effects, instead of a universal arbitrary cap.
- Field-level feedback, including the allowed range.
- Palette validation and whether an unused invalid template blocks a valid Active program.

Exact numeric bounds, precision, commit/reversion behavior, and Run/drag behavior after an invalid edit depend on these decisions and belong to the next interview round.

## First-round approvals

The user accepted all five recommendations:

1. Literal input uses ordinary decimal notation. A leading minus is permitted only for signed quantities. Letters, scientific notation, commas, spaces, and unit suffixes are rejected.
2. Incomplete text may exist while a field is focused. Leaving an empty or invalid field restores its last valid value with feedback. Invalid values cannot become committed inputs.
3. Limits depend on physical quantity. Calculated results and combined physical effects are validated as well as entered numbers.
4. Feedback identifies the offending field, including nested inputs, and gives the problem or allowed range. Invalid Active inputs disable Run.
5. Palette templates also validate and cannot be placed while invalid. An unused invalid template does not block a valid Active program. Invalid shared mass blocks execution because mass is global.

## Proposed teaching limits

These are recommendations for the next interview round, not implemented or approved yet. Existing wider engine safeguards remain as backstops.

| Quantity | Proposed inclusive range |
| --- | --- |
| Mass m | 0.1–100 kg |
| Gravity g | 0–30 m/s² |
| Displacement, initial position, and calculated position | −100–100 m |
| Velocity quantities and calculated velocity | −100–100 m/s |
| Acceleration quantities and calculated acceleration | −100–100 m/s² |
| Force and weight results, including a literal force input | 0–1000 N |
| Denominator Δt | 0.01–60 s |
| Position formula t and each Wait | 0–60 s |
| Total Wait duration | 0–120 s |
| Direction | 0–360°, with 360° normalized to 0° |

Sign constraints retain their semantic context: mass and divisors are strictly positive, gravity/weight/force are nonnegative, and signed motion values remain supported. Valid individual inputs can still produce an invalid result, which must receive equation-level feedback.

Proposed numeric editing rules are up to three fractional decimal digits and twelve total characters, with ordinary normalization on commit. Calculated results retain full numerical precision; the text-entry precision limit does not round formula arithmetic.

An invalid paste is rejected as a whole rather than stripping characters into a different number. Changes commit on Enter or focus loss. If an invalid edit ends because the player clicks Run or starts a drag, that action is cancelled, the old value is restored, and the player must try the action again.

Proposed simulation guard: exceeding 100 m/s actual speed or 100 m distance from the captured starting origin pauses with feedback and requires Reset. The guard does not silently clamp the orb's motion. This addresses combinations such as sustained acceleration across a long Wait even when each individual input is within range.

## Second decision frontier

- Accept or revise the proposed per-quantity limits.
- Text-entry precision, length, and normalization.
- Whole-paste rejection.
- Commit timing and whether live physical settings change only on commit.
- Cancellation of Run or drag that ends an invalid edit.
- Runtime motion limits and pause/reset behavior.

## Second-round response

The user approved questions 7–10:

- At most three fractional digits and twelve total characters. Normalize ordinary decimal text on commit, retaining full precision for calculated results.
- Reject invalid pasted text as a whole, preserving the previous text.
- Commit on Enter or focus loss. Shared mass and linked values update only after a successful commit.
- Cancel Run or drag when that action ends an invalid edit. Restore the previous committed value, show feedback, and require another attempt.

The user did not approve the proposed small teaching ranges or the 100 m/s and 100 m runtime pause thresholds. They want exaggerated numeric experiments that children can enjoy. Revised range and clamping behavior remain open. The proposed teaching-limit table above is not an approved contract.

## Approved implementation contract

The user approved a revised approach: allow exaggerated inputs, cap excessive physical movement while play continues, and show a red warning. They approved the warning text and instructed implementation.

### Decimal editing

`NumericRules` is the shared domain policy. Literal inputs use ordinary decimal notation, at most twelve characters and three fractional digits, with magnitude at most 1000000. Leading minus signs apply only to signed quantities. Reject scientific notation, plus signs, whitespace, separators, unit suffixes, NaN, infinity, and letters. Temporary empty text, a decimal point, or a lone minus may exist while editing; none can commit. Pasted invalid text is rejected as a whole, preserving the previous draft. Do not strip invalid characters into a different number.

Enter or focus loss commits valid input. Normalize leading zeros, `.5`, trailing decimal points, and negative zero. An invalid commit restores the previous committed value and retains red field feedback. Linked inputs and shared physical mass change on commit. Formula arithmetic retains full precision.

An invalid Active draft or any shared-mass draft blocks Run. Invalid unused palette drafts do not block a valid program. Invalid palette templates cannot be placed. Run or dragging that ends an invalid edit is cancelled; the player must retry after correction/restoration. Domain validation still rejects invalid raw model data independently of UI filtering.

### Numeric bounds

| Value | Implemented bound |
| --- | --- |
| Literal magnitude | At most 1000000 |
| Shared mass, including nested mass operands | 0.001 to 1000000 kg |
| Divisor Δt | 0.001 to 3600 seconds |
| Position t / individual Wait | 0 to 3600 seconds |
| Combined Wait | At most 3600 seconds |
| Signed motion operands | Both signs supported within literal bounds |
| Gravity, force, and weight | Nonnegative |
| Calculated results | Finite, magnitude at most 10¹⁸; no entry-precision rounding |
| Standalone scalar position and destination coordinates | Existing 1000000 m world bound |
| Direction | Signed decimal input within literal bounds, normalized modulo 360 |

These are prototype safeguards, not physical laws. Limits for inputs, results, world coordinates, and actual speed serve different purposes. For example, 1000 is a valid input; large finite velocity results can execute and trigger motion clamping. The existing total-Wait bound prevents an indefinitely long program. Invalid equation results receive equation feedback rather than changing a valid operand automatically.

### Physical speed cap and red warning

`WorkspaceSettings.MaximumSpeed` configures actual vector speed from 1 to 100 m/s, default 100, preserving the project's existing Unity engine speed limit. Cap magnitude while preserving direction. Clamp instantaneous Velocity and Force actions even without a Wait. During Wait, predict velocity from commanded acceleration and gravity before integration. If it exceeds the cap, supply the bounded velocity to Unity and suppress those acceleration contributions for that step. Use the same pre-integration path when individual contributions are extreme even if they cancel, avoiding large force arithmetic inside the engine; warn only when net speed actually needs capping. Unity still resolves collisions. Restore gravity scale afterward. Retain Unity's translation constraint as a secondary safeguard; it did not by itself enforce the intended within-step movement bound in the installed editor. Restore the previous global Unity constraint on scene exit.

Show this red, noninteractive warning in the play area:

> Speed limit reached. Movement capped at {limit} m/s.

The warning remains opaque while the cap is repeatedly reached. After the last cap event, hold it for one second, then fade it over one second using unscaled time. Reset clears it. Keep playing, retain entered numbers and calculated results, and do not pause merely because speed was capped. Actual ball measurements show the capped speed. Invalid/nonfinite physics and blocked Position destinations retain their existing runtime-error behavior.

Unity documents the engine constraint in [Physics2D.maxTranslationSpeed](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/physics2d/maxtranslationspeed).

### Ownership

- `NumericRules` parses and checks raw numeric values at the domain boundary. `FormulaEvaluator` checks nested operands and calculated results; `ProgramValidator` checks whole-program constraints.
- `NumericEditor` owns UI draft/commit/restoration feedback. `DecimalInputField` validates an entire paste before Unity appends its individual characters.
- `BlockWorkspace` owns shared bindings, palette feedback, placement gating, and invalid-edit gesture cancellation.
- `BallBody` owns immediate speed clamping and cap-event time. `SimulationController` owns the temporary global physics setting and per-step checks. `GameplayScreen` renders the warning.

The small teaching-limit proposal above was rejected. Do not reinstate its 100 m distance pause rule or reject large kicks merely because their uncapped velocity would exceed 10000 m/s.
