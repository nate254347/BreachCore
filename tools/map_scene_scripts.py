import re
import sys
import os
from collections import defaultdict

if len(sys.argv) < 2:
    print("Usage: map_scene_scripts.py <scene_file>")
    sys.exit(1)

scene_path = sys.argv[1]
root = os.path.dirname(os.path.dirname(scene_path))  # project root (assumes Assets/Scenes/...)
assets_root = os.path.join(root, 'Assets')

text = open(scene_path, 'r', encoding='utf-8').read()

# find blocks
pattern = re.compile(r"^--- (!u!(?P<type>\d+)) &(?P<id>\d+)\r?$", re.MULTILINE)
indices = list(pattern.finditer(text))
blocks_map = {}
for i, m in enumerate(indices):
    start = m.end()
    end = indices[i+1].start() if i+1 < len(indices) else len(text)
    t = m.group('type')
    id = int(m.group('id'))
    block_text = text[start:end]
    blocks_map[id] = (t, block_text)

# Map GameObjects -> name and component fileIDs
gameobject_name = {}
gameobject_components = defaultdict(list)
for id, (t, block) in blocks_map.items():
    if t == '1':  # GameObject
        mname = re.search(r"m_Name:\s*(.*)\r?\n", block)
        name = mname.group(1).strip() if mname else f"<unnamed {id}>"
        gameobject_name[id] = name
        comp_ids = re.findall(r"component:\s*\{fileID:\s*(-?\d+)\}", block)
        for cid in comp_ids:
            try:
                c = int(cid)
                gameobject_components[id].append(c)
            except:
                pass

# Map component fileID -> MonoBehaviour details (script guid)
component_script_guid = {}
for id, (t, block) in blocks_map.items():
    if t == '114':  # MonoBehaviour
        # m_GameObject
        mg = re.search(r"m_GameObject:\s*\{fileID:\s*(-?\d+)\}", block)
        ms = re.search(r"m_Script:\s*\{fileID:\s*(?P<fileid>-?\d+), guid:\s*(?P<guid>[0-9a-fA-F]+), type:\s*\d+\}", block)
        # Some scene files show m_Script: {fileID: 11500000, guid: f4688... , type: 3}
        m2 = re.search(r"m_Script:\s*\{fileID:\s*(?P<fileid>-?\d+), guid:\s*(?P<guid>[0-9a-fA-F]+), type:\s*\d+\}", block)
        if m2:
            guid = m2.group('guid')
        else:
            # fallback: look for 'm_Script:' line and extract guid anywhere in that line
            m3 = re.search(r"m_Script:.*guid:\s*([0-9a-fA-F]+)", block)
            guid = m3.group(1) if m3 else None
        component_script_guid[id] = guid

# Build mapping GameObject -> list of (component fileID, script guid)
go_to_scripts = defaultdict(list)
for go_id, comps in gameobject_components.items():
    for comp in comps:
        guid = component_script_guid.get(comp)
        if guid:
            go_to_scripts[go_id].append((comp, guid))

# Resolve GUID -> asset path by scanning .meta files under Assets
guid_to_asset = {}
for dirpath, dirnames, filenames in os.walk(assets_root):
    for fname in filenames:
        if fname.endswith('.meta'):
            full = os.path.join(dirpath, fname)
            try:
                with open(full, 'r', encoding='utf-8') as f:
                    meta = f.read()
            except:
                continue
            m = re.search(r"guid:\s*([0-9a-fA-F]+)", meta)
            if m:
                g = m.group(1)
                # asset path is same filename without .meta
                asset = full[:-5]
                # normalize path relative to project root
                rel = os.path.relpath(asset, root).replace('\\', '/')
                guid_to_asset[g] = rel

# Print mapping in human readable form
for go_id in sorted(gameobject_name.keys(), key=lambda x: gameobject_name[x]):
    name = gameobject_name[go_id]
    scripts = go_to_scripts.get(go_id, [])
    if not scripts:
        continue
    print(f"GameObject: {name} (fileID {go_id})")
    for comp_id, guid in scripts:
        asset = guid_to_asset.get(guid, None)
        if asset:
            print(f"  - component fileID {comp_id} -> {asset} [guid {guid}]")
        else:
            print(f"  - component fileID {comp_id} -> [guid {guid}] (asset not found in Assets/)" )
    print()

if not any(go_to_scripts.values()):
    print("No MonoBehaviour attachments found in scene (or GUIDs not present in scene file).")
