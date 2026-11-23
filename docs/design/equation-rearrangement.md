# Equation rearrangement

Status: all design and exact-copy decisions approved. Implementation and verification in progress.

## Requested behavior

- Equation edit pencils appear only in Blocks. Remove them from Active and nested program equations; no world-toolbar pencil.
- A palette pencil opens a separate equation editor showing the block visually.
- Variables can be dragged to equation locations. A nearby operation menu offers Divide, Subtract, Add and Multiply, disabling operations that are invalid for the proposed move.
- A swap control exchanges the two sides exactly. Its icon uses opposing arrows.
- Confirm new equation publishes only a valid, new arrangement. Confirmed variants become reusable palette templates with unlimited copies for this level visit. They do not persist into other levels.
- Supported equations and rearrangements are explicitly hardcoded. No symbolic algebra/rearrangement engine.
- Keep visible wording minimal and obtain approval for new UI copy before implementing it.

## Audit

The GDD describes Formula Flip as optional separate practice, without scoring or progression effects. The new request adds usable palette variants and therefore expands that earlier description. It does not authorize replacing the current physics execution contract with the GDD's proposed Duration system or adding authored levels.

The glossary already defines a formula variant as a curated arrangement with a fixed left-side output. The request states both left-only and right-only output rules; the sketch and existing game consistently place the isolated output on the left. Confirm that interpretation before implementation.

For F = m × a, moving a to the left through division yields F / a = m. Swapping then yields m = F / a. To isolate a, divide by m instead: F / m = a, then a = F / m. Dividing a by F is not the inverse operation required here.

The implementation currently identifies only five base formulas, each with one fixed output and one associated command. Variant identity, allowed edit transitions, render layouts, evaluation and runtime effects must be explicit. Changing display text alone is insufficient.

Units do not uniquely identify effects: Δs differs from absolute position s, Δv differs from velocity v, g differs from commanded acceleration a, and s₀/v₀ are arithmetic initial-condition variables. A time output does not implicitly authorize clock advancement. Solving mass interacts with the game's shared physical mass and pre-run compilation. Solving t in the motion equation can produce two roots or no real root.

## First decision frontier

1. Canonical output side: confirm isolated output on the left, editable numeric inputs on the right.
2. Confirm the concrete F/a = m, then swap to m = F/a interaction, including disabled confirmation before the isolated output is on the left.
3. Base-equation scope: all five current equations with a finite curated catalog; settle quadratic time handling separately.
4. Decide how rearranged outputs become executable actions versus pure nested values before adding new mass/time/initial-condition behavior.
5. Define whether creation is additive only and leaves source templates and existing program copies unchanged.
6. Define lifetime across Retry, Clear program, leaving/reentering the level and changing levels.

Subsequent rounds will settle the exact variant catalog, physics semantics, operation/drop rules, invalid drafts, numeric/domain restrictions, cancellation and input methods, duplicate handling and exact UI wording.

## First-round approvals

The user accepted decisions 1–6:

1. The isolated output is on the left; editable numeric inputs are on the right.
2. Moving a from F = m × a across through division gives F / a = m; Swap gives m = F / a. Confirm is disabled before the isolated output is on the left.
3. All five current base equations are in scope through a finite curated catalog. Quadratic time behavior remains to be settled.
4. Executable variants follow the newly solved variable's physical meaning, rather than retaining the original equation's action. Exact behaviors for newly introduced outputs remain unresolved.
5. Confirm adds a new palette template. Original templates and existing program copies remain unchanged, subject to the shared-mass rules that need extension for solved mass.
6. Created variants survive Retry/Replay and Clear program. Leaving and reentering the level discards them.

The user clarified the teaching purpose: a player cannot type into or freely change the left-side output. Rearrangement changes which variable is computed and which variables are available as right-side inputs, enabling puzzles that require alternate variables. The editor rearranges symbols; it must not provide a numeric override for the output.

## Second-round approvals

The user accepted Q7–Q12:

7. A displacement-output command moves from the current position along a fresh Direction. It preserves velocity/acceleration, advances no time and rejects solid-overlap destinations.
8. A velocity-change command adds its directed result to current velocity, using a fresh Direction. The original velocity command still replaces velocity.
9. A gravity-output command sets downward gravity without Direction or an extra kick. Negative gravity is invalid.
10. Calculated time is a pure value usable in compatible inputs, including Wait. It has no standalone Active action; only Wait advances time.
11. Solved initial position s₀ and initial velocity v₀ are pure values for compatible inputs, without standalone physical actions.
12. A mass-output command sets ball mass when executed, preserving velocity and advancing no time. It needs no Direction. Later-equation mass lookup and the current precompilation model still require decisions.

Q13's earliest-nonnegative-time proposal needs further explanation before the root-selection policy is finalized. For s₀ = 0 m, v₀ = 10 m/s, a = -10 m/s² and s = 4 m, the two times are approximately 0.553 s while rising and 1.447 s while falling. Earliest-only selection makes the second crossing unavailable from that variant. These are roots of the configured arithmetic equation, not predictions from the live Unity scene; inputs do not automatically read the ball's state or account for collisions and changing forces.

## Quadratic branch controls approved

The user selected support for both time solutions, with labelled selection buttons. These buttons appear only when solving the motion-position equation for time. Confirm new equation is disabled until a branch is selected, in addition to the existing validity and novelty requirements. All quadratic and degenerate edge cases must be handled explicitly.

Open edge-case policies include root identity, negative and repeated roots, zero time, linear degeneration, underdetermined constant motion, numeric range failures and whether a selected branch may fall back to the other branch. Exact visible labels and error wording remain subject to the established copy approval gate.

## Time edge-case approvals

The user accepted Q14–Q21:

14. Branch selection creates a reusable symbolic formula, not a saved numerical answer. Creation validates the symbolic form; evaluation validates the actual inputs.
15. Order the two real roots chronologically, including negative roots. A selected negative root is invalid. Never replace it with the other branch.
16. Zero is a valid time result for Wait and advances no time. It remains invalid in an input that must be positive, such as a denominator duration.
17. When both quadratic roots coincide, either selected branch returns that same root while retaining its branch identity.
18. No real solution produces an error and prevents execution. Do not substitute or clamp an answer.
19. With zero acceleration and nonzero initial velocity, use the linear solution (s − s₀) / v₀ for Earlier time. Later time is invalid because a second solution does not exist.
20. With both acceleration and initial velocity zero, distinguish an unreachable target from an underdetermined time when the target equals the initial position. Both cases are errors; do not invent a time.
21. Computed times must be within 0–3600 seconds. Out-of-range results are invalid with red feedback and retained inputs. The cumulative program Wait limit remains 3600 seconds.

Evaluation must also handle cancellation, repeated-root rounding, finite-result checks and division by zero explicitly within the hardcoded formulas.

## Remaining interaction and mass decisions

Current code precompiles command values before execution. Plain mass fields are shared and edit the starting physical mass. An execution-time mass command therefore needs ordered validation that carries forward its result for later equations while preserving all-or-nothing preflight rejection. Nested mass values need a separate, explicit rule about whether they are pure arithmetic inputs or physical assignments.

The four-operation move menu also needs term-level rules for the motion equation. Moving the additive product v₀t across equality requires subtracting the whole term; once a product is isolated, moving a factor requires division. Solving a quadratic for t requires a hardcoded root transformation beyond individual add/subtract/multiply/divide moves. The user has approved both root branches but not yet the editor gesture that reaches that form.

All other rearranged denominators must be validated too: zero velocity/acceleration when calculating average-equation time, zero a/g when calculating mass, and zero t when calculating v₀ or a. No combination may produce NaN, infinity, a fabricated answer or an implicit fallback. Existing positive mass and positive average-equation denominator constraints remain applicable.

## Mass and interaction approvals

The user accepted Q22–Q27 and requires all rules and assumptions to be documented:

22. Plain m inputs use the ball mass at the current execution point. In Ready, editing a plain mass field still edits shared starting mass. Validation walks the program in order, carrying forward calculated mass for later commands; any invalid later expression rejects the entire run before physical execution.
23. A nested mass equation supplies a local arithmetic input and does not change the physical ball's mass. Only an executable mass command assigns physical mass.
24. The editor supports whole additive terms as well as individual variable factors. Highlight the full item that will move. Only hardcoded valid transitions enable an operation. For example, moving v₀t across equality requires subtracting the whole term; an isolated product permits dividing by one of its factors.
25. Dragging t to the output position offers the motion equation's special hardcoded quadratic transformation and reveals the branch controls. This exception does not introduce a general algebra engine.
26. Unsupported drops return the dragged item to its previous position. Supported intermediate forms remain visible, with confirmation disabled until one output is isolated on the left.
27. Opening the editor pauses a running attempt and blocks gameplay input. Close/Escape discards the unconfirmed draft, returns to gameplay and does not resume. Confirmation also leaves the attempt paused.

## Consolidated approved contract

This section collects the current approvals. Unresolved details are listed afterward; they must not be silently treated as approved defaults.

### Scope and lifetime

- Rearrangement is available through pencils in Blocks only. No edit button appears in Active, nested program expressions, the game/map toolbar or drag previews.
- Support the five existing base equations with explicitly curated arrangements and transitions. Do not add the GDD's proposed Duration system, authored puzzles, scoring or unlock behavior.
- The equation editor is a separate blocking menu, opened for the selected palette equation.
- Creation adds a reusable template to the current visit's Blocks palette. It does not modify the source template or existing copies in the program.
- Created templates have unlimited copies, like original palette entries.
- Retry/Replay and Clear program retain created templates. Leaving and reentering the level discards them; changing levels never carries them over. No cross-level or disk persistence is requested.

### Editor mathematics and input

- Only a single isolated variable on the left qualifies as the output. That output has no numeric editor or override.
- Right-side values supply the computed output and remain editable according to the existing numeric rules.
- The symbolic editor moves supported variables and term groups. It highlights exactly what is being moved.
- A nearby operation menu contains Divide, Subtract, Add and Multiply. Unsupported operations are gray and disabled.
- Allowed operation transitions are hardcoded. Unmatched drop targets restore the previous placement. Equivalent but unfinished intermediate forms may remain visible while confirmation is disabled.
- Swap exchanges the entire left and right expressions one-to-one, without changing their internal mathematics. It is a convenience operation, not automatic isolation or an equation solver.
- Moving a from F = m × a using Divide gives F / a = m. Swapping gives m = F / a. Moving m instead can produce a = F / m.
- The motion equation has a specific hardcoded time-isolation transformation. Root selection does not require implementing symbolic square-root extraction or a general rearrangement algorithm.
- Creation requires a supported, isolated-left arrangement that is absent from the palette. A time-isolation form additionally requires explicit branch selection.
- Creation validates symbolic form. Numerical values are validated whenever the resulting equation is used. A created variant remains the same formula when its inputs change.

### Output effects

| Solved quantity | Standalone execution | Direction |
| --- | --- | --- |
| Existing velocity output | Replace current velocity | Fresh immediately preceding Direction |
| Existing acceleration output; acceleration from force or motion equation | Set persistent commanded acceleration alongside gravity | Fresh immediately preceding Direction |
| Position s | Instant placement from the attempt's captured starting origin | Fresh immediately preceding Direction |
| Displacement Δs | Instant displacement from current position | Fresh immediately preceding Direction |
| Velocity change Δv | Add to current velocity | Fresh immediately preceding Direction |
| Force F | One kick using the existing force contract | Fresh immediately preceding Direction |
| Weight W | Set downward gravity using the existing weight contract | None |
| Gravity g | Set downward gravity directly | None |
| Mass m | Set ball mass, preserving velocity | None |
| Time Δt or t | Pure compatible input value; no standalone Active action | Not applicable |
| Initial position s₀; initial velocity v₀ | Pure compatible input value; no standalone physical action | Not applicable |

- Instant assignments and kicks do not advance simulation time. Only Wait advances time.
- Position and displacement preserve velocity and commanded acceleration and reject solid-overlap destinations. Teleporting does not traverse or collide with the intervening path and does not itself satisfy the landing win condition.
- Nested formulas are pure calculations and have no command side effects. This includes nested mass, weight and gravity equations.
- Existing signed directed-output behavior remains applicable. Mass is positive; gravity, force, weight and usable time cannot be negative.
- s₀/v₀ and other arithmetic inputs do not automatically read or assign live ball state. Plain m inputs are the explicit shared-mass exception.

### Mass and validation order

- Plain mass fields in Ready edit shared starting mass, as before.
- During ordered program validation, begin with starting mass. A mass command computes its result using the mass available at that point, then updates the mass used by subsequent plain m inputs.
- Nested mass expressions supply local values only. They neither update shared starting mass nor assign physical mass.
- Validate the complete command sequence before executing any physical action. A failure after a hypothetical mass change must not partially execute preceding commands.
- Physical execution applies the validated mass commands in the same sequence. Reset/Retry restore the pre-run starting configuration while retaining the program and created templates.
- Existing mass bounds of 0.001–1000000 kg apply to calculated mass as well as typed mass. Existing finite-result, destination, input-format and speed-cap protections remain applicable.

### Time roots and edge cases

- Branch controls appear only for the motion-position equation solved for time. A reusable variant retains its chosen branch.
- With two distinct real roots, Earlier means the numerically smaller root and Later the larger root, regardless of the sign of acceleration. Never equate the minus sign in the quadratic formula with Earlier unconditionally.
- Do not filter roots before assigning branch identity. Negative selected roots are invalid; selecting one branch never silently falls back to the other.
- Zero is a valid time. Wait with zero advances no time; positive-denominator inputs still reject zero.
- Equal roots are returned by either branch while preserving branch identity.
- Negative discriminant: no real solution; evaluation fails.
- a = 0 and v₀ ≠ 0: Earlier uses (s − s₀) / v₀. Later fails because there is no second solution.
- a = 0 and v₀ = 0 and s ≠ s₀: unreachable target; evaluation fails.
- a = 0 and v₀ = 0 and s = s₀: time is underdetermined; evaluation fails rather than choosing an arbitrary answer.
- Every calculated time must be within 0–3600 seconds. The full program's requested Wait durations also total no more than 3600 seconds.
- Use stable double-precision evaluation and explicit handling of cancellation, repeated-root roundoff, zero denominators and nonfinite values. Preserve full precision internally; the three-decimal input/display convention must not round intermediate calculations.
- A genuinely negative discriminant must not be silently clamped into a solution. Any roundoff tolerance must be scale-aware and documented alongside its tests. Small but nonzero coefficients must not be treated as zero merely because the display rounds them to zero.

### Menus, feedback and documentation

- Opening the editor pauses Running, ends incompatible drag activity and blocks underlying UI/camera input. Confirming or closing does not resume physics.
- Close/Escape discards only the unconfirmed editor draft. It does not delete already-created templates or existing program blocks.
- Invalid numeric values retain the established red-feedback behavior. Errors must prevent execution without NaN, infinity, invented answers or silent branch changes.
- Confirm new equation stays disabled for unfinished or unsupported arrangements, duplicates, and motion-time forms without branch selection.
- Review the exact new UI text with the user before implementing it.
- The implementation specification must include the final catalog, allowed transition examples, variant identities, numeric restrictions, lifetime rules, modal behavior, failure cases and acceptance checks. Keep historical superseded documents clearly linked to the new authority.

## Details raised during the interview, resolved below

- Exact catalog and displayed quadratic/linear forms, including branch identity on palette blocks.
- Default values for variables newly exposed as inputs after rearrangement.
- Duplicate identity and feedback; editing an already-created template; removing created templates.
- Visual distinction and drop feedback for pure-value variants that cannot stand alone in Active.
- Returning through editor operations, dismissing the operation menu, Undo/Reset controls and pointer/touch positioning.
- Confirmation close behavior and focus return, branch-selection reset behavior, and keyboard/touch editing support.
- Exact visible labels and red warning text.

## Derived finite catalog

The approved five equations, solved outputs and two motion-time branches give 18 canonical variants: five existing originals and 13 additional forms. These are algebraic identities, not a request to generate arbitrary equivalent expressions.

| Family | Supported forms |
| --- | --- |
| Average velocity | v̄ = Δs / Δt; Δs = v̄ × Δt; Δt = Δs / v̄ |
| Average acceleration | ā = Δv / Δt; Δv = ā × Δt; Δt = Δv / ā |
| Force | F = m × a; m = F / a; a = F / m |
| Weight | W = m × g; m = W / g; g = W / m |
| Motion position | s = s₀ + v₀t + ½at²; s₀ = s − v₀t − ½at²; v₀ = (s − s₀ − ½at²) / t; a = 2(s − s₀ − v₀t) / t²; earlier t; later t |

For nonzero acceleration, define D = v₀² + 2a(s − s₀), r₋ = (−v₀ − √D) / a and r₊ = (−v₀ + √D) / a. Earlier uses min(r₋, r₊); Later uses max(r₋, r₊). This is a fixed numeric calculation for one supported equation, not symbolic manipulation. The approved zero-acceleration exception applies only to Earlier. UI rendering must faithfully represent chronological branch identity and the linear exception; a fixed minus sign must not be labelled Earlier when a is negative.

Repeated occurrences of the same variable refer to one value, including repeated t in the original motion equation and repeated coefficients in root displays. A rearrangement does not create independently editable duplicates of one physical symbol.

## Template and editor-control approvals

The user accepted Q28–Q34:

28. Duplicate identity is base equation + solved variable + time branch, independent of numeric values and cosmetic term order. Earlier/Later remain distinct variants even when their evaluated roots coincide.
29. Preserve valid input values by variable identity. When the old output becomes an input, use its calculated source value if valid and within literal input limits; otherwise use a documented catalog default. Symbolic creation must remain possible with an invalid numerical example.
30. Pure time, initial-position and initial-velocity variants use rounded value-block shapes without main-stack connectors. They can nest only in compatible inputs and cannot be dropped into empty Active space.
31. Provide Undo and Reset in the editor. Undo restores the previous completed edit, including Swap. Reset restores the draft as it was when the editor opened. Neither changes the program or already-created templates.
32. Created palette templates also have pencils and can be rearranged to create other new variants. Creation remains additive. Deleting palette templates is deferred; at most the 18 catalog variants can be present.
33. Successful confirmation closes the editor, scrolls Blocks to the new template and focuses its pencil. Simulation remains paused.
34. When the operation popup is open, clicking outside or pressing Escape cancels the pending move and restores the prior arrangement. A subsequent Escape closes the editor and discards its draft. Disabled operations have no effect.

## Acceptance requirements already established

- No pencil remains on any Active/program equation or nested program equation. Every eligible palette template has an enabled pencil, including created templates, subject to the editor/state rules settled below.
- Every canonical formula has explicit output-variable identity, dimensions, right-side input definitions, defaults, valid transitions, numeric evaluation and command/pure-value classification.
- Copying a variant preserves its formula identity, branch, variable bindings and independent non-mass inputs. Changing a template does not rewrite existing copies.
- Test all 18 forms, all allowed output types, duplicate detection, original-template preservation and repeated numeric occurrences sharing one value.
- Test all time cases: two positive roots, negative/positive roots, two negative roots, zero root, repeated root, no real root, both acceleration signs, linear positive/zero/negative root, unavailable second linear root, stationary unreachable, stationary underdetermined and time-range failures.
- Test numerical cancellation and near-repeated roots with full-precision nested values. Do not derive root identity from numerator sign or round intermediate values to three decimals.
- Test each new denominator's zero and underdetermined cases, invalid calculated mass and gravity, and all existing input bounds.
- Test chronological mass validation and runtime application, pure nested mass, later-command invalidation without partial execution, and Reset restoring pre-run mass.
- Test required Direction for displacement, velocity-change and new acceleration commands; gravity and mass have no Direction requirement. Only Wait advances physics.
- Test displaced/teleported destination rejection, preserved motion settings, and landing rules.
- Test mouse/touch drop behavior, disabled operations, invalid targets, intermediate confirmation blocking, side swapping, popup cancellation, Undo and Reset.
- Test modal camera/input blocking and pause retention, close/cancel with no palette changes, confirm focus/scroll behavior, duplicates and branch selection.
- Test Retry/Replay and Clear preserving templates, level exit/reentry discarding them, created-template pencils and infinite copies.
- Test palette/program fit at the supported desktop sizes with vertical scrolling and no horizontal scrollbar. Check all new math displays, including roots, fractions, superscripts and the selected time branch.
- Run the existing native and browser regression suites. At the end obtain the user-required independent adversarial review with a score of at least 9/10 and no substantive missing approved features.

## Final proposals, since approved

35. Opening a source time variant preselects that variant's existing branch. Isolating t from a different form starts with neither branch selected. Leaving the isolated-time form clears the branch; returning requires a new selection. Undo restores the complete previous draft, including selection; Reset restores the opening draft.
36. Support mouse and touchscreen symbolic dragging and operation selection. Keyboard supports the ordinary button navigation/activation and Escape cancellation. Full keyboard-only manipulation of symbols and new gameplay hotkeys are deferred.
37. The editor manipulates symbolic variables/terms, not numeric fields. Numeric values remain editable in palette/program blocks according to the existing rules. Root choices choose reusable symbolic forms rather than numeric snapshots.
38. Approve the exact new UI copy below, together with brief Controls-reference additions. Existing numeric-format/range messages are reused rather than rewritten.

### Proposed visible editor text

| Element | Exact copy |
| --- | --- |
| Heading | Edit equation |
| Confirmation | Confirm new equation |
| History action | Undo |
| Restore opening arrangement | Reset |
| Operation menu | Divide; Subtract; Add; Multiply |
| Time branches | Earlier time; Later time |

Swap uses the requested opposing-arrow icon. Close uses ×. Controls-reference rows give these symbols their meanings without adding persistent captions to the icon buttons.

### Proposed editor feedback and new evaluation errors

| Condition | Exact copy |
| --- | --- |
| Incomplete isolation or output on wrong side | Isolate one variable on the left. |
| Existing variant | Equation already available. |
| Time output without selected branch | Select a time. |
| Negative selected time | Time cannot be negative. |
| Negative discriminant | No real time. |
| Later branch in linear case | No second time. |
| Stationary unreachable position | Target cannot be reached. |
| Underdetermined time | Time is undetermined. |
| Other rearranged equation has no solution | No solution. |
| Other rearranged output is underdetermined | Result is undetermined. |
| Time exceeds allowed range | Time range: 0–3600 s. |
| Negative assigned gravity | Gravity cannot be negative. |

Existing literal syntax, numeric range, force/weight sign, finite-result, physical-range and destination errors remain applicable. Do not add motivational prose, success banners or a tutorial paragraph. State precedence must show the actionable reason preventing confirmation; numerical evaluation errors do not prevent symbolic creation.

### Proposed Controls-reference additions

Under Blocks, add Edit equation with the matching pencil graphic and Swap sides with the matching opposing-arrow graphic. Add a Rearrange row with Drag a variable or term on each device tab. These describe supported gestures without inventing hotkeys.

### Math rendering requirements

These are correctness requirements rather than additional UI prose: typeset actual multiplication, fractions, roots, parentheses and superscripts; repeated symbols share one input. Root displays must distinguish Earlier/Later without incorrectly treating fixed minus/plus numerator signs as chronological order for negative a. The zero-acceleration linear exception must be represented honestly. Preserve compact width through vertical layout, not horizontal scrolling or illegibly small text. The final implementation must document its chosen rendering and numerical strategies.


## Final interview approvals

The user accepted Q35–Q38. Existing time variants open with their branch selected. First-time isolation starts unselected; leaving it clears selection; Undo restores complete draft state; Reset restores opening state. Mouse and touch symbolic dragging are supported; keyboard handles button navigation/activation and Escape, while keyboard-only symbol dragging and new hotkeys are deferred. The editor changes symbols and terms, with numeric editing remaining in Blocks/Active. All copy in the final proposal tables and the three Controls-reference additions is approved.

## Implementation decisions

- Canonical variant identity is an explicit FormulaId, with the first five serialized IDs preserved and 13 additional IDs appended. Family/output/command classification is explicit; pure variants cannot acquire a command accidentally through their base family.
- Allowed symbolic moves form a finite transition graph. Product/ratio families have their explicitly listed inverse moves and swaps. Motion states enumerate subtraction of the three additive terms, isolation of product factors, and the dedicated time transformation. This graph is not a symbolic algebra system. Unsupported gestures do not change it.
- Full-precision evaluation accepts an internal numeric mass value separately from typed input parsing. Compiled mass assignment is carried forward at the same float precision as the physical body's mass, avoiding disagreement between later force calculations and the body.
- Ready mass fields edit starting mass. While execution is running/paused, plain mass fields display current physical mass read-only; arithmetic inputs other than plain mass remain explicit entered values.
- Symbolic UI uses the same mathematical notation tree as variant block rendering. It renders fractions, powers, roots, absolute-value bars and multiplication signs, without evaluating arbitrary expressions.
- Chronological time display uses −v₀/a minus or plus √D/|a| for Earlier/Later respectively, so ordering is correct for negative acceleration. Earlier also displays the explicit a=0 linear case. The fixed evaluator uses a stable quadratic calculation and then sorts the two roots.
- Repeated-root tolerance is 8 × machine epsilon × (|v₀²| + |2a(s−s₀)|), used only to correct a tiny negative discriminant caused by floating-point roundoff. Exact zero acceleration uses the linear case; merely small acceleration does not.
- Newly exposed input values use the source output only when finite and within the literal input bound; they are normalized to the existing three-decimal literal format. Other same-variable inputs and nested expressions are deep-copied. A failed source evaluation uses the catalog defaults documented in FormulaVariants.Additional.
- New templates are runtime palette entries owned by the workspace and destroyed when that level visit ends. They are never saved as project assets or player data.


### Default values for added forms

Values below are fallback inputs. When creating from a valid source, matching variable values and nested inputs are preserved first. The old output becomes an input using the source's calculated value when it fits the literal bounds. Source evaluation uses starting mass in Ready and current physical mass while paused.

| Variant | Fallback right-side inputs |
| --- | --- |
| Δs from velocity | v̄ = 3, Δt = 4 |
| Δt from velocity | Δs = 12, v̄ = 3 |
| Δv from acceleration | ā = 3, Δt = 2 |
| Δt from acceleration | Δv = 6, ā = 3 |
| m from force | F = 10, a = 10 |
| a from force | F = 10, m = 1 |
| m from weight | W = 9.81, g = 9.81 |
| g from weight | W = 9.81, m = 1 |
| s₀ from motion | s = 16, v₀ = 3, a = 4, t = 2 |
| v₀ from motion | s = 16, s₀ = 2, a = 4, t = 2 |
| a from motion | s = 16, s₀ = 2, v₀ = 3, t = 2 |
| Earlier/Later t | s = 16, s₀ = 2, v₀ = 3, a = 4 |

For the last row, Later is 2 s and Earlier is negative. Creating Earlier is allowed because creation is symbolic; its numerical template shows the approved negative-time error until values are changed. Do not silently substitute Later or alter the inputs.

Popup focus is confined to its own buttons/cancel surface. Clicking anywhere outside its menu, including the full-screen dim backdrop, cancels only the pending move. Resize cancels an unfinished drag/popup and rerenders the unchanged draft to fit the new viewport.


### Additional numerical and lifecycle safeguards

Quadratic coefficients are normalized by a common scale before computing the discriminant, so small but representable coefficients are not mistaken for zero through squaring underflow. Exact zero displacement has explicit roots 0 and −2v₀/a. If normalization or a required nonzero discriminant product underflows beyond supported precision, evaluation reports the existing numeric-range error instead of inventing a root. Source values and branch selection remain unchanged.

Preflight guarantees concern numeric/domain validity and command ordering. World collision and displacement destination checks still run against the physical scene at execution, following the existing runtime stop behavior. They do not rewind past Wait steps or previously executed actions.

Time variants' supported symbolic editing includes branch selection and whole-side swaps. Other unsupported radical manipulations are rejected; all other family variants remain reachable through the original motion equation's pencil. No extra square, square-root or arbitrary-expression operation is introduced.
