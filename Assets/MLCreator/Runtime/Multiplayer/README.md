# MLCreator Multiplayer

Network synchronization and multiplayer systems using Unity Netcode for GameObjects.

## Purpose

This module handles all multiplayer functionality:
- State synchronization between clients
- Server/Client RPC communication
- NetworkBehaviour components
- GameCreator character network adapters

## Subdirectories

- `Sync/` - State synchronization adapters
- `RPC/` - Remote procedure call handlers
- `NetworkBehaviours/` - Custom NetworkBehaviour components

## Namespace

`MLCreator.Runtime.Multiplayer`

## Key Patterns

```csharp
// NetworkBehaviour with GameCreator integration
public class NetworkCharacterAdapter : NetworkBehaviour
{
    private NetworkVariable<Vector3> m_Position = new NetworkVariable<Vector3>();

    [ServerRpc]
    private void MoveServerRpc(Vector3 direction) { }

    [ClientRpc]
    private void NotifyMoveClientRpc() { }
}
```

## Usage

Use `/mlcreator-multiplayer` Claude skill for guidance on creating networked systems.
