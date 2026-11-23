# Map camera zoom verification

Status: accepted after 101 native tests, 29 browser check groups, and independent review at 9.3/10.

Contract and Inspector setup: [map camera zoom](../../design/map-camera-zoom.md).

## Implementation

The existing `OrbCamera` component exposes player zoom enablement, minimum and maximum orthographic sizes, wheel sensitivity and pinch sensitivity in the Unity Inspector. Default range is 2 to 15, with default level size 5. The shared Gameplay prefab supplies the same behavior in InGame and Sandbox.

Wheel input is accepted only over the current map viewport, outside UI. A two-finger pinch requires both touches over the map. UI, modals/outcomes, sidebar scrolling, and block dragging do not trigger zoom. Pinch interruptions and finger replacement establish a new baseline. Zoom preserves the ball's screen position where camera bounds permit, and follow framing scales with magnification. Physics and its clock remain separate.

## Verification

The native suite includes injected Input System mouse/touch events and checks map-only wheel behavior, pinch cancellation/reentry, UI and drag exclusion, zoom limits, unchanged Ready/Paused physical state, sidebar reflow, default Reset, and Sandbox behavior. Browser verification uses real wheel events and Chromium two-finger touch events alongside the full previous gameplay suite.

The first Web run passed all 29 groups with no application errors. A subsequent independent source review found two additional Inspector/Reset corner cases, corrected before final acceptance:

- Sandbox Reset now restores the original camera position as well as its size after zooming around a moved ball. The regression moves the ball before zooming and checks exact camera restoration.
- Reset computes clamped magnification before applying the shared scaled follow offset. A regression configures minimum 1 and maximum 2 against the shipped level default 5 and verifies that the orb remains inside the viewport.

The native gesture harness clones Input System settings for the pinch case, ignores editor focus changes for that synthetic device sequence, and restores the original settings during teardown. Diagnostics showed the editor disabling the synthetic touchscreen between frames in the complete suite; production correctly cancelled that lost-device gesture. This test-only setup does not change project input settings.

The corrected implementation passed all **101 native tests**, 52 EditMode and 49 PlayMode. Web build `4b7ea1a0fb0347c1b6105b5bfd6e54ae` completed successfully. The final browser run passed all **29 groups**, including real wheel and two-finger touch gestures, with no application errors. It completed with browser exit code zero.

The magnified capture shows zoom **2.5**, camera Y **1.25**, ball at **(0, 0)**, Ready state and simulation time **0**. The ball and platform grow while the sidebar and HUD dimensions remain unchanged. Reset returns to size 5. The native suite also verifies the custom maximum of 2 and displaced Sandbox reset.

An earlier Chromium renderer closure during screenshot capture was not counted as a passing run. The unchanged corrected build then completed all 29 checks. Physical handset hardware was not part of this verification.

- [EditMode results](../results/editmode-results.json)
- [PlayMode results](../results/playmode-results.json)
- [Web build result](../results/web-build-result.json)
- [Browser results](../results/web-results.json)
- [Camera zoom capture](../screenshots/camera-zoom.png)
- [Captured camera state](../results/camera-zoom.json)


## Independent review

The [adversarial reviewer](map-camera-zoom-adversarial-review.md) rated the final implementation **9.3/10**, with no substantive missing approved features or open required corrections.
