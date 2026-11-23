# Outcome and Controls menus adversarial review

Review date: 2026-11-22. Final score: **9.3/10. Pass.** No required corrections or substantive missing approved features remain.

## Scope and authority

I reviewed `GameplayScreen.cs`, `ControlsReference.cs`, `Editor/MenuAssets.cs`, the relevant `PassTwoAssets.cs` composition, `MenuTests.cs`, README guidance, and the menu additions to `Tools/verify-web.mjs`. The [approved menu contract](../../design/outcome-and-controls-menus.md#exact-ui-copy-proposal) controls exact visible wording and supersedes its earlier proposals. I also checked [pass1spec.md](../../design/pass1spec.md) against the later approved contracts and retained verification coverage. Unrelated earlier working-tree changes and user-owned `.gitignore` and GDD edits are outside this feature review.

## Findings

There are no open substantive findings.

Failure presents Try again, Retry, then Level Select. Success presents Level complete, Level Select, then Replay. Both use compact centered cards, a dim backdrop, labelled vertical actions, and primary-action focus. The ambiguous status cross and unavailable Next action are gone. Escape and backdrop clicks do not dismiss outcomes. Retry and Replay use the existing experiment reset, retain program and panel visibility, restore camera defaults and Follow, and return to editable Ready without running.

Settings adds Controls at the approved location with the same anchored size as Return to title. Controls has one Back control and three explicit device tabs. Back and Escape return to Settings and restore focus to Controls; closing Settings leaves the experiment paused. Selected-tab state lasts for the gameplay visit. Active-modal focus confinement prevents navigation to background controls.

The reference matches the approved literal copy. Touch omits Cancel edit and desktop menu-key rows; Pan + zoom appears only on Touch. Playback, panel, and camera actions use matching existing icons. The reference does not invent gameplay hotkeys, advertise unused Player bindings, or print rendering instructions such as Matching target icon or Omitted. Text remains readable at both checked desktop sizes. Vertical scrolling exposes the remaining rows, and the visible scrollbar also supports keyboard operation. There is no horizontal scrolling.

No substantive missing features remain from pass1spec.md as superseded by the approved presentation, standalone-equation, numeric-validation, camera, and menu contracts. Shared gameplay composition, program editing and nesting, formula and physical-action semantics, pause/reset/clear, navigation, and scene lifecycle retain passing regression coverage. Superseded title/outcome presentation, nested-only equation restrictions, and earlier numeric policies are not requirements to restore. Deferred progression, remapping, gameplay shortcuts, and functional level authoring remain outside the approved scope.

## Final evidence

I read the completed artifacts after verification finished:

- [EditMode results](../results/editmode-results.json): 52 passed, zero failures or skips.
- [PlayMode results](../results/playmode-results.json): 62 passed, zero failures or skips, including all three menu tests. Native total is 114 passed.
- [Web build result](../results/web-build-result.json): job `890e1787cd994831969663c797767050` completed without recorded errors.
- [Full browser results](../results/web-results.json): 33 interaction groups passed, with zero application errors. These include actual Escape behavior, tab switching, reference scrolling, camera/input shielding, outcome choices, retained-program replay, and existing gameplay regressions.
- [Focused reference results](../results/menu-reference-results.json): all three tab bottoms are reachable; held ArrowDown moves the scrollbar; a continuous Chromium touch drag scrolls the reference; touch Back returns to Settings. No application errors were recorded.

I personally inspected the [1280 capture](../screenshots/controls-1280.png), [1920 capture](../screenshots/controls-1920.png), Trackpad and scrolled Touch captures, all three [tab](../screenshots/controls-bottom-0.png) [bottom](../screenshots/controls-bottom-1.png) [captures](../screenshots/controls-bottom-2.png), and the [success](../screenshots/pass2-won.png) and [failure](../screenshots/pass2-lost.png) cards. Labels, selected-tab indication, icons, bottom key rows, and Back remain legible and properly contained.

An initial native assertion compared layout float vectors exactly and failed despite matching displayed sizes. The final assertion uses a sub-pixel tolerance and the full suite passes. Initial focused browser probes used an instantaneous key press and a single touch jump; final probes hold the key across Unity frames and move the touch continuously through drag initiation. These verification corrections did not change production behavior.

I independently inspected source, saved execution results, and actual screenshots; I did not launch the Unity or browser jobs myself. Chromium touch emulation does not prove every physical device. That coverage limit leaves no approved feature unimplemented.
