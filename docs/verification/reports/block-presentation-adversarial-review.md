# Block presentation adversarial review

Rating: 9.1/10 for the reviewed implementation and visual evidence.

Substantive missing features: no. I confirm that the approved block-presentation and snapping scope has no substantive missing features in the reviewed implementation. The stale validation-message regression found during final visual inspection is fixed and verified in both native and browser checks. No unresolved acceptance blocker remains.

## Scope and evidence

Reviewed `docs/design/block-presentation-and-snapping.md`, the applicable pass 1/pass 2 requirements, the workspace renderer and snapping code, drag/drop handlers, connector mesh, input factory, block controls, verification telemetry, native tests, and browser verification script. The concurrent GDD changes are outside this review's acceptance scope.

Inspected the actual `blocks-equation-palette.png`, `blocks-connected-stack.png`, `blocks-position-exponent.png`, `blocks-snap-preview.png`, and latest `pass2-nested.png` captures. These show white centered RHS fields, fixed outputs, readable variable/unit labels, fractions and mean bars, the raised exponent, aligned unequal-width connectors, the translucent drag block, and nested equations with adjacent disabled pencils. Long equations require the approved horizontal scrolling.

The saved corrected native results show 26/26 EditMode and 24/24 PlayMode tests passed, with no skips. Saved layout verification passes. The corrected WebGL build completed successfully, and the final browser run passed all 18 interaction groups with no captured errors. This review did not run or mutate the shared Unity editor.

## Findings

No unresolved blocking correctness defects or substantive missing features were found in the reviewed source.

A medium-severity regression was found during the lead agent's final visual inspection in `Assets/Scripts/Runtime/UI/BlockWorkspace.cs:108`. Rebuilding the program discarded error bindings for palette controls that remained alive. Typing an invalid mass and correcting it could leave a stale palette error visible beside the valid value. I independently confirmed that cause and reviewed the correction: rebuild now prunes inactive error bindings while preserving live palette bindings. The expanded native test reproduces the problem after two program rebuilds and requires both palette mass errors to clear after correction. The corrected native run passes all 50 tests, including this regression. The rebuilt browser player passes the explicit invalid-to-valid mass group and the additional scientific-input check. I inspected the final `pass2-invalid.png`: both palette mass values show 0.0001 without stale errors, while the deliberately unsafe Active acceleration correctly retains its own Out of range message. This finding is resolved.

One low-severity usability consideration remains in `Assets/Scripts/Runtime/UI/BlockWorkspace.Snapping.cs:70`. Inserting inside an existing stack shows the ghost and connection marker over the existing blocks; the following blocks move only after release. The marker identifies the insertion and satisfies the approved requirement. Opening a temporary gap would make the resulting order easier to preview, but that refinement is not required for acceptance.

The implementation preserves the relevant boundaries and behaviors:

- Palette values are runtime templates copied on placement. Existing commands retain their values; all mass fields still edit the shared physical mass.
- Both position-equation time fields reference the same numeric input and update together.
- Output labels cannot receive numeric edits. Equations render one equals sign, proper multiplication, subscripts, fraction bars, mean bars, and a raised power. The detached result line is removed as approved.
- Command placement checks connector distance. Invalid placement keeps the existing program, reorder removes and inserts one command, and value-only formulas cannot become top-level commands.
- Typed nesting checks output dimensions and cycles before changing the source or target. Compatible hover feedback remains separate from drop acceptance.
- Per-equation pencils remain disabled. Palette clicks have no append handler. Formula evaluation, physics timing, command validation, outcomes, and session ownership are preserved and retain native regression coverage.

## Evidence limit

The final corrected browser result has `passed: true`, 18 interaction groups, and an empty errors array. The saved build result identifies successful corrected build job `881b43d0fe0c47f88a788f973d611e99`. These results supersede the earlier incomplete browser run and the passing pre-correction run. The verification gate is satisfied.

This is an independent source-and-artifact review, not an exhaustive proof of every pointer trajectory or numeric string. The 9.1 score reflects the implemented scope, inspected visuals, and final corrected verification evidence.
