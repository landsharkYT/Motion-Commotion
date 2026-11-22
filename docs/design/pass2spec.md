# Pass 2 specification

Later behavior authority: the [standalone equation contract](standalone-equation-actions.md) supersedes the first three equations' nested-only restriction. It defines current action semantics and implementation status.

Status: implemented. The initial pass 2 review passed at 9.2/10. Per-equation pencil placement was subsequently clarified and corrected; see the [correction evidence](../verification/reports/equation-edit-affordance.md).

## 1. Purpose and authority

Pass 2 turns the tested mechanics foundation into the intended player-facing interface. It supplies the reference-based title, level selection, collapsible coding sidebars, following camera, minimal overlays, and a replaceable gap-crossing fixture with success and failure detection.

The team owns final creative level design. The fixture is temporary Level 1 content used to exercise the interface and outcome flow. It is not the first completed campaign puzzle. Slots 2 through 5 contain no levels.

This specification supersedes the presentation-only and no-outcome restrictions of [pass1spec.md](pass1spec.md) where explicitly stated. Pass 1's equation mathematics, physical semantics, validation, numeric limits, and Wait-only simulation timing remain the baseline. Do not interpret a UI redesign as authorization to replace those mechanics.

Use the [reference audit](pass2-ui-audit.md) to understand discrepancies and ambiguous drawings. The user's interview answers take precedence over incidental marks in any image.

### 1.1 Source interpretation

| Source | Required interpretation | Excluded interpretation |
| --- | --- | --- |
| [IngameDiagram.png](../references/IngameDiagram.png) | Two narrow left sidebars, compact connected blocks, large world, menu, pencil, gap and landing strip | Static zoomed-out whole-level camera; world aiming arrow |
| [IngameDiagram2.png](../references/IngameDiagram2.png) | Cleaner visual treatment and compact Scratch-like block language | Mandatory lower-left floating dock; mandatory ramp; trajectory ghosts; simultaneous equation editor |
| [colorone.png](../references/colorone.png) | Supplied outline logo, plain background, cyan oval PLAY | New logo, tagline, demo subtitle, list of levels on the title view |
| [levelSelectWireframe.png](../references/levelSelectWireframe.png) | Level Select heading, five square tiles in a three-over-two arrangement, numeral 1 and locks | Face/figure above final tile, boss character, decorative story content |
| [settingsWireFrame.png](../references/settingsWireFrame.png) | Settings overlay, speaker, disabled slider, close X, Return to title | Working audio settings or new settings categories |
| [hintWireframe.png](../references/hintWireframe.png) | Hint overlay with close X and teammate-authored conceptual guidance | Automatic complete solution or unreadable decorative hint drawing |
| [manipulateEquationsWireframe.png](../references/manipulateEquationsWireframe.png), [Sample_equation_editor.png](../references/Sample_equation_editor.png) | Continued placement of the disabled pencil-with-sparkles affordance | Rearrangement, Create new Block, equation editing modal, symbolic algebra |

The three additional conversation images clarify edge-chevron sidebar collapse, generic Scratch-style connectors/inline sockets, and upper-right Settings and Hint buttons. They are supplemental evidence even though they are not separate files in docs/references. Red annotation strokes and checkerboard transparency patterns are not game artwork.

## 2. Scope

### 2.1 Included

- Separate title and level-selection views.
- Level 1 placeholder entry, four locked slots, and explicit return navigation.
- Two independently collapsible left sidebars named Blocks and Active.
- Generic Scratch-style block shapes, inline fields, typed formula sockets, temporary colors, and two-axis workspace scrolling.
- Large remaining-world viewport with a fixed-zoom camera following the orb.
- Compact icon controls that remain accessible when sidebars close.
- Basic primitive gap fixture, landing strip, fall boundary, and deterministic outcomes.
- Minimal outcome row with status, retry, Level Select, and next arrow.
- Settings and Hint overlays with deliberate pause and input behavior.
- A visibly disabled placeholder volume slider and disabled equation-edit icon.
- Removal of permanent player-facing measurements, logs, and explanatory paragraphs.
- Short actionable validation and a defined visible-text vocabulary.
- Inspector-editable configuration, reusable prefabs, native tests, WebGL build, actual browser interaction checks, and visual comparison against the references.

### 2.2 Deferred

- Final authored puzzle designs and levels 2 through 5.
- A functional level editor or player-accessible editor route.
- Textures, decorative scenery, new characters, narrative, sound, music, and working volume control.
- Scoring, accuracy tiers, rewards, stars, difficulty modes, attempt bonuses, unlock progression, and persistence.
- Equation rearrangement, alternate formula creation, general expression parsing, or new equations.
- Aiming arrows, predicted trajectories, ghost orbs, and recorded motion trails.
- Phone/touch layouts and public hosting.

## 3. Application flow and scene ownership

### 3.1 Player flow

```text
Title -> PLAY -> Level Select -> tile 1 -> InGame
                        ^                    |
                        |---- outcome -------|
Title <---- Settings / Return to title ------|
```

The Level Select back arrow returns to Title. Outcome navigation returns directly to Level Select. Opening Level 1 from selection always creates a fresh program and experiment.

Keep the four established Unity scenes: Title, InGame, LevelEditor, and Sandbox. Title and Level Select are separate views within the existing Title scene; a fifth scene is unnecessary for this pass. Navigation must explicitly choose the intended Title-scene view rather than briefly showing the wrong screen during a transition.

LevelEditor remains an inaccessible placeholder. Sandbox remains a developer entry and must exercise the shared mechanics without receiving a player-facing menu button.

### 3.2 Title

- Plain background and the supplied black-outline Motion Commotion logo.
- One large cyan-outlined oval PLAY button, positioned beneath the logo as in the reference.
- No subtitle, campaign copy, five level buttons, demo button, or explanation of the development pass.
- Use only the logo portion of the supplied title composition when placing a separate functional PLAY button. Do not display the baked PLAY artwork behind a duplicate button.
- Do not generate replacement branding. Cropping/layout of the existing image is sufficient.

### 3.3 Level Select

- Heading exactly Level Select.
- Five approximately square tiles, three in the top row and two centered below.
- Tile 1 is enabled and shows 1. Tiles 2 through 5 show simple padlocks and are disabled.
- No face/figure, artwork invented for locked content, scores, descriptions, badges, or stars.
- Back-arrow icon returns to Title.
- Winning the fixture does not unlock another tile or save completion progress.
- Disabled tiles cannot acquire an activation action through pointer or keyboard input.

## 4. Gameplay composition

### 4.1 Region layout

With both sidebars open, target approximately 20% width for Blocks, 20% for Active, and the remaining 60% for the world. Use small separators and padding rather than broad gutters or a large top toolbar. The world fills the screen height behind its compact overlay controls.

Both sidebars are on the left. Active sits immediately to the right of Blocks when both are open. Closing Blocks moves Active to the left edge. Closing Active leaves Blocks at the left edge. Closing both gives the world the full available width.

At the reference desktop size, sidebar widths may vary slightly to accommodate borders and readable inputs. Their combined width must not reproduce the former oversized central form column. Do not move the camera into a small framed monitor with telemetry underneath.

### 4.2 Collapse and reopen

- Both sidebars begin open on each fresh gameplay visit.
- Each has an edge chevron for collapse.
- The world hamburger opens a small visibility menu with Blocks and Active toggles. It is not the Settings button.
- Either sidebar can be reopened independently from this menu.
- Preserve visibility during the current gameplay visit, including Run, Pause, Reset, and Retry.
- Returning through Title or Level Select and reopening Level 1 restores both sidebars open.
- Changing visibility never edits the program, advances time, changes mass/gravity, or reapplies a kick.
- Opening and closing panels remain available while Running or Paused; program editing remains locked in those states.
- Cancelling a drag because its panel closes must restore the existing draft without deleting or inserting a block.
- Reflow the world camera viewport and world controls into the remaining space. Keep fixed zoom; do not scale the whole screen as a substitute.

### 4.3 World controls

Place the hamburger and compact execution controls within the world header area. Do not place an equation-edit pencil in the world header; pencils belong beside individual equation blocks. Settings and Hint are separate buttons at the upper right, with Settings above Hint as indicated by the supplemental sketch.

Use icon controls for Play/Pause, Reset, and Clear. Active also contains the pictured Play shortcut. Both Play entry points invoke the same validated action and cannot start duplicate execution.

| State | Play/Pause | Reset | Clear | Program editing |
| --- | --- | --- | --- | --- |
| Ready, valid nonempty program | Play enabled | Enabled | Enabled if nonempty | Enabled |
| Ready, empty or invalid program | Play disabled | Enabled | Enabled if nonempty | Enabled |
| Running | Pause | Enabled | Disabled | Locked |
| Paused without runtime error | Resume | Enabled | Disabled | Locked |
| Stopped by runtime error | Resume disabled | Enabled | Disabled | Locked until Reset |
| Outcome displayed | Underlying controls blocked | Use Retry in overlay | Blocked | Locked until Retry |

Reset retains its existing restoration semantics and returns to Ready. Clear removes commands only while Ready and retains physical settings. There are no large Run/Pause/Reset/Clear text buttons.

## 5. Blocks and equations

### 5.1 Palette contents

Keep exactly these three section headings in order:

1. Direction Blocks
2. Equation Blocks
3. Wait Blocks

Direction Blocks contains the degree-input Direction tile. Equation Blocks contains the five equation tiles. Wait Blocks contains the seconds-input Wait tile. There is no separate Formulas heading and no separate generic Force tile.

The Equation Blocks category contains five equations with different output units. The current standalone actions are defined in the [equation contract](standalone-equation-actions.md).

| Equation tile | Output | Standalone program behavior | Nested behavior |
| --- | --- | --- | --- |
| Average velocity, v avg = delta s / delta t | Velocity | Replace velocity using its own immediately preceding Direction | Supply a compatible velocity input |
| Average acceleration, a avg = delta v / delta t | Acceleration | Set ongoing acceleration alongside gravity using its own Direction | Supply a compatible acceleration input |
| Motion position, s = s0 + v0*t + 0.5*a*t^2 | Length | Place the orb instantly from the captured starting origin using its own Direction | Supply a compatible length input |
| F = m*a | Force | Create a single-kick Force command requiring immediately preceding Direction | Supply a compatible force-valued expression |
| W = m*g | Force | Create the existing gravity-setting Weight command | Calculate a force value without changing gravity |

Use legible mathematical presentation. The table's ASCII spellings describe meaning and do not require programming operators in the final artwork. Keep output fixed on the left and editable inputs on the right. Display units next to inputs without explanatory paragraphs.

Historical restriction, superseded by [standalone equation actions](standalone-equation-actions.md): the first three tiles were originally nested-only. All five now join the main stack with their documented physical effects. All five formulas retain the pass 1 catalog's calculations, defaults, quantities, and input constraints.

### 5.2 Force command representation

The top-level F = ma tile represents a Force command directly, rather than a generic Force card containing a large form. Its command body still owns a force-valued expression slot internally so the existing typed substitution behavior is retained.

Distinguish the connected command outline from nested expression shapes. Replacing an expression inside a Force command does not replace the host command's execution behavior. In particular, W = mg nested in that force-valued slot calculates the kick magnitude; it must not set gravity. W = mg added as its own command sets gravity without a kick. Do not silently change command type when an expression is replaced.

This is a visual restructuring of existing command/expression semantics, not a new force interpretation or symbolic equation manipulation feature.

### 5.3 Shape and interaction

- Use generic Scratch-style top/bottom connectors for executable stack blocks and inset rounded expression sockets for nested values.
- Use arbitrary temporary block colors with readable text, consistent per block kind/formula. Color is not the only indication of type, selection, validity, or execution state.
- Inputs are compact inline fields. Do not render every variable as a full-width stacked row.
- Blocks may grow to fit nested equations. Active supports horizontal scrolling for width overflow and vertical scrolling for program length.
- The palette scrolls as needed without clipping the final Wait section.
- Keep text readable rather than shrinking an oversized equation to fit a narrow column.
- Connectors communicate program order; they do not replace quantity-compatible nesting rules.
- Retain drag-to-add, insertion, reorder, removal, literal editing, typed nesting, expression replacement/removal, invalid-drop feedback, and edited physical-mass synchronization.
- Interacting with a numeric field must not begin dragging its block. A drag begins from a clear non-input part of the tile.
- Every Force command needs its own immediately preceding Direction. Direction is consumed for that command only. Standalone Weight retains its existing rule and does not become a kick merely because it follows Direction.
- While execution is locked, both scroll axes and panel controls still work. Field edits, adds, deletes, reorders, and nested-expression changes do not.
- The directional icon within the Direction block may follow the reference. The large illustrative arrow above the orb is excluded.

### 5.4 Equation editor

Each of the five equation tiles in Equation Blocks has a small gray outlined pencil-with-sparkles button immediately beside its right edge. Every displayed equation in Active has the same affordance, including nested expressions. Use one per equation, without duplicating it on an enclosing command wrapper. Direction, Wait, a plain literal-force input, and transient drag previews have none.

Keep the button separate from the equation silhouette, numeric fields, drag grip, and remove control. The basic F = ma inputs and its button must fit inside Active without horizontal scrolling. Remove the world-toolbar pencil.

All equation-edit buttons remain disabled, have no action or keyboard navigation target, and open no menu. They must not change formulas, create a block, or introduce help/rearrangement behavior. See the [confirmed placement correction](equation-edit-affordance.md).

## 6. World and fixed-zoom camera

### 6.1 Replaceable fixture

Use basic untextured platform geometry with a gap, an orb on the starting platform, and a destination strip. Rectangles are sufficient for the ledges; do not reproduce decorative ravine outlines as final art. Keep the orb gray. A ramp is not required by the alternate sketch.

Use the reference's magenta/yellow/green/yellow/magenta strip treatment. Every segment has equal success semantics. The strip must correspond to actual destination-surface geometry rather than an unrelated UI bar floating over the screen.

Expose spawn, primitive environment composition, landing region, fall boundary, camera bounds/zoom, and hint content for teammate replacement through ordinary Unity assets and Inspector references. Do not implement a level editor to support this fixture.

Choose simple fixture dimensions and defaults with at least one verified successful program within existing physics limits. Keep that solution out of player UI and the hint. An automated verification fixture may contain known values to prove reachability. This authorizes mechanics validation, not puzzle balancing or final level design.

### 6.2 Camera

- Follow the orb horizontally and vertically with gentle configurable smoothing.
- Use fixed orthographic zoom for the entire attempt. Do not auto-fit spawn, destination, trajectory, or level bounds.
- Clamp the camera center so the visible area stays within configured bounds where those bounds are large enough. If bounds are smaller than the viewport, center that axis; do not change zoom.
- The destination may begin offscreen.
- A sidebar change alters the remaining viewport and horizontal visible extent, not the camera's orthographic size.
- Reset and Retry snap to the starting view rather than slowly tracking back from a failed jump.
- Pause, modal display, and outcomes freeze physics. Camera tracking must not imply that physics is still progressing. A necessary viewport reflow is permitted while paused.
- Camera following observes the rendered orb position and never moves the orb or advances simulation.
- No aiming arrow, trail, prediction, dashed orb copies, or whole-level overview camera is included.

## 7. Attempts and outcomes

### 7.1 Unchanged timing

Only Wait advances time. Direction, kicks, and Weight execute at their current simulation instant. Consecutive kicks without an intervening Wait combine before a physics step. Preserve the 0.1-second force-to-impulse conversion and existing fixed-step wait rounding.

Ending the program never starts free-running physics. A kick-only program with no subsequent Wait can finish at t = 0 and fail without traveling. The interface does not conceal this by appending an automatic Wait.

### 7.2 Success

Success requires orb contact with the top of any part of the marked destination strip from above. A resting duration, speed threshold, color tier, perfect center, or requirement to finish the remaining commands is not imposed.

Use actual physics contact/step information and the configured top surface. Side/underside contact, a trigger crossed in midair above the platform, and a contact already present while Ready do not count. A valid landing during an active attempt succeeds even if the orb still has horizontal velocity.

Latch success at the first qualifying step. Stop the runner and physics, discard remaining accumulated simulation work for that attempt, and show exactly one outcome overlay. Do not let remaining commands or callbacks subsequently overwrite success.

### 7.3 Failure

An active attempt fails when either:

- The orb passes below the configured fall boundary.
- Its program exhausts without a successful landing.

Reaching unmarked ground is not by itself another failure rule. A remaining Wait may still permit travel to the target. There is no automatic attempt timeout beyond the existing numeric/program limits.

On a final physics step that both lands and exhausts the program, success takes precedence. Evaluate step contacts and fall state before turning command exhaustion into failure. No failure detection runs against the spawn while merely editing.

Technical runtime errors retain a stopped/resettable error path. They are not silently counted as gameplay misses or displayed as successful outcomes.

### 7.4 Outcome presentation

Use a small plain overlay with these items from left to right:

| Position | Item | On success | On failure |
| --- | --- | --- | --- |
| 1 | Noninteractive status symbol | Checkmark | Cross |
| 2 | Retry icon | Enabled | Enabled |
| 3 | Button labeled Level Select | Enabled | Enabled |
| 4 | Right-arrow next-level icon | Disabled because no next level exists | Disabled |

Keep spacing and shapes simple. No result title, congratulations, explanation, score, stars, attempt counter, badge, or extra button text. The status symbol is not a close button. Underlying gameplay input is blocked.

Retry restores the captured edited starting physical configuration, orb pose, zero simulation time, camera start, and Ready editing state while retaining the program and current sidebar visibility. Reset/retry must clear outcome latches and old contact information so a second attempt can succeed independently.

Level Select discards the attempt and opens that view directly. Reopening tile 1 starts a fresh program and default experiment. There is no saved progress.

Retain the next arrow visibly disabled. Its future enablement requires both a successful attempt and available next-level content; neither winning the placeholder nor clicking a locked tile creates that content.

## 8. Settings, Hint, and menus

### 8.1 Modal behavior

Settings and Hint open above the world and sidebars with a plain backdrop that visually separates them and blocks pointer/keyboard actions behind them. A dim backdrop is sufficient; blur processing is not required.

Only one full modal is open at a time. Opening either closes the small hamburger visibility menu. The pencil cannot open an additional modal.

| State before opening | While open | After close |
| --- | --- | --- |
| Ready | Editing blocked; physics frozen | Ready |
| Running | Paused; physics frozen | Paused; explicit Resume required |
| Paused | Remains paused | Paused |

Focus goes into the modal and returns to the invoking control when closed. Escape closes Settings/Hint like their X button. It must not resume execution. Keyboard activation and clicks cannot pass through the overlay.

Outcome overlays are terminal choices, not dismissible pause windows. Their own controls handle navigation and Retry. Settings and Hint controls behind an outcome are blocked.

### 8.2 Settings

- Heading Settings.
- Close X.
- Speaker icon and one gray disabled slider in the reference arrangement.
- Slider ignores dragging, keyboard changes, and focus activation. It does not change any audio or preference and is visually disabled.
- Bottom button exactly Return to title. It discards the attempt and opens Title, not Level Select.
- No additional audio, graphics, controls, accessibility settings, or text explanation of the fake slider.

### 8.3 Hint

- Upper-right question-mark icon opens Hint.
- Heading Hint, close X, and a simple content area.
- Content is supplied through a teammate-editable configuration reference for the fixture.
- Use a short conceptual hint, with the initial draft for final confirmation: "Use an upward kick and allow time to land."
- Do not give an angle, mass, acceleration, force, duration, or full stack that solves the fixture.
- No penalties, scoring changes, hint counter, procedural hint generation, or decorative face drawing.

## 9. Visible text and visual restraint

### 9.1 Allowed fixed vocabulary

The following is the complete fixed player-interface vocabulary, excluding equations, variables, units, numeric values, and authored hint content:

| Location | Text |
| --- | --- |
| Title button | PLAY |
| Selection heading and outcome navigation | Level Select |
| Available tile | 1 |
| Sidebar headings and visibility toggles | Blocks; Active |
| Palette sections | Direction Blocks; Equation Blocks; Wait Blocks |
| Direction input cue | Degrees |
| Settings heading | Settings |
| Settings navigation | Return to title |
| Hint heading | Hint |

The supplied logo contains the game name. Do not add another textual brand heading over gameplay. Wait may show s alongside its numeric field. Formula symbols, input labels, and physical units must remain legible and accurate.

Use icons for menu, chevrons, Play/Pause/Resume, Reset, Clear, pencil, Settings, Hint entry, close, selection Back, outcome status, Retry, locks, and Next. Their internal control names and nonvisible accessibility names are not visible copy.

### 9.2 Validation vocabulary

Show a short message next to the relevant block or socket, with an error indication that remains understandable without color alone. Use the existing detailed validation results internally and map them to concise player wording.

| Condition | Visible message |
| --- | --- |
| Missing or incorrectly paired Direction | Add Direction |
| Blank, malformed, or nonfinite numeric input | Enter a number |
| Wrong physical quantity for a socket | Incompatible units |
| Nonpositive input where positive is required | Must be positive |
| Negative input where nonnegative is required | Must be zero or more |
| Numeric or cumulative operating limit exceeded | Out of range |
| Unusable placement/order other than missing Direction | Invalid placement |
| Technical runtime stop | Simulation stopped |

These exact short phrases and the initial hint are specification proposals under the user's approved functional-text exception, included for final confirmation. Do not expose exception stack traces, long physics-budget explanations, or source-code terms to players. Detailed diagnostic data remains available to developers.

An invalid drop leaves the prior valid draft intact. Do not replace the entire screen with an error panel. Empty programs simply keep Play disabled without a permanent instructional paragraph.

### 9.3 Removed presentation

Remove the former persistent position, velocity, mass, gravity, elapsed-time/state display, validation paragraph, action log, drag-header instructions, foundation subtitle, demo label, and explanatory physics prose from the player screen. Mass is still editable in equation inputs and remains physically synchronized.

Development telemetry and internal feedback may remain for verification. Do not introduce an additional player-facing measurements panel in this pass.

### 9.4 Styling

Use flat backgrounds, basic shapes, compact controls, thin separators, readable text, and temporary distinguishable block colors. Keep the original cyan title-button accent and colored destination strip. Do not add shadows, gradients, ornamental cards, decorative icons, invented characters, textures, loading slogans, or promotional copy.

## 10. Code and asset responsibilities

Follow the [approved project layout](../architecture/project-layout.md). Do not recreate Assets/MotionCommotion, duplicate scene roots, numbered formula assets, or external scripts folders.

- Core continues to own formula mathematics, quantity compatibility, program validation, instant command order, and Wait scheduling without Unity dependencies.
- Runtime session/physics code owns execution, snapshots, stepping, mass/gravity, and clean pause/reset behavior.
- A focused level-outcome component owns landing/fall detection and terminal attempt state. Keep goal detection out of UI handlers and formula evaluation.
- Camera behavior observes the orb and viewport. It does not own physics, command timing, or level success.
- Navigation chooses screens and loads scenes. It does not infer success from renderer position or a button label.
- UI components own sidebar visibility, compact block presentation, drag/drop, control states, modal input, and outcome display.
- Configuration assets hold the fixture's environment references, spawn, target/fall/camera settings, availability, and hint. Runtime attempts must not mutate shared definitions.
- Preserve the five existing assembly boundaries. Editor generation and build helpers remain editor-only.

Share UI and mechanics with Sandbox while keeping the fixture's environment replaceable. Do not let the new player attempt policy remove the developer mechanics harness's ability to test normal command completion. Existing tests may update presentation selectors and explicitly scoped player outcomes; they must continue to assert the original physical behaviors rather than being deleted to obtain a pass.

Keep prefabs and settings editable in Unity. Update default generation to produce the approved presentation and asset paths, but do not require regeneration on every project open. Generating defaults remains an explicit operation with its existing overwrite caveat.

Use one authoritative attempt state and one simulation owner. At most one system may turn a physics step into a terminal outcome. Remove runner subscriptions and transient modal/outcome state on scene teardown. Returning to Title and opening the fixture again must not retain stale program data, camera targets, event listeners, or physics globals.

## 11. Verification and acceptance

### 11.1 Required visual captures

Capture the actual Unity/WebGL application at 1440 by 900 and a smaller landscape size such as 1280 by 720. Also inspect a wider desktop size such as 1920 by 1080. These are verification targets, not a mobile-layout commitment.

Produce reviewable screenshots of:

1. Title with supplied logo and cyan PLAY.
2. Level Select with five tiles and four locks.
3. Gameplay with both left sidebars open and compact connected blocks.
4. Blocks closed, Active open.
5. Active closed, Blocks open.
6. Both closed with accessible world controls.
7. Wide/deep nested equations, demonstrating readable horizontal and vertical scrolling.
8. Camera following the airborne orb at unchanged zoom.
9. Settings with the visibly disabled slider.
10. Hint with conceptual content.
11. Successful landing and minimal outcome row.
12. Failed attempt and corresponding cross/retry/navigation row.

Compare against source images by composition and agreed interpretation. Test passes alone are insufficient evidence that the new UI resembles the references. The old form-card layout cannot pass merely because it has three columns and functioning buttons.

### 11.2 Native behavior checks

Verify meaningful outcomes rather than exact implementation structure:

- Retained formulas, units, mass synchronization, impulse response, Weight command versus nested value, Direction adjacency, snapshots, Wait timing, Pause, Reset, and lifecycle cleanup.
- Five equation tiles with standalone actions and pure compatible nesting, as updated by the standalone equation contract.
- A successful top landing anywhere across the strip, including its outer colors, without a dwell or zero-speed requirement.
- Side, underside, and airborne trigger crossings do not win.
- Fall failure and command-exhaustion failure; kick-only zero-time exhaustion remains possible.
- Success on the final Wait step wins over exhaustion. No remaining command executes after terminal success/failure.
- Reset/Retry clear accumulated time, outcome/contact latches, and stale callbacks while retaining the intended draft and edited starting settings.
- Multiple attempts and return-through-selection produce the specified retained versus fresh state.
- Sidebar toggling and camera reflow never advance physics or change orthographic size.
- Running -> modal -> close remains Paused, with no repeated kick on Resume.
- Locked and disabled controls cannot activate through keyboard or pointer input.
- Native scenes load directly with valid references; LevelEditor stays a placeholder.

### 11.3 Browser interaction checks

Use real mouse/keyboard interaction with the rendered player. Retain read-only telemetry for assertions; do not inject commands to bypass the UI.

Exercise Title/PLAY/selection/level entry; both sidebars and all four visibility combinations; hidden-panel execution controls; compact input editing; nested formula scrolling; locked editing with working scroll; modal pause/input/focus behavior; disabled volume and pencil; successful and failed attempts; Retry; disabled Next; direct return to Level Select; Settings return to Title; and fresh entry.

Rebuild WebGL from the changed project before evaluating it. Update selectors to meaningful stable control identities and the approved UI vocabulary, not the removed toolbar text. Check captured browser errors and finite telemetry.

Run the layout verifier, Unity EditMode and PlayMode suites, WebGL build, and browser suite. Record actual results under docs/verification/results and captures under docs/verification/screenshots. A failing test requires investigation; do not label an unexecuted check passed or weaken physical assertions to match a UI regression.

### 11.4 Completion checklist

- [x] Reference-based title and separate five-tile selection are implemented without extra copy or invented art.
- [x] Blocks and Active are independently collapsible left sidebars, with a large reflowing world viewport.
- [x] Fixed-zoom camera follows the orb and resets correctly.
- [x] Scratch-style compact equation blocks replace the former large form cards.
- [x] All five equations and the existing physical semantics remain correct.
- [x] Replaceable gap fixture has a verified reachable target and deterministic success/failure.
- [x] Outcome row has the approved order, status, Retry, Level Select, and disabled Next.
- [x] Settings and Hint follow reference placement and the explicit pause/input policy.
- [x] Slider and pencil remain visibly disabled and inert.
- [x] Permanent diagnostics, world arrow, trajectories, scoring, and extra wording are absent.
- [x] Layout checks, native suites, WebGL build, browser interactions, and visual comparisons pass.
- [x] Verification evidence distinguishes observed results from untested assumptions and existing baseline limitations.

## 12. Decision record

The user approved the initial scope questions 1 through 10 with these corrections: independent left sidebars, following fixed-zoom camera, placeholder fixture to be replaced, no invented figure, entire strip valid, fake disabled slider, and no arrow/trail.

Questions 11 through 21 were approved with a minimal outcome-row revision and the requirement that force blocks be equations. Arbitrary temporary block colors were explicitly allowed.

Questions 22 through 28 were all approved: five equation tiles with unchanged execution/nesting semantics, visibly disabled Next, cross on failure, retained sidebar state, two-axis Active scrolling, no player measurements panel, and direct selection navigation with fresh level entry.

This document turns those decisions into concrete behavior and acceptance criteria. The specified implementation details, concise validation vocabulary, initial hint, and verification sizes are presented together for final shared-understanding confirmation. No additional gameplay features are implied.
