# Project layout migration

The approved folder reorganization is applied. The [layout guide](../../architecture/project-layout.md) lists contributor destinations; the [audit](../../architecture/project-layout-audit.md) records the original problems and approved decisions.

## Changes

- One scene tree, with Application and Testing subfolders. Editor is now LevelEditor and remains an inaccessible placeholder.
- All custom C# under Assets/Scripts, with five preserved assembly boundaries and separate configuration and diagnostics folders.
- Descriptive command/formula assets under Data/Blocks; separate gameplay settings, rendering settings, and input assets.
- Prefabs grouped by purpose, meshes and materials in visible asset-type folders, and the JavaScript plugin under Plugins/WebGL.
- Unused SampleScene and the optional URP scene-template pair removed. Active rendering and input assets retained.
- Authored documentation and original references consolidated under docs; external verification helpers moved to Tools.
- Generator, tests, Build Settings, browser selectors, documentation links, and ignore patterns updated.

Metadata moved with assets. The [migration manifest](../results/layout-migration.json) records original paths, final paths, pre-migration content hashes, GUIDs, and the three intentional removals. All 64 moved asset GUIDs were preserved. Rendering assets, input assets, meshes, materials, gameplay scenes, and prefabs retain their original content. Content changes under Assets are limited to generator paths, test scene and asset references, palette object names, and the renamed placeholder root.

The generator was not run over editable assets. Its destinations were inspected and checked against the approved layout.

## Verification

| Check | Result | Evidence |
| --- | --- | --- |
| Layout, GUID preservation, assembly ownership, scene inventory, documentation links | Passed | [Layout results](../results/layout-results.json) |
| Unity compilation | Passed | EditMode and PlayMode suites compiled and executed |
| EditMode | 26 passed, zero failed | [Results](../results/editmode-results.json) |
| PlayMode | 10 passed, zero failed | [Results](../results/playmode-results.json) |
| WebGL development build | Completed | [Build result](../results/web-build-result.json) |
| Browser interaction suite | 18 passed, no captured browser errors | [Results](../results/web-results.json) |
| Unity missing-script and serialized-reference scan | 669 objects checked; no missing scripts or new unresolved references | [Raw scan and migration assessment](../results/unity-reference-results.json) |

The raw reference scan reports six unresolved `Renderer2D.probeVolumeResources` debug-resource references. The renderer's SHA256 exactly matches its pre-migration hash. These are pre-existing template references, not migration regressions. The raw scan keeps its failed status; the separate migration assessment identifies the baseline and reports zero new unresolved references. This migration does not claim that the original template was entirely free of unresolved references.

The initial browser attempt stopped because selectors used the former numbered palette asset names. Selectors now use the descriptive command/formula names. The application had no captured browser errors during that attempt.

The Unity test runner initially remained pending after the transition from the temporary migration scene. It was cancelled and PlayMode was rerun from a saved Title scene; all ten tests completed in 6.48 seconds.

## Scope and history

No authored levels, working level editor, new formulas, or gameplay behavior were added. Historical adversarial reviews retain their original ratings and source-path text. Their links were relocated, and the manifest maps former asset paths to current ones. Canonical result files contain post-migration runs.

The workspace does not expose a usable Git repository to git status, so verification used the pre-migration manifest, Unity import and tests, an actual WebGL build, and browser interactions rather than a Git diff.

## Independent review

The [final migration review](layout-adversarial-review.md) rated the implementation 9.4/10, passed the acceptance gate, and explicitly found no substantive missing features from pass1spec.md. No blockers remain. The reviewer independently checked GUIDs, content hashes, assembly boundaries, scene consumers, documentation, and the renderer baseline, and inspected the Unity/build/browser results.
