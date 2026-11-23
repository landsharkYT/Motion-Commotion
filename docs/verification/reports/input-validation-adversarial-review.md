# Independent input validation review

Verdict: **9.3/10. No substantive approved features are missing.** No blocking correctness or regression findings remain in the reviewed change.

This verdict covers the [approved numeric contract](../../design/input-validation.md#approved-implementation-contract) and preservation of the [pass 1 foundation](../../design/pass1spec.md) under later approved changes. Pass 2 supersedes the original title, presentation, and no-outcome restrictions. The standalone equation contract supersedes the original nested-only restriction. The numeric contract supersedes invalid-draft preservation and old kick/velocity budgets. Those historical rules are not missing features to restore.

## Review scope

Independently inspected changed C# source against baseline `a2e9c59`, new numeric policy/editor components, native regression tests, the browser verification changes, final result files, and the two screenshots below. This review did not run another Unity editor, test, or build process. Unrelated user changes to `.gitignore` and the GDD were excluded.

The source review traced decimal parsing and normalization, nested units/signs/ranges and finite results, total Wait limits, commit-only shared bindings, invalid-draft execution and placement gates, rejection of a gesture ending an invalid edit, immediate kick/velocity clamping, pre-integration acceleration/gravity handling, reset behavior, and restoration of global physics settings. The inactive-parent lookup now supports rebuilding a program while both sidebars are collapsed; the native regression exercises Direction, Force, and Wait together in that state.

No substantive missing pass 1 features were identified after applying the later approved specifications. The retained foundation includes the four scenes and shared runtime, five formula calculations, pure compatible nesting, direction adjacency, fixed 0.1-second kick convention, Wait-only clock advancement, mass/gravity ownership, pause/reset/clear behavior, disabled equation editing, and scene lifecycle cleanup. Existing mechanics tests and the final browser regression exercise these alongside the newer equation actions and interface flows.

## Evidence

| Evidence | Observed result |
| --- | --- |
| [EditMode results](../results/editmode-results.json) | 52 passed, zero failed or skipped |
| [PlayMode results](../results/playmode-results.json) | 43 passed, zero failed or skipped |
| [Layout results](../results/layout-results.json) | Passed |
| [Final Web build](../results/web-build-result.json) | Job `379f78e8894c4579a85172cf34c39602` completed successfully, no reported errors |
| [Final browser results](../results/web-results.json) | Passed all 27 checks, empty errors list |
| [Nested invalid denominator](../screenshots/pass2-positive-error.png) | Readable labeled Δt range below the nested equation, red outline on the offending input, unavailable Run |
| [Speed warning](../screenshots/speed-cap-warning.png) | Exact approved warning appears in red in the play area; controls remain visible and the exaggerated input is retained |

The physical regression checks actual displacement as well as final velocity: a 20 m/s configured cap permits at most 0.401 m in a 0.02-second high-acceleration step. Another check exercises extreme opposing gravity and commanded acceleration without a false cap warning. Native checks cover warning hold/fade/reset timing; browser checks cover the rendered warning, retained input, capped measured velocity, and Reset clearing it.

## Limits of the verdict

The browser evidence covers desktop Chromium at 1280×720 and 1920×1080. It does not establish behavior in every browser or with every input method. Phone/touch layout is explicitly deferred by the approved scope. These coverage limits do not leave a substantive approved feature unimplemented. The score reflects strong source and runtime evidence, without treating finite automated cases as exhaustive proof.
