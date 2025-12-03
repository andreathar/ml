# Network Variables Sync

**Priority:** P0 CRITICAL
**Last Updated:** 2025-12-03
**Class:** GameCreator.Netcode.Runtime.NetworkVariablesSync

## Overview

NetworkVariablesSync provides synchronization for GameCreator Variables across the network using Unity Netcode NetworkList.

## Component Setup

Attach `NetworkVariablesSync` to any NetworkObject to sync variables.

```csharp
[RequireComponent(typeof(NetworkObject))]
public class NetworkVariablesSync : NetworkBehaviour
```

## Sync Modes

```csharp
public enum NetworkSyncMode
{
    Everyone,    // All clients can read the value
    OwnerOnly,   // Only the owner can read and write
    ServerOnly   // Only the server can read and write
}
```

## Configuration

In the Inspector, configure synced variables:

```csharp
[Serializable]
public class SyncedVariableConfig
{
    public string variableName;           // Name to identify variable
    public NetworkSyncMode syncMode;       // Permission level
    public float syncRate = 10f;          // Updates per second (Hz)
}
```

## Runtime API

### Setting Values

```csharp
NetworkVariablesSync sync = GetComponent<NetworkVariablesSync>();

// String (base method)
sync.SetVariable("playerName", "Alice");

// Typed helpers
sync.SetFloat("health", 100f);
sync.SetInt("score", 50);
sync.SetBool("isReady", true);
```

### Getting Values

```csharp
// String (base method)
string name = sync.GetVariable("playerName");

// Typed helpers with defaults
float health = sync.GetFloat("health", 100f);   // default 100
int score = sync.GetInt("score", 0);            // default 0
bool ready = sync.GetBool("isReady", false);    // default false
```

### Listening for Changes

```csharp
sync.EventVariableChanged += (variableName, newValue) =>
{
    Debug.Log($"{variableName} changed to {newValue}");
    
    if (variableName == "health")
    {
        UpdateHealthUI(float.Parse(newValue));
    }
};
```

## Permission Validation

The system automatically validates write permissions:

```csharp
// Server can always write to Everyone and ServerOnly
// Owner can write to Everyone and OwnerOnly
// Clients can only write to Everyone (via RPC to server)

// Example: ServerOnly variable
// Only server can modify - clients get warning if they try
sync.SetFloat("serverScore", 100f); // Only works on server
```

## NetworkString64

For NetworkList compatibility, strings are stored as fixed-size structs:

```csharp
public struct NetworkString64 : INetworkSerializable
{
    private FixedString64Bytes m_Value; // Max 64 bytes
    
    public NetworkString64(string value);
    public override string ToString();
}
```

**Limitation:** Maximum 64 bytes per value. For longer strings, use multiple variables or a different approach.

## Usage Patterns

### Player Stats

```csharp
// On spawn, initialize stats
public override void OnNetworkSpawn()
{
    base.OnNetworkSpawn();
    
    if (IsServer)
    {
        sync.SetFloat("health", 100f);
        sync.SetFloat("mana", 50f);
        sync.SetInt("level", 1);
    }
}

// Take damage (server authority)
[Rpc(SendTo.Server)]
private void TakeDamageRpc(float damage)
{
    float health = sync.GetFloat("health");
    sync.SetFloat("health", Mathf.Max(0, health - damage));
}
```

### Player Ready State

```csharp
// Owner sets ready state
public void SetReady(bool ready)
{
    if (!IsOwner) return;
    sync.SetBool("isReady", ready);
}

// Server checks all players
public bool AreAllPlayersReady()
{
    foreach (var player in players)
    {
        if (!player.sync.GetBool("isReady")) return false;
    }
    return true;
}
```

## Rate Limiting

Sync rate is configured per variable to prevent network flooding:

```csharp
// In inspector: syncRate = 10 (10 Hz = 10 updates/second)
// Rapid calls are throttled:

void Update()
{
    // Only actually syncs 10 times per second
    sync.SetFloat("position_x", transform.position.x);
}
```

## Related Files

- `Assets/Plugins/GameCreator/Packages/Netcode_for_GameObjects_Integration/Runtime/Sync/NetworkVariablesSync.cs`
- `.serena/personas/netcode-core.llm.txt`
