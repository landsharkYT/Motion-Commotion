# Outcome and Controls menus interview

Status: behavior and exact UI copy approved by the user. Implemented and verified.

## User request

Replace the unintuitive retry/outcome presentation. Add a Settings button labelled Controls, in the annotated area, matching the existing Return to title button's size. Open a minimal reference explaining controls, including block dragging and supported keyboard shortcuts.

## Audit

The current outcome is a wide horizontal row containing a cross/check status icon, an unlabelled reset-arrow Retry button, Level Select, and an unavailable Next arrow. There is no outcome heading. The cross is a status indicator rather than a close control, which is visually ambiguous. Next has no implementation and is disabled for both outcomes.

Retry restores the pre-run physical snapshot, camera defaults, Follow view and Ready editing state. It preserves the program and sidebar visibility and never automatically runs the program. The outcome cannot currently be dismissed through Escape or outside clicks.

Settings currently contains its heading, close X, disabled volume slider and Return to title. The return button occupies 50% of the modal width and 12% of modal height. Controls can match those anchored dimensions and sit above it without changing the slider. Current Settings/Hint overlays pause running physics, block underlying controls, restore focus on close, and leave the simulation paused.

## Verified controls

- Blocks: drag by header/grip from Blocks to an Active connector, reorder by dragging, remove via × or return drag to Blocks; nest formulas in compatible inputs and detach with × or return drag.
- Values: enter decimal text; Enter/keypad Enter or focus loss commits; Escape cancels the current text edit through UGUI's existing InputField behavior.
- Playback: toolbar or Active Play runs; toolbar pause/play pauses/resumes; Reset resets the attempt and camera; Clear removes the program while editable.
- Camera: middle-button drag or Space plus left-drag pans; wheel/trackpad scrolling zooms; one touch pans and two touches combine pan/pinch; Center restores following while retaining zoom.
- Panels: edge chevrons collapse; world hamburger toggles Blocks/Active visibility.
- Menus: Escape closes Settings/Hint. It does not currently open Settings, navigate to title, or dismiss outcomes.

There are no dedicated Run/Pause/Reset/Clear keyboard shortcuts or keyboard block-delete action. Generic Player mappings in the Input Actions asset, such as Jump, are not connected to gameplay and must not appear as supported controls. UI navigation bindings exist but are context-dependent; focused number fields use their own edit keys. Do not claim Tab commits a field: current verification established that it can leave the field focused.

## Design basis

[Microsoft's game UI navigation guidance](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/112) recommends consistent navigation and predictable focus. For this prototype, clear action labels and a single parent-menu Back path address the immediate ambiguity. This does not introduce a full accessibility/remapping project.

## First decision frontier

First-round proposals, since approved:

1. A compact centred failure card with heading Try again, a prominent labelled Retry button and a secondary Level Select button, vertically arranged. Remove the misleading cross status icon and unlabelled action row. Preserve the plain minimal style and dim backdrop.
2. Give success the same visual treatment, with heading Level complete. Settle its action emphasis after confirming this scope.
3. Hide unavailable Next actions until progression exists rather than displaying a dead arrow. No progression feature is introduced.
4. Retain Retry's current Ready/editable behavior, preserving blocks and sidebar visibility and restoring attempt/camera state. Do not automatically rerun the same program.
5. Controls is a child of Settings: Back and Escape return to Settings; closing Settings returns to the game, preserving its paused state. Back restores focus to the Controls button.
6. Keep the Controls reference brief with explicit Mouse & Keyboard, Trackpad and Touch tabs instead of displaying all input methods in a dense page. Exact copy follows this presentation choice.
7. Make this a read-only reference for implemented controls. New gameplay hotkeys and remapping require a separate explicit decision.
8. Outcomes remain terminal choices: background interactions, outside clicks and Escape do not dismiss them. Initial focus goes to the primary action and stays within the active modal.

The requested Controls button location and size are already explicit user instructions, not an unresolved design choice. The existing footer text is Return to title; it is distinct from Level Select and should remain precise unless the user requests a rename.

## Dependent decisions

After this round, settle success primary action, exact Controls rows/tab content, header/footer wording, key labels and submenu closing behavior. If new hotkeys are requested, define conflict handling around numeric input, Space-pan, paused/terminal states and modals before implementation.

## First-round approvals

The user accepted questions 1–8:

1. Failure uses a compact centred card, heading Try again, prominent labelled Retry and secondary Level Select, vertically arranged. Remove the cross and unlabelled row; retain the minimal plain style and dim backdrop.
2. Success uses the same visual treatment and Level complete heading.
3. Hide Next until it has an implementation. No progression feature is added.
4. Retry preserves the program/sidebar state, restores the experiment and camera, and returns to Ready without automatically running.
5. Controls is a Settings submenu. Back/Escape return to Settings, restore focus to the Controls button, and leave physics paused. Settings close returns to gameplay without auto-resume.
6. Controls has explicit Mouse & Keyboard, Trackpad and Touch tabs.
7. This pass is a read-only reference to implemented controls. No new gameplay shortcuts or remapping.
8. Outcomes require a choice; Escape/backdrop do not dismiss them. Failure initially focuses Retry; focus remains within the active modal.

## Second-round proposals, since approved

9. Success primary action is Level Select and secondary action is Replay. Replay uses the same retained-program Ready behavior as Retry.
10. Controls uses one Back control, without a separate X that could imply closing the whole Settings flow. Escape has the same one-level Back behavior.
11. Approve the concise control inventory below. Render actual matching toolbar icons for button-driven actions rather than relying on unfamiliar icon names or invented hotkeys.
12. Start on Mouse & Keyboard, remember the selected tab during the current gameplay visit, and focus the selected tab when reopening. Back restores focus to Settings > Controls. Do not infer trackpad type from device names or scroll deltas.
13. Keep text readable and permit vertical scrolling when needed, with no horizontal scrolling or tiny text to force a fit.

### Proposed Controls copy

The following action names are shared across tabs; the gesture wording follows the selected input method.

| Action | Mouse & Keyboard / Trackpad | Touch |
| --- | --- | --- |
| Add block | Drag from Blocks to Active | Drag from Blocks to Active |
| Reorder | Drag to a connector | Drag to a connector |
| Remove block | × or drag back to Blocks | × or drag back to Blocks |
| Nest equation | Drag to a matching input | Drag to a matching input |
| Edit number | Click a white input. Enter applies; Esc cancels. | Tap a white input. Tap outside to apply. |

| Camera action | Mouse & Keyboard | Trackpad | Touch |
| --- | --- | --- | --- |
| Pan | Middle-drag or Space + left-drag | Space + click-drag | One-finger drag |
| Zoom | Mouse wheel | Two-finger scroll | Two-finger pinch |
| Pan + zoom | Not a separate shortcut | Not a separate shortcut | Drag and pinch two fingers |
| Center camera | Target button | Target button | Target button |

Do not print the Not a separate shortcut cells; omit that extra row outside Touch. Include the small shared note: Camera gestures work only on the map.

Playback displays matching toolbar icons beside Run, Pause / resume, Reset attempt, and Clear program. These are button actions, not keyboard shortcuts. Center camera likewise uses its matching target icon.

Panel navigation uses Edge chevron to hide a panel and Menu button to show panels. Mouse & Keyboard and Trackpad also show the contextual menu keys: Arrows select buttons; Enter activates; Esc goes back or closes a menu. Touch uses the visible Back/close controls. Do not advertise WASD as ball movement, Tab as committing edits, keyboard block deletion, or unused Player-action bindings.

Section labels are Blocks, Camera, and Playback & menus. Disabled pencil/volume placeholders are not listed as working controls. No equation tutorial, scoring explanation or extra outcome prose is added.

## Second-round approvals and copy gate

The user accepted decisions 9–13, but explicitly requires review of the exact wording of every new UI element before final confirmation. Implementation remains pending that copy approval. Use minimal informational labels only.

Approved behavior: success primary Level Select, secondary Replay using retained-program Ready behavior; Controls has Back without a separate X; initial Mouse & Keyboard tab and visit-local remembered selection/focus; readable vertically scrolling content with no horizontal scrolling.

## Exact UI copy proposal

The user approved this exact copy. This section supersedes earlier draft prose.

### Outcome and Settings labels

- Failure heading: Try again
- Failure primary: Retry
- Failure secondary: Level Select
- Success heading: Level complete
- Success primary: Level Select
- Success secondary: Replay
- Settings new button: Controls

### Controls frame

- Heading: Controls
- Back control: Back
- Tabs: Mouse & Keyboard; Trackpad; Touch
- Section headings: Blocks; Camera; Playback & menus
- Camera scope note: Map only

### Blocks rows

| Label | Mouse & Keyboard / Trackpad | Touch |
| --- | --- | --- |
| Add block | Drag Blocks → Active | Drag Blocks → Active |
| Reorder | Drag to a connector | Drag to a connector |
| Remove block | × or drag back to Blocks | × or drag back to Blocks |
| Nest equation | Drag to a matching input | Drag to a matching input |
| Edit number | Click a white input | Tap a white input |
| Apply edit | Enter or click outside | Tap outside |
| Cancel edit | Esc | Omitted |

Omitted is a design marker, never visible UI text. The Touch tab does not advertise keyboard cancellation.

### Camera rows

| Label | Mouse & Keyboard | Trackpad | Touch |
| --- | --- | --- | --- |
| Pan | Middle-drag or Space + left-drag | Space + click-drag | One-finger drag |
| Zoom | Mouse wheel | Two-finger scroll | Two-finger pinch |
| Pan + zoom | Omitted | Omitted | Drag and pinch |
| Center camera | Matching target icon | Matching target icon | Matching target icon |

Matching target icon is a rendering instruction, not printed text. Omit Pan + zoom outside Touch.

### Playback and menus rows

Render the existing matching toolbar icons alongside these labels: Run; Pause / resume; Reset attempt; Clear program; Hide panel; Show panels.

Desktop and trackpad tabs additionally show:

| Label | Input |
| --- | --- |
| Select button | Arrow keys |
| Activate button | Enter |
| Back / close | Esc |

The key rows describe menu navigation, not ball movement or program playback shortcuts. No extra explanatory prose, outcome message, score text or inactive-control description is proposed.


## Implementation

- `GameplayScreen` owns outcome actions, the Settings/Controls parent flow, retained tab focus and modal input confinement.
- `ControlsReference` renders only the approved reference copy and existing procedural icons. It keeps device selection within the current gameplay visit and resets scroll position when switching tabs. Its vertical scrollbar also supports keyboard navigation.
- `FoundationAssets.UpgradeOutcomeAndControlsMenus` updates the existing gameplay prefab without regenerating levels or scenes. Running the upgrade again replaces these menu elements instead of duplicating them. The foundation generator uses the same builder.
- Outcomes require Retry/Replay or Level Select. Background clicks and Escape leave them open.

Native verification: 52 EditMode tests and 62 PlayMode tests passed, including child Back/focus restoration, tab memory, readable scrolling structure, outcome action order and retained-program reset. The full browser suite passed 33 check groups with zero application errors. A focused browser check also passed keyboard scrollbar movement, continuous touch scrolling, touch Back and reaching the bottom of all three tabs. See [verification evidence](../verification/reports/outcome-and-controls-menus.md).
