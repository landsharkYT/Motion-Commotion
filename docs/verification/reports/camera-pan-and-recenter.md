# Camera pan and recenter verification

Status: accepted after 111 native tests, 32 browser check groups, and independent review at 9.2/10.

Contract: [approved navigation behavior](../../design/camera-pan-and-recenter.md#final-approvals-and-implementation-contract).

## Implemented behavior

Middle-button drag and Space plus left-button drag pan the map. Ordinary wheel/trackpad scrolling retains zoom behavior. Touch uses one-finger pan and combined two-finger pan/pinch. Gestures retain map/UI ownership and stop safely at boundaries or focus changes. Space-pan excludes text editing and does not submit a previously focused UI control.

Panning enters Free view until Center or Reset. Free zoom anchors the cursor/touch midpoint; pure pinch in Follow retains following. The target icon after the trash indicates Follow versus Free. Center returns smoothly to the current ball's normal framing without changing zoom, program, physical state or clock. Reset/Retry restore default view and Follow. Sandbox now follows while Running using its own initial framing.

Follow and Center can reach a ball outside map bounds. Starting a manual pan there temporarily includes that camera position in the legal pan range, avoiding a snap. Returning to the ordinary range restores normal bounds. The shared prefab and future default generation both include the button; the targeted upgrade is idempotent and does not rebuild level content.

## Native evidence

All **111 tests passed**, 52 EditMode and 59 PlayMode. The 10 added navigation tests cover thresholds, mouse/Space input and typing, UI ownership, boundary/focus cancellation, combined touch gestures, Free zoom anchoring, sidebar preservation, paused and interruptible recenter, out-of-bounds range recovery, modal/block-drag exclusions, Reset, and Sandbox follow.

A review finding exposed touch-ID reuse and a contact first observed after Began had already become Moved. Contacts now use ID plus start time and retain ownership from start position. The regression queues End+Began with a reused ID before one input update, then moves the new UI-origin touch onto the map. It also queues Began+Moved before one update for a legitimate map contact. Both retain correct ownership.

The input fixture clones and restores Input System settings to isolate synthetic devices from editor focus policy. The initial PlayMode startup stalled once; the existing verification runner restarted it before collecting the final complete passing result. Intermediate runs are not counted as passing evidence.

- [EditMode results](../results/editmode-results.json)
- [PlayMode results](../results/playmode-results.json)
- [Layout verification](../results/layout-results.json)

## Browser evidence

Corrected Web build `3527eed558ec4e1182b2eb9bb323c6aa` completed successfully. The final browser run passed **32 check groups**, with an empty application-error list and browser exit code zero. It exercised actual middle drag, Space plus primary drag, Free cursor anchoring, the visible Center button, zoom-preserving recenter, sidebar reflow, UI-origin touch rejection, one-finger pan, combined two-finger translation/magnification, and touch Reset. Existing gameplay, validation, zoom, outcome, scene and paused-scroll checks also passed.

The first navigation browser run did not activate a mouse Reset click immediately after disabling touch emulation. That attempt was not counted as a pass. The accepted run taps Reset with touch before leaving touch mode, verifies Follow/default zoom, then moves the mouse to establish a clean device handoff. No production workaround was introduced for the browser driver transition.

Visual inspection confirms the target icon after trash, outline state in Free view, filled centre in Follow, unchanged UI sizing, and preserved magnification after recenter. Testing used desktop Chromium with emulated touch; physical handset hardware was not part of this matrix.

- [Web build result](../results/web-build-result.json)
- [Browser results](../results/web-results.json)
- [Free view](../screenshots/camera-free-view.png) and [captured state](../results/camera-free-view.json)
- [Recentered view](../screenshots/camera-recenter.png) and [captured state](../results/camera-recenter.json)
- [Combined touch navigation](../screenshots/camera-touch-navigation.png) and [captured state](../results/camera-touch-navigation.json)

## Independent review

The [independent reviewer](camera-pan-and-recenter-adversarial-review.md) rated the final implementation **9.2/10**, with no blocking findings or substantive missing approved features. The pass 1 foundation remains preserved under the later approved contracts.
