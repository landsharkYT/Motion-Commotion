# Compact block layout verification

Status: complete. The [independent adversarial review](compact-layout-adversarial-review.md) rates this pass 9.1/10 with no substantive missing features.

The [approved compact layout](../../design/compact-block-layout.md) replaces horizontal sidebar scrolling with bounded cards, wrapped mathematical terms, and linked child equations.

## Result

- Blocks and Active scroll vertically. Their horizontal scrollbar objects are removed from the saved gameplay prefab, and the generator creates vertical-only containers.
- All seven default palette entries fit at 1280×720 and 1920×1080. The existing 20/20/60 screen allocation and sidebar collapse behavior remain.
- The position equation wraps into two lines with its ordinary defaults. Wider terms can wrap further while keeping every numeric input on the right of the fixed output.
- Compact fractions retain fraction bars and mean-variable overbars. Their variable/unit labels sit beside the white fields.
- Input fields, typography, card padding, and actions are smaller. The executable formula's extra visible inner box is removed while preserving formula and command removal.
- Nested equations appear in linked child rows with variable-path labels. Indentation stays fixed as nesting deepens; all fields remain accessible through vertical scrolling.
- Long numeric drafts retain their complete values inside bounded single-line fields. Internal text scrolling handles longer values during editing.
- Shared mass, palette template copying, linked time fields, typed nesting, disabled pencils, snapping, and simulation semantics remain covered by verification.

## Checks

| Check | Result | Evidence |
| --- | --- | --- |
| EditMode | 26 passed | [Results](../results/editmode-results.json) |
| PlayMode | 26 passed | [Results](../results/playmode-results.json) |
| Browser interaction groups | 21 passed; no captured page or console errors | [Results](../results/web-results.json) |
| WebGL build | Completed | [Build result](../results/web-build-result.json) |
| Layout and asset identities | Passed | [Results](../results/layout-results.json) |
| Unity console | No errors or compilation failure | [Results](../results/compact-console-results.json) |
| Independent review | 9.1/10; no substantive missing features | [Review](compact-layout-adversarial-review.md) |

Two new PlayMode tests verify real control bounds in the palette and a 12-level nested formula tree, constant child indentation, preserved long input values, and bindings retained across collapsed-sidebar program rebuilds. Existing native physics and block-presentation tests pass.

The browser uses real pointer and keyboard input. It checks full card/control bounds, the absence of horizontal scrolling, all seven default entries at both target resolutions, long numeric editing, linked equation ownership, nesting/removal, snapping/reordering, validation, modals, physics outcomes, paused scrolling, and navigation.

## Captures

- [Default palette at 1280×720](../screenshots/compact-default-1280.png)
- [Default palette at 1920×1080](../screenshots/compact-default-1920.png)
- [Compact palette and wrapped position equation](../screenshots/compact-palette.png)
- [Linked nested equations](../screenshots/compact-nested.png)
- [Bounded long numeric value](../screenshots/compact-long-value.png)
- [Edited program at 1280×720](../screenshots/compact-1280.png)
- [Edited program at 1920×1080](../screenshots/compact-1920.png)

Only the shared gameplay/card prefabs and UI rendering/generation/verification code were changed for this pass. Scene and level assets were preserved. Concurrent GDD edits were left intact.
