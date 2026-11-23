# Block presentation and snapping audit

Current numeric editing and speed-cap contract: [input validation](input-validation.md). This supersedes earlier numeric draft and kick-budget rules.
Later behavior authority: the [standalone equation contract](standalone-equation-actions.md) supersedes the first three equations' nested-only restriction. It defines current action semantics and implementation status.

Status: complete. Implementation, native tests, browser interaction checks, visual inspection, and independent adversarial review passed.

The later [compact layout specification](compact-block-layout.md) supersedes this pass's horizontal-scrolling choice and the placement of labels above fraction inputs.

## Requested requirements

- Equation inputs must appear only to the right of the equals sign. The left side is output, never an editable input.
- Editable values must use an open white square or rounded-square field that clearly signals typing.
- Executable blocks must join by their connector edges, with no sound required.
- Equations need proper mathematical notation and alignment, including multiplication and raised exponents.
- Preserve existing physics, typed formula compatibility, single-program execution, per-equation disabled pencils, and the deferred equation-editor scope unless the user explicitly changes them.

## Baseline audit

The active equation's output and operators are already read-only; its numeric fields are on the right. However, the equation presentation is assembled from plain labels and individually laid-out fields. A separate read-only result line such as `= 10 N` adds another equals sign below it.

The palette renders the whole equation as plain text. Its Direction 0 and Wait 1 are also labels, not editable fields. Therefore the current palette has no white input sockets. Active uses white rectangular InputFields, with left-aligned values and additional translucent rectangles around variable/unit groups.

The renderer uses programming-style text: asterisks, slashes, v avg, s0, v0, and t^2. Text defaults to upper-left alignment. There are no fraction bars or mathematical baseline layout. A proper presentation can be composed from symbols, fraction bars, editable fields, and raised/lowered text without changing the evaluator or adding symbolic algebra.

The position equation stores one time input. Its first occurrence is editable, while the final t^2 is currently literal text. The design needs to state how repeated occurrences display and edit that same value.

Dragging currently shows a generic blue text rectangle. Command drops use cursor Y versus block centers to choose insertion. No connector proximity test, insertion preview, or actual block-shaped drag preview exists. A vertical layout with spacing -7 approximates joined blocks.

Connector horizontal locations vary with each surface's width: a=min(30,width*0.2) and b=min(68,width*0.45). Left-aligning blocks of different widths does not guarantee that their sockets and tabs coincide. The geometry and snap-target layout need one shared connector convention.

Clicking a palette header also appends executable commands. This shortcut currently bypasses any edge-based placement interaction.

## Existing infrastructure

The project uses UGUI Text and InputField with LegacyRuntime.ttf. The installed Unity UI package includes TextMeshPro source and its essential-resource package locally, but the project has no imported TMP/font assets or TMP assembly reference. Choice of rendering mechanism is an implementation responsibility; no external equation parser or algebra library is required for five explicit formulas.

The current per-equation pencil correction and concurrent GDD edits are existing work. Preserve them. Continue using targeted prefab changes rather than regenerating unrelated geometry and configuration.

## Approved decisions

The user accepted all nine recommendations in the first design round.

1. The palette contains real editable white numeric fields. Editing a template changes subsequent copies only, except mass, which remains the shared physical mass of the ball. Existing program values otherwise remain unchanged.
2. Fields start with valid numeric defaults. Variable names and units appear outside the white editing area.
3. Palette and Active use multiplication signs, stacked fractions, subscripts, superscripts, and overbars for mean quantities. Values are centered, and operators and fields align within each mathematical expression.
4. Each displayed equation has one equals sign with a fixed, read-only output symbol on the left. Remove the detached evaluated-result line. Calculations and developer telemetry remain available internally.
5. Executable blocks use a block-shaped drag preview, connector proximity detection, and a visible insertion preview. Release at a valid connection joins the edges exactly. Invalid new drops create nothing; invalid moves retain the original program. There is one vertical executable stack and no sound.
6. Reordering moves one command at a time. Neighbors reconnect. Moving a connected tail is deferred.
7. Remove the palette header click-to-append shortcut. Users place blocks by dragging and snapping; clicking a white field edits its value.
8. Small variable and unit labels sit above equation inputs. Direction and Wait have white degree and second fields without artificial equals signs. The right-side-only rule applies to equations.
9. Both occurrences of time in the position equation are editable white fields linked to the same time input within that expression. Editing either updates the other. The second occurrence has a raised exponent.

Keep the existing per-equation disabled pencil buttons. This work does not add equation rearrangement, a general formula editor, new physics, sound, or authored levels.

## Final placement and layout decisions

The user accepted all three final recommendations.

- Empty Active shows a faint connector-shaped start target near its top-left. The first executable block snaps there; further commands join existing connections.
- A compatible equation input highlights on hover. Release nests the equation. An incompatible drop keeps the existing value and uses the existing short error.
- Long equations retain readable font and input sizes. Sidebars scroll horizontally rather than shrinking the expression.

## Verification targets

- Palette and Active expose editable values only on the right of equation equals signs; output symbols cannot receive focus or edits.
- Numeric fields have opaque white backgrounds and centered values. Labels remain outside them.
- All five formulas use the approved notation, with no visible programming asterisks or caret exponents.
- Repeated time fields stay synchronized. Palette defaults affect newly placed copies without mutating existing commands; mass stays globally synchronized.
- Unequal-width executable blocks share connector offsets and visibly meet without gaps or mismatched edges.
- Drag previews show the actual block appearance and highlight the proposed insertion. Drops outside valid placement targets leave the program unchanged.
- Single-command reorder, compatible nested formulas, incompatible-unit rejection, cycle rejection, and disabled equation pencils continue to work.
- Existing physics and session behavior pass the native and browser verification suites. Capture the revised palette, joined stack, nested formula, and snap preview for visual review.

## Implementation

The UI composes the five equations from text symbols, opaque white numeric fields, fraction bars, a mean-variable overbar, and a raised exponent. It does not parse equation strings or introduce symbolic algebra. The workspace uses the bundled DejaVu Sans font for mathematical character coverage, with its license beside the font asset in Assets/Fonts.

Palette templates live in the workspace for the current visit. Placing a block copies its values and nested expression data. Template edits do not mutate ScriptableObject defaults. Mass fields continue to edit the shared physical mass.

All executable surfaces share connector offsets of 30 and 68 UI units and a depth of 7. The workspace accepts a connection within 36 UI units, shows a marker at that connection, and aligns a translucent copy of the dragged block with the proposed position. Insertion before an existing command uses that command's top position; release moves the following commands down. The program retains room below its last command so users can reach its final connector in a scrolling sidebar.

Equation rendering and snapping are separated into BlockWorkspace.Equations.cs and BlockWorkspace.Snapping.cs. Physics, the formula evaluator, and program execution are unchanged.
