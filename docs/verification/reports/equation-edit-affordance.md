# Per-equation edit affordance verification

Status: complete. Native tests, WebGL build, browser checks, visual captures, and layout verification passed.

The [approved correction](../../design/equation-edit-affordance.md) replaces the single toolbar pencil with disabled per-equation buttons. It does not implement the deferred edit menu.

## Result

- All five equation palette tiles have an outlined gray pencil-with-sparkles button beside their right edge.
- Each rendered equation in Active has one, including nested expressions and standalone Weight.
- Direction, Wait, enclosing command wrappers, literal-only force input, and drag previews have none.
- The world-toolbar pencil is removed from the saved gameplay prefab and both generation stages.
- Buttons remain disabled, have no action or keyboard-navigation target, and add no text.
- The shared card separates its colored Surface from its Actions column. Numeric fields and drag grips remain separate from edit/removal controls.
- Basic F = ma inputs and its pencil fit inside Active without horizontal scrolling. Wider nested equations retain horizontal scrolling.

Only the shared BlockCard and Gameplay prefabs were updated. The generator was corrected for future runs, but the full foundation generator was not run over level geometry, definitions, physical settings, or scene overrides. Concurrent GDD edits were not changed.

## Executed checks

| Check | Outcome | Evidence |
| --- | --- | --- |
| EditMode | 26 passed | [Results](../results/editmode-results.json) |
| PlayMode | 20 passed | [Results](../results/playmode-results.json) |
| WebGL build | Completed | [Build result](../results/web-build-result.json) |
| Browser interaction groups | 14 passed, including nested-pencil reachability, with no captured browser errors | [Results](../results/web-results.json) |
| Layout and preserved asset identities | Passed | [Results](../results/layout-results.json) |

The new native check verifies palette eligibility, per-expression ownership, no duplicate command-wrapper buttons, disabled state, empty action bindings, inert invocation, and placement outside the equation silhouette. It also verifies literal Force has none and standalone Weight has exactly one.

The browser uses real pointer input to click the disabled active-equation pencil, checking that the program, time, and modal state do not change. Counts distinguish five palette buttons, one additional basic Force equation, and three additional rendered equations for a nested expression chain. Existing edit, nesting, reorder/remove, scrolling, modal, physics outcome, and navigation checks remain active.

The first PlayMode invocation remained pending without entering Play Mode. It was cancelled and restarted from the saved Title scene; all 20 tests then passed.

## Actual player captures

- [Five palette pencils, with none on Direction or Wait](../screenshots/equation-edit-palette.png)
- [Active Force equation with its adjacent pencil and readable inputs](../screenshots/equation-edit-active.png)
- [Nested equation placement](../screenshots/equation-edit-nested.png)

These are captures of the rebuilt player. No formula-edit menu, algebra manipulation, or new block behavior was added.

The final nested capture scrolls to the innermost equation button and shows all three equation-specific pencils. All 14 browser groups passed after this accessibility check was added.

The final Unity console snapshot reports zero errors, zero warnings, and no compilation failure. Level scenes, level definitions, and environment prefabs are unchanged. Unity refreshed the existing Input Actions reference in PlayerSettings.preloadedAssets during verification; no physical defaults were altered.
