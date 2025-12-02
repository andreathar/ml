# Unity KB Schema Snapshot (Auto-Generated)

- Generated: 2025-12-01
- Source: Qdrant collection `unity_project_kb` (health green via port 6333)
- Points: 77,914; Assemblies: 67; Namespaces: 184; Fields: 33
- Kind counts: scene_element 47,437; method 15,576; property 7,669; class 5,636; documentation 898; enum 351; struct 231; interface 116
- Note: Scene entries dominate because every scene object is indexed as a component/transform element.

## Field Inventory
- Core: `name`, `kind`, `assembly_name`, `namespace`, `signature`, `code_type`, `file_path`, `start_line`, `end_line`, `class_name`, `modifiers`, `return_type`, `asset_guid`, `chunk_index`, `content_hash`, `code_preview`, `documentation`
- Scenes: `scene_name`, `element_type`, `properties`, `indexed_at`
- Docs: `doc_type`, `doc_title`, `doc_section`, `title`, `tags`, `category`, `name_path`, `content`, `headings`, `doc_keywords`, `doc_priority`, `doc_word_count`
- Misc: `properties` (scene metadata), `element_type` (scene), `code_preview` and `documentation` carry truncated content for classes/methods.
- Relationships: Members share `class_name`; scene components carry `properties.file_id`; docs carry `name_path` for hierarchy; no explicit reference list is stored.

## Kind → Field Coverage
- class: assembly_name, asset_guid, chunk_index, class_name, code_preview, code_type, content_hash, documentation, end_line, file_path, kind, modifiers, name, namespace, return_type, signature, start_line
- method/property: assembly_name, asset_guid, chunk_index, class_name, code_preview, code_type, content_hash, documentation, end_line, file_path, kind, modifiers, name, namespace, return_type, signature, start_line
- enum/struct/interface: assembly_name, asset_guid, chunk_index, class_name, code_preview, code_type, content_hash, documentation, end_line, file_path, kind, modifiers, name, namespace, return_type, signature, start_line
- scene_element: assembly_name, code_type, element_type, file_path, indexed_at, kind, name, properties, scene_name, signature
- documentation: assembly_name, category, chunk_index, code_preview, code_type, content, content_hash, doc_keywords, doc_priority, doc_section, doc_title, doc_type, doc_word_count, documentation, end_line, file_path, headings, kind, name, name_path, namespace, signature, start_line, tags, title

## Top Assemblies (by symbol count)
- Unity.Scene: 47,437
- GameCreator.Runtime.Core: 9,639
- GameCreator.Editor.Core: 2,097
- GameCreator.Runtime.Inventory: 1,852
- MLCreator_Multiplayer.Runtime: 1,512
- GameCreator.Runtime.Tactile: 1,360
- GameCreator.Editor.Behavior: 954
- GameCreator.Runtime.Stats: 899
- (none/empty): 728
- GameCreator.Runtime.Quests: 672
- NinjutsuGames.Runtime.Factions: 666
- GameCreator.Runtime.Behavior: 661
- undream.llmunity.Runtime: 659
- GameCreator.Runtime.Perception: 531
- MLCreator_Multiplayer.Runtime.RPC: 487
- GameCreator.Runtime.Dialogue: 467
- MLCreator_Multiplayer.Runtime.VisualScripting: 426
- GameCreator.Editor.Inventory: 414
- AssemblyGraph.Core: 411
- MLCreator.EditorTools: 392

## Top Namespaces (by symbol count)
- (none/scene): 48,566
- GameCreator.Runtime.Common: 4,014
- GameCreator.Runtime.VisualScripting: 2,044
- GameCreator.Runtime.Characters: 1,522
- GameCreator.Runtime.Inventory: 1,459
- GameCreator.Runtime.Tactile: 1,360
- GameCreator.Editor.Behavior: 954
- GameCreator.Runtime.Variables: 881
- GameCreator.Runtime.Stats: 812
- GameCreator.Editor.Common: 714
- LLMUnity: 708
- GameCreator.Runtime.Behavior: 661
- NinjutsuGames.Runtime.Factions: 564
- GameCreator.Runtime.Quests: 555
- GameCreator.Runtime.Perception: 499
- MLCreator_Multiplayer.Runtime.RPC: 487
- GameCreator.Runtime.Cameras: 463
- GameCreator.Runtime.Inventory.UnityUI: 387
- MLCreator_Multiplayer.Runtime.Components: 370
- GameCreator.Runtime.Dialogue: 360

## Regeneration (refresh this snapshot)
```bash
@'
from collections import Counter
from qdrant_client import QdrantClient
client = QdrantClient(host="localhost", port=6333, timeout=60)
limit = 1000; offset = None
assembly_counter, namespace_counter, kind_counter = Counter(), Counter(), Counter()
while True:
    res, offset = client.scroll("unity_project_kb", limit=limit, offset=offset, with_payload=True, with_vectors=False)
    if not res:
        break
    for point in res:
        payload = point.payload or {}
        assembly_counter[payload.get("assembly_name", "")] += 1
        namespace_counter[payload.get("namespace", "")] += 1
        kind_counter[payload.get("kind", "")] += 1
    if offset is None:
        break
print("assemblies", assembly_counter.most_common(20))
print("namespaces", namespace_counter.most_common(20))
print("kinds", kind_counter)
'@ | python -
```

## Usage Notes
- Use `scripts/unity-kb/direct_qdrant_client.py` helpers (`search_unity_symbols`, `get_unity_class_members`) for symbol queries.
- Scene entries dominate; filter `kind != "scene_element"` for code-only views.
- There is no explicit `base_classes` or `references` payload; derive relationships by matching `class_name` for members and grouping by namespace/assembly for structure.
