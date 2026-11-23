# Camera pan and recenter adversarial review

Review date: 2026-11-22. Independent score: **9.2/10. Pass.** No blocking findings, required corrections, or substantive missing approved features remain in the reviewed camera change. This clears both acceptance conditions: at least 9/10 and no substantive approved feature omissions.

## Scope and authority

I read all 18 accepted decisions and the final implementation contract in [the navigation design](../../design/camera-pan-and-recenter.md). I reviewed the three `OrbCamera` partials, `GameplayScreen`, `ControlIcon`, the editor upgrade and default builder, the shared Gameplay prefab, all ten `CameraNavigationTests`, and the three new browser navigation groups in `Tools/verify-web.mjs`. I also checked the saved native, build, browser and layout results and personally inspected all three final navigation screenshots with their paired state snapshots.

This is a source and recorded-evidence review. I did not run Unity commands, tests, builds or browser sessions, change implementation, or perform Git mutations. Older zoom and numeric work are the existing baseline. Unrelated user edits to `.gitignore` and the GDD are outside this review.

## Contract assessment

Middle-button drag and Space plus left-button drag have explicit map ownership, configurable thresholds and fresh-press behavior. Crossing UI or the viewport boundary stops mouse panning. Focus changes, sidebar changes, disablement and Reset cancel gestures. Space excludes an actively edited input and clears a previously selected button before UI submission can act. The component's execution order supports that ordering.

Touch ownership uses retained start position and identifies each contact by ID plus start time. A replacement generation resets the gesture baseline. This addresses both reuse of an ID between rendered frames and a contact whose first observed phase is already Moved. UI-origin contacts cannot acquire map ownership by moving onto the map. Map contacts rebaseline after UI crossings, contact replacement or more than two contacts. One finger pans; two fingers pan by midpoint translation and zoom by distance. Pure pinch preserves Follow until meaningful midpoint movement crosses the threshold.

Manual movement enters Free and clears follow velocity. Running does not silently reattach. Free zoom preserves the world point under the pointer or midpoint, subject to approved bounds. Follow zoom retains ball-relative framing. Ordinary wheel input remains zoom, without device-name guesses or a separate trackpad mode. Drag displacement uses world coordinates at the current magnification and has no release momentum.

Center is a distinct operation from Reset. It preserves magnification, interpolates with unscaled time toward the current ball, and permits interruption by manual pan. Its default duration is 0.2 seconds. Follow and Center can reach beyond the level's manual pan limits. Entering Free there retains the starting extent without snapping, prevents further outward exploration, and drops that extension after returning to ordinary bounds. Sandbox uses its own initial framing and the same navigation modes without level clamping.

Ready, Running and Paused remain navigable. Modal/outcome screens and block drags reject navigation. No pan, zoom or recenter path changes program data, the simulation state, physics stepping or the clock. Reset/Retry call the existing reset path and restore constrained default magnification and Follow. Sidebar changes cancel active gestures and retain mode and magnification while clamping Free position where required.

The serialized shared prefab exposes the approved defaults: pan enabled, sensitivity 1, mouse threshold 6 pixels, touch threshold 12 pixels and recenter duration 0.2 seconds. Zoom settings remain independent. The Center enum value is appended, preserving existing icon indices. The icon gains a filled central mark during Follow and Recentering. The actual toolbar places it immediately after Clear. The default builder creates and wires it; the targeted upgrade reuses the named child and moves it last, so repeated invocation does not append duplicate controls or rebuild levels. This last conclusion is source inspection, not a newly executed upgrade check.

## Evidence and limits

- [Native EditMode results](../results/editmode-results.json) record 52 passed. [PlayMode results](../results/playmode-results.json) record 59 passed, including all ten navigation tests. There are no failures, skips or inconclusive results in either summary.
- [Web build result](../results/web-build-result.json) records completed job `3527eed558ec4e1182b2eb9bb323c6aa`, with no recorded errors or warnings.
- [Final browser results](../results/web-results.json) record `passed: true`, 32 passing interaction groups and `errors: []`. The new groups exercise actual mouse middle drag, Space-click drag, cursor anchoring, the visible Center control, sidebar reflow, UI-origin touch rejection, one-finger pan and combined two-finger pan/zoom.
- The final browser source resets through a touch tap before disabling CDP touch emulation, asserts Follow and default zoom, then moves the mouse for handoff. An earlier unsuccessful same-position mouse click during that emulation transition is not acceptance evidence.
- [Layout results](../results/layout-results.json) record a pass, 64 migrated-asset checks, four intended scenes and five assemblies.

The [Free capture](../screenshots/camera-free-view.png) and [state](../results/camera-free-view.json) show Free at zoom 5.809171, camera X -0.951689, with the ball at the origin and time zero. The [recenter capture](../screenshots/camera-recenter.png) and [state](../results/camera-recenter.json) show Follow at exactly the same magnification, camera X zero, and unchanged ball and clock. The toolbar's center mark changes visibly between those captures. The [combined touch capture](../screenshots/camera-touch-navigation.png) and [state](../results/camera-touch-navigation.json) show Free at zoom 2.857143, displaced framing and time zero. The views and recorded modes agree.

The score leaves room for broader device coverage and UI polish. Synthetic native touches and Chromium CDP touch emulation are not physical handset tests. There is no claim here of testing native trackpad hardware, every browser, or every timing interleaving. Some edge cases, such as more than two contacts and every Inspector override combination, are supported chiefly by source inspection rather than dedicated new execution evidence. These are coverage limits, not substantive missing approved features.

## Preserved foundation

No substantive missing pass 1 foundation features were identified under the later approved contracts. The current native and browser regressions retain shared scenes/runtime, compatible formula nesting, shared mass and gravity, direction validation, fixed-step simulation, Wait timing, kick behavior, pause/reset/clear, editing locks, fresh scene sessions and placeholder authoring scope. Later approved presentation, outcome, standalone equation, numeric-validation and camera contracts supersede the earlier restrictions where they conflict. They are not regressions to undo. The camera implementation does not introduce a second runtime, simulation clock or independent physics owner.

The acceptance verdict applies to the final reviewed implementation and completed saved evidence. No required source correction remains open.
