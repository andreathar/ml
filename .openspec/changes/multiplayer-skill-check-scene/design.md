# Design: Multiplayer Skill Check Scene

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         SERVER (Host)                            │
├─────────────────────────────────────────────────────────────────┤
│  NetworkNPCManager                                               │
│  ├── Spawns all NPCs on server start                            │
│  ├── Owns NPC NetworkObjects                                    │
│  └── Handles late-join sync                                     │
│                                                                  │
│  NPC Characters (Server-Owned)                                   │
│  ├── Character_Elf    [NetworkObject + NetworkFactionMemberSync]│
│  ├── Character_Demon  [NetworkObject + NetworkFactionMemberSync]│
│  ├── Character_Goblin [NetworkObject + NetworkFactionMemberSync]│
│  ├── Character_Orc    [NetworkObject + NetworkFactionMemberSync]│
│  └── Character_Human  [NetworkObject + NetworkFactionMemberSync]│
│                                                                  │
│  NetworkPlayerManager                                            │
│  └── Spawns player characters (existing)                        │
└─────────────────────────────────────────────────────────────────┘
                              │
                    NetworkVariables
                    (Faction state sync)
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENTS                                  │
├─────────────────────────────────────────────────────────────────┤
│  - See synced NPC faction state via NetworkVariables            │
│  - Request faction interactions via ServerRpc                   │
│  - Receive faction events via ClientRpc                         │
│  - Local UI updates from OnValueChanged callbacks               │
└─────────────────────────────────────────────────────────────────┘
```

## Component Design

### NetworkFactionMemberSync

Syncs a GameCreator `Member` component's faction state across the network.

```csharp
public class NetworkFactionMemberSync : NetworkBehaviour
{
    // Sync faction assignment
    private NetworkVariable<FixedString64Bytes> m_FactionId = new(
        writePerm: NetworkVariableWritePermission.Server
    );

    // Sync active membership status
    private NetworkVariable<bool> m_IsActive = new(
        value: true,
        writePerm: NetworkVariableWritePermission.Server
    );

    // Reference to GameCreator Member component
    private Member m_Member;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_Member = GetComponent<Member>();

        if (IsServer)
        {
            // Initialize from existing Member component
            if (m_Member != null && m_Member.Faction != null)
            {
                m_FactionId.Value = m_Member.Faction.Id;
            }
        }

        // All clients sync faction ID changes
        m_FactionId.OnValueChanged += OnFactionIdChanged;
        m_IsActive.OnValueChanged += OnActiveChanged;

        // Apply current value on spawn
        ApplyFactionState();
    }

    private void OnFactionIdChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        ApplyFactionState();
    }

    private void ApplyFactionState()
    {
        if (m_Member == null) return;

        // Find faction asset by ID and assign
        var faction = FactionManager.Instance.GetFaction(m_FactionId.Value.ToString());
        if (faction != null)
        {
            m_Member.SetFaction(faction);
        }
    }

    // Server RPC for faction changes (if needed)
    [ServerRpc(RequireOwnership = false)]
    public void SetFactionServerRpc(string factionId)
    {
        m_FactionId.Value = factionId;
    }
}
```

### NetworkNPCManager

Server-side manager that spawns and owns all NPCs.

```csharp
public class NetworkNPCManager : NetworkBehaviour
{
    [Serializable]
    public class NPCSpawnConfig
    {
        public GameObject prefab;
        public string factionId;
        public Transform spawnPoint;
    }

    [SerializeField] private List<NPCSpawnConfig> m_NPCConfigs;

    private Dictionary<string, NetworkObject> m_SpawnedNPCs = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SpawnAllNPCs();
        }
    }

    private void SpawnAllNPCs()
    {
        foreach (var config in m_NPCConfigs)
        {
            SpawnNPC(config);
        }
    }

    private void SpawnNPC(NPCSpawnConfig config)
    {
        var npc = Instantiate(config.prefab, config.spawnPoint.position, config.spawnPoint.rotation);
        npc.name = $"NPC_{config.factionId}_{m_SpawnedNPCs.Count}";

        var netObj = npc.GetComponent<NetworkObject>();
        netObj.Spawn(); // Server owns

        // Set faction after spawn
        var factionSync = npc.GetComponent<NetworkFactionMemberSync>();
        if (factionSync != null)
        {
            factionSync.SetFactionServerRpc(config.factionId);
        }

        m_SpawnedNPCs[npc.name] = netObj;
    }
}
```

### InstructionNetworkCollectFactionMembers

Network-aware version of faction member collection.

```csharp
[Title("Collect Faction Members (Network)")]
[Category("Multiplayer/Factions")]
public class InstructionNetworkCollectFactionMembers : Instruction
{
    [SerializeField] private PropertyGetGameObject m_Towards;
    [SerializeField] private CompareFactionStatus m_Status;
    [SerializeField] private PropertyGetDecimal m_Radius;
    [SerializeField] private PropertySetVariable m_SaveTo;

    protected override Task Run(Args args)
    {
        var from = m_Towards.Get<Member>(args);
        float radius = (float)m_Radius.Get(args);
        Vector3 origin = m_Towards.Get(args).transform.position;

        var results = new List<GameObject>();

        // Find all NetworkFactionMemberSync components in range
        var allFactionMembers = FindObjectsByType<NetworkFactionMemberSync>(FindObjectsSortMode.None);

        foreach (var factionSync in allFactionMembers)
        {
            // CRITICAL: Only include network-spawned members
            if (!factionSync.IsSpawned) continue;

            // Check distance
            float distance = Vector3.Distance(factionSync.transform.position, origin);
            if (distance > radius) continue;

            // Check faction status
            var member = factionSync.GetComponent<Member>();
            if (member == null) continue;

            if (!m_Status.Match(member.HighestStatusToMember(from).Key))
                continue;

            results.Add(factionSync.gameObject);
        }

        // Sort by distance
        results.Sort((a, b) =>
            Vector3.Distance(a.transform.position, origin)
            .CompareTo(Vector3.Distance(b.transform.position, origin)));

        // Save to variable
        m_SaveTo.Set(results, args);

        return DefaultResult;
    }
}
```

## Scene Conversion Workflow

### Step 1: Create NPC Prefabs
For each NPC in the scene (Elf, Demon, Goblin, Orc, Human):

1. Select NPC GameObject
2. Add components in this order:
   - NetworkObject
   - NetworkCharacterAdapter (NOT NetworkTransform!)
   - NetworkFactionMemberSync
   - NetworkStatsSync (if NPC has Stats)
3. Save as prefab in `Assets/Prefabs/NPCs/`
4. Delete original from scene

### Step 2: Set Up NetworkNPCManager
1. Create empty GameObject "NetworkNPCManager"
2. Add NetworkNPCManager component
3. Add NetworkObject component
4. Configure NPC spawn configs:
   - Prefab reference
   - Faction ID
   - Spawn point transform

### Step 3: Update Triggers
Replace instructions in:
- `Trigger_Friendly`: Use `InstructionNetworkCollectFactionMembers`
- `Trigger_Neutral`: Use `InstructionNetworkCollectFactionMembers`
- `Trigger_Hostile`: Use `InstructionNetworkCollectFactionMembers`

### Step 4: Test Sequence
1. Start as Host
2. Verify NPCs spawn with correct factions
3. Start Client
4. Verify Client sees same NPCs
5. Test faction triggers on both Host and Client
6. Test Door skill check on both Host and Client

## Data Flow

### Faction State Flow
```
1. Server spawns NPC with NetworkFactionMemberSync
2. NetworkFactionMemberSync reads faction from Member component
3. m_FactionId NetworkVariable is set on server
4. NetworkVariable replicates to all clients
5. Client's OnValueChanged updates local Member component
6. Client UI reflects correct faction
```

### Faction Interaction Flow
```
1. Player interacts with NPC (Client)
2. Client fires Trigger_Friendly (local)
3. Trigger runs InstructionNetworkCollectFactionMembers
4. Instruction filters to IsSpawned members only
5. Results saved to ListVariables
6. Subsequent instructions use filtered list
```

## Network Traffic Analysis

### Per-NPC Bandwidth
- `m_FactionId`: 64 bytes (FixedString64Bytes), only changes on faction change
- `m_IsActive`: 1 byte, only changes on enable/disable

### Estimated Initial Sync (5 NPCs)
- Faction state: 5 * 65 bytes = 325 bytes
- Plus NetworkObject spawn overhead

### Ongoing Traffic
- Minimal - faction state rarely changes
- Stats sync uses batching (86% reduction)
