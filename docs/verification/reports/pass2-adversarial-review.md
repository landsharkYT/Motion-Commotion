# Pass 2 independent adversarial review

Review date: 2026-11-20. Final score: **9.2/10**. The acceptance gate passes. No open required corrections or substantive feature omissions remain. The initial conditional review is preserved in [round 1](pass2-adversarial-review-round1.md).

## Resolved finding and final delta review

The P2 strictly-positive-input wording defect is fixed. `BlockWorkspace.ShortError` now recognizes the actual `greater than zero` wording from Core and maps it to `Must be positive`. Formula evaluation and physics behavior are unchanged.

I inspected the fix, `StrictlyPositiveInputsShowApprovedValidationText`, the browser interaction additions, the updated read-only telemetry, both new WebGL screenshots, their paired JSON snapshots, and the final native/build/browser result files. The native check asserts the actual rendered Error Text for both `dt=0` and `dt=-1`. The browser checker types those values into the real nested field, reveals the message, asserts Run is disabled, restores `2`, then continues through the remaining flows.

[Zero denominator](../screenshots/pass2-positive-error.png) and [negative denominator](../screenshots/pass2-negative-denominator.png) visibly show `Must be positive` beside the command. Their paired snapshots independently record the denominator values `0` and `-1`, that exact message, and disabled Run. The denominator itself is outside the horizontal viewport at the capture position; the snapshots establish its typed value without claiming it is visible in the PNG.

The incompatible-drop browser test now targets the mass socket, which cannot accept a velocity expression. This is a valid correction to an ambiguous pointer destination: the former outer acceleration container could expose an inner compatible velocity socket. Production quantity compatibility has not been weakened. Telemetry only adds existing Error/DropError Text rectangles and contents to its report; it does not mutate gameplay.

The final evidence records 26/26 EditMode and 19/19 PlayMode passes, completed WebGL build job `06805ca6f8c74118941eca2e8d5ef357`, and 13 passing browser groups with `errors: []`. I independently reran the layout verifier after the fix, and it passed. I did not rerun native/build/browser jobs myself during this delta review.

## Visual assessment

I personally inspected all eight reference images and the actual final captures for title, selection, program, Settings, Hint, win, loss, paused scrolling, wide nesting, airborne tracking, both sidebars closed, 1280 by 720, and 1920 by 1080.

The gameplay captures have the requested 20/20/60 composition, full-height world, narrow left sidebars, inline equation sockets, connected command shapes, compact world icons, and separate upper-right Settings/Hint. The ordinary F = ma fields fit Active. Wide expressions visibly overflow into a horizontal scroll region instead of reducing their text size. These are materially different from the earlier form-card layout.

The title uses the supplied logo with one functional cyan PLAY oval. Selection has the required three-over-two square layout and four locks. Settings and Hint use plain dimmed overlays and the approved restrained content. The outcome row has status, Retry, Level Select, and the disabled next arrow in order. No score, campaign content, invented figure, trajectory, aiming arrow, or extra explanatory copy appears. The destination begins outside the narrow initial viewport, which is explicitly permitted for the fixed-zoom following camera. Its five segments are visible on the actual landing platform in the win capture.

Remaining visual roughness is minor: small remove/toolbar glyphs and simple block outlines could be more polished. Temporary colors, primitive geometry, and the engine Development Build watermark are within the documented scope and do not count as omissions.

## Source and evidence assessment

I independently ran `python3 Tools/verify-layout.py`. It passed all 64 migrated-asset checks, with five assemblies, four intended scenes, and 116 documentation links.

I read the approved specification, original UI audit, implementation report, runtime attempt/session/camera/navigation/UI code, formula catalog and validation, asset generation, native test source, browser checker, and saved test/build results. I did not modify implementation or assets, regenerate defaults, or rerun Unity/build/browser jobs during this review. The final saved evidence records 26 passing EditMode tests, 19 passing PlayMode tests, a completed final WebGL build, and 13 passing browser interaction groups with no captured errors. Those are prior executed checks, not newly executed independent checks.

The terminal step ordering is sound: evaluate real contacts/fall after the physics step, finish the clock without draining later commands when terminal, and only then consider ordinary exhaustion. That preserves final-step success precedence and prevents subsequent Weight/kicks from overwriting a terminal result. The target uses actual collider contacts and rejects the tested airborne, side, underside, and pre-existing-contact cases. Outer-color landings and repeat attempts have meaningful native coverage.

Baseline mechanics are retained. Formula quantities and calculations, shared edited mass, consumed immediately preceding Direction, standalone Weight gravity versus nested Weight force value, the 0.1-second force-to-impulse conversion, fixed-step Wait rounding, combined instant kicks, frozen completion, pause/resume, and snapshot restoration remain represented in code and native checks. The runner's optional no-drain completion supports terminal ordering without changing its default behavior. Sandbox disables level outcomes and retains normal developer mechanics completion.

Modal input is isolated through the parent CanvasGroup and active overlay, with focus moved inside and restored on close. Running opens as Paused and remains Paused after closing. Sidebar changes preserve the draft, cancel rather than commit a drag, reflow the viewport without changing zoom, and remain available during locked execution. Definitions and environment prefabs expose the replaceable fixture, spawn, camera configuration, target reference, fall boundary, and hint.

## Acceptance conclusion

No substantive missing features from pass2spec.md. Baseline mechanics retained. The numerical and physical scope is respected, and the reference-based presentation clears the 9/10 quality threshold. The sole required correction from round 1 is fixed and supported by rendered native and browser evidence. Final gate passes at 9.2/10, with no required corrections remaining.
