# Independent adversarial review

Reviewed 2026-11-19 against the entire `pass1spec.md`, using the adversarial rubric and code-quality lens. This assessment applies to the implementation and Web build present before the fixes requested below.

Score: **8.0/10**. User gate: **FAIL**.

The implementation contains the requested feature categories and respects the deliberately raw proof-of-mechanics scope. However, numeric validation accepts a program that corrupts the physical body and halts the delivered Web player. Long programs also become impossible to inspect while execution is locked. These are substantive incomplete behaviors under sections 4.4, 9, and 10. I cannot give the requested unconditional conclusion that no substantive features are missing from the specification while those behaviors remain unresolved.

## Findings

### P1: Accepted finite input can overflow the body's velocity and halt the Web player

Locations: `Assets/MotionCommotion/Core/Programs.cs:88`, `Runtime/Session/MotionCommotionSession.cs:65`, `Runtime/Physics/BallBody.cs:19`, and `Runtime/UI/Plugins/Verification.jslib:3`.

Reproduce through the actual browser UI:

1. Open the mechanics demo and add Direction followed by Force.
2. Leave direction at 0 degrees. Set the force formula's shared mass to `0.0001` kg and acceleration to `1e42` m/s².
3. Observe that the program has no validation message and Run is enabled.
4. Press Run.

I reproduced this with Playwright mouse and keyboard events against `Builds/Web`. The inputs calculate approximately `1e38` N, which passes the validator's `float.MaxValue` comparison. The adapter applies approximately `1e37` N·s to a roughly `1e-4` kg body. The implied velocity change is approximately `1e41` m/s, outside the body's float representation.

The browser error proves the physical state became nonfinite: telemetry JSON contained `"vx":Infinity`. Its `JSON.parse` raised `SyntaxError: Unexpected token 'I'`. Unity then logged `Uncaught exception from main loop:` and `Halting program.` No useful in-application validation rejected this input before execution. Existing exception handling around command execution does not catch a silently nonfinite physics result or the later telemetry exception.

The root issue is checking representability of force in isolation, without establishing that the resulting physical operation is supported. The bridge exception is a consequence, not the sole defect. A bridge-only fix would leave a corrupted experiment.

Reject unsupported physical actions during full-program validation, before changing gravity or applying any kick. A documented, conservative supported operational range is acceptable for this teaching prototype. It should cover mass, forces or velocity changes, gravity, and cumulative effects rather than merely checking that an isolated force casts to float. Detect nonfinite physical state at the adapter boundary as a final runtime failure path, and keep telemetry from terminating the application. Regression checks should cover this exact input and more than one individually valid kick whose sum exceeds the supported range. Preserve the invalid draft and show a useful block-specific message.

This blocks the required safe validation and runtime-failure behavior in section 10. It does not call for an unbounded numeric engine.

### P2: Locking editing disables scrolling, hiding the current Wait and later commands

Locations: `Assets/MotionCommotion/Runtime/UI/WorkspaceEditingGate.cs:11` and `Runtime/UI/BlockWorkspace.cs:145`.

Use the normal nested example from the browser verifier: Direction, Force with Average acceleration nested into acceleration, then Wait 3 seconds. At 1600 × 1100, Run and then Pause with the program scrolled to the top. The Wait is below the visible program area. Attempt to scroll down with the pointer over the program.

I sent a 700-pixel downward wheel event and waited 600 ms. The Wait input remained at y = `1063.338`, unchanged before and after scrolling; the viewport ends near y = `979`. The active command remains inaccessible. The same gate applies in Running and Completed.

`WorkspaceEditingGate` sets the enclosing CanvasGroup's `blocksRaycasts` to false whenever the session is not editable. This removes the ScrollRect from pointer event delivery along with edit controls. Disabling editing is correct; disabling observation of the rest of the program is unnecessary and conflicts with the long-program scrolling requirement in section 4.4.

Keep scroll input available while preventing text edits, drag starts, and removal. Verify that a paused user can scroll to the active Wait while program content and physics remain unchanged. The existing handlers already guard their mutations with `session.Editable`; use a single clear edit policy instead of suppressing the whole scroll container's events.

## Acceptance coverage

| Specification area | Assessment and evidence |
| --- | --- |
| Scope and exclusions, sections 1–2 | Correct. No authored levels, functional editor, rearrangement, derivatives, narrative, persistence, or public hosting found. Those are deferred, not omissions. |
| Four scenes and lifecycle, section 3 | All four scenes exist. InGame and Sandbox reference the same Gameplay prefab. Editor contains EditorPlaceholder. Title offers five disabled slots and the mechanics demo. Navigation and fresh sessions passed my browser rerun. Direct Unity scene entry is supported by serialized composition and the recorded Sandbox Play Mode setup; I did not independently open Unity. |
| Presentation and desktop behavior, section 4 | Raw three-column layout, primitive gray ball, readable fields, scroll areas, disabled gray pencil-with-sparkles, and usable 1200 × 850 layout inspected. Paused workspace scrolling fails as above. |
| Program editing, section 5 | Stable IDs and authoritative ordered model are present. Actual browser rerun passed creation, insertion/reorder, numeric edits, removal without deleting neighbors, compatible nesting, restoration to literal, and incompatible-drop preservation. Self/descendant rejection is implemented in the drop path. |
| Five formulas and quantities, section 6 | Read all definitions and evaluator code. Explicit units, fixed outputs, compatible nesting, signed intermediate values, positive intervals, nonnegative motion time, and visible constant-acceleration assumption are implemented. Existing recorded Edit Mode results exercise all five formulas and cycles. I did not independently rerun Unity tests. |
| Force, weight, and timing, section 7 | Core runner uses explicit actions, 0.1-second kick conversion, zero-time command draining, upward-rounded fixed waits, and Completed freeze. Physics adapter uses Rigidbody2D impulses. Standalone Weight is separate from pure expression evaluation. Browser rerun passed standalone gravity change without a kick, timed completion, and Pause/Resume. Extreme accepted kicks fail as above. |
| Ball, mass, gravity, and geometry, section 8 | Real dynamic body, matching circle collider and mesh, shared session mass, independent active gravity, zero-contact defaults, and primitive prefab colliders inspected. Browser mass editing changed actual body mass. Finite supported mass bounds are documented implementation limits. |
| Session controls and feedback, section 9 | Explicit state enum, full snapshot, retained program, bounded feedback, units, clock, active highlighting, Reset and Clear are implemented. Browser rerun verified frozen position/time, Resume, restored physics and rendered pose, retained edited mass, Clear, and fresh scene entry. Locked scrolling prevents inspecting offscreen execution state. |
| Validation and runtime failures, section 10 | Ordinary malformed numbers, missing pairs, incompatible quantities, cycles, negative final force, and invalid intervals are handled. Physical numeric safety is incomplete and reproduced a player halt. |
| Architecture and assets, section 11 | Core asmdef enforces no Unity references. Unity-facing physics, session, UI, and navigation responsibilities are separated. Editor-only composition/build helper is isolated. Shared serialized references and Inspector assets are present. No giant manager, needless framework, mutable singleton, or copied gameplay implementation found. |
| Verification and handoff, sections 12–15 | Read actual tests, result JSON, browser script, server/build helpers, README, coverage map, implementation report, build output, and canonical screenshots. Independently reran the 14 browser interactions with temporary outputs; all baseline assertions passed before adversarial probes. Recorded Unity results show 23 Edit Mode and 8 Play Mode passes; these were inspected, not independently executed. The adversarial overflow and paused-scroll paths were absent from those tests. |
| Defaults, section 16 | Declared defaults broadly match the implementation. Operational numeric limits may be explicitly tightened without adding scope. Fixed kick conversion and formula/scene responsibilities must remain intact. |

## Visual concern and verification limits

I inspected the canonical paused, reset, and resized images and a fresh paused screenshot from my rerun. I did not reproduce controls rendering at positions inconsistent with telemetry. The visible nested inputs line up, and the Wait connector is clipped at the viewport's bottom. I do not flag a rendering corruption based on the earlier suspicion. The independently reproduced problem is that scrolling cannot reveal that clipped Wait while paused.

The complete implementation source read comprised all 20 C# files under Core, Runtime, and Editor, plus both test source files and assembly definitions. I checked the scene composition and relevant serialized prefab/settings values against the generator, including real body properties and shared prefab references. I inspected the supporting build template and telemetry bridge.

Browser verification used Chromium with software WebGL and actual UI events. Read-only telemetry supplied measurements and coordinates. Temporary probe code and screenshots were kept under `/tmp/motion-review*`; existing Verification evidence was not overwritten. The baseline browser suite completed successfully, then the additional overflow probe produced the errors described above. Its final script success flag refers to the baseline checks only and must not be treated as an adversarial pass.

I did not rerun the Unity editor tests, rebuild the player, test a second browser engine, or claim bit-for-bit physics portability. No implementation, asset, specification, or requirement was changed during this review.

The code structure is appropriate for the raw foundation and does not need a redesign to address these findings. Fix the numeric boundary and preserve observation while locking edits, then repeat the targeted regressions and review the resulting build before applying the user's gate.
