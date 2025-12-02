# Testing Patterns for Unity Multiplayer

**Category:** TOOLS - Testing Workflows
**Last Updated:** 2025-11-23
**Framework:** Unity Test Framework + Netcode for GameObjects

## UnityTest Fixture Pattern

### Basic Multiplayer Test Structure

```csharp
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace GameCreator.Multiplayer.Tests
{
    public class NetworkCharacterTests
    {
        private NetworkManager networkManager;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create NetworkManager
            var go = new GameObject("NetworkManager");
            networkManager = go.AddComponent<NetworkManager>();
            networkManager.NetworkConfig = new NetworkConfig();

            // Start as host
            networkManager.StartHost();

            // Wait for network setup
            yield return new WaitForSeconds(0.1f);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Shutdown network
            networkManager.Shutdown();
            Object.Destroy(networkManager.gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Character_WhenSpawned_HasNetworkObject()
        {
            // Arrange
            var prefab = CreateCharacterPrefab();

            // Act
            var instance = Object.Instantiate(prefab);
            var networkObject = instance.GetComponent<NetworkObject>();
            networkObject.Spawn();

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(networkObject.IsSpawned);
            Assert.IsNotNull(instance.GetComponent<Character>());

            // Cleanup
            networkObject.Despawn();
        }
    }
}
```

## NetworkTestHelpers Utility

### Common Helper Methods

```csharp
public static class NetworkTestHelpers
{
    public static GameObject SpawnNetworkObject(GameObject prefab, ulong? ownerClientId = null)
    {
        var instance = Object.Instantiate(prefab);
        var networkObject = instance.GetComponent<NetworkObject>();

        if (ownerClientId.HasValue)
        {
            networkObject.SpawnAsPlayerObject(ownerClientId.Value);
        }
        else
        {
            networkObject.Spawn();
        }

        return instance;
    }

    public static void DespawnNetworkObject(GameObject obj)
    {
        var networkObject = obj.GetComponent<NetworkObject>();
        if (networkObject != null && networkObject.IsSpawned)
        {
            networkObject.Despawn();
        }
        Object.Destroy(obj);
    }

    public static IEnumerator WaitForNetworkUpdate(float seconds = 0.1f)
    {
        yield return new WaitForSeconds(seconds);
    }

    public static IEnumerator WaitForCondition(System.Func<bool> condition, float timeout = 1.0f)
    {
        float elapsed = 0f;
        while (!condition() && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (elapsed >= timeout)
        {
            throw new System.TimeoutException($"Condition not met within {timeout}s");
        }
    }
}
```

### Using Helpers in Tests

```csharp
[UnityTest]
public IEnumerator NetworkVariable_WhenChanged_InvokesCallback()
{
    // Arrange
    var prefab = CreateTestPrefab();
    var obj = NetworkTestHelpers.SpawnNetworkObject(prefab);
    var component = obj.GetComponent<TestNetworkBehaviour>();

    bool callbackInvoked = false;
    component.OnValueChanged += () => callbackInvoked = true;

    // Act
    component.SetValue(42); // ServerRpc to change NetworkVariable
    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert
    Assert.IsTrue(callbackInvoked);

    // Cleanup
    NetworkTestHelpers.DespawnNetworkObject(obj);
}
```

## RPC Testing Patterns

### Testing ServerRpc

```csharp
[UnityTest]
public IEnumerator ServerRpc_WhenCalled_ExecutesOnServer()
{
    // Arrange
    var obj = NetworkTestHelpers.SpawnNetworkObject(prefab, ownerClientId: 0);
    var component = obj.GetComponent<MyNetworkBehaviour>();

    bool serverExecuted = false;
    component.OnServerExecution += () => serverExecuted = true;

    // Act
    component.TestServerRpc(); // Called from client
    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert
    Assert.IsTrue(serverExecuted, "ServerRpc should execute on server");

    // Cleanup
    NetworkTestHelpers.DespawnNetworkObject(obj);
}
```

### Testing ClientRpc

```csharp
[UnityTest]
public IEnumerator ClientRpc_WhenCalled_ExecutesOnAllClients()
{
    // Arrange
    var obj = NetworkTestHelpers.SpawnNetworkObject(prefab);
    var component = obj.GetComponent<MyNetworkBehaviour>();

    bool clientExecuted = false;
    component.OnClientExecution += () => clientExecuted = true;

    // Act (called from server)
    if (NetworkManager.Singleton.IsServer)
    {
        component.TestClientRpc();
    }
    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert
    Assert.IsTrue(clientExecuted, "ClientRpc should execute on all clients");

    // Cleanup
    NetworkTestHelpers.DespawnNetworkObject(obj);
}
```

## NetworkVariable Testing

### Testing Value Changes

```csharp
[UnityTest]
public IEnumerator NetworkVariable_WhenSet_SyncsToClients()
{
    // Arrange
    var obj = NetworkTestHelpers.SpawnNetworkObject(prefab);
    var component = obj.GetComponent<MyNetworkBehaviour>();

    int initialValue = component.Health;

    // Act
    if (NetworkManager.Singleton.IsServer)
    {
        component.Health = 50; // Set via property that wraps NetworkVariable
    }
    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert
    Assert.AreEqual(50, component.Health);
    Assert.AreNotEqual(initialValue, component.Health);

    // Cleanup
    NetworkTestHelpers.DespawnNetworkObject(obj);
}
```

### Testing Callbacks

```csharp
[UnityTest]
public IEnumerator NetworkVariable_Callback_ReceivesPreviousAndCurrentValues()
{
    // Arrange
    var obj = NetworkTestHelpers.SpawnNetworkObject(prefab);
    var component = obj.GetComponent<MyNetworkBehaviour>();

    int? previousValue = null;
    int? currentValue = null;

    component.OnHealthChanged += (prev, curr) =>
    {
        previousValue = prev;
        currentValue = curr;
    };

    // Act
    if (NetworkManager.Singleton.IsServer)
    {
        component.Health = 75;
    }
    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert
    Assert.IsNotNull(previousValue);
    Assert.IsNotNull(currentValue);
    Assert.AreEqual(100, previousValue.Value); // Assuming default was 100
    Assert.AreEqual(75, currentValue.Value);

    // Cleanup
    NetworkTestHelpers.DespawnNetworkObject(obj);
}
```

## GameCreator Character Testing

### Character Ownership Tests

```csharp
[UnityTest]
public IEnumerator Character_WhenOwned_IsNetworkOwnerTrue()
{
    // Arrange
    var characterPrefab = CreateCharacterPrefab();
    var instance = Object.Instantiate(characterPrefab);
    var character = instance.GetComponent<Character>();

    character.IsNetworkSpawned = true; // CRITICAL: Set before spawn

    // Act
    var networkObject = instance.GetComponent<NetworkObject>();
    networkObject.SpawnAsPlayerObject(0); // Spawn as player for client 0

    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert
    Assert.IsTrue(character.IsNetworkOwner, "Character should recognize ownership");
    Assert.AreEqual(0, networkObject.OwnerClientId);

    // Cleanup
    NetworkTestHelpers.DespawnNetworkObject(instance);
}
```

### Character Movement Tests

```csharp
[UnityTest]
public IEnumerator Character_Movement_OnlyOwnerExecutes()
{
    // Arrange
    var character = SpawnCharacterAsPlayer(clientId: 0);
    Vector3 initialPosition = character.transform.position;
    Vector3 targetDirection = Vector3.forward;

    // Act - Owner tries to move
    if (character.IsNetworkOwner)
    {
        character.Motion.MoveToDirection(targetDirection);
    }
    yield return new WaitForSeconds(0.5f);

    // Assert
    Assert.AreNotEqual(initialPosition, character.transform.position,
        "Owner should be able to move character");

    // Cleanup
    NetworkTestHelpers.DespawnNetworkObject(character.gameObject);
}
```

## Test Naming Conventions

### Pattern: Component_Condition_ExpectedBehavior

```csharp
// ✅ Good test names
[UnityTest] public IEnumerator NetworkVariable_WhenChanged_InvokesCallback()
[UnityTest] public IEnumerator ServerRpc_WithInvalidData_RejectsRequest()
[UnityTest] public IEnumerator Character_WhenNonOwner_CannotMove()
[UnityTest] public IEnumerator Inventory_WhenFull_RejectsNewItems()

// ❌ Poor test names
[UnityTest] public IEnumerator TestNetworkVariable()
[UnityTest] public IEnumerator Test1()
[UnityTest] public IEnumerator CharacterTest()
```

## Coverage Targets

**Recommended coverage levels:**

| Component Type | Target Coverage |
|----------------|----------------|
| NetworkBehaviour | 80%+ |
| RPC methods | 90%+ |
| NetworkVariable logic | 90%+ |
| Character integration | 80%+ |
| Visual scripting | 70%+ |
| Utility helpers | 60%+ |

## Mock and Stub Patterns

### Mocking RPCs

```csharp
public class MockNetworkBehaviour : NetworkBehaviour
{
    public bool ServerRpcCalled { get; private set; }
    public bool ClientRpcCalled { get; private set; }

    [ServerRpc]
    public void TestServerRpc()
    {
        ServerRpcCalled = true;
    }

    [ClientRpc]
    public void TestClientRpc()
    {
        ClientRpcCalled = true;
    }
}
```

### Stub Character

```csharp
public static Character CreateStubCharacter()
{
    var go = new GameObject("StubCharacter");
    var character = go.AddComponent<Character>();
    var networkObject = go.AddComponent<NetworkObject>();

    // Set up minimal requirements
    character.IsNetworkSpawned = true;

    return character;
}
```

## Performance Testing

### Network Message Count

```csharp
[UnityTest]
public IEnumerator RPC_Optimization_SendsMinimalMessages()
{
    // Arrange
    int initialMessageCount = GetNetworkMessageCount();

    // Act - perform operations
    for (int i = 0; i < 10; i++)
    {
        component.PerformActionServerRpc(i);
        yield return null;
    }

    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert
    int finalMessageCount = GetNetworkMessageCount();
    int messagesSent = finalMessageCount - initialMessageCount;

    Assert.LessOrEqual(messagesSent, 15, "Should batch messages efficiently");
}
```

## Integration Test Pattern

### Full Multiplayer Scenario

```csharp
[UnityTest]
public IEnumerator FullScenario_PlayerJoins_ReceivesWorldState()
{
    // Arrange - Set up world
    SetupGameWorld();
    SpawnWorldObjects();

    // Act - Player joins
    var player = SpawnPlayerCharacter(clientId: 1);
    yield return NetworkTestHelpers.WaitForNetworkUpdate();

    // Assert - Player has world state
    Assert.IsNotNull(player.GetComponent<Character>());
    Assert.IsTrue(player.GetComponent<NetworkObject>().IsSpawned);
    Assert.Greater(GetVisibleWorldObjects(player).Count, 0);

    // Cleanup
    CleanupGameWorld();
}
```

## Related Documentation

- `.serena/memories/CRITICAL/001_gamecreator_invasive_integration.md` - Character patterns
- `.serena/memories/CRITICAL/003_multiplayer_rpc_patterns.md` - RPC conventions
- `Assets/Tests/Runtime/` - Actual test implementations

## Quick Reference

| Test Type | Pattern | Typical Duration |
|-----------|---------|-----------------|
| RPC execution | Spawn → Call RPC → Wait → Assert | 0.1-0.2s |
| NetworkVariable sync | Spawn → Set value → Wait → Assert | 0.1s |
| Character ownership | Spawn as player → Assert ownership | 0.1s |
| Integration | Setup → Multiple operations → Validate state | 0.5-2s |
