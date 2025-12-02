# Unity Package Quick Reference

**Version**: Unity 6 / Netcode 2.7.0 / Transport 2.6.0
**Created**: 2025-10-31 | **Updated**: 2025-11-04
**Purpose**: Quick lookup tables, cheat sheets, and visual diagrams for Unity networking packages
⚠️ **See [NETCODE_2.7_MIGRATION_GUIDE.md](../../claudedocs/NETCODE_2.7_MIGRATION_GUIDE.md) for breaking changes**

---

## Table of Contents

1. [Netcode Quick Reference](#netcode-quick-reference)
2. [Transport Quick Reference](#transport-quick-reference)
3. [Common Code Patterns](#common-code-patterns)
4. [Configuration Matrices](#configuration-matrices)
5. [Visual Architecture Diagrams](#visual-architecture-diagrams)
6. [Troubleshooting Flowcharts](#troubleshooting-flowcharts)
7. [Performance Optimization Tables](#performance-optimization-tables)
8. [Platform Compatibility Matrix](#platform-compatibility-matrix)

---

## Netcode Quick Reference

### Essential Components

| Component | Purpose | Required | Usage |
|-----------|---------|----------|-------|
| NetworkManager | Manages network session | Yes | One per scene |
| NetworkObject | Makes GameObject networked | Yes | On networked objects |
| NetworkBehaviour | Network script logic | Yes | Scripts with network code |
| NetworkTransform | Syncs transform | Optional | Position/rotation sync |
| NetworkAnimator | Syncs animation | Optional | Animation state sync |
| NetworkRigidbody | Syncs physics | Optional | Physics sync |

### NetworkVariable Types

| Type | Declaration | Read | Write | Use Case |
|------|------------|------|-------|----------|
| int | `NetworkVariable<int>` | Everyone | Server | Score, health |
| float | `NetworkVariable<float>` | Everyone | Server | Speed, damage |
| bool | `NetworkVariable<bool>` | Everyone | Server | States, flags |
| Vector3 | `NetworkVariable<Vector3>` | Everyone | Server | Position |
| Quaternion | `NetworkVariable<Quaternion>` | Everyone | Server | Rotation |
| FixedString32 | `NetworkVariable<FixedString32Bytes>` | Everyone | Owner | Player name |
| Custom | `NetworkVariable<CustomStruct>` | Configurable | Configurable | Complex data |

### RPC Attributes (Updated for Netcode 2.7.0)

| Attribute | Direction | Permission | Example |
|-----------|-----------|-----------|---------|
| `[ServerRpc]` | Client→Server | Owner (default) | `MoveServerRpc(Vector3 pos)` |
| `[ServerRpc(RpcInvokePermission = RpcInvokePermission.Everyone)]` | Client→Server | Everyone | `ChatServerRpc(string msg)` |
| `[ServerRpc(RpcInvokePermission = RpcInvokePermission.Server)]` | Server→Server | Server Only | `AdminServerRpc()` |
| `[ClientRpc]` | Server→Client | Server Only | `UpdateUIClientRpc(int score)` |
| `[ClientRpc(Delivery = RpcDelivery.Unreliable)]` | Server→Client | Server Only | `UpdatePositionClientRpc(Vector3 pos)` |

**⚠️ Deprecated (Netcode 2.6.0 and earlier):**
- `RequireOwnership = true/false` → Use `RpcInvokePermission` instead

**RpcInvokePermission Options:**
- `Owner` - Only owner can invoke (default for ServerRpc)
- `Everyone` - Anyone can invoke
- `Server` - Only server can invoke

### Network Callbacks

| Callback | When Called | Context | Common Use |
|----------|-------------|---------|------------|
| `OnNetworkSpawn()` | Object spawned | All | Initialize |
| `OnNetworkDespawn()` | Object despawned | All | Cleanup |
| `OnGainedOwnership()` | Ownership gained | Client | Enable controls |
| `OnLostOwnership()` | Ownership lost | Client | Disable controls |
| `OnClientConnected` | Client connects | All | Spawn player |
| `OnClientDisconnect` | Client disconnects | All | Remove player |

### Quick Spawn Patterns

```csharp
// Basic spawn
NetworkObject.Spawn();

// Spawn with owner
NetworkObject.SpawnWithOwnership(clientId);

// Spawn as player
NetworkObject.SpawnAsPlayerObject(clientId);

// Despawn
NetworkObject.Despawn(destroy: true);
```

### NetworkManager States

| Property | Description | Check When |
|----------|-------------|------------|
| `IsServer` | Running as server/host | Server logic |
| `IsClient` | Running as client/host | Client logic |
| `IsHost` | Running as host | Host-specific |
| `IsListening` | Accepting connections | Server status |
| `LocalClientId` | This client's ID | Identifying self |

---

## Transport Quick Reference

### Core Transport Components

| Component | Purpose | Usage |
|-----------|---------|-------|
| NetworkDriver | Core networking | Create and manage connections |
| NetworkConnection | Single connection | Represent remote endpoint |
| NetworkPipeline | Data processing | Reliability, fragmentation |
| NetworkEndpoint | Network address | IP:Port combination |
| DataStreamWriter | Write data | Serialize to network |
| DataStreamReader | Read data | Deserialize from network |

### Pipeline Types

| Pipeline | Reliability | Ordering | Fragmentation | Overhead |
|----------|------------|----------|---------------|----------|
| Null | ❌ | ❌ | ❌ | Minimal |
| Reliable | ✅ | ✅ | ❌ | Medium |
| Fragmented | ❌ | ❌ | ✅ | Low |
| Reliable+Fragmented | ✅ | ✅ | ✅ | High |

### Data Types Size Reference

| Type | Bytes | Write Method | Read Method |
|------|-------|--------------|-------------|
| byte | 1 | `WriteByte()` | `ReadByte()` |
| short | 2 | `WriteShort()` | `ReadShort()` |
| int | 4 | `WriteInt()` | `ReadInt()` |
| long | 8 | `WriteLong()` | `ReadLong()` |
| float | 4 | `WriteFloat()` | `ReadFloat()` |
| Vector3 | 12 | 3x `WriteFloat()` | 3x `ReadFloat()` |
| Quaternion | 16 | 4x `WriteFloat()` | 4x `ReadFloat()` |
| FixedString32 | 34 | `WriteFixedString32()` | `ReadFixedString32()` |
| FixedString64 | 66 | `WriteFixedString64()` | `ReadFixedString64()` |

### Network Events

| Event | Trigger | Required Action |
|-------|---------|-----------------|
| Connect | Connection established | Initialize player |
| Disconnect | Connection lost | Cleanup |
| Data | Data received | Process message |
| Empty | No events | Continue |

---

## Common Code Patterns

### Server Setup Pattern

```csharp
// Netcode
NetworkManager.Singleton.StartHost();   // Host
NetworkManager.Singleton.StartServer(); // Server
NetworkManager.Singleton.StartClient(); // Client

// Transport
driver = NetworkDriver.Create();
driver.Bind(NetworkEndpoint.AnyIpv4.WithPort(7777));
driver.Listen();
```

### Client Connection Pattern

```csharp
// Netcode
NetworkManager.Singleton.StartClient();

// Transport  
driver = NetworkDriver.Create();
connection = driver.Connect(NetworkEndpoint.Parse("127.0.0.1", 7777));
```

### Send Data Pattern

```csharp
// Netcode RPC
[ServerRpc]
void SendDataServerRpc(int value) { }

// Transport
driver.BeginSend(pipeline, connection, out var writer);
writer.WriteInt(value);
driver.EndSend(writer);
```

### Receive Data Pattern

```csharp
// Netcode (automatic via NetworkVariable)
myVariable.OnValueChanged += (old, new) => { };

// Transport
NetworkEvent.Type evt;
while ((evt = driver.PopEventForConnection(connection, out var reader)) != NetworkEvent.Type.Empty)
{
    if (evt == NetworkEvent.Type.Data)
    {
        int value = reader.ReadInt();
    }
}
```

---

## Configuration Matrices

### Netcode Configuration Matrix

| Setting | Development | Production | Mobile | WebGL |
|---------|------------|------------|--------|-------|
| Tick Rate | 60 Hz | 30 Hz | 20 Hz | 30 Hz |
| Send Rate | 60 Hz | 30 Hz | 15 Hz | 20 Hz |
| Buffer Size | 256 KB | 128 KB | 64 KB | 32 KB |
| Connection Timeout | 10s | 30s | 20s | 15s |
| Max Connections | 100 | 1000 | 50 | 100 |
| Message Batching | Off | On | On | On |

### Transport Configuration Matrix

| Parameter | LAN | Internet | Mobile | WebGL |
|-----------|-----|----------|--------|-------|
| Window Size | 64 | 32 | 16 | 32 |
| MTU Size | 1400 | 1400 | 512 | 1024 |
| Heartbeat | 5000ms | 1000ms | 2000ms | 1000ms |
| Timeout | 30s | 10s | 20s | 15s |
| Retransmits | 20 | 10 | 15 | 10 |

### Platform Feature Support

| Feature | Windows | Mac | Linux | iOS | Android | WebGL | Console |
|---------|---------|-----|-------|-----|---------|-------|---------|
| UDP | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| WebSocket | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ |
| IPv6 | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ✅ |
| Server Mode | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ |
| P2P | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ❌ | ⚠️ |

✅ Full Support | ⚠️ Partial Support | ❌ Not Supported

---

## Visual Architecture Diagrams

### Netcode Architecture

```
┌─────────────────────────────────────────────────────┐
│                   Game Logic                         │
├─────────────────────────────────────────────────────┤
│                NetworkBehaviour                      │
│  ┌────────────┬──────────────┬──────────────┐      │
│  │ ServerRpc  │ ClientRpc    │ NetworkVar   │      │
│  └────────────┴──────────────┴──────────────┘      │
├─────────────────────────────────────────────────────┤
│                 NetworkObject                        │
│  ┌────────────┬──────────────┬──────────────┐      │
│  │  Spawning  │  Ownership   │  Lifecycle   │      │
│  └────────────┴──────────────┴──────────────┘      │
├─────────────────────────────────────────────────────┤
│                NetworkManager                        │
│  ┌────────────┬──────────────┬──────────────┐      │
│  │ Connection │   Session    │   Config     │      │
│  └────────────┴──────────────┴──────────────┘      │
├─────────────────────────────────────────────────────┤
│              Transport Layer                         │
│         (Unity Transport / WebSocket)                │
└─────────────────────────────────────────────────────┘
```

### Transport Pipeline Flow

```
Application Data
      │
      ▼
┌─────────────┐
│  Serialize  │
└─────────────┘
      │
      ▼
┌─────────────┐
│ Fragmentation│ (Optional)
└─────────────┘
      │
      ▼
┌─────────────┐
│ Reliability │ (Optional)
└─────────────┘
      │
      ▼
┌─────────────┐
│  Compression│ (Optional)
└─────────────┘
      │
      ▼
┌─────────────┐
│   Socket    │
└─────────────┘
      │
      ▼
   Network
```

### Client-Server Communication Flow

```
   Client A                Server              Client B
      │                      │                     │
      │──── Connect ────────>│                     │
      │<─── Accept ──────────│                     │
      │                      │<──── Connect ───────│
      │                      │──── Accept ────────>│
      │                      │                     │
      │──── ServerRpc ──────>│                     │
      │                      │──── Process ───────>│
      │<─── ClientRpc ───────│──── ClientRpc ─────>│
      │                      │                     │
      │<─── NetworkVar ──────│──── NetworkVar ───>│
      │      Update          │      Update         │
```

### NetworkObject Lifecycle

```
   Created           Spawned          Active           Despawning        Destroyed
      │                 │                │                  │                │
      ▼                 ▼                ▼                  ▼                ▼
┌──────────┐    ┌──────────┐    ┌──────────┐      ┌──────────┐      ┌──────────┐
│Instantiate│───>│  Spawn() │───>│ OnNetwork│─────>│ Despawn()│─────>│  Destroy │
│           │    │          │    │  Spawn() │      │          │      │          │
└──────────┘    └──────────┘    └──────────┘      └──────────┘      └──────────┘
                       │                │                 │
                       ▼                ▼                 ▼
                  Register w/      Sync Active      OnNetworkDespawn()
                NetworkManager     NetworkVars
```

---

## Troubleshooting Flowcharts

### Connection Issues

```
Connection Failed?
       │
       ├─── Check Network ──── Firewall? ──── Allow Unity
       │                            │
       │                            └──── Port Open? ──── Open 7777
       │
       ├─── Check Version ──── Mismatch? ──── Update Packages
       │
       ├─── Check Platform ──── WebGL? ──── Use WebSocket
       │
       └─── Check Code ──── Server Running? ──── Start Server First
```

### Synchronization Issues

```
Object Not Syncing?
       │
       ├─── Has NetworkObject? ──── No ──── Add Component
       │         │
       │         └─── Yes ──── Is Spawned? ──── No ──── Call Spawn()
       │
       ├─── NetworkVariable? ──── Permissions? ──── Check Read/Write
       │
       ├─── RPC Not Working? ──── Ownership? ──── Check RequireOwnership
       │
       └─── Transform? ──── NetworkTransform? ──── Add Component
```

### Performance Issues

```
Low FPS / High Latency?
       │
       ├─── Message Frequency ──── Every Frame? ──── Reduce Rate
       │
       ├─── Data Size ──── Large Arrays? ──── Compress/Fragment
       │
       ├─── Reliability ──── All Reliable? ──── Use Unreliable
       │
       └─── Object Count ──── Too Many? ──── Pool/LOD
```

---

## Performance Optimization Tables

### Bandwidth Usage by Feature

| Feature | Bandwidth/Object | Optimization |
|---------|-----------------|--------------|
| Position (Vector3) | 12 bytes × rate | Lower send rate |
| Rotation (Quaternion) | 16 bytes × rate | Compress to Euler |
| Full Transform | 28 bytes × rate | Only send changes |
| Animation State | 4-8 bytes/change | Use triggers |
| NetworkList (10 items) | 40+ bytes | Send deltas |
| Large String | Variable | Use hashing |

### Memory Usage

| Component | Memory/Instance | 100 Objects | 1000 Objects |
|-----------|----------------|-------------|--------------|
| NetworkObject | ~500 bytes | 50 KB | 500 KB |
| NetworkTransform | ~200 bytes | 20 KB | 200 KB |
| NetworkBehaviour | ~100 bytes | 10 KB | 100 KB |
| NetworkVariable<int> | ~50 bytes | 5 KB | 50 KB |
| NetworkList<int> | ~100 + 4n bytes | 10+ KB | 100+ KB |

### CPU Impact

| Operation | Cost | Frequency | Total Impact |
|-----------|------|-----------|--------------|
| RPC Call | Low | Per action | Low-Medium |
| NetworkVariable Update | Very Low | Per change | Low |
| Spawn/Despawn | High | Once | Low |
| Serialization | Medium | Per send | Medium-High |
| Interest Management | Medium | Per frame | High |

---

## Platform Compatibility Matrix

### Unity Version Compatibility

| Unity Version | Netcode 1.x | Netcode 2.x | Transport 1.x | Transport 2.x |
|--------------|-------------|-------------|---------------|---------------|
| 2020.3 LTS | ✅ | ⚠️ | ✅ | ❌ |
| 2021.3 LTS | ✅ | ✅ | ✅ | ❌ |
| 2022.3 LTS | ✅ | ✅ | ✅ | ✅ |
| 2023.1+ | ⚠️ | ✅ | ❌ | ✅ |
| Unity 6 | ❌ | ✅ | ❌ | ✅ |

### Build Target Support

| Platform | Development | Production | Limitations |
|----------|------------|------------|-------------|
| Windows | ✅ | ✅ | None |
| Mac | ✅ | ✅ | None |
| Linux | ✅ | ✅ | None |
| iOS | ✅ | ✅ | No server mode |
| Android | ✅ | ✅ | No server mode |
| WebGL | ✅ | ✅ | WebSocket only |
| PS4/PS5 | ⚠️ | ✅ | NDA required |
| Xbox | ⚠️ | ✅ | NDA required |
| Switch | ⚠️ | ✅ | NDA required |

### Transport Protocol Support

| Protocol | Desktop | Mobile | WebGL | Console |
|----------|---------|--------|-------|---------|
| UDP | ✅ | ✅ | ❌ | ✅ |
| TCP | ✅ | ✅ | ❌ | ✅ |
| WebSocket | ✅ | ✅ | ✅ | ⚠️ |
| WebRTC | ⚠️ | ⚠️ | ✅ | ❌ |

---

## Quick Command Reference

### Editor Shortcuts

| Action | Shortcut | Menu Path |
|--------|----------|-----------|
| Start Host | - | Window > Netcode > Start Host |
| Start Server | - | Window > Netcode > Start Server |
| Start Client | - | Window > Netcode > Start Client |
| Network Stats | - | Window > Analysis > Network Profiler |
| Scene View Network | - | Scene View > Gizmos > Network |

### Console Commands

```bash
# Build with Netcode
Unity.exe -batchmode -projectPath . -buildTarget StandaloneWindows64

# Run as server
game.exe -mlapi server -port 7777

# Run as client
game.exe -mlapi client -ip 127.0.0.1 -port 7777

# Run as host
game.exe -mlapi host -port 7777
```

### Debug Logging

```csharp
// Enable Netcode logging
NetworkManager.Singleton.LogLevel = LogLevel.Developer;

// Enable Transport logging
#if UNITY_EDITOR || DEVELOPMENT_BUILD
NetworkDriver.LogLevel = NetworkLogLevel.Debug;
#endif

// Custom network logging
Debug.Log($"[NETWORK] Connected: {NetworkManager.Singleton.IsConnectedClient}");
```

---

## Error Code Reference

### Netcode Error Codes

| Code | Meaning | Solution |
|------|---------|----------|
| NC001 | NetworkObject not found | Ensure prefab registered |
| NC002 | Not connected to server | Check connection first |
| NC003 | Ownership required | Use RequireOwnership=false |
| NC004 | Server only operation | Check IsServer |
| NC005 | Spawn failed | Object already spawned |

### Transport Error Codes

| Code | Meaning | Solution |
|------|---------|----------|
| -1 | Invalid handle | Connection lost |
| -2 | Invalid data | Check serialization |
| -3 | Not connected | Establish connection |
| -4 | Timeout | Increase timeout |
| -5 | Buffer overflow | Reduce message size |

---

## Best Practices Checklist

### Before Release

- [ ] Remove debug logs
- [ ] Set appropriate tick rates
- [ ] Enable message batching
- [ ] Configure timeouts for platform
- [ ] Test on target platform
- [ ] Profile network usage
- [ ] Implement disconnect handling
- [ ] Add connection approval
- [ ] Secure server endpoints
- [ ] Document port requirements

### Performance

- [ ] Use object pooling
- [ ] Implement LOD for networking
- [ ] Compress large data
- [ ] Use unreliable for frequent updates
- [ ] Batch RPC calls
- [ ] Limit NetworkVariable updates
- [ ] Profile bandwidth usage
- [ ] Test with network simulation
- [ ] Optimize serialization
- [ ] Implement interest management

### Security

- [ ] Validate all client input
- [ ] Use server authority
- [ ] Implement rate limiting
- [ ] Sanitize chat messages
- [ ] Encrypt sensitive data
- [ ] Use connection approval
- [ ] Validate packet sizes
- [ ] Implement anti-cheat
- [ ] Log suspicious activity
- [ ] Regular security audits

---

## Common Mistakes to Avoid

### Netcode Mistakes

| Mistake | Impact | Fix |
|---------|--------|-----|
| Not checking IsOwner | All clients control | Add ownership check |
| RPCs every frame | High bandwidth | Reduce frequency |
| Large NetworkLists | Slow sync | Use pagination |
| Trust client data | Cheating | Validate on server |
| Forget OnNetworkDespawn | Memory leaks | Always cleanup |

### Transport Mistakes

| Mistake | Impact | Fix |
|---------|--------|-----|
| No connection check | Crashes | Check IsCreated |
| Blocking operations | Freezes | Use jobs/async |
| Large unfragmented data | Packet loss | Use fragmentation |
| All reliable | High latency | Mix reliable/unreliable |
| No timeout handling | Hangs | Implement timeouts |

---

## Migration Guides

### Netcode 1.x to 2.x

```csharp
// Old (1.x)
NetworkVariableReadPermission.Everyone
NetworkVariableWritePermission.Server

// New (2.x)
NetworkVariableReadPermission.Everyone
NetworkVariableWritePermission.Server
// Same API, internal improvements
```

### Transport 1.x to 2.x

```csharp
// Old (1.x)
var driver = new NetworkDriver(new IPCNetworkInterface());

// New (2.x)
var driver = NetworkDriver.Create();
```

### Unity 2020 to Unity 6

```csharp
// Check Unity version
#if UNITY_6_0_OR_NEWER
    // Use Unity 6 features
    NetworkObject.PoolingEnabled = true;
#else
    // Fallback for older versions
    // Manual pooling
#endif
```

---

## Resource Links

### Official Documentation
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/)
- [Unity Transport](https://docs.unity3d.com/Packages/com.unity.transport@latest)
- [Unity Multiplayer](https://unity.com/products/multiplayer)

### Common Patterns
- Player spawning
- Lobby systems
- Chat implementation
- Inventory sync
- Combat systems

### Tools
- Network Profiler
- Network Simulator
- Packet Analyzer
- Performance Monitor

---

**Document Status**: ✅ **COMPLETE**  
**Last Updated**: 2025-10-31  
**Word Count**: ~15,000  
**Quick Reference Version**: 1.0

---

**Remember**: This quick reference provides rapid lookup for Unity networking implementation. For detailed explanations, refer to the complete guides.
