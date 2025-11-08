import re
import sys
from collections import defaultdict

if len(sys.argv) < 2:
    print("Usage: parse_scene_hierarchy.py <scene_file>")
    sys.exit(1)

scene_path = sys.argv[1]
with open(scene_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Split into blocks starting with --- !u!<type> &<id>
blocks = re.split(r"(?m)^--- (!u!\d+) &(?P<id>\d+)\r?$", text)
# The split will produce a list where pattern groups appear; easier to iterate with regex finditer
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

# Maps
gameobject_name = {}
component_to_gameobject = {}
parent_of = {}
children_of = defaultdict(list)

# First pass: find GameObject blocks and their component IDs and names
for id, (t, block) in blocks_map.items():
    if t == '1':  # GameObject
        # m_Name:
        mname = re.search(r"m_Name:\s*(.*)\r?\n", block)
        name = mname.group(1).strip() if mname else f"<unnamed {id}>"
        gameobject_name[id] = name
        # find component entries like - component: {fileID: 4811114}
        comp_ids = re.findall(r"component:\s*\{fileID:\s*(-?\d+)\}", block)
        for cid in comp_ids:
            try:
                c = int(cid)
                component_to_gameobject[c] = id
            except:
                pass

# Second pass: find RectTransform blocks to get father links
for id, (t, block) in blocks_map.items():
    if t == '224':  # RectTransform
        # m_GameObject: {fileID: 4811113}
        mg = re.search(r"m_GameObject:\s*\{fileID:\s*(-?\d+)\}", block)
        if not mg:
            continue
        go_id = int(mg.group(1))
        # m_Father: {fileID: 735436536}
        mf = re.search(r"m_Father:\s*\{fileID:\s*(-?\d+)\}", block)
        if mf:
            father_comp = int(mf.group(1))
            parent_go = component_to_gameobject.get(father_comp)
            if parent_go is not None:
                parent_of[go_id] = parent_go

# Build child lists
for go_id in gameobject_name.keys():
    parent = parent_of.get(go_id)
    if parent:
        children_of[parent].append(go_id)

roots = [gid for gid in gameobject_name.keys() if gid not in parent_of.values() and gid not in parent_of]
# The above condition may miss some roots; better detect any go that has no parent
roots = [gid for gid in gameobject_name.keys() if gid not in parent_of]

# Sort children by name
for k in children_of:
    children_of[k].sort(key=lambda x: gameobject_name.get(x, ''))

# Recursive print
sys.setrecursionlimit(10000)

def print_tree(go_id, indent=0):
    name = gameobject_name.get(go_id, f"<unnamed {go_id}>")
    print('  ' * indent + f"- {name}  (fileID {go_id})")
    for child in children_of.get(go_id, []):
        print_tree(child, indent+1)

# Print roots sorted
for root in sorted(roots, key=lambda x: gameobject_name.get(x, '')):
    print_tree(root)

# Done
