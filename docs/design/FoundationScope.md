# Motion Commotion foundation scope

## Confirmed requirements

The current work establishes reusable runtime game mechanics through primitive proofs of concept. Teammates create the actual puzzle designs. The level editor is deferred. These requirements supersede conflicting scope proposals in the original GDD.

- Physics learning is the purpose of the game.
- Players assemble coding blocks to control the orb.
- The planned game has only five levels, with one equation family as each level's focus. This foundation work creates no levels.
- The future level editor operates within Unity. Teammates have Unity installed. Building the editor is outside the current pass.
- The initial work consists of mechanics proofs of concept, not a polished playable level or campaign.
- Presentation stays raw and as close to the supplied wireframes as possible. No art or sprites are required.
- The main character is a gray ball.
- The ball initially has a physical mass of 1 kg. Editable mass inputs change the actual ball mass, not only a hypothetical formula parameter.
- Obstacles are exclusively basic geometric shapes, such as circles, triangles, squares, and rectangles.
- Blocks start from predefined original equations.
- The initial catalog contains five base formulas, listed below. Supported alternate rearrangements use explicit coded variants, not a symbolic rearrangement algorithm. The rearrangement UI remains deferred.
- Force blocks contain formulas that apply a single kick, not a continuous force.
- Each kick applies an instantaneous impulse equal to the calculated force multiplied by a fixed 0.1-second equivalent duration. For example, 10 N produces 1 N·s. Wait durations do not alter kick strength.
- Each force block requires its own immediately preceding direction block. An incomplete pair prevents Run and is highlighted.
- Direction blocks accept angles in degrees. Zero degrees points right, 90 degrees points up, and positive angles run counterclockwise.
- Wait blocks accept durations in seconds. A possible future change to milliseconds is outside the current scope.
- Equation rearrangement is deferred to a separate pass.
- The equation-edit button is a disabled gray pencil-with-sparkles icon. It performs no action.
- Formula outputs retain their physical quantities and feed compatible inputs. A velocity result is not treated as a force.
- The output of each equation is fixed on the left side. All right-side variables are editable inputs. No read-only mass input is required.
- Numeric slots accept typed numbers or compatible nested formula blocks. The interface uses nesting rather than arbitrary wires or a variable-management system.
- Players drag blocks from the palette, snap them into a vertical program, drag to reorder, and remove blocks. Formula inputs use compatible nested drop slots.
- Consecutive kicks occur at the same simulation instant. Only Wait advances program time. UI highlighting does not introduce execution delays.
- Gravity defaults to 9.81 m/s² downward unless changed through a weight block.
- Executing a weight block changes ongoing gravity without an additional kick. This is an exception to ordinary force-block execution. Evaluating a nested weight formula only calculates its force output and does not change gravity.
- Derivative equations and symbolic differentiation are deferred from this pass.
- Reset restores the ball and simulation clock while retaining the program. Clear removes the program. Persistence is deferred.
- Delivery includes Unity Play Mode and a verified Web build. Hosting and publishing are outside this pass.
- UI layout, appearance, labels, defaults, and palette availability are editable through prefabs and Inspector settings. New formula behavior requires a small C# addition.
- One controller manually steps physics at a fixed timestep and owns the simulation clock.
- Run captures the edited starting physical configuration. Reset restores that configuration, physical starting state, and clock while retaining the program. Edited mass is retained rather than reset to the factory default.

## Scenes and navigation

- Title contains the title screen, five visibly unavailable level-selection slots, and a temporary Open mechanics demo button.
- The demo button enters InGame with a plain mechanics test environment, not an authored level.
- InGame and Sandbox share gameplay prefabs and scripts. Both support direct Play Mode entry in Unity.
- LevelEditor is a placeholder scene with a named root, no authoring functionality, and no application navigation buttons.
- Sandbox is for developer testing and has no player-facing navigation button. Developers open it directly in Unity.
- Navigation replaces scenes. Gameplay sessions belong to their scenes rather than a persistent global manager.
- Returning to Title discards the current session. Entering InGame from Title starts a fresh program and experiment. This does not change Reset's program-retention behavior.

## Equation families from the supplied screenshots

The five families are velocity, acceleration, equations of motion, Newton's second law, and weight.

| Family | Supplied equations |
| --- | --- |
| Velocity | Average velocity = displacement / elapsed time; v = ds/dt |
| Acceleration | Average acceleration = change in velocity / elapsed time; a = dv/dt |
| Equations of motion | v = v0 + at; s = s0 + v0 t + 0.5 a t²; v² = v0² + 2a(s - s0); average velocity = 0.5(v + v0) |
| Newton's second law | Sum of forces = ma; sum of forces = dp/dt |
| Weight | W = mg |

These screenshot equations define the content boundary, not a requirement to implement every displayed equation. Formula outputs must preserve their physical quantities.

## Confirmed initial formula catalog

| Focus | Fixed left-side output | Editable right-side expression | Output units |
| --- | --- | --- | --- |
| Velocity | Average velocity | Δs / Δt | m/s |
| Acceleration | Average acceleration | Δv / Δt | m/s² |
| Equations of motion | s | s₀ + v₀t + 0.5at² | m |
| Newton's second law | F | ma | N |
| Weight | W | mg | N |

The motion equation assumes constant acceleration. The velocity and acceleration formulas produce interval averages. Derivative forms are deferred. Alternate rearrangements are explicit future variants; their UI is outside this pass.

## Wireframe references

- [Main gameplay layout](../references/IngameDiagram.png) shows a block palette, an active program column, and the simulation area.
- [Alternate gameplay layout](../references/IngameDiagram2.png) shows a compact block workspace and a formula-editing interaction.
- [Equation editor](../references/Sample_equation_editor.png) shows rearrangement from F = ma to m = F/a. The interaction is deferred. Only its disabled edit button belongs in this pass.

## Runtime proof

The runtime presents a block palette, a vertical active program, and a simulation viewport, following the main gameplay wireframe. The proof uses an empty geometric test space, not an authored puzzle.

Run starts the program. Pause freezes program execution and physics. Reset restores the physical starting state and clock while retaining the program. Clear removes the program. Editing is available while the simulation is stopped.

The runtime displays elapsed simulation time, position, velocity, mass, gravity, and kick feedback. Same-instant actions receive feedback without delaying their execution for animation.

Mass fields reflect one physical ball mass so blocks cannot display contradictory current masses. The fixed kick conversion is visible as a 0.1-second equivalent duration.

## Acceptance checks

- Drag, reorder, remove, and edit blocks in the actual runtime interface.
- Accept compatible nested formulas and reject incompatible physical quantities.
- Prevent Run for an incomplete direction-force pair or an invalid numerical input, with visible feedback.
- Confirm that a 10 N force produces a 1 N·s kick. For a free 1 kg ball, that kick changes velocity by 1 m/s in the selected direction.
- Execute consecutive kicks at one simulation instant. Advance program time only through Wait blocks.
- Confirm that changing mass affects the body's response to an impulse. Confirm that a weight command changes ongoing gravity without adding an impulse.
- Confirm that Pause freezes both physics and program time and that Reset retains the program while restoring the physical starting state.
- Keep the gray pencil-with-sparkles button disabled and without behavior.
- Verify Unity Play Mode and the Web build, including interaction in the browser.
- Verify Title-to-InGame navigation and fresh sessions, the five unavailable level slots, and direct Play Mode entry in InGame and Sandbox.
- Confirm that LevelEditor is a placeholder and that no runtime button enters LevelEditor or Sandbox.
- Include no designed levels, level editor, rearrangement system, derivative evaluator, scoring, narrative, hosting, or publishing.

The original GDD's scoring, prediction fields, difficulty settings, narrative, and numerical variations are not confirmed foundation requirements. Level authoring is deferred.
