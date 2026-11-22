# Motion Commotion

Unity 6000.4.0f1 physics teaching game built around editable equation blocks. Teammates own the final five level designs. Level 1 currently opens a replaceable gap-crossing fixture.

See the [project layout guide](docs/architecture/project-layout.md) for contributor destinations and the [standalone equation contract](docs/design/standalone-equation-actions.md) for the latest approved equation behavior. The pass 1 and pass 2 specifications record earlier baselines.

## Current equation contract

All five equations must join Active as standalone commands. Velocity replaces velocity; acceleration sets an ongoing acceleration alongside gravity; position places the orb instantly. Force remains a single kick, and Weight remains a gravity-setting command. Nested formulas supply values without executing these actions.

The runtime implements these approved meanings, and verification passed. Each directed action requires its own immediately preceding Direction. Position uses the captured starting point as its origin and rejects solid destination overlaps. Do not restore the old nested-only restriction. See the [contract and implementation status](docs/design/standalone-equation-actions.md).

## Open and play

Open `Assets/Scenes/Application/Title.unity` and press Play. Select PLAY, then tile 1. The other four tiles are locked placeholders.

Blocks and Active are independently collapsible left sidebars. Their edge chevrons close them; the world's hamburger menu reopens them. The camera follows the orb at a fixed zoom. World execution controls remain accessible when both sidebars close.

Drag Direction, F = ma, and Wait into Active. Edit the inline values. Velocity, Acceleration, Position, and Force each need their own immediately preceding Direction. Compatible equations nest into input sockets. Drag a command back to Blocks to delete it; drag a nested formula back to remove that formula. The Active workspace scrolls vertically; compact cards fit its width.

Only Wait advances simulation time and performs physics steps. Force applies a single kick using `impulse = force × 0.1 seconds`. The orb starts at 1 kg; editing an equation's mass input changes the actual physical mass and synchronizes the other mass fields. Standalone W = mg changes gravity without a kick; nested W = mg supplies a force value.

Land on the top of any colored target segment to win. Falling below the fixture boundary or finishing the program without landing fails. A kick without a following Wait can therefore fail immediately at t = 0. The outcome row contains status, Retry, Level Select, and a disabled Next arrow. Retry retains the program and edited starting configuration; selection reentry starts fresh.

Settings and Hint pause a running attempt. Closing either leaves it paused until Resume. The volume slider and per-equation pencil buttons are deliberately disabled. Each equation has its own pencil in the palette and in Active, including nested equations; Direction and Wait have none. Textures, audio, scoring, progression, equation rearrangement, and the level editor remain deferred.

`Assets/Scenes/Testing/Sandbox.unity` exercises the shared interface and mechanics with its primitive test chamber and ordinary program-completion behavior. `LevelEditor.unity` remains an inaccessible placeholder.

## Change configuration and content

- `Assets/Settings/Gameplay/Workspace.asset` controls physical defaults, simulation step, font, and palette definitions.
- `Assets/Data/Levels/PlaceholderLevel.asset` references the replaceable environment, spawn, camera settings, fall boundary, and conceptual hint.
- `Assets/Prefabs/Levels/GapFixture.prefab` contains the platforms and landing surface reference.
- `Assets/Prefabs/Gameplay/Gameplay.prefab` supplies shared mechanics and the player interface.
- `Assets/Prefabs/UI/BlockCard.prefab` supplies compact block presentation.
- `Assets/Data/Blocks` contains named command/formula definitions. The player palette shows Direction, the five equations, and Wait.

The supplied title reference is copied into `Assets/Textures/TitleReference.png` for the temporary logo. The title crops out the baked button and places its own functional PLAY control.

The [pass 1 operating limits](docs/verification/reports/coverage.md#numeric-limits) still apply. Player UI shows concise validation; detailed values and logs remain available through development telemetry.

## Generate defaults

The shipped scenes and prefabs are editable Unity assets. **Motion Commotion -> Generate foundation assets** explicitly rebuilds the default assets, including pass 2. It can overwrite customizations. Normal project opening does not run generation.

```sh
unity command 'editor invoke_menu' --path 'Motion Commotion/Generate foundation assets'
```

## Verify

With the project open in Unity:

```sh
python3 Tools/verify-layout.py
python3 Tools/verify-unity.py
```

To build and check the actual browser player:

```sh
unity command --detach 'editor invoke_menu' --path 'Motion Commotion/Build Web proof'
# Wait for the returned job to finish:
unity job wait JOB_ID
npm --prefix Tools ci
npx --prefix Tools playwright install chromium
npm --prefix Tools run verify:web
```

The browser checker starts its own localhost server if none is running. To serve manually, run `python3 Tools/serve-web.py`. Output is `Builds/Web`; checks and screenshots are written under `docs/verification`. The checker uses real mouse/keyboard input and read-only telemetry.

See the [pass 2 implementation report](docs/verification/reports/pass2-implementation.md) for checks and reference-comparison captures.

## Project documents

- [Standalone equation behavior contract](docs/design/standalone-equation-actions.md)
- [Equation action and block-return verification](docs/verification/reports/standalone-equation-actions.md)
- [Compact block layout specification](docs/design/compact-block-layout.md)
- [Compact block layout verification](docs/verification/reports/compact-block-layout.md)
- [Block presentation and snapping specification](docs/design/block-presentation-and-snapping.md)
- [Block presentation verification](docs/verification/reports/block-presentation-and-snapping.md)
- [Pass 2 specification](docs/design/pass2spec.md)
- [Reference-image audit](docs/design/pass2-ui-audit.md)
- [Pass 1 specification](docs/design/pass1spec.md)
- [Architecture](docs/architecture/FoundationArchitecture.md)
- [Domain terms](CONTEXT.md)
- [Layout migration evidence](docs/verification/reports/layout-migration.md)
- [Historical pass 1 review](docs/verification/reports/adversarial-review.md)

Historical reviews describe their reviewed versions; their ratings are not claims about a later implementation.

The initial pass 2 implementation passed [independent review](docs/verification/reports/pass2-adversarial-review.md) at **9.2/10**, with no substantive missing features. The subsequent [per-equation pencil correction](docs/verification/reports/equation-edit-affordance.md) passed 26 EditMode tests, 20 PlayMode tests, the WebGL build, 14 browser interaction groups, and layout checks.

The [block presentation and snapping pass](docs/verification/reports/block-presentation-and-snapping.md) adds editable white equation fields, mathematical notation, linked time inputs, and connector placement. It passed 26 EditMode tests, 24 PlayMode tests, 18 browser interaction groups, the WebGL build, and layout checks. Its independent review rated it 9.1/10 with no substantive missing features.

The [compact layout pass](docs/verification/reports/compact-block-layout.md) removes horizontal sidebar scrolling, wraps long equations, and displays nested formulas in bounded linked rows. It passed 52 native tests, 21 browser interaction groups, the WebGL build, layout checks, and independent review at 9.1/10 with no substantive missing features.

The [equation actions and block-return pass](docs/verification/reports/standalone-equation-actions.md) implements all five standalone equations and return-to-Blocks deletion. It passed 74 native tests, 27 browser groups, the WebGL build, layout checks, and independent review at 9.2/10 with no substantive missing features.
