# Pass 2 reference-image and current-UI audit

Status: image audit complete; pass 2 specification approved and implemented; independent review passed at 9.2/10. This document records observations and unresolved decisions, not approval to implement the sketches literally. Pass 2 specification follows the interview.

Confirmed interview decisions now live in the [working pass 2 specification](pass2spec.md). That document supersedes the unresolved alternatives in this initial audit as the user settles them. Additional user sketches establish independently collapsible left sidebars, upper-right Settings and Hint icons, and Scratch-style block silhouettes. The direction arrow and trajectory marks remain deferred/illustrative.

This audit describes the pre-pass-2 baseline. For the resulting interface and executed checks, see the [implementation report](../verification/reports/pass2-implementation.md).

## Evidence and scope

All eight files in docs/references were inspected individually at full image size. Current presentation was compared with the post-migration WebGL captures web-title.png and web-program.png, the baked asset generator, and a separate read-only code audit. No new live UI session was run for this audit. The existing mechanics, recent passing test suites, and WebGL build are treated as the functional baseline.

The user's current direction is to bring title, level selection, gameplay, and win-condition presentation much closer to the references. Textures remain deferred. The ravine/gap sketch provides the proposed baseline environment. Whether pass 2 may now include one playable sample level, or only a presentation and outcome-testing fixture, needs an explicit decision because pass 1 prohibited authored levels.

## Main gameplay image

Source: [IngameDiagram.png](../references/IngameDiagram.png), 1250 by 750.

### What it shows

- Full-height narrow Blocks palette, approximately the first 240 pixels or 19% of width.
- Full-height Active program column, approximately the next 260 pixels or 21%.
- World occupying approximately the remaining 60%, extending through most of the screen height.
- A triangular play control beside Active, rather than a full-width application toolbar.
- Explicit Direction Blocks, Force Blocks, and Wait Blocks sections.
- Compact interlocking block silhouettes. Direction has an arrow and an inline Degrees field. Force displays F = ma and a socket. Wait displays a seconds field.
- Pencil-with-sparkles affordances near the blocks/world boundary. Their exact ownership and click targets are not specified.
- A hamburger-like menu at the upper left of the world.
- Gray/outline orb positioned on the edge of a starting platform. A large arrow points diagonally upward from it.
- A gap between platforms. The destination has a horizontal magenta/yellow/green/yellow/magenta strip, partly cropped at the right edge.
- An explicit assumption t = 0 near the program column bottom.

### Current UI differences

The implementation has the same broad palette/program/world categories, but not the same screen emphasis or visual language. The palette takes 22% of screen width and the program 29%; the world camera is a 44%-wide, 44%-high inset. A large toolbar, permanent measurements, validation, and action-feedback regions consume much of the remainder. Blocks are large stacked rectangular forms with drag headers and vertically arranged labels/inputs. A nested formula makes a command very tall. The world is an enclosed rectangular test chamber with a floating starting orb, rather than platforms and a gap. There is no direction arrow, landing strip, menu overlay, success region, or level outcome.

### Unresolved interpretations

The strip may encode success tolerance, accuracy tiers, scoring, or simply a target marker. The sketch cannot determine which colors count as success. The arrow could preview the next force direction, represent an aiming control, or be illustrative only. The Force socket could indicate a direction input, a formula input, or a connection affordance. Current semantics require an immediately preceding Direction command; changing that is a separate mechanics decision.

## Alternate gameplay image

Source: [IngameDiagram2.png](../references/IngameDiagram2.png), 823 by 496.

This is a different layout proposal, not a more detailed version of the first. The compact palette and active program are docked in the lower-left region, while the world extends behind the interface. It shows a gap followed by a ramp, dashed orb positions along a rising path, a vertical green/yellow/red-like target strip, a menu icon, a pencil affordance, a logo at lower right, and an equation-editing overlay.

The current implementation has none of this overlay composition, ramp/gap environment, trajectory visualization, or compact connected stack. It does implement editable numeric values and nested formulas, but as full forms in the central column rather than the pictured overlay.

The dashed orbs are ambiguous between a predicted trajectory and a historical motion trail. A predicted trajectory would reveal information before Run and introduce a new teaching mechanic; a trail only records observed motion. A horizontal landing zone and a vertical height target imply different objectives. The two sketches cannot both be treated as exact layout requirements. The checkerboard is a transparency/background convention in the source, not an instruction to texture the game with checks.

## Title image

Source: [colorone.png](../references/colorone.png), 1600 by 1200.

The image shows a large hand-drawn Motion Commotion logo, an outlined moving orb above it, generous white space, and one large cyan oval PLAY button. It contains no level slots, subtitle, mechanics-demo label, or list of disabled items.

The current Title screen is a centered text heading, explanatory subtitle, five full-width unavailable level buttons, and Open mechanics demo. It combines the two steps shown by the separate title and level-select references. The sketch does not settle whether the rough logo is shippable temporary art or a composition reference for later branding.

## Level-selection image

Source: [levelSelectWireframe.png](../references/levelSelectWireframe.png), 1600 by 1200.

The image shows Level Select as a clear screen heading, five large approximately square tiles in a three-over-two arrangement, Level 1 unlocked, and four padlock tiles. There is a sun/face-like drawing above the final tile. There are no visible difficulty controls, descriptions, scores, star ratings, navigation controls, or completion-state examples.

The current UI has no separate selection view, square tile grid, lock icon states, selected state, completed state, or functional level navigation. Five unavailable rows are placeholders only.

The artwork does not establish whether later levels unlock sequentially, whether progress persists, whether locked tiles can be clicked for explanation, or what the drawing above the fifth tile means. It does not authorize a boss encounter. Accessibility labels and a way back to Title need design even though the sketch does not draw them.

## Settings image

Source: [settingsWireFrame.png](../references/settingsWireFrame.png), 1250 by 1000.

A large centered Settings modal overlays a still-recognizable gameplay screen. It has a heading, a close X, a speaker icon with one horizontal slider, and Return to title along the bottom. The underlying scene is visually softened. The screenshot does not specify separate music/effects channels, save behavior, keyboard behavior, or whether opening the modal pauses execution.

The current UI offers Back to title directly in the top bar, along with an explicit Pause control. There is no settings modal, volume control, background input blocking, or modal pause/resume policy. Audio itself was deferred in pass 1. A slider with no audible effect would need to be identified as a placeholder or deferred; it should not imply working audio settings.

## Hint image

Source: [hintWireframe.png](../references/hintWireframe.png), 1250 by 1000.

A centered Hint modal overlays gameplay, with a heading, close X, and large illustrated content area containing a block stack, upward arrow, and smiling face. No readable hint text or exact program values are provided.

There is currently no Hint control, hint modal, or authored hint content. The image leaves the hint entry point, level-specific content, whether it gives a complete solution, and any attempt/scoring consequences unspecified. A new hint should have an intentional teaching purpose rather than reproducing an unreadable drawing.

## Equation manipulation images

Sources: [manipulateEquationsWireframe.png](../references/manipulateEquationsWireframe.png), 1250 by 1000, and [Sample_equation_editor.png](../references/Sample_equation_editor.png), 1063 by 794.

The modal sketch has a close X, swap-side-like icon, help icon, large F = ma block, downward arrows toward a second block, and Create new Block. The explanatory sketch shows moving from F = ma to m = F/a, notes that the output must stay on the left, marks the intermediate arrangement invalid, and limits player-editable variables to the right.

The current UI only has a disabled gray pencil-with-sparkles button, plus direct numeric editing and typed formula nesting. It has no equation manipulation modal, variant selection, block creation, or help flow. This was deliberately deferred. These images are evidence of a future feature, not automatic approval to add it to pass 2. The already-agreed future approach is curated hardcoded variants, not symbolic algebra.

## Current runtime facts that constrain the design

- Scenes are Title, InGame, inaccessible LevelEditor, and Sandbox. Title currently navigates directly into the mechanics demo. There is no level catalog or level-select state.
- The five equation formulas, editable physical mass, Direction/Force adjacency, single-kick impulse conversion, and Wait-driven simulation are baseline mechanics.
- Consecutive non-Wait commands execute at one simulation instant. Physics advances only during Wait.
- Session states are Ready, Running, Paused, and Completed. Completed means command exhaustion and freezes physics, even if the orb is airborne. A kick without Wait finishes at t = 0.
- There is no Won or Lost state, goal/fail detector, attempt model, success dialog, next-level action, unlock system, or progress persistence.
- Runtime errors pause execution and show feedback. They are not gameplay losses.
- Reset restores the edited starting configuration and orb pose while retaining the program. Clear removes the program while Ready. Returning through Title creates a fresh session.
- There is no predicted trajectory, recorded trail, force-direction visualization, settings modal, hint modal, or working formula editor.

A future success detector must be independent of program completion. If landing is required, the design must settle whether time can continue after the command list ends or whether the player must explicitly provide enough Wait. The existing Wait-only rule must not change silently to make a sample level work.

## Scope boundaries to settle first

1. Whether the gap environment is a playable sample level or a presentation/test fixture, and whether the prior no-authored-levels rule changes for pass 2.
2. Which gameplay layout is authoritative: full-height three regions, or a world-first view with a lower-left dock.
3. Title -> Play -> five-tile level selection -> gameplay flow, with future level availability defined honestly.
4. Meaning of the colored target strip and the basic success/failure objective.
5. Flat untextured presentation, block silhouettes, typography, accents, and use of the supplied rough logo.
6. Inclusion of settings and hints, and continued deferral or inclusion of formula manipulation.
7. Meaning of arrows and dashed orb positions.

Detailed decisions about dimensions, input focus, modal behavior, success timing, retries, locks, persistence, and acceptance captures follow once the relevant scope choices are settled. No runtime changes were made during this audit.
