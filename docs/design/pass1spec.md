# Motion Commotion pass 1 specification

## 1. Purpose and status

Pass 1 establishes the reusable runtime foundation of Motion Commotion, a physics teaching game built around visual coding blocks. It delivers primitive mechanics demonstrations, not a designed puzzle or a finished game.

Teammates will design the five eventual levels. This pass supplies the mechanics, interfaces, reusable Unity assets, and verification needed for that work. It does not build the level editor.

This specification consolidates the scope and architecture agreed during the initial brainstorming session. It takes precedence over conflicting proposals in the original GDD for pass 1. The user's later instructions take precedence over this document.

The specification distinguishes two kinds of requirements:

- **Agreed requirements** record decisions explicitly settled in the conversation.
- **Implementation defaults** resolve smaller details that were not explicitly interviewed. Section 16 identifies them so an implementer does not mistake them for user decisions.

Script names, folder names, default dimensions, and internal method signatures can change if the documented behavior and dependency boundaries remain intact.

## 2. Delivery boundary

### 2.1 Included

Pass 1 delivers the following artifacts and behavior:

1. Four Unity scenes: Title, InGame, LevelEditor, and Sandbox.
2. A raw, usable block-programming interface matching the supplied wireframes.
3. Direction, force, and wait commands, plus the special weight command.
4. Five predefined base formulas and compatible nested formula inputs.
5. A gray physical ball with editable mass and configurable gravity.
6. Single-kick force behavior, simulation-time waits, and manual physics stepping.
7. Run, Pause, Reset, Clear, validation, measurements, and execution feedback.
8. Shared gameplay prefabs and Inspector-editable settings.
9. Core and Unity integration checks, interactive verification, and a checked Web build.
10. Documentation sufficient for another teammate or agent to continue without relying on the chat transcript.

### 2.2 Excluded

The following are outside this pass:

- Any authored puzzle, obstacle course, or playable campaign level.
- Functional level authoring, an authoring toolbar, or a standalone editor application.
- Equation rearrangement interactions or a symbolic rearrangement algorithm.
- Symbolic derivatives, calculus blocks, or derivative evaluation.
- General expression parsing, arbitrary arithmetic construction, or variable-management UI.
- Narrative, characters beyond the gray ball, dialogue, collectibles, or formula-recovery progression.
- Scoring, prediction-answer exercises, difficulty tiers, achievements, and leaderboards.
- Save files, saved programs, persistence between scene visits, and accounts.
- Original artwork, sprites, animation polish, audio, or an asset-generation pass.
- Hosting, deployment to a public URL, and publishing.
- A persistent global manager framework, dependency-injection framework, or general event-bus framework.

The presence of a feature in a reference image or the original GDD does not add it to this pass. For example, landing targets in the wireframes are not authorization to design a level.

### 2.3 Existing environment

The project uses Unity 6000.4.0f1 and the 2D URP template. Unity CLI and the matching editor are installed, and Web build support is available. The current project started without gameplay scripts or a preexisting gameplay architecture.

Retain the current Unity version and rendering pipeline. Installing unrelated packages is not part of this pass.

## 3. Scene structure and navigation

### 3.1 Scene inventory

| Scene | Responsibilities | Entry |
| --- | --- | --- |
| Title | Title text, five unavailable level-selection slots, temporary mechanics-demo entry | Application entry scene |
| InGame | Shared block workspace, simulation viewport, controls, measurements, and plain mechanics environment | Title demo button or direct Unity Play Mode |
| LevelEditor | Named placeholder root only, reserved for later authoring work | Open directly in Unity |
| Sandbox | Developer mechanics tests using the same gameplay prefabs and scripts as InGame | Open directly in Unity |

Use ordinary scene replacement for navigation. Do not add a fifth bootstrap scene or the previously discussed persistent App scene.

### 3.2 Title

Title must contain:

- The name Motion Commotion.
- Five visibly unavailable level slots, corresponding to the eventual five-level game.
- A temporary button labeled Open mechanics demo.

The five slots do not load placeholder puzzles. Their unavailable appearance must not suggest that the player can unlock them during this demo.

Open mechanics demo loads InGame. The destination is identified as a mechanics demonstration, not Level 1. No Title button enters LevelEditor or Sandbox.

### 3.3 InGame

InGame contains the working runtime proof. Its environment is a plain physics test fixture with a gray ball and only enough primitive geometry to demonstrate collisions and keep the ball observable.

There is no winning route, intended puzzle solution, collectible, scoring target, or level-completion condition. Mechanical tests can specify expected results without becoming puzzle designs.

Provide a Back to title action. Returning to Title discards the scene's session. Reentering InGame creates a new empty program and a fresh experiment.

### 3.4 LevelEditor placeholder

Create a real LevelEditor scene with a clearly named root, such as LevelEditorPlaceholder. Do not implement authoring controls, saving, object placement, or a route into this scene from application buttons.

The scene reserves the application mode. It is distinct from UnityEditor-only build or verification scripts, which belong in Editor-only script assemblies.

### 3.5 Sandbox

Sandbox is opened directly by developers in Unity. It has no player-facing entry button.

It must use the same workspace, session, formula implementation, runner, and physics components as InGame. It may supply different starting settings or test fixtures. Do not copy gameplay scripts and maintain a second implementation for tests.

Sandbox must make it practical to inspect kicks, gravity, mass, waits, and primitive collisions. It need not provide a runtime test-case browser or a complete testing tool UI.

### 3.6 Scene ownership and lifecycle

Each gameplay scene owns one session, one active ball, one simulation controller, and the UI objects it needs. Use shared prefabs to compose them.

InGame and Sandbox must work when a developer opens either scene and presses Play without visiting Title first. Required dependencies must be serialized or composed locally, not supplied by a hidden global object created in Title.

Scene unloading must release event subscriptions and scene-local state. Repeated navigation must not accumulate sessions, cameras, EventSystems, or callbacks.

## 4. Visual and interaction requirements

### 4.1 Presentation

Use raw shapes, readable text, and restrained colors. The main character is a gray ball. No character art or sprites are required.

All obstacles remain basic geometry: circles, triangles, squares, rectangles, or comparable simple shapes. Use appropriate 2D colliders. Do not build decorative terrain, elaborate environments, or a designed obstacle sequence.

The layout should closely follow [IngameDiagram.png](../references/IngameDiagram.png):

1. A palette on the left.
2. A vertical active-program workspace in the middle.
3. The physical simulation on the right.

The alternate sketch, [IngameDiagram2.png](../references/IngameDiagram2.png), may inform compact layout choices. It does not override the three-column arrangement or introduce its additional features.

### 4.2 Block presentation

The palette distinguishes direction, force, wait, and available formula blocks. Weight must be recognizable as a special command when placed in the program.

Blocks have visible labels, recognizable connection areas, and editable fields with units. Their shapes should suggest the wireframe's interlocking coding blocks. There is no requirement for polished custom block artwork.

The workspace must be usable with mouse and keyboard. Numeric fields support normal text editing and keyboard focus. Full keyboard-only drag-and-drop is not a pass 1 requirement.

### 4.3 Disabled equation-edit control

Provide a recognizable pencil-with-sparkles icon for equation editing. It is gray and disabled.

It must not open a modal, rearrange an equation, create another block, or respond as an active button. A disabled-state explanation may say Equation editing is not available in this pass.

Rendering this small interface symbol does not authorize an artwork pass or character sprites.

### 4.4 Responsive layout

The intended platform is a desktop browser with mouse and keyboard. The UI must fit an ordinary desktop window and keep controls usable when the window is resized.

Use scrolling for a long program or palette rather than clipping controls. Keep the simulation viewport distinct from the UI. Desktop support is required; a mobile or touch layout is not.

## 5. Program and block model

### 5.1 Program structure

A program is an ordered list of top-level commands. Each command and nested formula has a stable identity independent of its visual position.

The visual hierarchy is a view of this data. Moving a UI object must not be the only place command order exists.

Pass 1 has no loops, conditionals, event scripts, parallel programs, user-defined variables, or functions. Programs execute from top to bottom.

### 5.2 Top-level commands

| Command | Inputs | Effect |
| --- | --- | --- |
| Direction | Angle in degrees | Supplies direction to the immediately following force command |
| Force | Force-valued formula, initially F = ma, with editable compatible inputs | Applies one impulse using the paired direction |
| Wait | Duration in seconds | Advances simulation time before subsequent commands |
| Weight | W = mg with editable compatible inputs | Changes ongoing downward gravity without applying a kick |

Velocity, average acceleration, and motion expressions are value-producing formulas. They do not directly set the ball's velocity, teleport it, or run as standalone physical actions.

The force command has a force-magnitude expression slot. F = ma is its default formula. A compatible W = mg formula can replace that expression and supply a kick magnitude. In that context it is a value inside an ordinary force command, not the standalone weight command. Both formula forms keep their own fixed left-side outputs.

### 5.3 Direction-force pairing

Each ordinary force command requires its own immediately preceding direction command.

Valid structure:

```text
Direction 45 degrees
Force F = m * a
Wait 2 seconds
Direction 0 degrees
Force F = m * a
```

Invalid structures include a force with no direction, a direction followed by Wait before Force, and a direction at the end of the program.

Direction is not a persistent setting that silently supplies later forces. A weight command is the explicit exception to the force-command behavior and does not require a direction block.

Angles use 0 degrees to the right, 90 degrees upward, and positive counterclockwise rotation.

### 5.4 Dragging and editing

The user must be able to:

- Drag a palette item into the program to create an independent instance.
- Insert a command between existing commands.
- Drag an existing command to reorder it.
- Remove a command.
- Edit right-side numeric inputs.
- Drag a compatible formula into a numeric input slot.
- Remove or replace a nested formula and return to a literal input.

Palette items remain reusable. Creating two force blocks must not make their acceleration fields share mutable instance data. Ball mass is the deliberate shared-property exception described in section 8.

Rejected drops must leave the existing program intact. Prevent placing a formula inside itself or one of its descendants. Do not lose a block because the user releases it outside a valid target.

Removing a command does not silently remove its neighbor. If removing a direction leaves an invalid force, display that error and prevent Run until repaired.

Editing is available while stopped. Running and paused sessions do not permit structural edits. Reset returns the session to an editable state.

## 6. Formula catalog

### 6.1 Supported base formulas

| ID | Left-side output | Right-side inputs | Expression | Output unit |
| --- | --- | --- | --- | --- |
| AverageVelocity | Average velocity | Displacement Δs, elapsed interval Δt | Δs / Δt | m/s |
| AverageAcceleration | Average acceleration | Velocity change Δv, elapsed interval Δt | Δv / Δt | m/s² |
| MotionPosition | Position s | Initial position s₀, initial velocity v₀, acceleration a, time t | s₀ + v₀t + 0.5at² | m |
| Force | Force F | Ball mass m, acceleration a | ma | N |
| Weight | Weight W | Ball mass m, gravitational acceleration g | mg | N |

These are the five initial base formulas. The other equations in the supplied screenshots are not implicitly required.

The output stays on the left and cannot be renamed, moved to the right, or chosen by the user in this pass. All right-side variables remain manipulatable through appropriate literal or compatible formula inputs.

### 6.2 Formula evaluation

Evaluation returns a quantity or a useful validation error. It must not access scene objects, advance time, apply impulses, alter mass, or change gravity.

Editable mass fields update the session's mass setting through the editing path. Evaluation reads that value; it does not mutate it.

No separate prediction field is required. Formulas provide quantities for the mechanics. They are not automatically measurements of the live ball.

### 6.3 Quantities and nesting

Use explicit physical quantity information rather than treating every number as interchangeable. Support at least length in meters, time in seconds, velocity in meters per second, acceleration in meters per second squared, mass in kilograms, and force in newtons.

Examples of compatible nesting include:

- Average acceleration into the acceleration input of F = ma.
- Average velocity into a velocity input.
- An acceleration-valued expression into the g input of W = mg.
- W = mg into an ordinary force command's force-magnitude slot. This calculates the kick magnitude without changing gravity.

A velocity output cannot feed a force, time, or mass input merely because all values are numeric. Display the expected units at the slot and reject incompatible connections.

Position, displacement, average velocity, initial velocity, and gravitational acceleration retain distinct labels even where they share dimensions. Compatibility checks do not prove that a player's physical interpretation is correct. Do not relabel an absolute position as a measured displacement or an average as an instantaneous measurement.

### 6.4 Mathematical assumptions

The motion formula assumes constant acceleration. Make that assumption visible near the formula or in its description.

Average velocity and average acceleration describe an interval. Derivative forms and symbolic differentiation are deferred.

Gravity changes, impulses, and collisions can break a constant-acceleration assumption over a larger interval. The application must not claim that every formula value predicts the full live simulation under arbitrary conditions.

### 6.5 Future rearranged variants

The intended future approach is a curated list of explicitly coded arrangements, such as F = ma, m = F/a, and a = F/m. Do not implement a symbolic algebra system to derive them.

Pass 1 must make additional definitions possible without changing the UI or runner's basic ownership boundaries. It does not require the alternate variants to be exposed or implemented now.

## 7. Force, weight, and program timing

### 7.1 Ordinary force command

An ordinary force command applies one instantaneous kick. It does not apply a continuous force during a later Wait.

Convert the calculated force to impulse using the agreed fixed equivalent duration:

```text
kick duration equivalent = 0.1 seconds
impulse magnitude = calculated force * 0.1 seconds
impulse vector = magnitude * direction unit vector
```

Show the distinction between force in N and the resulting impulse in N·s. This is a documented prototype convention, not an assertion that force and impulse are the same quantity.

Apply the impulse through the real 2D physics body. Do not simulate the kick by moving the Transform directly or by assigning an arbitrary displacement.

For an isolated 1 kg ball, a 10 N calculated force produces 1 N·s and a velocity change of 1 m/s in the specified direction. Gravity and contact effects must be disabled or accounted for when checking that result.

### 7.2 Weight command

A top-level weight command evaluates W = mg for display and changes the ongoing gravitational acceleration from its g input. It applies no additional impulse.

Gravity remains vertical and downward in this pass. The command does not require a direction block. The selected g remains active until another weight command changes it or Reset restores the starting configuration.

When W = mg is nested as a formula, evaluating it only returns a force quantity. It does not modify the experiment. Command execution and formula evaluation must have separate paths.

### 7.3 Same-instant execution

Process consecutive zero-duration commands at the same simulation timestamp. A direction-force pair consumes no simulation time. Weight commands also take effect without a delay.

For example:

```text
t = 0: Direction 0, Force 10 N
t = 0: Direction 90, Force 10 N
t = 0: Wait 1 second begins
t = 1: next command becomes eligible
```

UI animations and execution highlighting must not insert physics steps between the two kicks. Record enough feedback to show that both occurred even if a single rendered frame cannot visibly highlight each in turn.

### 7.4 Wait

Wait uses seconds. It delays subsequent commands while the physics world advances. A later change to milliseconds is explicitly deferred.

Wait does not change kick duration or repeat a previous impulse. Gravity and collisions continue while the simulation advances through the wait.

A zero-second wait is allowed under the implementation defaults and consumes no simulation time. Negative durations are invalid.

### 7.5 Simulation clock

One simulation controller owns both the physics clock and coordination with the program runner. Use manual, fixed-step 2D physics simulation.

Render frames may trigger accumulated work, but render framerate must not define command timing or the physics integration step. UI scripts must not independently step physics or use their own delay coroutine to determine execution time.

The implementation default is a 0.02-second simulation step. Wait durations are rounded upward to a whole number of simulation steps so commands never execute before the requested wait. Display actual simulated elapsed time. Test exact timing with durations that are multiples of the configured step.

Pause must stop both physics advancement and the program clock. Do not use a second uncontrolled physics loop alongside manual stepping. Restore any changed global Unity physics settings when the owning session is disposed.

Fixed stepping does not promise bit-for-bit identical results across desktop and browser platforms.

### 7.6 End of program

The implementation default is to enter Completed and stop advancing physics once the last command finishes. Preserve the final physical state and measurements for inspection.

A program ending with a kick but no Wait has a nonzero final velocity without an interval of visible travel. Explain that a Wait is needed to observe motion over time. Do not insert an undocumented automatic wait.

## 8. Ball and physical environment

### 8.1 Ball

The ball is gray, circular, and backed by a dynamic 2D physics body and circular collider. It begins with a physical mass of 1 kg and configurable starting position and velocity.

Its visual geometry and collider must agree. A sprite-based character is not required.

### 8.2 Mass ownership

There is one physical ball mass per session. Every m input referring to that ball displays the same current value.

Editing one such mass input while stopped updates the physical body and the other mass fields. A formula must not show an independent hidden mass that disagrees with the ball. The evaluator reads mass from the session's numeric inputs without side effects.

Mass must be positive and finite. Invalid partial input may remain visible during editing, but it must not be sent to the physics body or allowed into a running program.

Changing mass can also change the computed F = ma result. Verification of the body's impulse response must distinguish a fixed applied impulse from recomputing a larger force for a larger mass.

### 8.3 Gravity

Initial gravity is 9.81 m/s² downward unless the starting configuration explicitly changes it. Executing a weight command can change it during a run.

Treat the body's current gravitational acceleration separately from editable g inputs in commands that have not executed. Editing a future command does not itself execute that command.

The implementation defaults allow zero gravity for controlled tests and do not expose a negative downward-gravity magnitude.

### 8.4 Geometry and material defaults

Provide reusable primitive geometry or prefabs appropriate for circles, triangles, squares, and rectangles. These need colliders, not a runtime placement tool.

The default test fixture may contain a plain floor and simple bounds. It must not contain a puzzle route or target sequence. The developer can arrange primitive fixtures in Unity for verification.

The implementation defaults are zero bounce, zero friction, and zero linear damping to simplify comparisons. These values remain Inspector-configurable. Do not present those defaults as a complete model of real rolling resistance.

## 9. Runtime controls and session state

### 9.1 State model

Use explicit states rather than unrelated booleans that allow contradictory combinations.

| State | Physics advances | Editing | Principal actions |
| --- | --- | --- | --- |
| Ready | No | Yes | Run, Clear, Back to title |
| Running | During scheduled waits | No | Pause, Reset, Back to title |
| Paused | No | No | Resume, Reset, Back to title |
| Completed | No | No until Reset | Reset, Back to title |

Invalid input is a validation condition in Ready, not a second hidden running mode. The exact state names can differ if behavior stays equivalent.

### 9.2 Run

Run validates the complete program before changing the simulation. An invalid program does not partly execute.

On success, capture the edited starting physical configuration, prepare an execution snapshot, set elapsed time to zero, and execute from the first command. The implementation default blocks Run for an empty program.

Do not allow a second Run call to start another runner while one is active.

### 9.3 Pause and resume

Pause freezes the clock and physical state. Resume continues the same run from the same command and remaining wait duration. Resume must not apply an already executed kick again.

One button can toggle Pause and Resume. Interface responsiveness must continue while simulation time is paused.

### 9.4 Reset

Reset cancels execution and restores the snapshot captured at Run. Restore at least position, rotation, linear velocity, angular velocity, physical mass, gravity, clock, current command, pending wait, and execution feedback.

Retain the authored program, literal inputs, nested expressions, and command order. Return to Ready so the user can edit and run again.

If the user sets mass to 3 kg before Run, Reset restores 3 kg. It does not restore the factory default of 1 kg. If a weight command changes gravity during the run, Reset restores the captured starting gravity.

Reset is not a scene reload and must not create another ball or duplicate UI objects.

### 9.5 Clear

The implementation default makes Clear available in Ready. It removes the authored program and associated validation feedback. It does not reset mass or other experiment settings to factory defaults.

During Running, Paused, or Completed, Reset must be used before editing or clearing. Do not add a second destructive-reset meaning to Clear.

### 9.6 Feedback and measurements

Display elapsed simulation time, ball position, velocity, actual physical mass, active gravity, execution state, and kick feedback.

Position and velocity should show their 2D components with units. Kick feedback should identify direction, calculated force, resulting impulse, and simulation timestamp. A bounded recent-action display is sufficient; a persistent analytics system is not required.

Highlight the active command and the current wait where practical. Highlighting is observational and must not determine execution timing.

Use sensible numeric formatting for display without rounding stored simulation values to the same precision.

## 10. Validation and error handling

Validate external and editable inputs at their boundaries. Keep valid internal program types trustworthy rather than repeating defensive checks throughout the runner.

Before Run, validate:

- Every required slot has a usable literal or expression.
- Numeric inputs parse and are finite.
- Mass is positive.
- Division intervals are strictly positive.
- Wait and motion time are nonnegative.
- Direction-force pairing is complete and adjacent.
- Nested quantities match their destination slots.
- The expression graph contains no cycles.
- Formula results are finite.
- Top-level kick magnitude and gravity magnitude satisfy their allowed ranges.

Under the implementation defaults, a kick magnitude cannot be negative. Direction controls its orientation. Signed position, displacement, velocity, and intermediate acceleration values are allowed, but a final negative force cannot silently reverse the selected direction.

Distinguish incomplete editing from a valid zero. Never silently convert an empty field, malformed number, or division by zero into a usable value.

Errors must identify the relevant block or slot and give a short explanation. Preserve the user's draft and allow correction. Do not rely on Console messages as the player's only feedback.

If a nonrecoverable runtime problem occurs, stop simulation advancement and surface a useful error. Never leave the runner claiming to be paused while physics continues independently.

## 11. Script and assembly architecture

### 11.1 Responsibility boundaries

| Area | Suggested types | Owns |
| --- | --- | --- |
| Program model | BlockProgram, Command, FormulaExpression | Ordered program data, stable identities, nested inputs |
| Mathematics | FormulaEvaluator, Quantity, FormulaDefinition | Explicit formulas, units, pure evaluation |
| Validation | ProgramValidator, ValidationIssue | Program and input diagnostics |
| Execution | ProgramRunner, SimulationAction | Command order, waits, emitted actions |
| Physics | BallBody, SimulationController | Real physics body, impulses, gravity, stepping, physical snapshots |
| Session | MotionCommotionSession | State transitions and coordination among model, runner, physics, and presentation |
| Workspace UI | BlockWorkspace, BlockView, FormulaSlotView | Dragging, snapping, numeric editing, rendering program data |
| Feedback UI | SimulationHUD | Controls, measurements, validation, execution feedback |
| Navigation | SceneNavigator, TitleController | Scene transitions and Title interactions |

These are responsibilities, not a requirement to create one file for every name. Avoid both a giant game-manager script and trivial one-caller layers that obscure the flow.

### 11.2 Dependency direction

The core uses ordinary C# and must not reference UnityEngine, UnityEditor, UI objects, GameObjects, or Rigidbody2D. It owns formulas, quantities, program data, validation, and scheduling.

Unity components reference the core. The runner produces explicit actions that the physics adapter applies. The UI requests program edits or session actions, then observes results.

UI handlers must not call Rigidbody2D methods, mutate Transform positions to simulate motion, or advance physics. Formula evaluation must not change the live experiment. Scene navigation must not become responsible for formula mathematics.

### 11.3 Assemblies and folders

Use assembly definitions to enforce the Unity-independent core and separate test code. Keep Unity-facing physics, UI, session, and navigation scripts in clearly named folders.

The current approved organization is:

```text
Assets/
  Scenes/
    Application/        # Title, InGame, LevelEditor
    Testing/            # Sandbox
  Scripts/
    Core/
    Runtime/
      Physics/
      Session/
      Configuration/
      Navigation/
      UI/
      Diagnostics/
    Editor/
    Tests/
      EditMode/
      PlayMode/
  Data/Blocks/
  Settings/
  Input/
  Prefabs/
  Materials/
  Meshes/
  Plugins/WebGL/
  WebGLTemplates/MotionCommotion/
```

The placeholder scene originally named Editor is now named LevelEditor. Its scope and accessibility are unchanged. See the [project layout guide](../architecture/project-layout.md) for destinations and contributor conventions.

Editor-only build helpers, if needed, belong to an explicitly Editor-only assembly. Do not let an Editor folder under a runtime assembly accidentally compile UnityEditor dependencies into the Web player.

### 11.4 Composition and state ownership

Use one explicit scene startup composition point and serialized references. Do not find dependencies repeatedly through scene-wide searches or mutable global singletons.

One session owns the editable program and starting settings. One runner owns execution progress. One physics adapter owns the body. Views do not retain independent authoritative copies of mass, command order, or execution time.

Subscribe and unsubscribe events with the owning object's lifecycle. Scene replacement must not leave stale event listeners.

### 11.5 Definition assets and prefabs

Use Inspector-editable assets and prefabs for layout, block appearance, labels, defaults, and palette availability. ScriptableObjects can store shared definitions and configuration.

Runtime edits must use session instances rather than mutating shared definition assets. Editing a block during Play Mode must not alter the project's default block definition for later sessions.

Adding new formula behavior requires a small explicit C# implementation with declared inputs, output quantity, and meaningful formula checks. It must not require copying the runner or creating another workspace implementation.

## 12. Verification requirements

### 12.1 Core checks

Verify the actual formula implementations with representative values:

| Formula | Inputs | Expected result |
| --- | --- | --- |
| Average velocity | Δs = 12 m, Δt = 4 s | 3 m/s |
| Average acceleration | Δv = 6 m/s, Δt = 2 s | 3 m/s² |
| Motion position | s₀ = 2 m, v₀ = 3 m/s, a = 4 m/s², t = 2 s | 16 m |
| Force | m = 2 kg, a = 3 m/s² | 6 N |
| Weight | m = 2 kg, g = 9.81 m/s² | 19.62 N |

Test invalid intervals, nonfinite values, incomplete pairs, incompatible quantities, and preservation of the program after a failed validation.

Verify that nested evaluation is side-effect free, including a weight expression supplying an ordinary force command. Verify command order and timestamps independently of scene loading.

### 12.2 Real physics checks

Use an isolated test setup with gravity and contacts disabled where needed. Compare against explicit tolerances suitable for floating-point physics.

Required checks include:

- A fixed 1 N·s impulse changes a 1 kg body's velocity by 1 m/s.
- The same fixed impulse changes a 2 kg body's velocity by 0.5 m/s.
- Orthogonal kicks without a Wait share a timestamp and combine without a physics step between them.
- Wait advances the body and clock for the expected number of fixed steps.
- A top-level weight command changes ongoing gravity without an extra impulse.
- Evaluating a nested weight expression leaves live gravity unchanged.
- Pause freezes position, velocity evolution, program time, and wait progress.
- Resume does not replay impulses.
- Reset restores the captured starting configuration and retains program content.
- Scene teardown restores modified global physics settings.

### 12.3 Interactive checks

Exercise the actual runtime UI rather than relying only on model tests:

1. Open Title and verify that all five level slots are unavailable.
2. Enter InGame through Open mechanics demo.
3. Drag, insert, reorder, edit, nest, unnest, and remove blocks.
4. Confirm invalid drops do not lose existing program data.
5. Attempt Run with an incomplete direction-force pair and observe visible feedback.
6. Build and execute a valid kick-and-wait program.
7. Change mass and confirm all mass fields and the body's physical mass agree.
8. Pause, resume, reset, and rerun without duplicate actions.
9. Confirm the pencil-with-sparkles control stays disabled.
10. Return to Title and reenter; confirm a fresh session.
11. Open InGame and Sandbox directly in Unity and press Play.
12. Inspect LevelEditor and confirm it is only the inaccessible placeholder.

### 12.4 Web verification

Produce a Web build using the installed Unity editor. Serve the build locally using a suitable development server and verify it in a browser.

Check scene entry, drag-and-drop, numeric fields, formula nesting, execution, Pause, Reset, navigation, and resizing. A successful build alone is not proof of browser behavior.

Record the build location and the checks actually performed. Report browser or tooling limitations honestly instead of marking an unperformed check as passed. Public hosting is not required.

## 13. Acceptance checklist

Pass 1 is complete when all of the following are true:

- [ ] Title, InGame, LevelEditor, and Sandbox exist with the specified responsibilities.
- [ ] Title has five unavailable slots and a working mechanics-demo entry.
- [ ] No authored levels or functional editor features were added.
- [ ] InGame and Sandbox use shared gameplay assets and support direct Play Mode entry.
- [ ] The interface follows the raw three-column wireframe.
- [ ] Blocks can be dragged, snapped, nested, reordered, removed, and edited.
- [ ] The five base formulas evaluate correctly and preserve physical quantities.
- [ ] Left-side outputs remain fixed and right-side inputs remain editable.
- [ ] Direction-force validation prevents incomplete programs from running.
- [ ] Kicks use the visible 0.1-second force-to-impulse convention.
- [ ] Consecutive zero-time commands execute at one simulation timestamp.
- [ ] Wait advances simulation time in seconds without repeating forces.
- [ ] The ball is gray, starts at 1 kg, and uses its edited physical mass.
- [ ] Gravity starts at 9.81 m/s² downward and weight commands change it without kicks.
- [ ] Nested formula evaluation has no physical side effects.
- [ ] Pause, resume, Reset, Clear, and fresh scene sessions follow this specification.
- [ ] Measurements and validation feedback are visible in the application.
- [ ] The equation-edit icon is gray, disabled, and without behavior.
- [ ] Core logic is independent of Unity presentation and physics.
- [ ] Prefabs and Inspector settings expose agreed teammate-editable properties.
- [ ] Appropriate core, physics, and interactive checks pass.
- [ ] A Web build is produced and its actual browser behavior is checked.
- [ ] Remaining limitations and any unperformed checks are recorded.

## 14. Suggested implementation sequence

Each stage ends with a reviewable, verified result. Do not build an entire UI before proving the execution model.

1. Create the project folders, assembly boundaries, core quantities, and five explicit formula definitions. Verify calculations and invalid inputs.
2. Implement command data, validation, direction-force pairs, and a runner with a controlled clock. Verify same-instant actions and waits.
3. Integrate the real ball and manual physics controller. Verify impulses, mass, gravity, Pause, and reset snapshots.
4. Build shared workspace prefabs and drag-and-drop editing against the program model. Verify compatible nesting and invalid-program feedback.
5. Compose InGame and Sandbox from those shared assets. Verify direct scene entry and the raw wireframe layout.
6. Add Title navigation, five unavailable slots, and the LevelEditor placeholder. Verify fresh scene sessions.
7. Run the interactive checks, produce and inspect the Web build, and update the handoff documentation.

Do not add deferred features to fill a stage. A plain, working result is the goal.

## 15. Handoff artifacts

The implementation handoff includes scene and prefab paths, the build output path, the method used to build and run checks, and a concise account of results and limitations.

Keep teammate-facing setup instructions near the project documentation. Explain how to open the demo and Sandbox, where defaults are configured, and where a new explicit formula is added. Do not require the original chat to discover core rules.

## 16. Implementation defaults requiring clear attribution

These details were chosen to make the specification executable. They are not additional decisions explicitly made by the user during the interview. They can be revised without reopening the agreed scope if another choice better preserves the agreed behavior.

| Detail | Pass 1 default |
| --- | --- |
| Physics step | 0.02 seconds, configurable through one simulation setting |
| Nonintegral waits | Round upward to whole steps; never execute the next command early |
| End of program | Freeze in Completed; append Wait to observe further travel |
| Empty program | Run is unavailable with a useful empty-program explanation |
| Clear | Available in Ready; removes the program without resetting experiment settings |
| Editing after a run | Reset returns to Ready before editing or clearing |
| Angles outside one revolution | Accept finite values and normalize equivalently for display and execution |
| Negative final force | Reject as a kick magnitude; use direction for orientation |
| Gravity magnitude | Allow zero or positive values, acting downward |
| Contact materials | Zero bounce and friction by default, Inspector-configurable |
| Linear damping | Zero by default, Inspector-configurable |
| Test geometry | Plain floor or bounds where useful; no puzzle arrangement |
| Initial program | Empty on a fresh gameplay session |
| Force formula composition | A force-valued expression slot defaults to F = ma and can accept W = mg without executing a standalone weight command |
| Mass editing | All ball-mass fields bind to one session setting while stopped |
| Runtime log | Bounded recent-action feedback, with no persistence or analytics service |

If implementation reveals that a default conflicts with an agreed rule, preserve the agreed rule and document the adjustment. Do not silently replace the fixed 0.1-second kick conversion, the five formulas, or scene responsibilities.

## 17. Supporting documents

- [Foundation scope](FoundationScope.md)
- [Foundation architecture](../architecture/FoundationArchitecture.md)
- [Domain glossary](../../CONTEXT.md)
- [Original GDD](490%20Motion%20Commotion%20GDD.md)
- [Program core separation decision](../adr/0001-separate-program-core-from-unity.md)
- [Simulation clock decision](../adr/0002-own-simulation-time.md)
- [Scene separation decision](../adr/0003-separate-application-scenes.md)
- [Main gameplay wireframe](../references/IngameDiagram.png)
- [Alternate gameplay wireframe](../references/IngameDiagram2.png)
- [Deferred equation editor reference](../references/Sample_equation_editor.png)
