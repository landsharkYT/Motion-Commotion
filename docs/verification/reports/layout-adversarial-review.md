# Independent layout migration review

Reviewed 2026-11-20 against the approved decisions in [project-layout-audit.md](../../architecture/project-layout-audit.md), the [contributor layout guide](../../architecture/project-layout.md), and the full [pass 1 specification](../../design/pass1spec.md).

Rating: **9.4/10**. Acceptance gate: **PASS**. No blocking findings.

**No substantive missing features from pass1spec.md.** The reorganization preserves the complete pass 1 mechanics foundation and its documented desktop prototype limits. Authored levels and a working level editor remain excluded requirements.

## Independent checks

I ran `python3 Tools/verify-layout.py` independently. It passed with 64 migrated asset GUID checks, 101 unique asset GUIDs, the four expected build scenes, five assembly definitions, and 61 valid local documentation links. I also checked every asset and folder for missing or orphaned metadata; none were found.

I independently compared current asset bytes with the pre-migration SHA256 values in [the manifest](../results/layout-migration.json). Of the 64 moved Unity assets, 51 are byte-identical. This includes every Core and Runtime script, all five assembly definitions, the gameplay scenes, all gameplay/UI/shape prefabs, meshes, materials, input assets, workspace settings, rendering assets, and the JavaScript plugin.

Thirteen assets differ. Nine palette assets recover their exact baseline hashes by restoring only their former `m_Name`. The LevelEditor scene recovers its baseline hash by changing `LevelEditorPlaceholder` back to `EditorPlaceholder`. PhysicsTests recovers its baseline hash by changing the teardown scene name back to `Editor`. CoreTests recovers its baseline hash by reversing only asset-path and scene-name changes. The remaining changed asset is FoundationAssets, whose generator destinations and build configuration I inspected directly. It writes to the approved directories, loads the relocated Input Actions asset, names command/formula assets from their identities, distinguishes Sandbox from application scenes, and emits the renamed placeholder and correct build-scene order. I did not run the generator.

Build Settings retain each scene's original GUID and point to Title, InGame, LevelEditor, and Sandbox in that order. The project input configuration retains its Input Actions GUID. All six quality settings still reference the retained URP pipeline; Graphics Settings still reference the retained global rendering settings. The WebGL template setting is still `PROJECT:MotionCommotion`, and its template remains in Unity's recognized directory.

Core has `noEngineReferences: true`. Runtime references Core and the existing Unity UI/input assemblies. Editor and EditMode tests are restricted to Editor; PlayMode tests retain their separate test assembly. All authored C# remains owned by exactly one of these five definitions. Moving Configuration and Diagnostics changed no Runtime source bytes or namespaces.

Runtime navigation still exposes only InGame and Title. The only direct test scene consumer affected by the placeholder rename now loads LevelEditor. Browser selectors use the descriptive palette asset names. The external scripts locate output relative to their new Tools directory, serve Builds/Web, and write results/screenshots under docs/verification. README links the contributor guide and provides working scene paths, setup instructions, generator cautions, and verification commands. Old authored trees are absent.

## Serialized references and the renderer baseline

I inspected the raw Unity scan in [unity-reference-results.json](../results/unity-reference-results.json). It records 669 inspected objects, no missing scripts, and exactly six unresolved properties under `Renderer2D.probeVolumeResources`. The raw result correctly remains `passed: false`; the separate migration assessment records the baseline exception.

I independently calculated Renderer2D's current SHA256:

```text
900455794d0d788504847c3aa54e03a0374fd3635afa295ed05c2f646f974705
```

It exactly matches the manifest's pre-migration hash. All six properties and their GUIDs are present in that unchanged asset. An independent text GUID scan against Assets and installed package metadata also found no unresolved GUIDs outside this unchanged renderer. That text scan additionally finds its legacy `m_FallOffLookup` GUID; this field is absent from the imported-object scan and is also baseline content. This supports the narrower conclusion of no new unresolved references, rather than claiming the original rendering template is entirely clean.

## Functional scope and regression evidence

I inspected the relocated formula catalog, runner, session coordination, navigation, tests, verification utilities, serialized assets, current results, prior full acceptance review, and the rendered program screenshot. The source and assets preserve the five formulas, typed nesting, Direction/Force pairing, 0.1-second force conversion, simulation-time waits, real physical mass, separate Weight execution, manual stepping, Pause/Resume, Reset/Clear, feedback, shared gameplay composition, and fresh scene sessions. The placeholder remains inaccessible through application navigation. No migration change alters gameplay logic.

The recorded post-migration EditMode run has 26 passes, zero failures, and no skipped tests. PlayMode has 10 passes, zero failures, and no skipped tests. The build job completed successfully, and actual Web.data, Web.framework.js, Web.loader.js, and Web.wasm output exists under Builds/Web. The build helper throws if Unity's build result is unsuccessful.

The [browser result](../results/web-results.json) records 18 passing interactions with no captured browser errors, covering scene entry, editing, compatible/incompatible nesting, unnesting, reorder/removal, mass, paused scrolling, frozen physics/time, Resume, Reset, Clear, Weight, numeric-limit regressions, resize, and fresh sessions. The inspected screenshot shows the intended raw three-column workspace, nested acceleration in Force, matching 3 kg mass, visible units/feedback, gray ball, primitive bounds, and disabled equation-edit control.

I independently reran the layout check and filesystem/hash/reference analysis. I inspected the Unity and browser run evidence rather than rerunning those expensive suites. I did not change implementation code or regenerate assets.

## Nonblocking limitations

- The migration manifest records asset GUIDs and asset content hashes, but no original full `.meta` hashes. It proves GUID continuity, not byte-for-byte preservation of every importer setting. Current import/build/browser evidence supports the moved plugin's actual behavior. Recording metadata hashes would strengthen a future migration audit.
- `Tools/verify-layout.py` checks identities, ownership, inventory, destinations, and links. It does not automatically compare baseline content hashes or validate every assembly reference/platform setting. This review performed those additional checks; its passing result alone should not be described as full behavioral verification.
- No usable Git repository is available in this workspace. Historical baseline provenance and version-control move tracking cannot be independently established here. The supplied manifest and actual imported/build artifacts provide the available comparison.

These limits do not reveal a migration regression or a substantive missing feature. The accepted layout improves contributor discovery and retains the existing functional scope with substantial regression evidence.
