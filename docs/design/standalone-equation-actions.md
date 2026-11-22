# Standalone equation actions

Status: complete. All physical rules and interaction fixes are implemented and verified. Independent review passed at 9.2/10 with no substantive missing features.

## Confirmed scope change

The user requires all five equation tiles to join the main Active stack. This supersedes the previous value-only restriction for average velocity, average acceleration, and position. Each must have a defined physical effect. Their outputs must retain their actual units; they must not be interpreted as Newtons.

The separate interaction corrections are authorized now: returning Active blocks to Blocks deletes them, dragging a primary equation selects its owning command, and every equation tile has visible block connectors.

## Existing invariants

- Formula outputs are velocity in m/s, acceleration in m/s², position in m, force in N, and weight in N.
- Formula time fields are arithmetic inputs. Only Wait advances the simulation clock.
- Force currently applies one kick, using its own immediately preceding Direction. Standalone Weight changes ongoing downward gravity.
- Nested formulas are pure values and do not trigger physical actions.
- All entered formulas compile and validate before any command executes.
- Reset restores the starting physical state and time. New persistent physical state must follow that rule.
- Landing and falling are evaluated after physics steps, using real contacts. Setting a position does not simulate the path between locations.

## Approved action meanings

The user explicitly approved these four decisions:

| Equation | Standalone command effect |
| --- | --- |
| v̄ = Δs / Δt | Replace the orb's current velocity with the calculated velocity. Do not add the result to the old velocity. |
| ā = Δv / Δt | Set an ongoing commanded acceleration alongside gravity. A later acceleration command replaces it. Zero turns commanded acceleration off. |
| s = s₀ + v₀t + ½at² | Instantly place the orb at the calculated position. Do not traverse or collide along the path between positions. |
| F = m × a | Preserve the existing single-kick behavior. |
| W = m × g | Preserve the existing standalone gravity-setting behavior. |

The palette heading is Equation Blocks, replacing Force Blocks. A force command remains the kick action; the category name does not turn every equation output into a force.

Nested formulas remain pure compatible values. Their standalone actions do not execute when their outputs supply another equation's input.

## Implementation and authority

The runtime implements all five equation commands. Native tests, browser checks, and independent review passed. Returning a command to Blocks removes the whole command, including its nested formulas. Returning a nested formula removes that formula from its input. Palette templates are not deleted by returning a palette copy.

Historical pass 2 tests and reports that call the first three equations nested-only describe the earlier baseline. Their rejection rule is superseded. Current acceptance requires real placement and execution of all five equation commands.

Do not add an implicit Wait or treat metres or metres per second as Newtons. Formula time fields remain arithmetic inputs; Wait alone advances simulation time.

## Original first decision frontier

1. Velocity output: replace current velocity or add a velocity change.
2. Acceleration output: ongoing acceleration or a one-time velocity change, including its relationship to gravity and replacement of earlier acceleration.
3. Position output: instantaneous placement or another explicitly defined mechanic.
4. Palette terminology: Equation Blocks accurately describes the category containing all five formulas.

Direction, coordinate origins, signed values, turning acceleration off, collision/goal handling, and interactions between the new actions follow once their effects are agreed. No equation-effect implementation is authorized by the earlier visual-layout approval alone.

## Second-round decisions, resolved below

- Direction requirements for each new action, including replacement of the whole velocity/acceleration vector.
- Position's coordinate origin and velocity preservation.
- Signed outputs and gravity's fixed direction.
- Destination overlap rejection and the existing physics-step-only landing policy.
- Reset and interaction rules for persistent commanded acceleration.

## Confirmed direction, position, and state rules

The user accepted the second decision round in full:

- Velocity, Acceleration, Position, and Force each require their own immediately preceding Direction. Direction is consumed by that one action. Zero degrees points right and 90 degrees points up.
- Weight always configures downward gravity and does not require Direction.
- Velocity replaces the complete velocity vector. Commanded acceleration replaces the complete commanded-acceleration vector.
- Negative velocity, acceleration, and position results reverse the chosen direction. Force magnitudes and gravity retain their existing nonnegative constraints.
- Position uses the physical position captured immediately before Run as its fixed coordinate origin. Repeated Position actions do not accumulate displacement from the last assigned point.
- Position preserves linear velocity, angular velocity, and commanded acceleration. The scalar s₀ remains an entered formula value, not an automatic live-position variable.
- Position moves instantly without traversing intermediate obstacles. A destination penetrating a solid collider is rejected atomically. Contact within Unity's configured contact margin is permitted. Collider distance includes that margin even when visible surfaces touch.
- Rejected destinations pause with a runtime error and require Reset. They do not leave the orb at the invalid location.
- Position does not directly award success. Landing history is reset after placement, and a later Wait physics step must establish a valid landing approached from above.
- Commanded acceleration acts alongside gravity during physics steps. Velocity, Force, Position, and Weight do not clear it. A later Acceleration action replaces it; zero turns it off.
- Reset restores commanded acceleration with the other captured starting physical state. New sessions begin with zero commanded acceleration.
- New standalone outputs are bounded to 10000 m/s velocity, 10000 m/s² commanded acceleration, and 1000000 m scalar position. Destination coordinates also remain within the existing world-coordinate bounds.

All new actions execute instantaneously at the runner's current time. Formula t and Δt inputs never imply elapsed simulation time. Only Wait advances the clock or performs a physics step.

## Arithmetic versus motion

Velocity is assigned once. Gravity, commanded acceleration, and contacts may change it on later physics steps. The average-acceleration equation supplies commanded acceleration, not a measurement of the orb's actual net acceleration.

Position's v₀, a, and t are arithmetic inputs used to calculate s. Executing Position does not assign those inputs to the orb's velocity or acceleration, and t does not advance time. The position action uses only the calculated s and its paired Direction.

Force's 0.1-second impulse scale remains an equivalent-duration factor, not elapsed simulation time. Nested Velocity, Acceleration, Position, Force, and Weight formulas calculate compatible values without executing any command effect.
