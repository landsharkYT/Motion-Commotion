# Map camera zoom

The later [pan and recenter contract](camera-pan-and-recenter.md#final-approvals-and-implementation-contract) adds Free view, pointer-anchored zoom in Free view, and out-of-bounds ball following.

The player can zoom in and out over the map with the scroll wheel, trackpad scroll, or a two-finger touch pinch. This supersedes the earlier fixed-zoom restriction. The camera continues to follow the ball at the player's chosen magnification; following and sidebar reflow never automatically change magnification.

## Unity Inspector

Open `Assets/Prefabs/Gameplay/Gameplay.prefab` and select its root `Gameplay` object. Its `OrbCamera` component has a **Player zoom** section:

| Setting | Default | Meaning |
| --- | --- | --- |
| Allow Zoom | Enabled | Enable or disable player zoom |
| Minimum Size | 2 | Closest view, orthographic half-height in world units |
| Maximum Size | 15 | Farthest view, orthographic half-height in world units |
| Wheel Sensitivity | 0.15 | Proportional change per normalized scroll unit |
| Pinch Sensitivity | 1 | Finger-distance response; 1 follows the distance ratio |

Smaller orthographic size means closer zoom. Minimum size is at least 0.1 and maximum is at least minimum. Initial and Reset magnification use the level's `CameraSize`, or the original Sandbox camera size, constrained to these zoom limits. The shared prefab configures both InGame and Sandbox. Existing instances can override its values in the Inspector.

## Input and state rules

- Wheel up zooms in; wheel down zooms out. Input System's default uniform scroll normalization supports wheel and trackpad deltas. Sensitivity is adjustable.
- Spreading two fingers zooms in; moving them together zooms out. Exactly two active touches are required, both within the map and outside UI. Starting, replacing, ending, or interrupting a pinch establishes a new baseline so reentry does not jump.
- Sidebar scrolling stays with its ScrollRect. HUD controls, open modal/outcome screens, and block dragging reject map zoom. Hit testing uses the current camera viewport, so independently collapsing sidebars immediately expands the area accepting map input.
- Zoom works in Ready, Running, and Paused states, independently of the physics clock. Outcome and modal screens block it. Zoom never runs commands, changes the ball, resumes a paused attempt, or advances time.
- Magnification changes immediately and proportionally around the ball, preserving its screen position where camera bounds permit. The follow offset scales with magnification so close zoom does not push the ball below the view. Existing follow smoothing and level camera bounds still apply. Cursor-centred panning is outside this feature.
- Sidebar reflow retains player zoom. Reset and Retry restore the default magnification and framing, including Sandbox's original camera position. Clamped level defaults use the same scaled follow offset as ordinary tracking. Scene changes start fresh. No extra player-facing labels or controls are added.

`OrbCamera.cs` retains viewport, follow, bounds and Reset ownership. `OrbCamera.Zoom.cs` contains the serialized zoom configuration, UI hit testing, wheel handling and touch-gesture state. Both files form the existing component, preserving its prefab identity and Inspector configuration.

## Verification

Native input tests cover wheel direction, sidebar and HUD rejection, drag rejection, clamping, sidebar reflow, modal/outcome blocking, paused physics, Sandbox Reset, and touch-pinch cancellation/reentry. The browser suite exercises real wheel events and a two-finger Chromium touch gesture alongside existing gameplay checks.
