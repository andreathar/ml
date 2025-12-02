# How to Make Queries to KB and Take Decisions

> **Purpose**: Guide for AI coding agents to perform self-learning workflows by querying the Qdrant Knowledge Base and making informed decisions based on results.

**Generated**: 2025-12-01
**KB Collection**: `unity_project_kb`
**Total Symbols**: 77,914

---

## Table of Contents

1. [KB Connection Setup](#1-kb-connection-setup)
2. [Query: Sync Components Catalog](#2-query-sync-components-catalog)
3. [Query: Classes Handling Sync States](#3-query-classes-handling-sync-states)
4. [Query: GameObjects Holding Sync Components](#4-query-gameobjects-holding-sync-components)
5. [Query: Prefabs with NetworkTransform](#5-query-prefabs-with-networktransform)
6. [Decision Making Patterns](#6-decision-making-patterns)

---

## 1. KB Connection Setup

### Qdrant Configuration

```python
from qdrant_client import QdrantClient

QDRANT_HOST = "localhost"
QDRANT_PORT = 6333
COLLECTION_NAME = "unity_project_kb"

def get_client() -> QdrantClient:
    return QdrantClient(
        host=QDRANT_HOST,
        port=QDRANT_PORT,
        timeout=60,
        check_compatibility=False
    )
```

### KB Schema (Payload Fields)

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Symbol name (class, method, property, etc.) |
| `kind` | string | Symbol type: `class`, `method`, `property`, `field`, `struct` |
| `file_path` | string | Full path to source file |
| `namespace` | string | C# namespace |
| `assembly_name` | string | Assembly name (e.g., `GameCreator.Runtime.Inventory`) |
| `signature` | string | Method/property signature |
| `code_preview` | string | Code snippet (first ~300 chars) |

---

## 2. Query: Sync Components Catalog

### Goal
Find all scripts that make Sync calls (NetworkVariable, RPCs, etc.) in the Plugins folder.

### Python Script

```python
#!/usr/bin/env python3
"""
Query KB for all Sync-related scripts in Plugins folder
"""

import sys
sys.stdout.reconfigure(encoding='utf-8')

from qdrant_client import QdrantClient
from collections import defaultdict

# Sync-related patterns to search for
SYNC_PATTERNS = [
    # NetworkVariable patterns
    "NetworkVariable", "NetworkList", "NetworkDictionary",
    # Sync method patterns
    "Sync", "SyncVar", "SyncList",
    # RPC patterns
    "ServerRpc", "ClientRpc", "Rpc",
    # Get/Set patterns for network data
    "GetNetwork", "GetSync", "GetRpc",
    "SetNetwork", "SetSync",
    # Specific sync components
    "NetworkBehaviour", "NetworkObject",
    # Callback patterns
    "OnValueChanged", "OnListChanged",
    # Serialization patterns
    "NetworkSerialize", "INetworkSerializable",
    # State sync patterns
    "SyncState", "StateSync"
]

def query_sync_scripts(client: QdrantClient):
    """Query all sync-related scripts from Plugins folder"""

    all_sync_items = []
    seen_files = set()
    offset = None

    while True:
        scroll_result = client.scroll(
            collection_name="unity_project_kb",
            limit=10000,
            offset=offset,
            with_payload=True,
            with_vectors=False
        )

        points = scroll_result[0]
        if not points:
            break

        for point in points:
            payload = point.payload
            file_path = payload.get("file_path", "")
            name = payload.get("name", "")
            kind = payload.get("kind", "")
            signature = payload.get("signature", "")
            assembly = payload.get("assembly_name", "")
            code_preview = payload.get("code_preview", "")

            # Filter for Plugins folder
            if "Plugins" not in file_path and "GameCreator" not in assembly:
                continue

            # Check for sync patterns
            matched_patterns = []
            search_text = f"{name} {signature} {code_preview}".lower()

            for pattern in SYNC_PATTERNS:
                if pattern.lower() in search_text:
                    matched_patterns.append(pattern)

            if matched_patterns:
                unique_key = f"{file_path}:{name}:{kind}"
                if unique_key not in seen_files:
                    seen_files.add(unique_key)
                    all_sync_items.append({
                        "name": name,
                        "kind": kind,
                        "file_path": file_path,
                        "assembly": assembly,
                        "matched_patterns": list(set(matched_patterns))
                    })

        offset = scroll_result[1]
        if offset is None:
            break

    return all_sync_items
```

### Results Summary

```
Total Sync Items: 1,714
Files with Sync Code: 374

Category Breakdown:
  - Server RPCs: 74
  - Client RPCs: 59
  - Sync Methods: 112
  - Get Methods: 47
  - Set Methods: 23
  - Serializable Structs: 23
  - Sync Components: 19
  - Network Variables: 5
  - Callbacks: 1
```

### Decision Point
- **If adding new sync functionality**: Check existing patterns in the 1,714 items first
- **If debugging sync issues**: Filter by assembly to narrow down scope

---

## 3. Query: Classes Handling Sync States

### Goal
Identify the 19 core classes that handle state synchronization.

### Query Approach
Filter KB results for classes with "Sync" in name within MLCreator namespaces.

### Results: 19 Sync Component Classes

| # | Class | Namespace | File | Purpose |
|---|-------|-----------|------|---------|
| 1 | `AsyncManager` | GameCreator.Runtime.Common | AsyncManager.cs | Core async operations |
| 2 | `BehaviorTreeSync` | MLCreator_Multiplayer.Runtime.Behavior | BehaviorTreeSync.cs | AI behavior tree sync |
| 3 | `EventOnNetworkVariableSynced` | MLCreator_Multiplayer.Runtime.VisualScripting | EventOnNetworkVariableSynced.cs | VS event for sync |
| 4 | `InstructionNetworkSyncVariable` | MLCreator_Multiplayer.Runtime.VisualScripting | InstructionNetworkSyncVariable.cs | VS instruction |
| 5 | `InstructionRPCSyncVariable` | MLCreator_Multiplayer.Runtime.RPC | InstructionRPCSyncVariable.cs | RPC variable sync |
| 6 | `NetworkBehaviorSync` | MLCreator_Multiplayer.Runtime.Modules | NetworkBehaviorSync.cs | GC Behavior module sync |
| 7 | `NetworkDialogueSync` | MLCreator_Multiplayer.Runtime.Modules | NetworkDialogueSync.cs | Dialogue state sync |
| 8 | `NetworkInventorySync` | MLCreator_Multiplayer.Runtime.Components | NetworkInventorySync.cs | Inventory items/slots sync |
| 9 | `NetworkModuleSyncBase` | MLCreator_Multiplayer.Runtime.Modules | NetworkModuleSyncBase.cs | Base class for all syncs |
| 10 | `NetworkModuleSyncManager` | MLCreator_Multiplayer.Runtime.Modules | NetworkModuleSyncManager.cs | Orchestrates sync lifecycle |
| 11 | `NetworkPerceptionSync` | MLCreator_Multiplayer.Runtime.Modules | NetworkPerceptionSync.cs | AI perception/awareness sync |
| 12 | `NetworkQuestSync` | MLCreator_Multiplayer.Runtime.Modules | NetworkQuestSync.cs | Quest progress sync |
| 13 | `NetworkStatsSync` | MLCreator_Multiplayer.Runtime.Modules | NetworkStatsSync.cs | Stats/attributes sync |
| 14 | `NetworkVariablesSync` | MLCreator_Multiplayer.Runtime.Modules | NetworkVariablesSync.cs | Core variable sync |
| 15 | `RPCVariableSyncReceiver` | MLCreator_Multiplayer.Runtime.RPC | InstructionRPCSyncVariable.cs | RPC receiver |
| 16 | `StateMachineSync` | MLCreator_Multiplayer.Runtime.Behavior | StateMachineSync.cs | State machine sync |
| 17 | `SupabaseVariableSyncManager` | GameCreator.Multiplayer.Runtime.Supabase | SupabaseVariableSyncManager.cs | Cloud persistence |
| 18 | `SyncedVariable` | MLCreator_Multiplayer.Runtime.Modules | NetworkVariablesSync.cs | Individual variable wrapper |
| 19 | `VariableSyncProfile` | MLCreator_Multiplayer.Runtime.Modules | NetworkVariablesSync.cs | Sync behavior profile |

### Decision Point
- **When creating new sync component**: Inherit from `NetworkModuleSyncBase`
- **When adding GameCreator module sync**: Follow pattern of existing module syncs (Inventory, Stats, etc.)
- **Architecture**: `NetworkModuleSyncManager` orchestrates all module syncs

---

## 4. Query: GameObjects Holding Sync Components

### Goal
Identify which GameObjects hold sync-triggering components.

### Query Methods

#### Method 1: Search for AddComponent/GetComponent patterns
```bash
grep -r "AddComponent.*Sync\|GetComponent.*Sync\|RequireComponent.*Sync" Assets/ --include="*.cs"
```

#### Method 2: Search for AddComponentMenu attributes
```bash
grep -r "AddComponentMenu.*Sync\|AddComponentMenu.*Network" Assets/ --include="*.cs"
```

### Results: GameObject → Component Mapping

#### Auto-Created Singletons (Scene-Level Managers)

| GameObject Name | Component | Pattern |
|-----------------|-----------|---------|
| `[NetworkModuleSyncManager]` | NetworkModuleSyncManager | Auto-created singleton, `DontDestroyOnLoad` |
| `[Network Variables Sync]` | NetworkVariablesSync | Auto-created singleton, `DontDestroyOnLoad` |

**Code Pattern (NetworkModuleSyncManager.cs:30-32)**:
```csharp
GameObject go = new GameObject("[NetworkModuleSyncManager]");
s_Instance = go.AddComponent<NetworkModuleSyncManager>();
DontDestroyOnLoad(go);
```

#### Player Character GameObject

Components attached via `NetworkPlayerController`:

| Component | Attachment Method | Code Location |
|-----------|-------------------|---------------|
| `NetworkInventorySync` | `gameObject.AddComponent<NetworkInventorySync>()` | NetworkPlayerController.cs:383 |
| `NetworkPerceptionSync` | `gameObject.AddComponent<NetworkPerceptionSync>()` | NetworkPlayerController.cs:403 |
| `NetworkStatsSync` | Pre-attached via Inspector | Uses `[AddComponentMenu]` |

**Required Components on Player Prefab**:
- `GameCreatorCharacter` (RequireComponent)
- `NetworkObject` (RequireComponent)
- `NetworkPlayerController`
- `NetworkCharacterAdapter`

#### NPC/AI Character GameObjects

| Component | Attached To | Purpose |
|-----------|-------------|---------|
| `NetworkBehaviorSync` | NPC with Behavior Tree | Syncs AI behavior state |
| `NetworkProcessor` | NPC with `Processor` | Syncs AI processor state |
| `BehaviorTreeSync` | AI character | Syncs behavior tree nodes |
| `StateMachineSync` | AI character | Syncs state machine states |
| `NetworkPerceptionSync` | NPC | Syncs AI awareness |

#### Interactive Objects

| GameObject Type | Component | AddComponentMenu Path |
|-----------------|-----------|----------------------|
| Pickup items | `NetworkPickup` | `GameCreator/Multiplayer/Network Pickup` |
| Stat pickups | `NetworkStatPickup` | `GameCreator/Multiplayer/Network Stat Pickup` |
| Doors | `NetworkDoor` | `GameCreator/Multiplayer/Network Door` |
| Triggers | `NetworkTriggerAdapter` | `GameCreator/Multiplayer/Network Trigger Adapter` |
| Hotspots | `NetworkHotspotExtension` | Extends hotspot interactions |

### Component Hierarchy Summary

```
SINGLETONS (Auto-Created):
├── [NetworkModuleSyncManager]    ← NetworkModuleSyncManager
└── [Network Variables Sync]      ← NetworkVariablesSync

PLAYER PREFAB:
├── NetworkPlayerController
├── NetworkCharacterAdapter
├── NetworkInventorySync          ← Added dynamically
├── NetworkPerceptionSync         ← Added dynamically
├── NetworkStatsSync              ← Pre-attached
└── NetworkObject

NPC PREFAB:
├── NetworkProcessor
├── NetworkBehaviorSync
├── NetworkPerceptionSync
├── BehaviorTreeSync / StateMachineSync
└── NetworkObject

DIALOGUE SPEAKER:
└── NetworkDialogueSync

QUEST GIVER:
└── NetworkQuestSync
```

### Decision Point
- **When adding new player feature**: Check if component should be dynamically added or pre-attached
- **When creating NPC**: Include appropriate sync components based on NPC capabilities
- **Singleton pattern**: Use auto-create pattern for scene-level managers

---

## 5. Query: Prefabs with NetworkTransform

### Goal
Find all prefabs using Unity's NetworkTransform component.

### Query Command
```bash
grep -r "NetworkTransform" Assets/ --include="*.prefab"
```

### Results: 9 Prefabs with NetworkTransform

| # | Prefab Name | Location | Type |
|---|-------------|----------|------|
| 1 | `FX_Land.prefab` | GameCreator.Characters@1.8.25/Assets/Particles/ | VFX |
| 2 | `FX_Footstep.prefab` | GameCreator.Characters@1.8.25/Assets/Particles/ | VFX |
| 3 | `FX_Poison.prefab` | Stats.Classes@1.3.7/_StatusEffects/Poison/Assets/ | VFX |
| 4 | `Chest_Storage_Grid.prefab` | Inventory.Examples@1.6.13/_Prefabs/Chests/ | Interactive Object |
| 5 | `Chest_Loot_Random.prefab` | Inventory.Examples@1.6.13/_Prefabs/Chests/ | Interactive Object |
| 6 | `Chest_Loot_Items.prefab` | Inventory.Examples@1.6.13/_Prefabs/Chests/ | Interactive Object |
| 7 | `Pickup_Coin_Copper_1.prefab` | Inventory.Items@1.3.13/_Prefabs/Pickups/ | Pickup |
| 8 | `Pickup_Coin_Silver_1.prefab` | Inventory.Items@1.3.13/_Prefabs/Pickups/ | Pickup |
| 9 | `Pickup_Coin_Gold_1.prefab` | Inventory.Items@1.3.13/_Prefabs/Pickups/ | Pickup |

### Decision Point

> ⚠️ **CRITICAL ARCHITECTURE RULE**:
> **NEVER use NetworkTransform on player prefabs** - use `NetworkCharacterAdapter` instead.

**Acceptable NetworkTransform usage**:
- VFX particles (FX_Land, FX_Footstep, FX_Poison) - simple position sync
- Static interactive objects (Chests) - rarely move
- Simple pickups (Coins) - basic position sync

**Not acceptable**:
- Player characters - use `NetworkCharacterAdapter`
- NPCs with complex movement - use custom sync

---

## 6. Decision Making Patterns

### Pattern 1: Before Adding Network Feature

```
1. Query KB for existing implementations
   → Search: "{feature_name}" in Plugins folder

2. Check if similar pattern exists
   → If YES: Follow existing pattern
   → If NO: Check NetworkModuleSyncBase for extension point

3. Verify component attachment
   → Player feature: Dynamic or pre-attached?
   → Scene feature: Singleton or per-object?

4. Validate against architecture rules
   → No NetworkTransform on characters
   → Use NetworkCharacterAdapter for movement
```

### Pattern 2: Debugging Sync Issues

```
1. Identify the sync component involved
   → Query: Class name in sync-scripts-catalog

2. Find the triggering GameObject
   → Query: GetComponent<{SyncClass}> patterns

3. Trace the RPC flow
   → ServerRpc: Client → Server
   → ClientRpc: Server → All Clients

4. Check ownership
   → IsOwner, IsServer, IsHost conditions
```

### Pattern 3: Creating New GameCreator Module Sync

```
1. Create class inheriting NetworkModuleSyncBase
2. Add [AddComponentMenu("GameCreator/Multiplayer/Network {Module} Sync")]
3. Implement required sync methods:
   - RequestSyncServerRpc()
   - SyncStateClientRpc()
4. Register with NetworkModuleSyncManager
5. Add to appropriate prefab (Player, NPC, or Scene object)
```

---

## Quick Reference: KB Query Templates

### Find all classes of a type
```python
# Filter by kind and namespace
for point in points:
    if point.payload.get("kind") == "class":
        if "MLCreator" in point.payload.get("namespace", ""):
            # Process class
```

### Find methods with specific pattern
```python
# Search in name and signature
search_text = f"{name} {signature}".lower()
if "serverrpc" in search_text:
    # Found ServerRpc method
```

### Find component attachments
```python
# Search in code_preview
code = point.payload.get("code_preview", "")
if "AddComponent" in code or "GetComponent" in code:
    # Found component attachment
```

### Filter by assembly
```python
# Restrict to specific assembly
assembly = point.payload.get("assembly_name", "")
if assembly == "MLCreator_Multiplayer.Runtime":
    # Process MLCreator multiplayer code
```

---

## File Locations

| Resource | Path |
|----------|------|
| Sync Scripts Catalog | `.serena/personas/sync-scripts-catalog.llm.txt` |
| Module Persona Schemas | `.serena/personas/{module}.llm.txt` |
| Architecture Rules | `.serena/memories/CRITICAL/002_network_architecture_never_forget.md` |
| KB Query Scripts | `scripts/unity-kb/` |

---

*This guide enables AI agents to self-learn the codebase architecture through KB queries and make informed implementation decisions.*
