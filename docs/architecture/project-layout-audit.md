# Project layout audit

Status: migration applied; layout, Unity tests, WebGL build, and browser checks passed. Independent adversarial review passed at 9.4/10 with no substantive missing features. The findings below describe the original layout.

## Findings

The current layout makes implementation boundaries more visible than the assets teammates need to find. Assembly separation is useful, but it does not require hiding scripts outside a clearly named Scripts folder.

| Finding | Evidence | Effect on teammates |
| --- | --- | --- |
| Three scene directories | `Assets/MotionCommotion/Scenes`, `Assets/Scenes`, `Assets/Settings/Scenes` | No obvious authoritative scene location |
| No Scripts folder | Custom C# lives under `Assets/MotionCommotion/Core`, `Runtime`, `Editor`, and `Tests` | Scripts are hard to discover |
| Mixed settings | `Assets/MotionCommotion/Settings` contains workspace configuration, palette entries, and input references | Folder name does not explain asset ownership |
| Numbered palette assets | `Command0.asset` through `Command3.asset`, `Formula0.asset` through `Formula4.asset` | Asset purpose requires opening the Inspector or generator |
| Scattered documentation | `Information`, `ReferenceImages`, `Verification`, `docs`, and root Markdown files | No single documentation entry beyond README |
| Hidden generator coupling | `FoundationAssets.cs` hardcodes the asset root and input-actions path | Moving assets without updating generation can recreate the old tree |
| Ambiguous tooling names | Unity editor tooling, the future level-editor scene, and external `scripts` are separate concepts | Similar names refer to different workflows |

## Scene and starter-content audit

| Content | Current role | Cleanup recommendation pending approval |
| --- | --- | --- |
| `Assets/MotionCommotion/Scenes/Title.unity` | Enabled build scene, title and level selection | Keep |
| `Assets/MotionCommotion/Scenes/InGame.unity` | Enabled build scene, mechanics demonstration | Keep |
| `Assets/MotionCommotion/Scenes/Editor.unity` | Enabled build scene, inaccessible future level-editor placeholder | Keep; distinguish from Unity editor tooling |
| `Assets/MotionCommotion/Scenes/Sandbox.unity` | Enabled build scene, testing entry | Keep; group separately within the scene hierarchy |
| `Assets/Scenes/SampleScene.unity` | Starter scene, no inbound serialized references found | Remove |
| `Assets/Settings/Scenes/URP2DSceneTemplate.unity` | Referenced by `Lit2DSceneTemplate.scenetemplate` only | Remove together if the team does not need that New Scene template |

Template origin does not imply unused content. All six quality levels reference `Assets/Settings/UniversalRP.asset`, which references `Renderer2D.asset`. Graphics settings reference the root URP global settings asset, which references `DefaultVolumeProfile.asset`. These rendering assets must remain available.

The root `InputSystem_Actions.inputactions` is also active. Scenes, the shared gameplay prefab, UI action-reference assets, and project configuration use it. `Assets/WebGLTemplates/MotionCommotion/index.html` is selected through the project setting `PROJECT:MotionCommotion`; a GUID-only dependency scan would miss this dependency.

Square and Triangle prefabs are intentional reusable mechanics assets even though no scene currently references them. Generation and tests explicitly include them.

## Migration constraints

- Preserve asset GUIDs and import settings by moving assets with their `.meta` files, preferably through Unity's AssetDatabase.
- Preserve the five assembly boundaries: Core, Runtime, Editor, EditModeTests, and PlayModeTests. Core must remain independent of UnityEngine; editor code must remain editor-only.
- Update `FoundationAssets.cs` generation destinations, input-actions lookup, and build-scene paths. Do not regenerate assets merely to relocate them: generation rewrites configured defaults and baked content.
- Update exact paths in `CoreTests.cs` and current documentation. Build Settings must reference the moved scenes.
- Keep the WebGL template in Unity's recognized template location. Preserve plugin import settings when moving `Verification.jslib` out of the UI script tree.
- Runtime navigation and browser checks use scene names. Keeping names avoids unnecessary behavior changes.
- Preserve historical review reports as evidence. Clearly separate them from current instructions and freshly generated results.
- Unity-generated `Library`, `Temp`, `Logs`, `UserSettings`, project files, and build output are a separate concern from authored project organization. Do not relocate Unity-managed directories to make the root appear smaller.

After migration, verify GUID preservation, missing references, assembly ownership, compilation, EditMode and PlayMode tests, WebGL build, and browser behavior. Verify that generation targets only the approved layout.

## Decisions for the interview

1. Direct asset-type folders under Assets versus one first-party project container with asset-type folders inside it.
2. Removal of unused starter scene and optional scene-template pair.
3. Descriptive command/formula asset names and separation of palette data from project settings.
4. Consolidation of authored documentation and reference images, with a clear location for verification history and developer tools.

Detailed script subfolders and scene subfolders follow once the top-level convention is settled. No gameplay redesign is required by these findings.

### Approved first round

The team approved all four recommendations:

- Use asset-type folders directly under `Assets`, including an obvious `Scripts` folder.
- Remove SampleScene and the optional URP scene-template pair. Preserve active rendering and input assets.
- Give command/formula assets descriptive names and separate palette data from settings.
- Consolidate authored documentation, reference images, and verification evidence under `docs`; put external verification utilities under `Tools`.

The second round below settles script categories, scene grouping and naming, and the detailed asset/documentation map.

### Approved second round

The team approved the following detailed layout. This supersedes the open questions above.

```text
Assets/
  Scenes/
    Application/        # Title, InGame, LevelEditor
    Testing/            # Sandbox
  Scripts/
    Core/
    Runtime/
      Physics/
      Session/
      Configuration/
      Navigation/
      UI/
      Diagnostics/
    Editor/
    Tests/
      EditMode/
      PlayMode/
  Data/
    Blocks/
      Commands/
      Formulas/
  Settings/
    Gameplay/
    Rendering/
  Input/
    Actions/
    UIReferences/
  Prefabs/
    Gameplay/
    UI/
    Shapes/
  Materials/
  Meshes/
  Plugins/
    WebGL/
  WebGLTemplates/
    MotionCommotion/
docs/
  design/
  architecture/
  adr/
  references/
  verification/
    reports/
    results/
    screenshots/
Tools/
```

Only create folders with actual content. Preserve existing namespaces and assembly names unless a path-dependent correction requires a change.

### Concrete migration scope

- Move Core, Runtime, Editor, and both test assemblies under Scripts, preserving their assembly definitions and asset GUIDs.
- Move WorkspaceSettings and PaletteDefinition script definitions from Runtime/Session to Runtime/Configuration. Keep MotionCommotionSession in Runtime/Session.
- Move VerificationTelemetry into Runtime/Diagnostics and its native JavaScript plugin into Plugins/WebGL.
- Move application scenes into Scenes/Application and Sandbox into Scenes/Testing. Rename the scene Editor to LevelEditor and update scene-name consumers as needed.
- Move command/formula assets into the approved Data branches and replace numerical file names with names derived from their actual command/formula identities.
- Move Workspace configuration into Settings/Gameplay, active URP configuration into Settings/Rendering, input actions into Input/Actions, and UI action references into Input/UIReferences.
- Group Gameplay, BlockCard, and shape prefabs into the corresponding Prefabs subfolders. Move generated geometry meshes into Meshes. Retain materials and physics material under Materials.
- Remove the unused SampleScene and the optional URP template scene and descriptor together, including their metadata.
- Move the GDD, scope, and pass1spec into docs/design; architecture notes into docs/architecture; reference images into docs/references; verification reports, results, and screenshots into their approved folders. Preserve the existing decision records in docs/adr and the root domain context document.
- Move external verification helpers and their package manifests into Tools. Update their result destinations, serving paths, invocation instructions, and ignore patterns.
- Update generator paths, exact-path tests, Build Settings, current documentation, and README links. Historical review text remains historical evidence; add relocation context where needed rather than rewriting old findings as new results.
- Remove obsolete empty authored folders after migration. Leave Unity-managed directories and generated IDE files where Unity expects them.
- Add a concise guide explaining where contributors should put scripts, scenes, configuration, prefabs, documentation, and tools, linked from README.

### Completion checks

1. Compare pre-move and post-move asset GUIDs and inspect serialized references for missing targets.
2. Confirm assembly boundaries and editor-only code isolation remain intact.
3. Reimport and compile through Unity; inspect error output and build-scene references.
4. Run EditMode and PlayMode tests using the relocated tools.
5. Build WebGL and run the browser checks, including scene navigation and verification telemetry.
6. Check generator destinations for old paths without regenerating over the team's editable content.
7. Check current documentation links and confirm the old authored tree no longer exists.

The migration changes organization and the Editor scene's descriptive name. It does not add levels, implement the level editor, or change the approved mechanics.

## Guidance consulted

[Unity's project organization guide](https://unity.com/how-to/organizing-your-project) explicitly allows different folder structures. It recommends consistent documented conventions, separating sandbox work and third-party assets, and preserving metadata during moves. The problem here is discoverability and consistency, rather than violating a single prescribed Unity directory tree.

## Audit limits

The initial audit used filesystem, serialized-reference, code-path, and official-guidance checks. Unity was not launched and assets were not moved during that audit. The authorized migration subsequently moved assets with metadata, and its results are recorded in the [migration verification report](../verification/reports/layout-migration.md). The workspace did not expose a Git repository to `git status`, so the audit does not establish version-control tracking or move history.
