# Pass 1 verification coverage

This map connects the acceptance requirements in [docs/design/pass1spec.md](../../design/pass1spec.md) to the implementation and repeatable checks. Test-result files and browser screenshots contain the observed outcomes.

| Requirement | Implementation | Verification |
| --- | --- | --- |
| Four scenes and navigation | Scenes under Assets/Scenes; SceneNavigator; persistent Title and Back button events | Core scene inventory check; Play Mode navigation check; browser Title and fresh-session checks |
| Shared InGame and Sandbox implementation | Both scenes instantiate Prefabs/Gameplay.prefab | Scene inspection; direct Sandbox loading in Play Mode checks |
| No authored levels or functional editor | Title has five unavailable slots; LevelEditor contains LevelEditorPlaceholder; gameplay has primitive test bounds | Asset inventory check; browser Title check; scene inspection |
| Raw three-column UI and primitive gray ball | Gameplay.prefab; BlockCard.prefab; geometry meshes and 2D colliders | Editor and browser screenshots |
| Drag, insert, reorder, remove, and edit | BlockWorkspace, BlockDrag, ProgramDrop | Browser interaction script |
| Compatible nested formulas and literal restoration | FormulaDrop and recursive expression views | Core quantity and cycle checks; browser compatible, incompatible, and removal checks |
| Five correct formulas with fixed outputs | Core/Formulas.cs and palette definition assets | Five formula calculation cases; browser expression UI |
| Editable physical mass and synchronized fields | Session.EditMass and workspace mass-field bindings | Play Mode impulse and reset checks; browser mass editing |
| Pair validation before execution | ProgramValidator compiles a complete snapshot | Adjacency, invalid-program, and snapshot checks; browser disabled Run |
| Single kicks with explicit units | ProgramRunner.KickEquivalentSeconds and BallBody.Kick | Real-body impulse tests; browser movement and feedback |
| Same-instant commands and time-advancing waits | ProgramRunner and SimulationController | Core timestamps, zero wait, rounded wait, and sub-step wait cases |
| Weight command versus nested value | Pure FormulaEvaluator; separate Gravity action | Play Mode weight and reset checks; browser standalone weight check |
| Fixed clock and Pause/Resume | Session state and manual physics controller | Play Mode and browser frozen-time and frozen-position checks |
| Reset retains program and restores edited configuration | PhysicalSnapshot and ResetExperiment | Play Mode and browser reset assertions for both physics position and rendered Transform |
| Replaced blocks stop rendering immediately | UiFactory.Clear disables old views before deferred destruction | Play Mode orphan-card check; browser screenshots |
| Clear and fresh sessions | Session.Clear and ordinary scene replacement | Browser Clear and Title round-trip checks |
| Disabled pencil-with-sparkles | EquationEditIcon geometry and disabled button | Browser disabled-control assertion; screenshot inspection |
| Independent core and clear script responsibilities | Core asmdef has noEngineReferences; Runtime folders separate physics, session, UI, navigation | Assembly compilation and source inspection |
| Inspector-editable reusable assets | Workspace and palette ScriptableObjects; gameplay, card, and shape prefabs | Asset inventory and persistent-input-reference regression check |
| Small nonzero measurements stay visible | Five-significant-digit HUD, formula, and action formatting | Play Mode small-value display check; browser HUD assertion at 0.0001 kg |
| Browser delivery and resize | Web template, build method, local server | Browser interaction suite at two desktop sizes |
| Lifecycle cleanup | SimulationController restores Unity globals; session unsubscribes events | Play Mode teardown assertions and repeated scene entry |

## Repeatable commands

Run `python3 Tools/verify-unity.py` with the project open in Unity. Run `npm --prefix Tools run verify:web` while `python3 Tools/serve-web.py` serves the development Web build. See [README.md](../../../README.md) for setup and build commands.

The browser script interacts with the rendered UI through mouse and keyboard events. Read-only development telemetry supplies measured values and UI coordinates for assertions. It does not expose a command injection path.

## Numeric limits

The prototype validates the complete program against conservative physical operating limits before applying any command. Invalid drafts remain editable and prevent Run. The core evaluator otherwise preserves full numeric precision, while displayed values are rounded for readability.

| Physical quantity | Supported operating range |
| --- | --- |
| Ball mass | 0.0001 through 1000000 kg |
| Sum of kick velocity-change magnitudes | At most 10000 m/s per program, calculated as the sum of force × 0.1 seconds / mass |
| Downward gravity | 0 through 10000 m/s² |
| Sum of requested waits | At most 3600 seconds per program |
| Starting velocity components | Within ±10000 m/s |
| Starting position components | Within ±1000000 m |
| Configured physics step | 0.001 through 1 second |

The kick budget is conservative and includes kicks that point in opposing directions. The physics adapter also detects nonfinite body state and stops the experiment with a Reset path. Development telemetry reports a physical-state validity flag and never emits nonfinite measurement numbers into JSON.

Waits use a configurable fixed step, initially 0.02 seconds. A positive wait shorter than a step still advances one step. Completion freezes the experiment; a Wait is required to observe motion after a kick.

## Adversarial regressions

The first independent review found an extreme finite kick that overflowed the body's velocity and a locked workspace that could not scroll. Targeted checks now exercise the exact extreme draft, cumulative individually valid kicks, rejection before an earlier gravity command can execute, and scrolling to an offscreen Wait while paused without changing the program or physical state. See the current test results and [review history](adversarial-review-round1.md).
