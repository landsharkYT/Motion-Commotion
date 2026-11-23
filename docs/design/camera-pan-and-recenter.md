# Camera pan and recenter interview

Status: all design decisions approved, implemented and verified. Acceptance: 111 native tests, 32 browser groups, independent review 9.2/10.

## Requested behavior

- Holding the middle mouse button and dragging moves the camera over the map.
- A camera-centering button sits immediately after the trash icon in the world toolbar.
- Establish appropriate equivalents for trackpads and touchscreens before implementing.

## Existing behavior and constraints

`OrbCamera` follows the ball every LateUpdate while a level is Running. A manual pan would be overwritten without a follow policy. Ready and Paused views retain position; Sandbox does not automatically follow.

`Snap` currently resets position and zoom together. Reset/Retry use it. Recentring without changing magnification therefore needs a separate operation. Current zoom is anchored around the ball, which must be revisited if manual panning introduces a detached view.

Level bounds constrain the whole viewport. At the fixture's default bounds of 36 by 22 units, maximum zoom size 15 shows 30 vertical units, so vertical panning is already constrained to the bounds' centre. Sandbox is unbounded. Existing pointer gating excludes UI, modals/outcomes, and block drags.

The world toolbar has a procedural icon renderer and references serialized by the editor builder. Its current slots contain menu, Run/Pause, Reset, and Clear. The requested button belongs after Clear, with layout checked at both sidebar widths and supported desktop resolutions.

## Input convention evidence

[Godot's 2D editor documentation](https://docs.godotengine.org/en/stable/tutorials/2d/introduction_to_2d.html) uses middle-button drag and Space plus left-button drag for panning. These provide a familiar mouse shortcut and a keyboard-assisted fallback for devices without a middle button.

The installed Unity Input System documents Mouse.scroll as wheel or touchpad gestures in `Library/PackageCache/com.unity.inputsystem@21a28c3a6c83/Documentation~/Mouse.md`. Its Mouse API has no portable trackpad-type or native-pinch flag. Device names and scroll magnitudes are heuristics, not a reliable cross-platform distinction.

[MDN's wheel event documentation](https://developer.mozilla.org/en-US/docs/Web/API/Element/wheel_event) confirms that trackpads can deliver wheel events; [its ctrlKey documentation](https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/ctrlKey) describes browser trackpad pinch as simulated wheel input carrying ctrlKey. This browser behavior is not a guarantee of native OS gesture availability in every Unity player.

## First decision frontier

These recommendations are not approved yet:

1. Middle-button drag pans. Space plus left-button drag provides a trackpad/no-middle-button fallback and is disabled while typing into UI fields.
2. Keep wheel and ordinary two-finger trackpad scrolling as zoom for this pass. Use Space plus click-drag to pan. A dedicated two-finger-scroll-pan mode would change the existing zoom contract and require an explicit input-mode/zoom alternative decision.
3. On touchscreens, one-finger map dragging pans. Two fingers combine pinch zoom with movement of their midpoint for panning. UI gestures remain with UI.
4. Manual pan suspends automatic ball following until the Center camera button is pressed. Camera movement does not pause simulation.
5. Center camera frames the current ball using normal follow framing and preserves zoom. It never resets the ball, program or clock. Use an icon-only framing/target symbol after the trash icon.
6. Keep existing level camera bounds; Sandbox remains unbounded. An axis cannot pan when its whole level bounds already fit inside the viewport.
7. Pan/recenter work in Ready, Running and Paused. Existing modal/outcome and block-drag exclusions remain.
8. Direct dragging moves the map with the pointer/finger, without momentum after release. Match apparent movement across zoom levels.

## Later decisions

If a detached view is approved, settle its zoom anchor, follow/recenter transitions, Reset/Retry behavior and out-of-bounds ball handling next. Gesture cancellation/reentry, Inspector parameters, button state and transition timing follow the approved interaction model. Do not silently treat recenter as the existing physics Reset or commit to platform-specific trackpad gesture behavior before that choice is resolved.

## First-round approvals

The user accepted all eight recommendations:

1. Middle-button drag pans; Space plus left-button drag provides the trackpad/no-middle-button fallback and does not activate while typing into UI fields.
2. Keep ordinary wheel/two-finger trackpad scrolling as zoom. No automatic device guessing or dedicated trackpad mode in this pass.
3. One-finger map dragging pans. Two-finger touch combines pinch magnification with midpoint translation. UI gestures remain with UI.
4. Manual panning enters Free view and suspends automatic follow until recentering. Simulation continues.
5. The icon-only Center camera button follows Clear in the world toolbar. It frames the current ball using normal follow framing, preserves zoom, and resumes following. It does not reset the experiment or program.
6. Manual panning respects existing level camera bounds; Sandbox remains unbounded. Axes fully covered by the viewport cannot pan.
7. Pan and recenter are available in Ready, Running and Paused, with existing modal/outcome and block-drag exclusions.
8. Direct dragging moves the map with the pointer or finger, without momentum, consistently across zoom levels.

## Second decision frontier

Not approved yet:

- In Free view, zoom around the cursor/touch midpoint; in Follow view retain ball-relative framing. Zoom alone never exits Free view.
- Smooth recenter over an Inspector-configurable short duration, using unscaled time; a fresh manual pan interrupts it.
- Reset/Retry restore default magnification and normal follow mode. Sidebar changes preserve the selected mode/view subject to bounds and cancel active gestures.
- Resolve a ball beyond the level camera bounds: literal focus may require a follow/recenter exception to those bounds. The approved manual-pan limit remains separate.
- Cancel gestures crossing UI/map boundaries or losing focus, with a fresh press/baseline needed before further movement.
- Use a small configurable drag threshold before entering Free view; no inertia. Expose navigation enablement/sensitivity/threshold/recenter duration in the Inspector.

If recentering may travel outside map bounds, the transition from that view back to bounded manual panning needs its own decision in the next round.

## Second-round approvals

The user accepted questions 9–16:

9. Free view zoom anchors the place under the cursor or pinch midpoint. Zoom alone does not leave Free view.
10. Recenter smoothly over approximately 0.2 seconds, configurable in the Inspector and using unscaled time. Manual panning interrupts it.
11. Reset/Retry restore default zoom and Follow view; sidebar changes preserve zoom/mode and the view subject to bounds.
12. Follow and recenter may travel past the level's camera bounds to keep an out-of-bounds ball visible. Manual panning remains bounded, with the transition from an out-of-bounds view still to settle.
13. A small configurable drag threshold prevents taps/jitter from disabling follow.
14. Camera gestures start on the map, cancel across UI/window boundaries, and reenter without jumps. Mouse requires a fresh press; touch establishes a fresh baseline. Gestures beginning in UI retain UI ownership.
15. Sandbox uses the same navigation rules, including automatic following while Running.
16. The icon-only Center camera button indicates Follow view with an active state and Free view with a neutral state.

Inspector pan enablement, sensitivity, drag thresholds and recenter duration were recommended alongside this round. Exact parameter defaults can use the established camera conventions.

## Final behavior proposals

These require confirmation:

17. Starting a pan outside ordinary bounds must not snap the camera. Temporarily extend the permitted camera-centre range just enough to include that starting view. Let the player drag within that temporary range, but not farther out than its starting extent. Once the view returns to the ordinary permitted range, restore normal pan bounds. This exception is a transition from out-of-bounds following, not unlimited free-view exploration.
18. Zoom alone in Follow view keeps Follow view and its ball-relative framing. In a two-finger gesture, only meaningful midpoint translation past the pan threshold enters Free view; then combined pan/zoom uses the touch midpoint. Pinch jitter alone does not detach follow.

The final round should confirm that this completes the interaction contract before implementation starts. Other input conflicts, such as preventing Space-pan from activating a previously focused UI button, are correctness requirements rather than additional user-facing navigation modes.

## Final approvals and implementation contract

The user approved 17 and 18, completing shared understanding and authorizing implementation.

- Out-of-bounds follow transitions into Free view without a snap. Temporarily extend the permitted camera-centre range to include its entry position. Restore ordinary bounds once the camera returns to its normal permitted range. Reaching an edge stops movement without inertia.
- Pure zoom preserves Follow view and ball-relative framing. A two-finger gesture enters Free view only when its midpoint crosses the pan threshold. Free view zoom anchors the world point under the cursor or touch midpoint; combined gestures apply translation and magnification together.
- `CameraMode` distinguishes Follow, Free and Recentering. Recentring counts as an active Follow indication. Running in Free view does not reattach automatically. Reset/Retry and a new scene restore default zoom/framing and Follow.
- The Center camera control is icon-only, follows Clear, preserves magnification and experiment data, and reaches the current ball even beyond level pan bounds. Recenter uses unscaled time, default 0.2 seconds, and tracks the current ball while transitioning. Manual pan interrupts it. Modals/outcomes and block drags block navigation.
- Mouse middle-button drag and Space plus left-button drag share map ownership and a fresh-press requirement. Space-pan does not take over focused text editing or submit a previously focused UI button. Mouse pan stops at UI/map boundaries and on focus loss.
- Touch tracks gesture origins separately from current positions. Each contact is identified by its ID and start time, and ownership uses the retained start position even when its first rendered phase is already Moved. A gesture beginning in UI never becomes map input. One finger pans; two fingers combine pinch and midpoint pan; more than two cancel the current baseline. UI crossings and finger replacement rebaseline without a jump.
- Sandbox shares the same Follow/Free/Recenter behavior, using its initial ball-relative framing and no level pan clamp.

### Unity Inspector

On the shared Gameplay prefab's `OrbCamera`, **Player pan and recenter** exposes Allow Pan, Pan Sensitivity, Mouse Drag Threshold, Touch Drag Threshold, and Recenter Duration. Defaults are enabled, sensitivity 1, 6 screen pixels for mouse, 12 for touch, and 0.2 seconds. Existing **Player zoom** controls remain independent. `LevelDefinition.CameraBounds` now limits manual pan, not automatic following or Center camera.

### Source ownership

`OrbCamera.cs` owns view modes, ball framing, recenter transitions, manual pan bounds and coordinate conversion. `OrbCamera.Zoom.cs` owns serialized navigation settings, UI gating and zoom anchoring. `OrbCamera.Input.cs` translates mouse, keyboard and touch input into navigation actions and tracks gesture ownership. These remain the same prefab component.

`GameplayScreen` binds and indicates Center camera. `ControlIcon` draws its framing/target icon. The editor builder includes the control in newly generated defaults. **Motion Commotion > Upgrade camera navigation controls** adds/wires the button idempotently on an existing shared Gameplay prefab without rebuilding levels or scenes.
