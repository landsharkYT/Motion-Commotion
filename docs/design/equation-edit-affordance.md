# Equation edit affordance correction

Status: all five recommendations approved, implemented, and verified. The audit below describes the earlier placement.

## Reported discrepancy

The supplied reference crop shows a small pencil-with-sparkles button beside an equation block. The current implementation has one disabled pencil in the world toolbar and none beside equation blocks. The pass 2 specification also records the toolbar interpretation. That interpretation missed the requested per-block association.

The intended affordance represents the edit menu for a particular equation. Its placement does not by itself bring the deferred equation editor or rearrangement mechanics into scope.

## Current implementation

- PassTwoAssets creates the disabled global EquationEdit control under WorldHUD/Controls.
- EquationEditIcon already draws a pencil and sparkles; it can be reused.
- BlockCard and its prefab have no edit-button binding or child.
- BlockWorkspace creates five equation palette tiles, executable command wrappers, and recursively nested equation cards.
- Force/Weight command wrappers enclose rendered equation cards. Adding an icon indiscriminately to every card would duplicate icons and put them on Direction/Wait.
- Removing a root Force expression can leave a literal Force command with no displayed equation. Its eligibility must follow the agreed definition of equation editing.
- The browser checker currently proves only that one matching EquationEdit control is disabled. It does not verify per-block placement, eligibility, or count.
- Both generator stages contain the global-toolbar interpretation, although the pass 2 stage replaces the earlier canvas.

## Decisions to settle

1. Whether all five equation tiles qualify, with Direction and Wait excluded.
2. Whether eligible equations carry the affordance in the palette, Active, and nested expressions.
3. Whether the global toolbar pencil is removed.
4. Whether the new per-equation buttons remain gray, disabled, and inert.
5. Placement beside the equation silhouette, separate from removal and numeric input, while preserving the basic Force block's fit and drag targets.

After confirmation, update the pass 2 specification, prefab generation, actual prefabs, and placement-aware verification. Preserve the concurrent GDD edits and keep formula-editor behavior deferred unless explicitly requested.

## Confirmed behavior

The user approved all five recommendations. Every one of the five equation tiles gets a pencil in the palette, and every rendered equation gets one in Active, including nested expressions. Direction, Wait, enclosing command wrappers, literal-only force values, and drag previews have none. The toolbar pencil is removed.

The control is a small gray outlined square containing the existing pencil-with-sparkles graphic. It sits immediately to the right of the equation silhouette, outside the numeric-input/drag area and separate from removal. It stays disabled, has no action or keyboard navigation target, and opens no menu. No labels, equation editor, or rearrangement behavior are added.

The block prefab separates its colored Surface from an adjacent Actions column. This reuses the former remove-control width rather than widening the basic Force equation unnecessarily. Ordinary F = ma inputs and its pencil must both remain visible inside Active without horizontal scrolling.

Only the shared BlockCard and Gameplay prefabs need a targeted update. Level geometry, definitions, physical defaults, and scene overrides do not need regeneration. Both generator stages must omit the old toolbar button, and future default-card generation must reproduce the corrected layout.

## Verification

- Exactly five palette pencils, all disabled. No pencil on Direction or Wait.
- Exactly one button on each actual rendered equation, including nested values and standalone Weight; no duplicate on its command wrapper.
- A literal Force value has no pencil.
- No world-toolbar pencil.
- Buttons sit outside the equation silhouette, have no action, and do not affect time, draft, state, or modal display.
- Basic F = ma inputs and pencil fit Active. Dragging, nesting, scrolling, removal, and execution retain their behavior.

Executed checks and player captures are recorded in the [verification report](../verification/reports/equation-edit-affordance.md).
