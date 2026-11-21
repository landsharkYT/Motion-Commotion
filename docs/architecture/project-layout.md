# Where things belong

Open `Assets/Scenes/Application/Title.unity` to run the demonstration. Open `Assets/Scenes/Testing/Sandbox.unity` to test mechanics directly.

## Unity assets

| Location | Put here |
| --- | --- |
| `Assets/Scenes/Application` | Title, InGame, and the inaccessible LevelEditor placeholder |
| `Assets/Scenes/Testing` | Sandbox and future nonproduction test scenes |
| `Assets/Scripts/Core` | Plain C# equations, program models, validation, and execution |
| `Assets/Scripts/Runtime/Physics` | Unity physics adapters and simulation control |
| `Assets/Scripts/Runtime/Levels` | Landing/fall outcomes and environment references |
| `Assets/Scripts/Runtime/Session` | Scene-local gameplay composition and session state |
| `Assets/Scripts/Runtime/Configuration` | ScriptableObject C# definitions |
| `Assets/Scripts/Runtime/Navigation` | Scene navigation |
| `Assets/Scripts/Runtime/UI` | Block editing, dragging, layout, and HUD |
| `Assets/Scripts/Runtime/Diagnostics` | Development verification telemetry |
| `Assets/Scripts/Editor` | Unity editor tools, default-asset generation, and build commands |
| `Assets/Scripts/Tests` | EditMode and PlayMode test assemblies |
| `Assets/Data/Blocks/Commands` | Named Direction, Force, Wait, and Weight definition assets |
| `Assets/Data/Blocks/Formulas` | Named equation definition assets |
| `Assets/Data/Levels` | Placeholder-level environment, camera, fall, and hint configuration |
| `Assets/Settings/Gameplay` | Workspace and simulation defaults |
| `Assets/Settings/Rendering` | Active URP renderer, pipeline, global settings, and volume configuration |
| `Assets/Input/Actions` | Input Actions assets |
| `Assets/Input/UIReferences` | Persistent UI input-action references |
| `Assets/Prefabs/Gameplay` | Shared gameplay composition |
| `Assets/Prefabs/Levels` | Replaceable primitive environments |
| `Assets/Prefabs/UI` | Reusable UI prefabs such as BlockCard |
| `Assets/Prefabs/Shapes` | Reusable primitive collision shapes |
| `Assets/Materials` | Rendering materials and physics materials |
| `Assets/Textures` | Supplied temporary title-logo reference only; scenery textures remain deferred |
| `Assets/Meshes` | Primitive geometry meshes |
| `Assets/Plugins/WebGL` | Native JavaScript integration with its plugin import settings |
| `Assets/WebGLTemplates/MotionCommotion` | Unity's selected custom browser template |

Scripts define behavior and asset types. Data and Settings contain configured asset instances. Runtime edits belong to session instances, not shared definition assets.

The five existing assembly definitions stay with their scripts. Core has no Unity dependencies. Runtime references Core. Unity editor tools and EditMode tests are editor-only; PlayMode tests have their own test assembly. Create new scripts within the assembly that owns their responsibility.

## Documentation and tools

| Location | Contents |
| --- | --- |
| `docs/design` | Original GDD, agreed scope, and pass1spec |
| `docs/architecture` | Architecture notes and this guide |
| `docs/adr` | Architectural decision records |
| `docs/references` | Original wireframes and reference images |
| `docs/verification/reports` | Coverage, implementation evidence, and historical independent reviews |
| `docs/verification/results` | Machine-readable verification results and migration evidence |
| `docs/verification/screenshots` | Captured application screenshots |
| `Tools` | External Python/Node verification helpers and their package manifests |

README is the project entry point. CONTEXT defines domain terms. Unity owns Library, Temp, Logs, UserSettings, and generated IDE project files; leave them in their expected locations. Build output goes to the ignored Builds directory.

## Naming and moving content

Use descriptive PascalCase asset and folder names. Name definition assets after their command or formula identity, such as Direction or AverageVelocity. Create folders when they have content. Keep future third-party imports separate from these first-party assets.

Move Unity assets in the Project window so metadata follows them. If a tool moves files, it must move the paired `.meta` files and preserve GUIDs. Update string-based paths in generator code, tests, build configuration, and documentation as part of the same change.

The default generator writes to this layout. It rebuilds shipped defaults and can overwrite teammate customizations. Moving assets does not require running it.

The former Editor scene is named LevelEditor. It remains an inaccessible placeholder; this organization change adds no level-authoring functionality.

## Verification

From the project root:

```sh
python3 Tools/verify-layout.py
python3 Tools/verify-unity.py
npm --prefix Tools run verify:web
```

The layout check verifies migrated GUIDs, removal of obsolete content, build-scene paths, asset references to removed GUIDs, assembly ownership, generator destinations, and local documentation links. Unity and browser verification exercise the actual imported project and built application. See [README](../../README.md) for build and dependency setup.

Historical reviews preserve their original source paths and ratings. Use the migration result's old/new mapping when reading those reports. New migration verification is recorded separately.
