# Motion Commotion

Unity 6000.4.0f1 physics teaching game built around editable equation blocks. Teammates own the final five level designs. Level 1 currently opens a replaceable gap-crossing fixture.

See the [project layout guide](docs/architecture/project-layout.md) for contributor destinations and the [pass 2 specification](docs/design/pass2spec.md) for the approved interface and behavior.

## Open and play

Open `Assets/Scenes/Application/Title.unity` and press Play. Select PLAY, then tile 1. The other four tiles are locked placeholders.

Blocks and Active are independently collapsible left sidebars. Their edge chevrons close them; the world's hamburger menu reopens them. The camera follows the orb at a fixed zoom. World execution controls remain accessible when both sidebars close.

Drag Direction, F = ma, and Wait into Active. Edit the inline values. Each Force command needs its own immediately preceding Direction. Compatible equations nest into input sockets. The Active workspace scrolls horizontally and vertically.

Only Wait advances physics. Force applies a single kick using `impulse = force × 0.1 seconds`. The orb starts at 1 kg; editing an equation's mass input changes the actual physical mass and synchronizes the other mass fields. Standalone W = mg changes gravity without a kick; nested W = mg supplies a force value.

Land on the top of any colored target segment to win. Falling below the fixture boundary or finishing the program without landing fails. A kick without a following Wait can therefore fail immediately at t = 0. The outcome row contains status, Retry, Level Select, and a disabled Next arrow. Retry retains the program and edited starting configuration; selection reentry starts fresh.

Settings and Hint pause a running attempt. Closing either leaves it paused until Resume. The volume slider and equation-edit pencil are deliberately disabled. Textures, audio, scoring, progression, equation rearrangement, and the level editor remain deferred.

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

- [Pass 2 specification](docs/design/pass2spec.md)
- [Reference-image audit](docs/design/pass2-ui-audit.md)
- [Pass 1 specification](docs/design/pass1spec.md)
- [Architecture](docs/architecture/FoundationArchitecture.md)
- [Domain terms](CONTEXT.md)
- [Layout migration evidence](docs/verification/reports/layout-migration.md)
- [Historical pass 1 review](docs/verification/reports/adversarial-review.md)

Historical reviews describe their reviewed versions; their ratings are not claims about a later implementation.

Pass 2 passed [independent review](docs/verification/reports/pass2-adversarial-review.md) at **9.2/10**, with no substantive missing features. Verification passed 26 EditMode tests, 19 PlayMode tests, the WebGL build, 13 browser interaction groups, and layout checks.
