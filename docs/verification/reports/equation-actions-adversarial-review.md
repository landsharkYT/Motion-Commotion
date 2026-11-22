# Equation actions adversarial review

Rating: 9.2/10. Final acceptance: pass.

Substantive missing features: no. I found no substantive missing feature in the approved standalone-equation, return-to-Blocks deletion, whole-command drag, or preserved compact-interface scope. I found no blocking regression in the applicable pass 1/pass 2 behavior. The score clears the required 9/10 gate.

## Scope and authority

I used `docs/design/standalone-equation-actions.md` as the current behavior contract, including both rounds of approved physical decisions. The compact layout, block-presentation, and per-equation pencil decisions remain applicable where that contract does not supersede them. The GDD and `.gitignore` edits are outside this review. Deferred equation rearrangement, authored levels, progression, sound, and the level editor are not missing features in this scope.

I read the command model, validator, runner, physics limits, physical snapshot, BallBody, SimulationController, session execution/reset, landing evaluation, drag/drop ownership, connector snapping, equation rendering, current regression tests, and browser interaction assertions. I inspected the actual `equation-stack-velocity.png`, `equation-stack-acceleration.png`, `equation-stack-position.png`, `block-return-delete.png`, `compact-default-1280.png`, `compact-default-1920.png`, `compact-nested.png`, and `compact-long-value.png` captures. This review did not run or modify the shared Unity editor and made no source changes.

## Correctness assessment

The compiler maps each formula to its distinct command kind and validates all commands before execution. Velocity, Acceleration, Position, and Force require an immediately preceding Direction; Direction itself must precede one directed equation. Weight remains directionless and downward. New signed outputs retain their sign through the directed-vector conversion, and their scalar limits are checked before execution. Position also checks its resulting world coordinates at runtime.

The runner emits each action at its current tick without consuming formula time. Only Wait requests a physics step. Velocity replaces the entire velocity. Commanded acceleration is separate from gravity, is applied as mass times acceleration before every physics step, survives the other actions, is replaced by a later acceleration assignment, and is disabled by zero. Captured/reset physical state includes commanded acceleration.

Position uses the snapshot captured immediately before Run, so repeated placements do not accumulate from the prior destination. It leaves velocity, angular velocity, and commanded acceleration intact. The destination check tests solid colliders using the ball's collision mask and explicit ignored collisions, permits the configured contact margin, and restores the previous position before throwing on rejected penetration. Rejection pauses with an error and prevents Resume until Reset. Placement does not simulate the intervening path. Clearing landing-approach history prevents an earlier above-target approach from being reused after a teleport; a subsequent physics step must establish a valid landing.

Root equation drags resolve to their owning command and clone that whole card for the ghost. Returning a command removes the command and its expression tree. Returning a linked child formula clears only its source input. Palette returns preserve templates. All five palette equations now have stack connectors and instantiate the corresponding command at a valid snap. Active root drags cannot accidentally execute formula-slot replacement because they carry a command payload, while nested formulas remain pure compatible values.

The new runtime behavior does not add implicit elapsed time, reinterpret metres as Newtons, or make nested expressions execute actions. Existing Force impulse scaling, Weight gravity configuration, typed nesting, copied palette defaults, shared mass, linked time fields, single-command reordering, invalid-drop preservation, compact vertical-only sidebars, disabled equation pencils, and pass 2 outcome/navigation behavior remain represented by active regression coverage.

## Evidence

- `editmode-results.json` reports 36 passed, zero failed, skipped, or inconclusive.
- `playmode-results.json` reports 38 passed, zero failed, skipped, or inconclusive. The saved results include all-five actual snap placement, palette return deletion, nested-only return deletion, whole-command ghost ownership, ongoing acceleration and Reset, fixed-origin position, state preservation, blocked destinations, skipped path obstacles, landing-history reset, and later valid landing.
- `layout-results.json` reports passed.
- `web-build-result.json` reports successful completion of corrected build job `fdd0680a9bc040d28ad36ab48af23cca`.
- `web-results.json` contains `passed: true`, 27 interaction groups, and an empty errors array. The implementation owner confirmed the current browser process completed with exit code zero against the corrected build. I read the saved result directly.

The browser checker uses actual mouse and keyboard interaction with read-only telemetry. Its added checks place and run all three newly executable equations at zero time, return visible primary equations and commands to Blocks, observe ongoing acceleration through Wait and Reset, and reject a solid position destination without movement or elapsed time. The captures show joined command edges, readable white fields and notation, Equation Blocks naming, bounded cards, and an empty Active start connector after deletion. Additional native tests cover semantics that a single browser screenshot cannot establish.

## Nonblocking findings

1. Resolved during review, documentation. `CONTEXT.md` initially defined Direction as force-only. The owner corrected it to cover Velocity, Acceleration, Position, and Force with their angle convention. I read the correction directly.
2. Resolved during review, historical-document clarity. `docs/design/pass2spec.md` initially retained Force Blocks and first-three nonexecutable entries in its live-looking palette description and table despite supersession notices. The owner updated the heading, fixed vocabulary, per-equation pencil text, and action table to the current contract. I read those corrections directly.
3. Low severity, pre-existing cosmetic inconsistency. `BlockWorkspace.Snapping.cs` clones disabled equation pencils into the actual-card ghost, while the earlier pencil specification excludes transient previews. The prior compact-layout review already records this inconsistency. The ghost's CanvasGroup disables interaction and raycast blocking. It does not affect command ownership, deletion, nesting, or execution and is not a new substantive regression.

No high- or medium-severity defect was established.

## Evidence limits

I independently inspected source, saved results, and captures; I did not personally rerun the native or browser suites. The result artifacts support the reviewed behaviors but do not prove every possible floating-point draft, collider configuration, or pointer path. Angular velocity preservation follows the placement implementation, while the new placement test directly asserts linear velocity and commanded acceleration. The configured contact-margin rule has native coverage through the target-contact placement scenarios; it is not a promise of exact visible-surface separation for every custom collider.

The implementation meets the approved contract at 9.2/10 with no substantive missing features.
