#!/usr/bin/env python3
"""Check the approved project layout and the asset migration's preserved identities."""
from pathlib import Path
from urllib.parse import unquote
import json
import re

ROOT = Path(__file__).resolve().parent.parent
RESULTS = ROOT / 'docs/verification/results'

def verify():
    migration = json.loads((RESULTS / 'layout-migration.json').read_text())
    assets = [entry for entry in migration['moved'] if entry['guid']]
    for entry in assets:
        path = ROOT / entry['new']
        assert path.exists(), f'Missing moved asset: {path}'
        meta = Path(str(path) + '.meta')
        assert meta.exists(), f'Missing metadata: {path}'
        assert re.search(r'^guid: ' + entry['guid'] + r'$', meta.read_text(), re.M), f'Changed GUID: {path}'
    obsolete = ['Assets/MotionCommotion', 'Assets/Settings/Scenes', 'Information', 'ReferenceImages', 'Verification', 'scripts', 'pass1spec.md']
    for old in obsolete:
        assert not (ROOT / old).exists(), f'Obsolete authored path: {old}'
    removed = {entry['guid'] for entry in migration['removed']}
    known = {}
    for meta in (ROOT / 'Assets').rglob('*.meta'):
        match = re.search(r'^guid: (\w+)$', meta.read_text(), re.M)
        assert match, f'Metadata without GUID: {meta}'
        assert match[1] not in known, f'Duplicate GUID: {meta} and {known.get(match[1])}'
        known[match[1]] = meta
    for folder in ['Assets', 'ProjectSettings']:
        for path in (ROOT / folder).rglob('*'):
            if not path.is_file() or path.suffix == '.meta':
                continue
            try:
                text = path.read_text()
            except UnicodeDecodeError:
                continue
            for target in re.findall(r'guid: ([0-9a-f]{32})', text):
                assert target not in removed, f'Reference to deleted asset: {path}'
    scenes = [
        'Assets/Scenes/Application/Title.unity',
        'Assets/Scenes/Application/InGame.unity',
        'Assets/Scenes/Application/LevelEditor.unity',
        'Assets/Scenes/Testing/Sandbox.unity',
    ]
    settings = (ROOT / 'ProjectSettings/EditorBuildSettings.asset').read_text()
    assert re.findall(r'    path: (.*)', settings) == scenes, 'Unexpected build scene inventory/order'
    assert sorted(str(p.relative_to(ROOT)) for p in (ROOT / 'Assets').rglob('*.unity')) == sorted(scenes)
    scripts = ROOT / 'Assets/Scripts'
    assemblies = {p.parent: json.loads(p.read_text()) for p in scripts.rglob('*.asmdef')}
    assert len(assemblies) == 5
    assert assemblies[scripts / 'Core']['noEngineReferences'] is True
    assert assemblies[scripts / 'Editor']['includePlatforms'] == ['Editor']
    for path in (ROOT / 'Assets').rglob('*.cs'):
        owners = [folder for folder in assemblies if folder in path.parents]
        assert len(owners) == 1, f'Unexpected assembly ownership: {path}'
    generator = (scripts / 'Editor/FoundationAssets.cs').read_text()
    assert 'Assets/MotionCommotion' not in generator
    for branch in ['Settings/Gameplay/', 'Input/UIReferences/', 'Prefabs/Gameplay/', 'Prefabs/UI/', 'Prefabs/Shapes/', 'Data/Blocks/Commands/', 'Data/Blocks/Formulas/', 'Meshes/']:
        assert branch in generator, f'Generator missing destination: {branch}'
    links = 0
    for path in [ROOT / 'README.md', *(ROOT / 'docs').rglob('*.md')]:
        for url in re.findall(r'\[[^\]]+\]\(([^)]+)\)', path.read_text()):
            if re.match(r'[a-z]+:', url) or url.startswith('#'):
                continue
            target = path.parent / unquote(url.split('#')[0])
            assert target.exists(), f'Broken link in {path.relative_to(ROOT)}: {url}'
            links += 1
    return dict(passed=True, migratedAssetChecks=len(assets), uniqueAssetGuids=len(known), buildScenes=scenes, assemblyCount=len(assemblies), documentationLinks=links)

if __name__ == '__main__':
    result = verify()
    RESULTS.mkdir(parents=True, exist_ok=True)
    (RESULTS / 'layout-results.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
