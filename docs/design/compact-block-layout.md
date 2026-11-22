# Compact block layout audit

Status: complete. Native tests, browser verification, target-resolution captures, and independent adversarial review passed.

## Request

The current blocks occupy too much space, and the position equation extends beyond the palette. The user wants a more compact fullscreen presentation and questions the horizontal scrollbar.

This revisits the previous [block presentation decision](block-presentation-and-snapping.md), which explicitly allowed horizontal scrolling to keep long equations readable.

## Preserved requirements

- Equation inputs remain on the right of a fixed output and equals sign.
- Editable numbers remain white fields, with proper mathematical notation.
- Shared mass, copied palette templates, linked time occurrences, typed nesting, disabled equation pencils, and connector snapping remain functional.
- The current five formulas and deferred equation editor remain the scope.

## Confirmed layout facts

CanvasScaler uses Scale With Screen Size, reference 1440 by 900, and a 0.5 width/height match. At both 1280 by 720 and 1920 by 1080, the logical canvas is approximately 1518 by 854. Each sidebar has roughly 298 logical units of usable viewport width. Fullscreen increases physical pixel size, but does not give equations more logical room at the same aspect ratio.

The position equation, actions, and content padding currently need roughly 503 logical units. That is about 1.7 times the available width. A modest font reduction alone cannot solve the overflow.

## Initial design frontier

- Whether horizontal movement must disappear from both sidebars, rather than merely hiding its scrollbar.
- Primary fullscreen resolution and smallest supported landscape window.
- Whether compactness must retain the current 20 percent Blocks, 20 percent Active, 60 percent world split.
- Whether palette and program use the same compact presentation, retaining their existing behavior.

The source-and-artifact audit confirmed these sizing constraints. These questions were resolved by the user's first-round approval below.

## Confirmed direction

The user accepted all four first-round recommendations:

1. Both sidebars must work without horizontal scrolling; vertical scrolling remains available.
2. Target 1920 by 1080 fullscreen and verify usability at 1280 by 720.
3. Keep the current 20/20/60 allocation while both sidebars are open, including existing collapse behavior.
4. Compact both palette and Active presentation through reduced padding, shorter fields, moderate typography reductions, and removal of the extra enclosing box around executable equations. Preserve readable variables, units, white numeric fields, and mathematical notation.

## Approved layout

The user approved these layout decisions.

### Common sizing

- Derive each card's maximum width from its current sidebar viewport, including controls and margins.
- Start with math text at 16 logical units, numbers at 14, and labels at 11. Check the actual font bounds and player captures at both target sizes before accepting these sizes.
- Use white fields around 26 units high with a 36-unit minimum width. Keep comfortable drag areas and bounded field widths.
- Reduce repeated vertical padding and unused layout spacing. Preserve shared connector geometry and edge alignment.
- Render a top-level executable formula as one colored connected card instead of a stack wrapper containing another full equation box. Retain its disabled pencil and removal action.
- Aim for all seven palette entries and their category headings to fit vertically with ordinary default values at the target sizes. Vertical scrolling handles expanded content and errors.

### Position equation

Use mathematical term groups rather than a single unbounded row. With default values, target two lines:

```text
s = [2] + [3] × [2]
    + ½ × [4] × [2]²
```

Variable/unit labels remain outside the white fields. Both time fields remain linked. Continuation lines stay within the right-hand expression; they do not introduce another equals sign. Additional line breaks handle wider values without shrinking text below the readability target.

### Fractions

Keep actual fraction bars and mean-variable overbars. Put variable/unit labels beside the numerator and denominator fields to reduce their stacked height. Default fraction cards should be roughly 75–85 logical units high, compared with the current 131, subject to measured layout and readable captures.

### Nested formulas

Show attached formulas as linked child rows below their owning equation instead of recursively extending the equation sideways. Each row receives a bounded width. Use a capped visual indent and show which right-hand input owns each child; do not shrink the available width at every nesting depth.

The full formula tree stays visible and editable through vertical scrolling. This is a presentation of the existing expression tree, not the deferred equation-edit menu. Every displayed equation keeps its disabled pencil. Drag/drop still targets compatible right-hand slots, and moving an executable command moves its associated expressions together.

### Long numeric values

Bound numeric field widths so a single long draft cannot force a card outside the sidebar. Normal single-line editing may scroll the text inside the focused field. Preserve the complete stored value, validation, and keyboard access. This does not permit horizontal scrolling of the sidebar or block program.

## Final approved decisions

- Approve wrapped position equations and shorter stacked fractions with labels beside fields.
- Approve full-width linked child rows for nested formulas, with all inputs accessible by vertical scrolling.
- Approve internal text scrolling for unusually long numeric values inside bounded fields.

## Acceptance checks

- At 1280 by 720 and 1920 by 1080, neither sidebar has horizontal scrolling, and no block or action is clipped outside its viewport width.
- Ordinary default palette entries fit vertically at both sizes.
- Wrapped position equations preserve operator order, labels, linked time values, and fixed left-hand output.
- The existing Force → average acceleration → average velocity nesting example fits within Active and retains every editable field and equation pencil.
- Deeper valid nesting does not continually consume additional horizontal indentation.
- Long numeric drafts remain editable and retain their complete values without increasing card width beyond the viewport.
- Snapping, reordering, formula compatibility, mass synchronization, validation cleanup, and existing physics behavior continue to pass verification.

The user accepted all three remaining recommendations: mathematical wrapping and compact fractions, linked child rows for nesting, and internal text scrolling for long numeric drafts.

## Implementation notes

EquationFlowLayout places mathematical terms within the available width and moves continuation rows below the right-hand expression. It supports another break inside a long term when numeric fields grow. Card widths include their actions and margins.

Nested expressions retain the existing domain tree. The renderer displays their equations in a flat list of linked child rows under the executable command. Each row identifies its parent input using a variable path and unit. Indentation remains fixed at eight logical units regardless of expression depth.

The outer executable surface contains the primary equation directly, with the former inner equation surface and outline made transparent. This preserves formula removal/replacement and command removal while eliminating the visible duplicate box and its padding.

Live layout and input bindings remain registered while a sidebar is collapsed. Only detached or destroyed controls are pruned, so hidden palette values and sizing remain synchronized after program rebuilds.

Only the shared BlockCard and Gameplay prefabs were targeted. Both sidebar scrollbar objects were removed from the saved gameplay prefab, and the generator now creates vertical-only scroll containers. No full level regeneration was performed.
