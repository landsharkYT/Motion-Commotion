# Equation rearrangement continuation

This is a requested checkpoint commit, not a declaration that the entire feature has passed final acceptance.

## Authority and approvals

Read [the complete approved contract](equation-rearrangement.md), especially Consolidated approved contract, Derived finite catalog, Final interview approvals, Implementation decisions and acceptance requirements. Questions 1–38 and all new UI wording were approved. Do not reopen settled questions or silently change physics semantics. The latest user required committing and pushing before continuing because usage was running low.

The contract documents all 18 forms, hardcoded transition behavior, readonly left output, palette-only functional pencils, initial values, pure-value restrictions, mass sequencing, time branches and edge cases, modal behavior, copy, template lifetime and numeric protections. CONTEXT.md holds the glossary. The GDD is context; later explicit approvals take precedence. Preserve the user's GDD and .gitignore edits, which are included in this checkpoint.

## Implemented and verified

- New core catalog and graph: Assets/Scripts/Core/FormulaVariants.cs, FormulaNotation.cs, EquationDraft.cs. Original serialized IDs are preserved; 13 additional variants are appended.
- Editor/runtime UI: EquationEditor.cs, EquationMathLayout.cs, BlockWorkspace.Variants.cs, plus workspace, screen and card integration. The gameplay prefab has been upgraded. Future generation includes the same skeleton.
- New mass, gravity, displacement and velocity-change actions; ordered preflight mass evaluation; pure time and initial-condition values.
- 85 EditMode + 68 PlayMode tests passed, 153 total. Result artifacts are docs/verification/results/editmode-results.json and playmode-results.json.
- New Web build succeeded: job 884e9d948fd2485fad9c42294ba15eb4, recorded in web-build-result.json.
- MOTION_REARRANGEMENT_ONLY=1 npm --prefix Tools run verify:web passed four groups with zero application errors. See rearrangement-web-results.json and screenshots/results named rearrange-*. These include actual mouse operation menus, Undo, time selection, pure time nesting in Wait, mass runtime behavior, Retry/Clear/level lifetime, and CDP touch drag/Divide/Swap/Confirm without camera movement.
- Asset-layout checks passed: four build scenes, five assemblies, 154 unique GUIDs at checkpoint.

## Required remaining work

1. Personally inspect new rearrange-* screenshots, including editor math, nested time and 1280/1920 layouts. Reviewer should inspect them too.
2. Minor known notation issue: BlockWorkspace.Equations.cs VariableName still only maps s0/v0/ds/dv/dt, so new input labels vbar/abar can appear literally. Delegate that formatting to FormulaNotation.Symbol, and test the new displacement/velocity-change input labels. This has not been fixed in this checkpoint. If code changes, recompile, run appropriate native verification and rebuild the Web proof before treating browser artifacts as current.
3. Consider stopping the editor ScrollRect's inertia in EquationEditor.BeginDrag so a previous touch scroll cannot continue moving the source while symbols are dragged. This is a refinement, not a confirmed failing test.
4. Run the full browser regression suite against the final Web build: npm --prefix Tools run verify:web. Current web-results.json is the earlier menu build's 33-group pass, not a fresh rearrangement result. The script has been updated for functional palette-only pencils.
5. Run the focused rearrangement suite again if production code changes. Optionally rerun MOTION_MENU_REFERENCE_ONLY=1 for help scrolling after its three new rows.
6. Obtain final independent adversarial review >=9/10 and an explicit statement that no substantive approved features are missing from pass1spec.md as superseded by later contracts. The active reviewer /root/rearrangement_adversarial_review found no confirmed source blocker, but is holding its final report until fresh browser evidence and screenshot review. It must write docs/verification/reports/equation-rearrangement-adversarial-review.md. If that agent is unavailable in a later session, spawn a fresh reviewer; user explicitly authorized it.
7. Update the verification report and contract status only after final acceptance. Commit/push follow-up work when requested; this checkpoint was explicitly requested with message Implemented camera controls, equation editor.

## Operational notes

Unity 6000.4.0f1 runs through local Pipeline on port 7800. Local Unity/browser network calls normally need the tool's require_escalated mode. Commands are unity command 'script recompile', unity command 'script recompile_status', and python3 Tools/verify-unity.py. Never edit C# during an active test run or Web build.

Build via Motion Commotion > Build Web proof, using detached editor invoke_menu and job wait when needed. Do not run the full foundation generator to upgrade this feature; it regenerates level/scene assets. The targeted menu is Motion Commotion > Upgrade equation editor.

The Web build can leave a temporary InputSystem UI action asset in ProjectSettings/ProjectSettings.asset preloadedAssets on disk. Its one known build-only entry has been restored to the original empty list before this checkpoint. Preserve other project settings.

Library, Temp, Logs, Builds, node_modules and local agent directories are ignored. Test screenshots/results and design documentation are intentional committed evidence. Do not publish build caches or credentials.

Real browser gestures need multiple movement frames after Unity's drag threshold; a single jump only starts UGUI dragging. Arrow navigation checks hold the key across a Unity input frame. Existing verify-web.mjs models these details. PlayMode test startup occasionally stalls; verify-unity.py handles one restart.
