# Pass 2 implementation and verification

Status: complete. Independent adversarial review passed at 9.2/10; no required corrections remain.

The implementation follows the [approved specification](../../design/pass2spec.md) and [reference-image audit](../../design/pass2-ui-audit.md). The fixture is replaceable Level 1 content, with no final puzzle designs or additional levels.

## Implemented behavior

- Supplied title-logo crop and cyan PLAY, separate five-tile Level Select, four locks, and explicit title/selection navigation.
- Independent Blocks/Active left sidebars, edge collapse controls, hamburger reopen menu, and expanding world viewport.
- Compact connected block shapes, inline numeric fields, nested equation reporters, visible horizontal scrollbars, vertical scrolling, and distinct drag grips.
- Exactly Direction Blocks, Force Blocks, and Wait Blocks sections. The five equation tiles retain quantity-compatible value semantics; only F = ma and W = mg can also create their existing executable commands.
- Fixed-zoom horizontal/vertical camera following with configurable smoothing, offset, bounds, and Reset snap.
- Primitive gap, colored target, real top-contact success, fall/exhaustion failure, and immediate terminal freeze before later commands.
- Minimal outcome row: status, Retry, Level Select, disabled Next. Retry retains the draft; selection reentry starts fresh.
- Settings and Hint at upper right, modal input/focus isolation, explicit Resume after close, conceptual hint, disabled volume slider, and disabled pencil.
- No permanent player telemetry/log paragraphs, world aiming arrow, trajectories, score, progression, textures, new artwork, or authored campaign levels.

Core equation mathematics and physical operating limits are retained. ProgramRunner's step completion accepts an explicit no-drain option so the final physical step can advance its clock without applying subsequent commands after a landing/fall. The session owns that ordering; UI does not simulate physics.

## Verification evidence

| Check | Result | Evidence |
| --- | --- | --- |
| Project layout and preserved identities | Passed; all 64 migrated identities and five assemblies retained | [Layout results](../results/layout-results.json) |
| EditMode | 26 passed, zero failed | [Results](../results/editmode-results.json) |
| PlayMode | 19 passed, zero failed | [Results](../results/playmode-results.json) |
| WebGL | Completed with the final compact layout and disabled-slider thumb | [Build job](../results/web-build-result.json) |
| Browser interactions | 13 grouped checks passed, no captured browser errors | [Results](../results/web-results.json) |

Native integration tests include the original physics/lifecycle tests and nine new checks for actual gap reachability, outer target segments, side/air/underside contact rejection, pre-existing contact rejection, zero-time failure, fall, final-step success precedence, retained Retry configuration, panel/zoom invariants, modal pause behavior, and rendered zero/negative-denominator validation.

The browser checker uses actual mouse and keyboard interactions. Read-only telemetry supplies coordinates and physical values. It does not inject gameplay commands. It covers typed nesting, rejection, wide/deep horizontal scroll, reorder/removal, paused vertical scroll, overlays, physical success/failure, Retry, navigation, unsafe values, and desktop sizes.

## Visual evidence

The captures use the pass2 prefix under screenshots. The following comparison records the interpreted reference requirements rather than claiming a literal sketch reproduction:

| Reference requirement | Actual capture |
| --- | --- |
| Supplied logo and cyan oval PLAY, no extra copy | [Title](../screenshots/pass2-title.png) |
| Five square tiles, three above two, four locks, no figure | [Level Select](../screenshots/pass2-selection.png) |
| Two left sidebars, compact equations, large remaining world | [Program](../screenshots/pass2-program.png) |
| Each sidebar independently collapsible | [Active only](../screenshots/pass2-active-only.png), [Blocks only](../screenshots/pass2-blocks-only.png), [Both closed](../screenshots/pass2-both-closed.png) |
| Wide/deep equations scroll instead of shrinking text | [Nested equations](../screenshots/pass2-nested.png) |
| Fixed-zoom camera tracks the actual airborne orb | [Paused airborne view after tracking](../screenshots/pass2-airborne.png) |
| Settings with speaker and inert gray slider | [Settings](../screenshots/pass2-settings.png) |
| Minimal conceptual Hint | [Hint](../screenshots/pass2-hint.png) |
| Approved outcome row, with no invented result wording | [Win](../screenshots/pass2-won.png), [Loss](../screenshots/pass2-lost.png) |
| Locked editing with working long-program scrolling | [Paused scroll](../screenshots/pass2-paused-scroll.png) |
| Smaller and wider landscape sizes | [1280 by 720](../screenshots/pass2-1280.png), [1920 by 1080](../screenshots/pass2-1920.png) |

The basic F = ma inputs are asserted to fit inside Active without horizontal scrolling. Units sit above their compact inline sockets; wide nesting still uses the visible scrollbar. The gray orb and primitive gap replace the former boxed test chamber in player gameplay. Camera tracking, rather than overview zoom, reveals the destination. The [direct Unity title capture](../screenshots/pass2-editor-title.png) independently confirms the corrected logo crop contains no second baked PLAY button.

Earlier captures and failure results were diagnostic, not acceptance claims. Initial verification caught missing CanvasRenderer dependencies, a source-image crop assumption, horizontal wheel limitations, and a reorder drag initiated inside a numeric field. These were corrected through explicit icon requirements, measured cropping, visible mouse-accessible scrollbars, and distinct non-input drag grips.

## Scope limits

Unity development-player captures include the engine's Development Build watermark. It is not additional authored game-interface copy.

The six unresolved URP debug-resource references documented during the layout migration are pre-existing template content. This pass does not change that renderer asset. The final environment and puzzle content remain team work. No authored levels beyond the explicitly approved replaceable fixture were created.

## Adversarial correction

The [first independent review](pass2-adversarial-review-round1.md) rated the implementation 9.1/10 and found no substantive missing features, with one required wording correction. Strictly-positive inputs emitted Core's greater-than-zero message, which the UI mapped to Out of range. The adapter now maps that condition to Must be positive. A native UI regression and actual browser input cover both zero and negative denominators. See [zero](../screenshots/pass2-positive-error.png) and [negative](../screenshots/pass2-negative-denominator.png) feedback. Restoring a positive value clears the error and the full browser suite continues through success, failure, and navigation.

## Final acceptance

The [final independent review](pass2-adversarial-review.md) rates the implementation **9.2/10**, passes the gate, and concludes **No substantive missing features from pass2spec.md. Baseline mechanics retained.** The wording finding is closed. Final evidence is 26 passing EditMode tests, 19 passing PlayMode tests, a completed WebGL build, 13 passing browser interaction groups with no captured errors, and passing layout checks. The final Unity error snapshot also reports zero console errors and no compilation failure.

## Subsequent placement correction

The user subsequently clarified that the disabled pencil belongs beside each equation rather than in the world toolbar. The [per-equation correction report](equation-edit-affordance.md) records the updated placement and newer checks. The independent rating above describes the original reviewed pass 2 state; canonical result files and screenshots now reflect the later correction.
