# Pass 2 independent adversarial review

Review date: 2026-11-20. Score: **9.1/10**. One required validation-wording correction remains before acceptance. No substantive feature omission was found.

## Finding

**P2: strictly positive equation inputs receive the wrong actionable message.** In `Assets/Scripts/Runtime/UI/BlockWorkspace.cs:151`, `ShortError` recognizes the word `positive`. The actual error from `FormulaEvaluator.Read`, at `Assets/Scripts/Core/Formulas.cs:126`, is `dt must be greater than zero.` Therefore a zero or negative denominator in Average velocity or Average acceleration renders `Out of range`, rather than the specified `Must be positive`. The mathematics and execution rejection are correct, but the player sees the wrong recovery cue.

I independently evaluated the mapper's branches against that actual error string and obtained `Out of range`. This is a source-confirmed finding; I did not run a new Unity/browser reproduction. Recognize the existing positivity error wording, then verify zero and negative denominator input through the rendered interface. The current passing division tests only prove rejection in Core and do not detect this UI mismatch.

## Visual assessment

I personally inspected all eight reference images and the actual final captures for title, selection, program, Settings, Hint, win, loss, paused scrolling, wide nesting, airborne tracking, both sidebars closed, 1280 by 720, and 1920 by 1080.

The gameplay captures have the requested 20/20/60 composition, full-height world, narrow left sidebars, inline equation sockets, connected command shapes, compact world icons, and separate upper-right Settings/Hint. The ordinary F = ma fields fit Active. Wide expressions visibly overflow into a horizontal scroll region instead of reducing their text size. These are materially different from the earlier form-card layout.

The title uses the supplied logo with one functional cyan PLAY oval. Selection has the required three-over-two square layout and four locks. Settings and Hint use plain dimmed overlays and the approved restrained content. The outcome row has status, Retry, Level Select, and the disabled next arrow in order. No score, campaign content, invented figure, trajectory, aiming arrow, or extra explanatory copy appears. The destination begins outside the narrow initial viewport, which is explicitly permitted for the fixed-zoom following camera. Its five segments are visible on the actual landing platform in the win capture.

Remaining visual roughness is minor: small remove/toolbar glyphs and simple block outlines could be more polished. Temporary colors, primitive geometry, and the engine Development Build watermark are within the documented scope and do not count as omissions.

## Source and evidence assessment

I independently ran `python3 Tools/verify-layout.py`. It passed all 64 migrated-asset checks, with five assemblies, four intended scenes, and 116 documentation links.

I read the approved specification, original UI audit, implementation report, runtime attempt/session/camera/navigation/UI code, formula catalog and validation, asset generation, native test source, browser checker, and saved test/build results. I did not modify implementation or assets, regenerate defaults, or rerun Unity/build/browser jobs during this review. The saved evidence records 26 passing EditMode tests, 18 passing PlayMode tests, a completed final WebGL build, and 12 passing browser interaction groups with no captured errors. Those are prior executed checks, not newly executed independent checks.

The terminal step ordering is sound: evaluate real contacts/fall after the physics step, finish the clock without draining later commands when terminal, and only then consider ordinary exhaustion. That preserves final-step success precedence and prevents subsequent Weight/kicks from overwriting a terminal result. The target uses actual collider contacts and rejects the tested airborne, side, underside, and pre-existing-contact cases. Outer-color landings and repeat attempts have meaningful native coverage.

Baseline mechanics are retained. Formula quantities and calculations, shared edited mass, consumed immediately preceding Direction, standalone Weight gravity versus nested Weight force value, the 0.1-second force-to-impulse conversion, fixed-step Wait rounding, combined instant kicks, frozen completion, pause/resume, and snapshot restoration remain represented in code and native checks. The runner's optional no-drain completion supports terminal ordering without changing its default behavior. Sandbox disables level outcomes and retains normal developer mechanics completion.

Modal input is isolated through the parent CanvasGroup and active overlay, with focus moved inside and restored on close. Running opens as Paused and remains Paused after closing. Sidebar changes preserve the draft, cancel rather than commit a drag, reflow the viewport without changing zoom, and remain available during locked execution. Definitions and environment prefabs expose the replaceable fixture, spawn, camera configuration, target reference, fall boundary, and hint.

## Acceptance conclusion

No substantive missing features from pass2spec.md. Baseline mechanics retained. The numerical and physical scope is respected, and the reference-based presentation clears the 9/10 quality threshold. Fix and verify the specific positive-input validation message above before final acceptance; no redesign or additional campaign features are needed.
