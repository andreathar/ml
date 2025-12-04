# Design: Perception Netcode Integration

## Data Structures

### TrackedTargetData (NetworkSerializable)
```csharp
public struct TrackedTargetData : INetworkSerializable, IEquatable<TrackedTargetData>
{
    public ulong TargetNetworkId;     // NetworkObject.NetworkObjectId of tracked target
    public float Awareness;            // 0-1 awareness level
    public byte Stage;                 // AwareStage enum as byte
    public float LastIncreaseTime;     // For forgetting calculation

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref TargetNetworkId);
        serializer.SerializeValue(ref Awareness);
        serializer.SerializeValue(ref Stage);
        serializer.SerializeValue(ref LastIncreaseTime);
    }

    public bool Equals(TrackedTargetData other) =>
        TargetNetworkId == other.TargetNetworkId;

    public override int GetHashCode() => TargetNetworkId.GetHashCode();
}
```

### EvidenceData (NetworkSerializable)
```csharp
public struct EvidenceData : INetworkSerializable, IEquatable<EvidenceData>
{
    public FixedString32Bytes EvidenceTag;
    public bool IsTampered;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref EvidenceTag);
        serializer.SerializeValue(ref IsTampered);
    }

    public bool Equals(EvidenceData other) =>
        EvidenceTag.Equals(other.EvidenceTag);

    public override int GetHashCode() => EvidenceTag.GetHashCode();
}
```

### NoiseEventData (NetworkSerializable)
```csharp
public struct NoiseEventData : INetworkSerializable
{
    public Vector3 Position;
    public float Intensity;
    public float Radius;
    public FixedString32Bytes Tag;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref Intensity);
        serializer.SerializeValue(ref Radius);
        serializer.SerializeValue(ref Tag);
    }
}
```

## Component Architecture

### NetworkPerception.cs
```csharp
namespace GameCreator.Netcode.Runtime
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Perception))]
    [AddComponentMenu("Game Creator/Network/Network Perception")]
    public class NetworkPerception : NetworkBehaviour
    {
        // CONSTANTS
        private const float SYNC_THRESHOLD = 0.01f;  // Min change to sync

        // NETWORK STATE
        private NetworkList<TrackedTargetData> m_TrackedTargets;
        private NetworkList<EvidenceData> m_Evidences;

        // LOCAL REFERENCES
        [NonSerialized] private Perception m_Perception;
        [NonSerialized] private bool m_IsInitialized;

        // PROPERTIES
        public Perception Perception => m_Perception;
        public bool IsServerOwned => IsServer || (IsSpawned && !IsOwner);

        // LIFECYCLE
        private void Awake()
        {
            m_TrackedTargets = new NetworkList<TrackedTargetData>();
            m_Evidences = new NetworkList<EvidenceData>();
            m_Perception = GetComponent<Perception>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            NetworkPerceptionRegistry.Register(this);
            HookPerceptionEvents();
            m_IsInitialized = true;

            if (IsServer)
            {
                // Server: Push current state to NetworkLists
                SyncFromLocalPerception();
            }
            else
            {
                // Client: Apply networked state to local Perception
                ApplyNetworkState();
            }
        }

        public override void OnNetworkDespawn()
        {
            UnhookPerceptionEvents();
            NetworkPerceptionRegistry.Unregister(this);
            base.OnNetworkDespawn();
        }

        // SERVER RPCS
        [ServerRpc(RequireOwnership = false)]
        public void SetAwarenessServerRpc(ulong targetNetworkId, float awareness, ServerRpcParams rpcParams = default)
        {
            UpdateTrackedTarget(targetNetworkId, awareness);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddAwarenessServerRpc(ulong targetNetworkId, float delta, float maxAwareness = 1f, ServerRpcParams rpcParams = default)
        {
            var current = GetTrackedAwareness(targetNetworkId);
            var newAwareness = Mathf.Clamp(current + delta, 0f, maxAwareness);
            UpdateTrackedTarget(targetNetworkId, newAwareness);
        }

        [ServerRpc(RequireOwnership = false)]
        public void TrackTargetServerRpc(ulong targetNetworkId, ServerRpcParams rpcParams = default)
        {
            if (!HasTrackedTarget(targetNetworkId))
            {
                m_TrackedTargets.Add(new TrackedTargetData
                {
                    TargetNetworkId = targetNetworkId,
                    Awareness = 0f,
                    Stage = (byte)AwareStage.None,
                    LastIncreaseTime = Time.time
                });
            }
        }

        // CLIENT RPCS
        [ClientRpc]
        private void NotifyAwarenessChangeClientRpc(ulong targetNetworkId, float level, byte stage)
        {
            // Find local target GameObject
            var target = NetworkPerceptionRegistry.GetGameObjectByNetworkId(targetNetworkId);
            if (target == null) return;

            // Broadcast to visual scripting
            NetworkPerceptionEvents.NotifyAwarenessChanged(m_Perception, target, level);
            NetworkPerceptionEvents.NotifyAwarenessStageChanged(m_Perception, target, (AwareStage)stage);
        }

        // PRIVATE METHODS
        private void HookPerceptionEvents()
        {
            m_Perception.EventChangeAwarenessLevel += OnLocalAwarenessLevelChange;
            m_Perception.EventChangeAwarenessStage += OnLocalAwarenessStageChange;
            m_Perception.EventTrack += OnLocalTrack;
            m_Perception.EventUntrack += OnLocalUntrack;
        }

        private void OnLocalAwarenessLevelChange(GameObject target, float level)
        {
            if (!IsServer) return; // Only server processes local changes

            var networkObject = target.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned) return;

            UpdateTrackedTarget(networkObject.NetworkObjectId, level);
            NotifyAwarenessChangeClientRpc(networkObject.NetworkObjectId, level,
                (byte)Tracker.GetStage(level));
        }
    }
}
```

### NetworkPerceptionEvents.cs
```csharp
namespace GameCreator.Netcode.Runtime
{
    [AddComponentMenu("Game Creator/Network/Network Perception Events")]
    public class NetworkPerceptionEvents : MonoBehaviour
    {
        // SINGLETON
        private static NetworkPerceptionEvents s_Instance;
        public static NetworkPerceptionEvents Instance => s_Instance;

        // EVENTS: Awareness
        public static event Action<Perception, GameObject, float> EventAwarenessChanged;
        public static event Action<Perception, GameObject, AwareStage> EventAwarenessStageChanged;
        public static event Action<Perception, GameObject> EventTargetTracked;
        public static event Action<Perception, GameObject> EventTargetUntracked;

        // EVENTS: Sensory
        public static event Action<Perception, StimulusNoise> EventNoiseHeard;
        public static event Action<Perception, Evidence> EventEvidenceNoticed;

        // BROADCAST METHODS
        public static void NotifyAwarenessChanged(Perception perception, GameObject target, float level)
        {
            Debug.Log($"[NetworkPerceptionEvents] Awareness changed: {perception.name} -> {target.name} = {level:F2}");
            EventAwarenessChanged?.Invoke(perception, target, level);
        }

        public static void NotifyAwarenessStageChanged(Perception perception, GameObject target, AwareStage stage)
        {
            Debug.Log($"[NetworkPerceptionEvents] Stage changed: {perception.name} -> {target.name} = {stage}");
            EventAwarenessStageChanged?.Invoke(perception, target, stage);
        }

        public static void NotifyTargetTracked(Perception perception, GameObject target)
        {
            Debug.Log($"[NetworkPerceptionEvents] Target tracked: {perception.name} -> {target.name}");
            EventTargetTracked?.Invoke(perception, target);
        }

        public static void NotifyTargetUntracked(Perception perception, GameObject target)
        {
            Debug.Log($"[NetworkPerceptionEvents] Target untracked: {perception.name} -> {target.name}");
            EventTargetUntracked?.Invoke(perception, target);
        }

        public static void NotifyNoiseHeard(Perception perception, StimulusNoise noise)
        {
            Debug.Log($"[NetworkPerceptionEvents] Noise heard: {perception.name} tag={noise.Tag}");
            EventNoiseHeard?.Invoke(perception, noise);
        }

        // LIFECYCLE
        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
        }
    }
}
```

### NetworkPerceptionRegistry.cs
```csharp
namespace GameCreator.Netcode.Runtime
{
    public static class NetworkPerceptionRegistry
    {
        // STORAGE
        private static readonly Dictionary<ulong, NetworkPerception> s_ByNetworkId = new();
        private static readonly Dictionary<int, NetworkPerception> s_ByInstanceId = new();
        private static readonly List<NetworkPerception> s_AllPerceptions = new();

        // PROPERTIES
        public static IReadOnlyList<NetworkPerception> AllPerceptions => s_AllPerceptions;

        // REGISTRATION
        public static void Register(NetworkPerception perception)
        {
            if (perception == null || !perception.IsSpawned) return;

            var networkId = perception.NetworkObjectId;
            var instanceId = perception.gameObject.GetInstanceID();

            s_ByNetworkId[networkId] = perception;
            s_ByInstanceId[instanceId] = perception;

            if (!s_AllPerceptions.Contains(perception))
                s_AllPerceptions.Add(perception);

            Debug.Log($"[NetworkPerceptionRegistry] Registered: {perception.name} (NetworkId: {networkId})");
        }

        public static void Unregister(NetworkPerception perception)
        {
            if (perception == null) return;

            s_ByNetworkId.Remove(perception.NetworkObjectId);
            s_ByInstanceId.Remove(perception.gameObject.GetInstanceID());
            s_AllPerceptions.Remove(perception);

            Debug.Log($"[NetworkPerceptionRegistry] Unregistered: {perception.name}");
        }

        // LOOKUPS
        public static NetworkPerception GetByNetworkId(ulong networkId)
        {
            return s_ByNetworkId.GetValueOrDefault(networkId);
        }

        public static NetworkPerception GetByGameObject(GameObject go)
        {
            if (go == null) return null;
            return s_ByInstanceId.GetValueOrDefault(go.GetInstanceID());
        }

        public static NetworkPerception GetForCharacter(NetworkCharacter character)
        {
            if (character == null) return null;
            return character.GetComponent<NetworkPerception>();
        }

        public static GameObject GetGameObjectByNetworkId(ulong networkId)
        {
            if (NetworkManager.Singleton == null) return null;
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
            {
                return networkObject.gameObject;
            }
            return null;
        }
    }
}
```

## Visual Scripting Integration

### EventNetworkOnAwarenessStage.cs
```csharp
namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Network Awareness Stage")]
    [Description("Triggered when a networked Perception's awareness stage changes for any target")]
    [Category("Network/Perception/On Network Awareness Stage")]
    [Image(typeof(IconAwareness), ColorTheme.Type.Blue)]
    [Keywords("Network", "Perception", "Awareness", "Stage", "Alert", "Suspicious")]

    [Serializable]
    public class EventNetworkOnAwarenessStage : Event
    {
        [SerializeField] private AwareMask m_Stage = AwareMask.Alert;
        [SerializeField] private CompareGameObjectOrAny m_Perception = new();
        [SerializeField] private CompareGameObjectOrAny m_Target = new();

        [NonSerialized] private Args m_Args;

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            m_Args = new Args(trigger.gameObject);
            NetworkPerceptionEvents.EventAwarenessStageChanged += OnAwarenessStageChanged;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPerceptionEvents.EventAwarenessStageChanged -= OnAwarenessStageChanged;
        }

        private void OnAwarenessStageChanged(Perception perception, GameObject target, AwareStage stage)
        {
            if (!m_Stage.HasFlag((AwareMask)stage)) return;
            if (!m_Perception.Match(perception.gameObject, m_Args)) return;
            if (!m_Target.Match(target, m_Args)) return;

            // Set both perception and target as accessible from instructions
            var args = new Args(perception.gameObject, target);
            _ = m_Trigger.Execute(args);
        }
    }
}
```

### InstructionNetworkSetAwareness.cs
```csharp
namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Set Network Awareness")]
    [Description("Sets the awareness level for a target on a networked Perception (server-only)")]
    [Category("Network/Perception/Set Network Awareness")]
    [Image(typeof(IconAwareness), ColorTheme.Type.Green)]
    [Keywords("Network", "Perception", "Awareness", "Set")]

    [Serializable]
    public class InstructionNetworkSetAwareness : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();
        [SerializeField] private PropertyGetDecimal m_Awareness = GetDecimalDecimal.Create(1f);

        public override string Title => $"Set {m_Target} awareness to {m_Awareness}";

        protected override Task Run(Args args)
        {
            var perception = m_Perception.Get<NetworkPerception>(args);
            if (perception == null)
            {
                Debug.LogWarning("[InstructionNetworkSetAwareness] NetworkPerception not found");
                return DefaultResult;
            }

            var target = m_Target.Get(args);
            if (target == null) return DefaultResult;

            var targetNetworkObject = target.GetComponent<NetworkObject>();
            if (targetNetworkObject == null || !targetNetworkObject.IsSpawned)
            {
                Debug.LogWarning("[InstructionNetworkSetAwareness] Target has no spawned NetworkObject");
                return DefaultResult;
            }

            float awareness = (float)m_Awareness.Get(args);

            // Send to server for authoritative update
            perception.SetAwarenessServerRpc(targetNetworkObject.NetworkObjectId, awareness);

            return DefaultResult;
        }
    }
}
```

## Synchronization Flow

### Awareness Update Flow
```
┌─────────────────────────────────────────────────────────────────────┐
│ CLIENT (Local Perception Trigger)                                    │
│   ↓                                                                  │
│ Perception.AddAwareness() called locally                            │
│   ↓                                                                  │
│ NetworkPerception.OnLocalAwarenessLevelChange()                     │
│   ↓                                                                  │
│ [If IsServer] UpdateTrackedTarget() → NetworkList updated           │
│               NotifyAwarenessChangeClientRpc()                       │
│   ↓                                                                  │
│ [If Client] Ignored (server-authoritative)                          │
└─────────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────────┐
│ ALL CLIENTS (ClientRpc broadcast)                                    │
│   ↓                                                                  │
│ NotifyAwarenessChangeClientRpc(targetNetworkId, level, stage)       │
│   ↓                                                                  │
│ NetworkPerceptionEvents.NotifyAwarenessChanged()                    │
│   ↓                                                                  │
│ EventNetworkOnAwarenessStage.OnAwarenessStageChanged()              │
│   ↓                                                                  │
│ m_Trigger.Execute(args) → GameCreator Instructions                  │
└─────────────────────────────────────────────────────────────────────┘
```

### Noise Emission Flow
```
┌─────────────────────────────────────────────────────────────────────┐
│ ANY CLIENT (Noise Source)                                            │
│   ↓                                                                  │
│ InstructionNetworkEmitNoise executed                                │
│   ↓                                                                  │
│ NetworkNoiseManager.EmitNoiseServerRpc(position, intensity, tag)    │
└─────────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────────┐
│ SERVER                                                               │
│   ↓                                                                  │
│ Validate noise parameters                                           │
│   ↓                                                                  │
│ Find all NetworkPerceptions in range                                │
│   ↓                                                                  │
│ For each: perception.SensorHear.OnReceiveNoise(stimulus)            │
│   ↓                                                                  │
│ PropagateNoiseClientRpc(listeners[], noiseData)                     │
└─────────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────────┐
│ ALL CLIENTS                                                          │
│   ↓                                                                  │
│ Local Perception.SensorHear.OnReceiveNoise() (for local effects)    │
│   ↓                                                                  │
│ NetworkPerceptionEvents.NotifyNoiseHeard()                          │
│   ↓                                                                  │
│ EventNetworkOnNoiseHeard triggers                                   │
└─────────────────────────────────────────────────────────────────────┘
```

## Assembly Definition

```json
{
    "name": "GameCreator.Netcode.Runtime",
    "rootNamespace": "GameCreator.Netcode.Runtime",
    "references": [
        "Unity.Netcode.Runtime",
        "GameCreator.Runtime.Common",
        "GameCreator.Runtime.Characters",
        "GameCreator.Runtime.VisualScripting",
        "GameCreator.Runtime.Perception"  // Add this reference
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

## Testing Scenarios

### Scenario 1: NPC Awareness Sync
1. Host spawns NPC with NetworkPerception
2. Player approaches NPC (enters sight cone)
3. Server updates awareness via NetworkPerception
4. All clients receive ClientRpc with new awareness level
5. Visual scripting triggers fire on all clients

### Scenario 2: Multi-Player Noise Detection
1. Player 1 makes noise (footsteps, weapon fire)
2. InstructionNetworkEmitNoise sends to server
3. Server calculates which NPCs can hear
4. Server updates NPC perception states
5. ClientRpcs broadcast to all clients
6. All clients see consistent NPC reactions

### Scenario 3: Evidence State Sync
1. Player tampers with evidence object
2. Server updates NetworkEvidence.IsTampered
3. When NPC investigates, server syncs discovery
4. All clients see same evidence state
