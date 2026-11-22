# Block presentation and snapping verification

Status: complete. The [independent adversarial review](block-presentation-adversarial-review.md) rates the implementation 9.1/10 with no substantive missing features or unresolved acceptance blockers.

The [approved specification](../../design/block-presentation-and-snapping.md) covers the five equation tiles, numeric editing, and executable-block connections. Physics and authored-level scope are unchanged.

## Result

- Palette templates and Active equations use opaque white input fields with centered values. Fixed outputs remain on the left; variable and unit labels remain outside the fields on the right.
- Equations use multiplication signs, fraction bars, subscripts, overbars, and a raised exponent. DejaVu Sans supplies the mathematical glyphs; its redistribution license is included in Assets/Fonts.
- Both time occurrences in the position equation edit the same value. Palette template values are copied on placement, while mass remains shared with the physical ball.
- The detached evaluated-result line is removed. Developer measurements and underlying calculations remain available.
- Executable blocks share connector geometry, show a translucent block copy and connection marker while dragging, and join with exact connector overlap on release.
- Empty Active displays a faint starting target. Invalid new drops create nothing; invalid moves retain their existing command. Reordering moves one command and reconnects its neighbors.
- Compatible equation inputs highlight while hovering. Typed nesting, incompatible-unit rejection, and cycle rejection remain in place.
- Disabled per-equation pencils, sidebar collapse, horizontal scrolling, and simulation controls remain available. Palette header clicks no longer append commands.

## Verification

| Check | Result | Evidence |
| --- | --- | --- |
| EditMode | 26 passed | [Results](../results/editmode-results.json) |
| PlayMode | 24 passed | [Results](../results/playmode-results.json) |
| WebGL build | Completed | [Build result](../results/web-build-result.json) |
| Browser interaction groups | 18 passed; no captured page or console errors | [Results](../results/web-results.json) |
| Layout and preserved asset identities | Passed | [Results](../results/layout-results.json) |
| Unity console | No errors or compilation failure | [Console result](../results/block-presentation-console-results.json) |
| Independent review | 9.1/10; no substantive missing features | [Review](block-presentation-adversarial-review.md) |

Four new PlayMode checks cover notation and field placement, template copying and shared values, snapping and rejected moves, and compatible nesting feedback. The existing small-value check now verifies editable input text and the formula calculation, following the approved removal of the result line.

Visual inspection caught missing glyphs, fraction-tile alignment, wrapped variable labels, and a clipped scientific value. The bundled font and measured text widths correct these. Native checks verify glyph coverage, common palette alignment, single-line label bounds, and the complete visible scientific value.

A final capture also exposed stale palette mass errors after invalid-to-valid typing. Rebuild now prunes inactive error bindings while preserving live palette bindings. Native tests reproduce the sequence after multiple rebuilds and verify both palette messages clear. Browser checks type invalid then valid mass, verify clearing, and repeat the check after entering a small mass and an unsafe scientific value.

One headless-browser run closed unexpectedly without captured page or console errors. An unchanged rerun completed successfully. The final corrected build then passed all 18 browser groups, including the new validation regression.

The Unity verification script leaves Play Mode before starting tests, captures failed results from Pipeline's local status file when its CLI returns an error, and retries the observed stalled PlayMode startup once.

## Player captures

- [Palette inputs and mathematical notation](../screenshots/blocks-equation-palette.png)
- [Connected executable stack](../screenshots/blocks-connected-stack.png)
- [Linked time field and raised exponent](../screenshots/blocks-position-exponent.png)
- [Block-shaped drag and connection preview](../screenshots/blocks-snap-preview.png)
- [Nested equations under horizontal scrolling](../screenshots/pass2-nested.png)
- [Visible scientific input and corrected palette validation](../screenshots/pass2-invalid.png)

Insertion previews overlay the current stack; following blocks move on release. The reviewer identified opening a temporary gap as optional future polish. The approved connection marker and final placement are implemented.

The full foundation generator was not run over team level assets. Concurrent GDD edits were preserved.
