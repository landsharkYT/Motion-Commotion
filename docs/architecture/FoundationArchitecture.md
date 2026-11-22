# Motion Commotion foundation architecture

## Status

The architecture and scene behavior below are accepted for the runtime foundation. The four scenes are Title, InGame, LevelEditor, and Sandbox. The script names below describe responsibilities and can change during implementation.

## Script responsibilities

| Area | Scripts or types | Responsibility |
| --- | --- | --- |
| Program data | BlockProgram, Command, FormulaExpression | Ordered commands, nested formulas, and stable block identities. No scene objects or UI references. |
| Formula rules | FormulaEvaluator, ProgramValidator, Quantity | Numeric evaluation, compatible units, valid inputs, and direction-force pairing. Evaluation has no physics side effects. |
| Execution | ProgramRunner | Command order, wait durations, and simulation-time actions. No drag handlers, UI animation, or Rigidbody2D access. |
| Unity physics | BallBody, SimulationController | Apply impulses, change physical mass and gravity, step physics, pause, and reset. |
| Presentation | BlockWorkspace, BlockView, FormulaSlotView, SimulationHUD | Dragging, snapping, fields, command requests, and display of execution feedback. |
| Composition | MotionCommotionSession | Connect the program, runner, physics adapter, and UI at scene startup. Coordinate Run, Pause, Reset, and Clear. |

One responsibility can need several files. Closely related small types can stay together. Script count is not the measure of separation.

## Dependency direction

The plain C# core owns program data, formula rules, and command scheduling. It has no UnityEngine or UnityEditor references. Unity-facing scripts reference the core, not the reverse.

UI handlers edit program data or request session actions. They do not apply forces or advance the simulation. The runner emits actions for the physics adapter instead of reaching into a Rigidbody2D. Presentation observes execution results and cannot insert timing delays.

One session owns mutable runtime state. Mass fields refer to the same physical mass setting. Formula evaluation returns values without mutating that setting or gravity. Executing a weight command is a distinct operation from evaluating a nested weight formula.

## Unity practices

- Use assembly definitions to enforce a Unity-independent core and isolate tests. Keep runtime presentation and physics in distinct folders. Isolate future UnityEditor code in an Editor-only assembly when it exists.
- Use MonoBehaviours for scene components and Unity lifecycle integration. Keep arithmetic, validation, and scheduling in ordinary C# types.
- Use serialized references and one explicit startup composition point. Avoid global mutable singleton state and scene searches for dependencies.
- Keep definition assets separate from runtime instances. A ScriptableObject can hold authoring defaults, labels, and block definitions. The editable program and current experiment state belong to a session instance.
- Keep event subscriptions local to the owning object's lifecycle. Avoid a global event bus for this small runtime.
- Give one simulation controller ownership of physics stepping and time. Use fixed-size steps, independent of render framerate. Do not promise identical floating-point results across platforms.
- Provide Inspector-editable UI prefabs and settings. Teammates can change layout, appearance, labels, defaults, and palette availability without editing scripts.

## Formula catalog

The initial catalog has five base formulas: average velocity = Δs / Δt, average acceleration = Δv / Δt, s = s₀ + v₀t + 0.5at², F = ma, and W = mg. The motion equation assumes constant acceleration. Each formula and supported rearranged variant has an explicit implementation, fixed left-side output, defined right-side inputs, and physical quantities. Adding behavior requires a small C# change. Presentation and defaults remain configurable.

There is no symbolic rearrangement algorithm or general expression parser. Future rearrangement interactions select from the curated variants. Coding those variants does not bring the rearrangement UI into the current pass.

The five equation families in the scope are the content boundary, not a requirement to implement every equation shown in the screenshots. The [foundation scope](../design/FoundationScope.md#confirmed-initial-formula-catalog) records the accepted initial catalog and output units.

## Simulation and reset

One controller manually advances physics in fixed simulation steps and coordinates the runner against the same simulation clock. UI animations cannot advance program time. Consecutive kicks execute before physics advances when no Wait separates them.

Run captures the edited starting physical configuration. Reset restores that configuration and the physical starting state and clock while retaining the program. Editing mass to 3 kg before Run therefore makes Reset restore 3 kg, not the factory default of 1 kg. Factory defaults are distinct from Reset.

## Verification boundaries

Core tests exercise formulas, quantities, invalid programs, same-instant command order, and waits without loading a scene. Unity integration tests exercise impulses, mass, gravity, pause, and reset against real physics bodies. Runtime interaction checks exercise dragging, nesting, validation feedback, and browser behavior.

## Scene plan

| Scene | Purpose |
| --- | --- |
| Title | Title screen and level selection. |
| InGame | Shared gameplay interface and runtime mechanics. |
| LevelEditor | Placeholder for future level authoring, with a named root and no authoring functionality. Inaccessible through application buttons in this pass. |
| Sandbox | Mechanics testing without authored puzzle designs. |

InGame and Sandbox instantiate the same gameplay prefabs and use the same core, runner, and physics scripts. Sandbox adds test configuration rather than a second implementation of the mechanics. Title uses navigation scripts. LevelEditor will use its own authoring scripts when implemented. Scene navigation is separate from the gameplay session controller.

Navigation uses ordinary scene replacement, with scene-local gameplay sessions. Shared UI and gameplay objects are prefab assets rather than a duplicated hierarchy maintained separately in each scene.

## Scene entry and lifecycle

- Title contains five visibly unavailable level slots. No authored levels are included.
- A temporary Open mechanics demo button enters InGame with a plain mechanics test environment. It is not presented as Level 1.
- Entering InGame from Title starts a fresh program and experiment. Returning to Title discards the current session. Reset within InGame retains the program as specified above.
- Developers open Sandbox directly in Unity. No player-facing navigation button enters Sandbox.
- Developers open LevelEditor directly in Unity. No application button enters LevelEditor, and its placeholder scene has no authoring behavior.
- InGame and Sandbox support direct Play Mode entry without first opening Title.

The scene architecture must retain the current scope: a mechanics test space, not a designed level or an implemented level editor.

## Architectural decisions

- [Separate the program core from Unity presentation and physics](../adr/0001-separate-program-core-from-unity.md)
- [Own simulation time with one controller](../adr/0002-own-simulation-time.md)
- [Separate application scenes and share gameplay prefabs](../adr/0003-separate-application-scenes.md)

## Unity references

- [Unity 6.4 physics simulation API](https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Physics2D.Simulate.html) documents manual stepping and recommends fixed step values.
- [Unity 6.4 assembly definitions](https://docs.unity3d.com/6000.4/Documentation/Manual/assembly-definitions-intro.html) explains assembly dependencies and Editor assembly isolation.
- [Unity data separation guidance](https://unity.com/how-to/separate-game-data-logic-scriptable-objects) describes ScriptableObjects as data containers separated from game logic.

## Pass 2 player interface and attempts

The [pass 2 specification](../design/pass2spec.md) supersedes the former demonstration-only presentation. Title now contains distinct title and selection views. Tile 1 opens a replaceable gap fixture; the other tiles are locked. LevelEditor remains a placeholder.

GameplayScreen owns sidebar visibility, modal input/focus, and outcome presentation. BlockWorkspace presents the existing typed program as compact connected blocks. LevelAttempt owns top-surface landing and fall detection. LevelEnvironment supplies a serialized landing-surface reference, and LevelDefinition supplies environment, spawn, camera, fall, and hint configuration. OrbCamera follows the rendered orb at fixed zoom and responds to sidebar viewport changes.

MotionCommotionSession remains the single execution/physics coordinator. It evaluates level contacts after each physical step and before dispatching any following instant commands. A terminal physical step is recorded in ProgramRunner without draining subsequent commands. This preserves correct elapsed time and ensures a final-step landing takes precedence over exhaustion. Sandbox disables level-outcome policy and retains ordinary command completion for mechanics tests.

The session's Completed execution state is distinct from LevelAttempt's Won/Lost outcome. Only Wait advances time. Opening Settings or Hint pauses execution; closing an overlay never resumes it. Scene replacement destroys the session and its subscriptions. Title-screen view selection is transient navigation intent, not saved progression.

The default-asset generator includes pass 2 and remains an explicit overwrite operation. Existing asset GUIDs and the five assembly boundaries remain in place. See [README](../../README.md) for current entry points and verification commands.

## Standalone equation execution

The [standalone equation contract](../design/standalone-equation-actions.md) defines the current behavior and supersedes older nested-only restrictions.

`EquationCommands` maps the five formula identities to Velocity, Acceleration, Position, Force, and Weight command kinds. `ProgramValidator` checks dimensions, output ranges, and fresh Direction pairing before execution. `ProgramRunner` emits instantaneous actions at the current timestamp. Only Wait requests physics steps.

`MotionCommotionSession` applies those actions through `BallBody`. Velocity replaces the velocity vector. Commanded acceleration is stored separately from gravity and applied as mass times acceleration before each physics step. Position uses the captured starting origin, checks destination overlap, and clears prior landing-approach history. Physical snapshots include commanded acceleration for Reset.

A nested formula remains a pure scalar value. It does not emit its standalone action. Formula inputs remain entered values rather than automatic references to current body state.

The Blocks panel is also the return/delete target. Primary equation drags resolve to their owning command; linked child equation drags resolve to their source input. Returning a command removes the command and its expression tree. Returning a nested expression clears only its source input.
