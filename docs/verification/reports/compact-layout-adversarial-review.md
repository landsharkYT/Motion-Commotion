# Compact layout adversarial review

Rating: 9.1/10. Final acceptance: pass.

Substantive missing features: no. The approved compact layout scope is implemented, and I found no substantive regression in the applicable pass 1, pass 2, or previous block-presentation behavior. The score exceeds the user's 9/10 gate. One pre-existing cosmetic discrepancy is recorded below; it does not prevent using or editing the program.

## Evidence reviewed

I reviewed the approved `docs/design/compact-block-layout.md`, applicable pass 1/pass 2 requirements, the earlier block-presentation and equation-pencil decisions, workspace rendering and width bindings, `EquationFlowLayout`, snapping and drag/drop handlers, shared card controls, prefab-generation changes, validation feedback, native regression tests, and browser checks. Concurrent speculative GDD edits are outside this acceptance scope.

I inspected the actual final `compact-default-1280.png`, `compact-default-1920.png`, `compact-palette.png`, `compact-nested.png`, and `compact-long-value.png` captures. Both target-size default palettes fit vertically. The position equation wraps into two mathematical lines, and the fields, adjacent actions, labels, and equation boundaries stay inside their sidebars. The nested capture shows the Force equation and its acceleration/velocity children in bounded linked rows, with ownership labels and editable inputs. The long-value capture shows internal field text scrolling rather than an enlarged block.

Final saved evidence is complete:

- `editmode-results.json`: 26 passed, zero failed or skipped.
- `playmode-results.json`: 26 passed, zero failed or skipped. This includes the 12-level valid equation tree, bounded indentation and fields, collapsed-sidebar binding retention, shared mass, linked time, snapping/reorder, typed nesting, physics, and session regression checks.
- `web-build-result.json`: successful completed build job `31b8b2fcf3a247058c65e8f36e333b1c`.
- `web-results.json`: `passed: true`, 21 interaction groups, empty errors array. The complete run includes both compact target sizes, long drafts, linked time, nested rows, invalid-drop preservation, removal/reorder, validation cleanup, paused vertical scrolling, win/loss, and responsive controls.
- `layout-results.json`: passed.

I read these final artifacts directly. I did not run or alter the shared Unity editor.

## Findings

No blocking correctness defect or substantive missing feature was found.

One low-severity pre-existing discrepancy remains at `Assets/Scripts/Runtime/UI/BlockWorkspace.Snapping.cs:30`. The actual-card drag ghost clones the source card's disabled pencil, while the earlier pencil-placement document and pass 2 section 5.4 say transient previews have none. The subsequent block-presentation decision calls for the actual block appearance, and that cloned appearance already existed before this compact pass. The ghost is noninteractive through its CanvasGroup, and this does not introduce an equation editor, change placement semantics, or remove a usable control. I treat it as a cosmetic documentation/behavior inconsistency, not a compact-layout regression or a substantive missing feature. The current checks do not explicitly resolve that historical detail.

## Requirement assessment

Both sidebar scroll containers are vertical-only in runtime setup, the saved gameplay prefab, and future prefab generation. Width bindings account for the actions column and margins. Term wrapping retains the fixed output and equals sign on the first line; continuation lines contain only the right-hand expression. Fraction bars, mean bars, multiplication, subscripts, units, and the raised exponent remain present. Numeric fields remain white and bounded, without discarding the full stored draft.

The top-level executable formula uses the connected command's visible surface. The inline equation surface and outline are transparent, eliminating the duplicate visible equation box while retaining independent formula removal and command removal. Nested expressions retain the domain tree and appear as linked rows with constant indentation and variable-path ownership. Longer ownership labels have measured wrapping heights. Vertical scrolling keeps deep trees accessible.

Bindings prune detached controls rather than hidden live controls, preserving sizing, shared mass, and linked inputs across collapsed-sidebar rebuilds. Palette values remain copied runtime templates, with mass retaining its shared physical meaning. Both occurrences of position time still reference one input. Typed compatibility and cycle rejection occur before source/target mutation. Connector geometry, invalid-drop preservation, single-command reorder, disabled per-equation controls, and literal restoration remain intact.

The implementation does not change the formula catalog, evaluator, force/Weight interpretation, Wait-only timing, or outcome rules. The complete native and browser evidence covers those preserved behaviors. Title/selection, independent sidebar collapse, fixed camera zoom, settings/hint pause behavior, Retry, and fresh-session navigation retain verification coverage.

## Limits and judgment

This is an independent source-and-artifact review, not proof of every possible numeric draft or pointer path. Small labels at 1280 by 720 and the denser linked-row presentation leave room for visual refinement, but the inspected captures meet the approved sizing and notation decisions without hiding required interactions. The final evidence supports acceptance at 9.1/10 with no substantive missing features.
