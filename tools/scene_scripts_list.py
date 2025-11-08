import re
import sys
import os
from collections import defaultdict

if len(sys.argv) < 2:
    print("Usage: scene_scripts_list.py <scene_file>")
    sys.exit(1)

scene_path = sys.argv[1]
# compute project root: go up from Assets/Scenes/... to repo root
project_root = os.path.abspath(os.path.join(os.path.dirname(scene_path), '..', '..'))
assets_root = os.path.join(project_root, 'Assets')
packages_root = os.path.join(project_root, 'Packages')

text = open(scene_path, 'r', encoding='utf-8').read()
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

# GameObjects
gameobject_name = {}
gameobject_components = defaultdict(list)
for id, (t, block) in blocks_map.items():
    if t == '1':
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

# Component -> script GUID (for MonoBehaviour blocks)
component_script_guid = {}
for id, (t, block) in blocks_map.items():
    if t == '114':
        # get m_GameObject to know ownership (not strictly needed here)
        mg = re.search(r"m_GameObject:\s*\{fileID:\s*(-?\d+)\}", block)
        # extract guid
        m2 = re.search(r"m_Script:\s*\{fileID:\s*-?\d+, guid:\s*([0-9a-fA-F]+), type:\s*\d+\}", block)
        if m2:
            guid = m2.group(1)
            component_script_guid[id] = guid
        else:
            # fallback: try looser match
            m3 = re.search(r"m_Script:.*guid:\s*([0-9a-fA-F]+)", block)
            if m3:
                component_script_guid[id] = m3.group(1)

# Build GameObject -> list of GUIDs
go_to_guids = defaultdict(list)
for go_id, comps in gameobject_components.items():
    for comp in comps:
        guid = component_script_guid.get(comp)
        if guid:
            go_to_guids[go_id].append((comp, guid))

# Build guid -> asset path by scanning Assets/ and Packages/
guid_to_asset = {}
search_paths = [assets_root, packages_root]
for root in search_paths:
    if not os.path.exists(root):
        continue
    for dirpath, dirnames, filenames in os.walk(root):
        for fname in filenames:
            if fname.endswith('.meta'):
                meta = os.path.join(dirpath, fname)
                try:
                    with open(meta, 'r', encoding='utf-8') as f:
                        data = f.read()
                except Exception:
                    continue
                m = re.search(r"guid:\s*([0-9a-fA-F]+)", data)
                if m:
                    g = m.group(1)
                    asset_path = os.path.normpath(meta[:-5])
                    rel = os.path.relpath(asset_path, project_root).replace('\\', '/')
                    guid_to_asset[g] = rel

# Format output: GameObject > Name1 (path), Name2 (path)
lines = []
for go_id in sorted(gameobject_name.keys(), key=lambda x: gameobject_name[x].lower()):
    name = gameobject_name[go_id]
    entries = []
    for comp_id, guid in go_to_guids.get(go_id, []):
        asset = guid_to_asset.get(guid)
        if asset:
            base = os.path.basename(asset)
            entries.append(f"{base} ({asset})")
        else:
            entries.append(f"[guid:{guid}]")
    if entries:
        lines.append(f"{name} > " + ", ".join(entries))

if not lines:
    print("No MonoBehaviour scripts attached (or none resolved).")
else:
    print("\n".join(lines))

# Also print a small summary counts
print()
print(f"Total GameObjects with resolved scripts: {len(lines)}")
print(f"GUIDs resolved to assets: {len([g for g in guid_to_asset])}")
