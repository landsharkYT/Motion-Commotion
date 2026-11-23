# Outcome and Controls menus verification

Contract: [approved menu behavior and exact copy](../../design/outcome-and-controls-menus.md).

The outcome is a centred white card over a dim backdrop. Failure offers Retry and Level Select; success offers Level Select and Replay. Retry/Replay restore the pre-run experiment, default camera and Ready state while keeping the program and sidebar visibility. Outcomes require a choice and have no dead Next button.

Settings has a Controls button matching Return to title. Controls contains Mouse & Keyboard, Trackpad and Touch tabs with only the approved text and matching toolbar icons. Its content scrolls vertically with a visible scrollbar. Back/Escape returns to Settings and restores focus to Controls. Closing Settings leaves running physics paused. Selection stays within the active modal.

## Checks

- Native: 52 EditMode and 62 PlayMode tests passed. The three menu tests cover retained program/sidebar state, success and failure actions, primary focus, child navigation, visit-local tab memory, pause preservation, modal focus confinement and vertical-only scroll layout.
- The first run caught an overly strict exact floating-point size assertion. The assertion now allows less than 0.001 logical pixels of layout rounding; all tests pass.
- Asset layout: four build scenes, five assemblies, unique GUIDs and preserved migrated asset identities.
- Browser: all 33 full-suite check groups passed with zero application errors. The menu checks use real clicks, Escape, tab switching and wheel input, and capture 1280×720 and 1920×1080 layouts.
- Focused browser check: every tab reaches its bottom; actual arrow-key input moves the scrollbar; continuous touch dragging scrolls; tapping Back returns to Settings. Touch gestures contain movement after UGUI crosses its drag threshold; keyboard navigation holds the key across a Unity input frame.
- Screenshots inspected: [1280 Controls](../screenshots/controls-1280.png), [1920 Controls](../screenshots/controls-1920.png), [desktop bottom](../screenshots/controls-bottom-0.png), [touch bottom](../screenshots/controls-bottom-2.png), [success](../screenshots/pass2-won.png), [failure](../screenshots/pass2-lost.png).
- Result artifacts: [full browser run](../results/web-results.json), [focused reference run](../results/menu-reference-results.json), [EditMode](../results/editmode-results.json), [PlayMode](../results/playmode-results.json).
- [Independent adversarial review](outcome-and-controls-menus-adversarial-review.md): 9.3/10. No required corrections or substantive missing features from pass 1 as superseded by approved later contracts.

## Maintenance

Use **Motion Commotion > Upgrade outcome and Controls menus** to apply the targeted prefab upgrade. This replaces only these menu elements and their references; it does not regenerate level assets or scenes. The foundation generator calls the same builder.


To repeat the focused help check against the current Web build:

```sh
MOTION_MENU_REFERENCE_ONLY=1 npm --prefix Tools run verify:web
```
