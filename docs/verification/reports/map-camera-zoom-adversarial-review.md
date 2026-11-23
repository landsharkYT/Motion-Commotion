# Map camera zoom adversarial review

Review date: 2026-11-22. Final score: **9.3/10. Pass.** No open required corrections or substantive missing approved features remain in the reviewed change.

## Scope and authority

I reviewed the [map camera zoom contract](../../design/map-camera-zoom.md), `OrbCamera.cs`, `OrbCamera.Zoom.cs`, `CameraZoomTests.cs`, the wheel and pinch additions to `Tools/verify-web.mjs`, README/CONTEXT guidance, final result artifacts, and the actual zoom screenshot. The [pass 2 override](../../design/pass2spec.md) correctly supersedes fixed player magnification while retaining the rule that following never changes magnification automatically.

This review isolates zoom changes from the existing numeric-validation work and unrelated user edits. It does not treat the entire working-tree diff as this feature.

## Required findings resolved

Two Reset framing defects were found during review and corrected before acceptance.

1. A permitted Inspector maximum below the level's `CameraSize` initially clamped magnification after positioning with the full camera offset. With size 2 and the default vertical offset 2.5, the orb could start outside the view. `Snap` now clamps size first and uses the same scaled `FollowPosition` as running follow. `InspectorLimitsBelowLevelDefaultStillFrameBallOnReset` verifies the constrained magnification and both viewport coordinates of the orb.
2. Sandbox zoom moves the camera around the orb, but Reset initially restored only magnification. Reset could therefore retain a displaced camera after the orb returned. `Awake` now caches the original camera position, and Sandbox `Snap` restores it. The Sandbox regression moves the orb, proves zoom translated the camera, then asserts the exact original camera position and size after Reset.

Both fixes preserve the existing level bounds. No further required source corrections remain.

## Behavior assessment

Wheel and two-finger pinch input use the current map viewport and reject UI raycast hits, open modal/outcome screens, and block dragging. Exactly two active touches are required. Finger identity, cancellation, and distance baselines prevent a resumed gesture from applying an old distance. Wheel direction and pinch direction agree with the contract.

The serialized controls expose the approved enable switch, size limits, and sensitivities with defaults of true, 2, 15, 0.15, and 1. Validation constrains Inspector values. Zoom changes orthographic size immediately, preserves the orb's screen position where bounds permit, and scales the follow offset. Following and sidebar reflow do not write magnification. Reset and scene initialization restore the constrained level or original Sandbox default. Zoom does not change commands, physics state, or simulation time.

No substantive missing pass 1 foundation features were identified under the later approved specifications. Shared scenes/runtime, formula and compatible nesting semantics, direction handling, Wait-only time advancement, mass/gravity ownership, pause/reset/clear behavior, and scene lifecycle remain covered by the retained implementation and regression suite. Earlier presentation, outcome, nested-only equation, and numeric rules superseded by approved later contracts are not omissions to restore.

## Final evidence

I inspected the completed artifacts rather than treating queued jobs or partial browser runs as passes:

- [EditMode results](../results/editmode-results.json): 52 passed, zero failures or skips.
- [PlayMode results](../results/playmode-results.json): 49 passed, zero failures or skips. Together, native verification passes 101 tests.
- [Web build result](../results/web-build-result.json): job `4b7ea1a0fb0347c1b6105b5bfd6e54ae` completed with no recorded errors.
- [Browser results](../results/web-results.json): `passed: true`, all 29 interaction groups, and `errors: []`. New groups use real wheel events and a Chromium two-finger touch gesture alongside the existing gameplay regression.

I personally inspected [the magnified map capture](../screenshots/camera-zoom.png) and its [paired state](../results/camera-zoom.json). They show zoom 2.5, camera Y 1.25, the orb at the origin, Ready state, and time zero. The orb and platform are visibly magnified while sidebar and HUD sizes remain unchanged. This capture occurs before Reset and demonstrates magnification.

An intermediate native pinch failure was traced to the Editor disabling the synthetic touchscreen between frames. The test now temporarily clones input settings to accept test input without focus, then restores the original settings in teardown. Production behavior was not weakened. An interrupted browser screenshot run was also excluded from acceptance. The unchanged final build subsequently completed all 29 checks.

I inspected source and saved execution evidence independently; I did not rerun Unity, build, or browser jobs during this review. Native synthetic touches and Chromium touch emulation do not establish behavior on every physical touchscreen or browser. That coverage limit does not leave an approved feature unimplemented.
