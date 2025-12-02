# Network Object Spawn Workflow: Never Place NetworkObjects in Scene

**Priority:** P0 CRITICAL
**Last Updated:** 2025-12-01
**Applies To:** All NetworkObject prefabs, NPCs, pickups, doors, interactive objects

## CRITICAL: Never Add NetworkObject to Scene GameObjects

### The Problem

When you add `NetworkObject` component to a GameObject that exists in the scene at design time, Netcode will **automatically spawn** it when `NetworkManager.StartHost()` or `StartClient()` is called. This causes several critical issues:

1. **Timing Issues**: Scene objects with NetworkObject spawn before players, causing null reference exceptions when trying to target "local player"
2. **Disabled NetworkBehaviours Warning**: If any NetworkBehaviour is on an inactive GameObject, Netcode logs 24+ warnings
3. **Prefab GUID Corruption**: Modifying prefab instances in scene can break prefab references
4. **Authority Confusion**: Scene-placed objects don't have clear ownership until host assigns it

### The Symptoms

```
[Netcode] Disabled NetworkBehaviours on the NetworkObject ... will not be synchronized
[NetworkStatPickup] Coroutine couldn't be started because the game object is inactive!
[NullReferenceException] Target player is null
```

## The Solution: Runtime Spawn Pattern

### Architecture

```
Scene Design Time:
├── NetworkManager ✅ (singleton, handles network startup)
├── NetworkPlayerManager ✅ (spawns players)
├── NetworkNPCManagerSimple ✅ (spawns NPCs/pickups AFTER player)
├── Scene Geometry ✅ (floors, walls - no NetworkObject)
└── ❌ NO scene objects with NetworkObject component

Runtime (Host starts):
1. NetworkManager.StartHost()
2. NetworkPlayerManager spawns player prefab
3. NetworkNPCManagerSimple waits for player
4. NetworkNPCManagerSimple spawns NPCs/pickups from prefabs
```

### NetworkNPCManagerSimple Pattern

```csharp
// Wait for player before spawning anything
private IEnumerator WaitForPlayerThenSpawn()
{
    // Wait until at least one player is spawned
    while (NetworkPlayerManager.Instance.RegisteredPlayers.Count == 0)
    {
        yield return new WaitForSeconds(0.1f);
    }

    // Additional delay to ensure player is fully initialized
    yield return new WaitForSeconds(spawnDelayAfterPlayer);

    yield return SpawnAllObjects();
}

// Spawn from prefab, not scene object
public GameObject SpawnPickup(PickupSpawnEntry entry)
{
    // Validate prefab has NetworkObject
    if (entry.prefab.GetComponent<NetworkObject>() == null)
    {
        Debug.LogError($"Prefab {entry.prefab.name} missing NetworkObject!");
        return null;
    }

    // Instantiate and spawn
    GameObject instance = Instantiate(entry.prefab, entry.spawnPosition, Quaternion.identity);
    NetworkObject netObj = instance.GetComponent<NetworkObject>();
    netObj.Spawn(); // Server spawns it, clients receive it

    return instance;
}
```

### Prefab Registration

**CRITICAL**: All network prefabs must be registered in `DefaultNetworkPrefabs.asset`:

```yaml
# Assets/DefaultNetworkPrefabs.asset
List:
  - Prefab: {fileID: ..., guid: ..., type: 3}  # NetworkXPPickup
  - Prefab: {fileID: ..., guid: ..., type: 3}  # NetworkDoor
  - Prefab: {fileID: ..., guid: ..., type: 3}  # Other network prefabs...
```

Without registration, clients cannot instantiate prefabs spawned by the server.

## Workflow Checklist

### Creating a New Network Pickup/NPC/Object

1. **Create prefab in Assets folder** (not in scene)
   - Add `NetworkObject` component
   - Add your custom `NetworkBehaviour` (e.g., `NetworkStatPickup`)
   - Configure default values

2. **Register prefab with NetworkManager**
   - Open `Assets/DefaultNetworkPrefabs.asset`
   - Add your prefab to the List

3. **Configure spawn in NetworkNPCManagerSimple**
   - Add entry to `pickupSpawnList` or `npcSpawnList`
   - Set spawn position (world coordinates)
   - Set spawn conditions (spawnOnStart, respawns, etc.)

4. **DO NOT place prefab instance in scene**
   - The manager spawns it at runtime
   - Scene stays clean

### Converting Existing Scene Object to Network

**❌ WRONG - Don't do this:**
```
1. Select scene object
2. Add NetworkObject component
3. Add NetworkBehaviour
// This causes the auto-spawn timing issues!
```

**✅ RIGHT - Do this instead:**
```
1. Create new prefab in Assets/
2. Add NetworkObject + NetworkBehaviour to prefab
3. Register prefab with NetworkManager
4. Add spawn entry to NetworkNPCManagerSimple
5. Remove or keep original scene object (without NetworkObject)
```

## File Locations

| Asset | Path |
|-------|------|
| Network Prefabs | `Assets/Plugins/GameCreator/Packages/MLCreator_Multiplayer/Runtime/Prefabs/NetworkObjects/` |
| Prefab Registry | `Assets/DefaultNetworkPrefabs.asset` |
| Spawn Manager | Scene: `NetworkNPCManagerSimple` GameObject |
| Manager Script | `Runtime/Characters/NPC/NetworkNPCManagerSimple.cs` |

## Example Prefabs

### NetworkXPPickup
- **Purpose**: XP orb that adds to strength stat
- **Components**: NetworkObject, NetworkStatPickup, SphereCollider (trigger)
- **Config**: statId="strength", statAmount=10, respawns=true

### NetworkDoor
- **Purpose**: Door that opens when player has enough strength
- **Components**: NetworkObject, NetworkDoor, BoxCollider (trigger)
- **Config**: requiredStatId="strength", requiredStatValue=24

## Troubleshooting

### "Disabled NetworkBehaviours" Warning
**Cause**: Scene object with NetworkObject was inactive when host started
**Fix**: Remove NetworkObject from scene object, use spawn pattern instead

### "Coroutine couldn't be started" Error
**Cause**: NetworkBehaviour trying to start coroutine on inactive object
**Fix**: Check `gameObject.activeInHierarchy` before `StartCoroutine()`

### "Target player is null"
**Cause**: Network object spawned before player was ready
**Fix**: Use NetworkNPCManagerSimple which waits for player

### Prefab Not Spawning on Clients
**Cause**: Prefab not registered in DefaultNetworkPrefabs.asset
**Fix**: Add prefab to the NetworkPrefabsList

## Related Documentation

- `002_network_architecture_never_forget.md` - NetworkTransform removal, server authority
- `003_multiplayer_rpc_patterns.md` - ServerRpc/ClientRpc patterns
- `NetworkNPCManagerSimple.cs` - Spawn manager implementation
- `NetworkStatPickup.cs` - Example pickup implementation
- `NetworkDoor.cs` - Example interactive object

## Quick Reference

| Question | Answer |
|----------|--------|
| Put NetworkObject in scene? | **NO - causes timing issues** |
| How to spawn network objects? | Use **NetworkNPCManagerSimple** |
| When are objects spawned? | **After player is spawned and ready** |
| Where are prefabs stored? | `Runtime/Prefabs/NetworkObjects/` |
| How to register prefabs? | Add to `DefaultNetworkPrefabs.asset` |
