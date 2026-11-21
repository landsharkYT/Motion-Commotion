# Final independent adversarial review

Reviewed 2026-11-20 against the entire original `pass1spec.md`, using the adversarial rubric and code-quality lens. The first review remains archived unchanged in `adversarial-review-round1.md`; the preceding passing re-review is archived in `adversarial-review-round2.md`.

Score: **9.4/10**. User gate: **PASS**.

**No substantive missing features from pass1spec.md.**

Both first-round blockers and the subsequent minor numeric-display finding are resolved in the inspected source and final rebuilt Web player. I found no remaining substantive omission, material physics defect, architectural blocker, or actionable finding. This verdict applies to the current frozen implementation, within its documented prototype operating limits and desktop scope.

## First-round findings resolved

### P1: Unsafe finite kick no longer reaches the body

I inspected `Core/PhysicsLimits.cs`, the complete program validator, session preflight, physics adapter checks, and telemetry bridge. The fix establishes conservative supported physical ranges before execution, including cumulative kick velocity-change magnitudes. It addresses the physical boundary rather than merely suppressing the telemetry exception.

I independently reran the real browser reproduction with mouse and keyboard events: Direction 0, Force, mass `0.0001` kg, acceleration `1e42` m/s². The current player remains Ready, shows the kick-limit explanation, disables Run, retains `1e42` for correction, and reports a finite physical state. No command executes and no browser error occurs.

I also reran the cumulative case. One acceleration of `60000` m/s² with mass 1 kg is accepted. Two such kicks exceed the documented 10000 m/s total velocity-change budget and disable Run with a combined-kicks explanation. This closes the obvious bypass through individually valid commands.

The new Play Mode test `UnsafeKickRejectsWholeProgramBeforeGravityChanges` checks rejection even when a gravity command precedes the unsafe kick. I inspected its source and its passing result in the current raw result JSON. I did not independently run Unity tests.

The adapter checks finite physical state after impulses and around manual stepping. The session can stop and offer Reset if that boundary fails. Development telemetry sanitizes measurement numbers while separately reporting `physicalStateFinite`; the JavaScript bridge records parse errors without throwing into Unity's frame loop. These are reasonable final failure paths alongside prevalidation.

The additional operating limits are explicitly documented in `coverage.md` and linked from README. They preserve the agreed formulas, scene responsibilities, and fixed 0.1-second force-to-impulse conversion. A bounded prototype physical range is appropriate; arbitrary symbolic or unbounded numeric support was never required.

### P2: Locked workspaces remain scrollable

Both workspace gates retain raycast delivery while disabling Selectable interaction. Drag starts, drops, structural edits, and text mutations still check `session.Editable`.

I independently reran the actual browser wheel interaction with the nested Direction/Force/Wait example. While paused, the previously offscreen Wait scrolls into view. The input remains disabled; program count, simulation time, and ball position remain unchanged. Resume then completes at the expected simulated time. The current paused screenshot visibly shows the highlighted Wait and its duration.

I found no recurrence of the previously suspected controls-rendering corruption in either round. The first-round confirmed issue was the inaccessible offscreen content, and that is fixed.

## P3 resolved: small values remain visible

The final display-only change uses invariant `G5` formatting in the HUD, formula previews, timestamps, and action feedback. Stored program and physics values remain unchanged. The HUD exposes its actual measurement text through a read-only property for development assertions; it does not introduce another authoritative value or a command-injection route.

I independently tested the final Web player through mouse and keyboard input with mass `0.0001` kg and acceleration `0.001` m/s². The HUD showed `Mass  0.0001 kg`, and I visually inspected the fresh screenshot showing the force formula result `1E-07 N`. Running that program produced an actual vx of approximately `0.0000999999975` m/s, consistent with the unchanged physical calculation.

After Reset and Clear, I ran a standalone Weight command with g = `0.0001`. The completed HUD showed both mass and gravity as `0.0001`, and the visible action feedback retained the small nonzero gravity. The formula preview showed `1E-08 N`. The focused final browser run completed with no captured errors or telemetry parse-error flag. Fresh screenshots and telemetry are under `/tmp/motion-final/`.

The current canonical extreme-input screenshot also visibly shows the small physical mass and the large force result in scientific notation. I inspected its raw JSON measurement text and the new native `SmallPhysicalValuesRemainVisibleInHudAndFormulaPreview` test and passing recorded result. The P3 finding is closed.

## Full acceptance assessment

| Specification area | Current assessment and supporting evidence |
| --- | --- |
| Scope and exclusions, sections 1–2 | Complete. The foundation contains the required mechanics and assets. No authored levels, functional editor, rearrangement, derivatives, narrative, scoring, persistence, or public hosting were added. These exclusions are not missing features. |
| Scenes and lifecycle, section 3 | Title, InGame, Editor, and Sandbox exist. InGame and Sandbox share Gameplay.prefab and local serialized composition. Editor is only EditorPlaceholder. Five unavailable slots, demo navigation, and fresh sessions passed my browser reruns. Direct Sandbox entry and cleanup are covered by the inspected Play Mode tests and their recorded results. |
| Presentation and desktop interaction, section 4 | Raw three-column workspace, gray physical ball, primitive fixture, labels, units, scroll areas, disabled pencil-with-sparkles, and resized desktop layout are present. Current and fresh screenshots inspected. Paused scrolling now passes. |
| Program model and editing, section 5 | Stable identities and ordered model are separate from view hierarchy. Actual browser tests passed palette creation, insertion/reorder, removal without deleting neighbors, numeric editing, nesting, restoration to literal, and rejected-drop preservation. Self/descendant rejection is in the drop path. |
| Formula catalog and quantities, section 6 | All five formulas, fixed outputs, typed compatible slots, pure evaluation, and visible constant-acceleration assumption are implemented. Actual formula calculations, incompatible quantities, cycles, and invalid intervals are covered in inspected core tests. |
| Force, weight, and time, section 7 | Real Rigidbody2D impulses use the visible 0.1-second convention. Core runner drains zero-time commands together and advances fixed-step waits with upward rounding. Weight execution is separate from value evaluation. Browser Pause, Resume, timed completion, and standalone Weight checks passed. An additional independent nested-Weight probe passed as detailed below. |
| Physical ball and experiment, section 8 | Gray dynamic body and matching circular geometry/collider, shared physical mass, separate active gravity, reusable primitive colliders, zero-friction/bounce defaults, and Inspector settings are present. Browser mass changes and physics telemetry agree within float precision. |
| Session behavior and feedback, section 9 | Explicit states, full reset snapshot, retained program, Clear semantics, measurements, bounded kick feedback, active highlighting, and fresh scene sessions implemented. Browser rerun passed Pause/Resume, Reset of physical and rendered pose, retained edited mass and commands, Clear, and navigation. |
| Validation and error handling, section 10 | Whole-program validation covers malformed/nonfinite numbers, quantity mismatch, cycles, missing pairs, intervals, signed-force rules, and supported physical limits. Both first-round failure cases now pass targeted actual-player regressions. Invalid drafts remain editable. Runtime finite-state failure has a stop and Reset path. |
| Architecture and reusable assets, section 11 | Core remains Unity-independent and enforced by asmdef. Physics, session, workspace, HUD, and navigation retain clear responsibilities. Editor-only composition/build code remains isolated. Shared serialized dependencies and session instances avoid copied gameplay implementations or shared-asset mutation. No new structural blocker found. |
| Verification, delivery, and handoff, sections 12–15 | Current raw results show 26 Edit Mode, 10 Play Mode, and 18 browser checks passed. I inspected test source, detailed result files, build result/output, README, coverage, implementation report, and screenshots. The preceding re-review independently reran all 17 then-current browser checks and an extra nested-Weight probe; the final build passed the additional focused small-number browser run described above. Web build is at Builds/Web, with repeatable local verification commands documented. |
| Implementation defaults, section 16 | Declared step, wait rounding, Completed freeze, empty program, Reset-before-editing, Clear, angle execution, gravity direction, shared mass, default force composition, and recent feedback match the foundation. Conservative numeric operating limits are attributed as prototype constraints rather than new user requirements. |

## What I independently verified across the reviews

The original review read the complete implementation and test sources, assembly definitions, scene/prefab composition, settings, verification scripts, and handoff against the full specification. This re-review reread the revised validator, new physics-limit definitions, session, body/controller, workspace gates and mutation guards, telemetry and bridge, changed test cases, and current raw results. I rechecked the core and Editor assembly boundaries and current build evidence.

In the preceding re-review, I ran the then-current browser suite against the rebuilt `Builds/Web` with Chromium and software WebGL. All 17 checks passed independently. Outputs were isolated under `/tmp/motion-review2/`; canonical Verification evidence was not overwritten. That complete rerun preceded the final display-only change.

I added one independent probe beyond that suite: drag W = mg into an ordinary Force command's force-valued slot, set its g input to 2, then Run with mass 1 kg and direction 0. The actual player completed at t = 0 with vx approximately `0.200000003` m/s while active gravity remained approximately `9.81000042` m/s². This proves the nested weight supplied a kick magnitude without executing a gravity command. Those 18 browser checks together passed with no captured browser errors or telemetry parse-error flag during the preceding re-review.

I inspected canonical paused, extreme-input, cumulative-input, reset, and resized visual evidence, as well as fresh screenshots during the browser rerun. The earlier reset pose fix remains covered by physical and rendered-position assertions.

For the final targeted recheck, I inspected the changed formatting sites, read-only HUD telemetry exposure, native display regression, current raw 26/10/18 result files, and final completed build job `a8f3269ba2af48c59374ed47dfad7f31`. I independently ran the final player through the focused small-mass, small-formula, actual tiny-kick, and small-gravity checks above, then inspected the resulting screenshots. This final run did not repeat the entire browser suite because the remaining change only affects presentation.

I did not independently rerun the Unity editor tests or rebuild the player. Their current detailed recorded results were inspected, not treated as my own executions. I did not test another browser engine or claim bit-for-bit physics portability. No source, specification, scene, prefab, setting, or requirement was changed during review.

The implementation is ready to hand off as the agreed raw mechanics foundation. No review findings remain open.
