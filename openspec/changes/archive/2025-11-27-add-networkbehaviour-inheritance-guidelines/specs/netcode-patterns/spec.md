## ADDED Requirements

### Requirement: NetworkBehaviour Inheritance Field Constraints

Classes inheriting from `NetworkBehaviour` SHALL NOT declare serialized or private fields with names that shadow or conflict with base class properties provided by Unity Netcode.

#### Scenario: Developer creates NetworkBehaviour subclass

- **WHEN** a developer creates a class that inherits from `NetworkBehaviour`
- **AND** attempts to add a field named `m_NetworkObject`, `NetworkObject`, `m_IsOwner`, `IsOwner`, `m_IsServer`, `IsServer`, `m_IsClient`, `IsClient`, `m_OwnerClientId`, or `OwnerClientId`
- **THEN** the field SHALL be identified as a violation
- **AND** documentation SHALL direct developer to use the base class property instead

#### Scenario: Unity serialization conflict detected

- **WHEN** Unity's serialization system encounters duplicate field names in inheritance hierarchy
- **AND** the error message is cryptic (e.g., "SerializationException" or "Duplicate field name")
- **THEN** developer SHALL check for redundant NetworkBehaviour base class field declarations
- **AND** remove the redundant field, using `this.PropertyName` instead

### Requirement: NetworkBehaviour Base Class Property Usage

All NetworkBehaviour subclasses SHALL use the inherited properties from the base class for accessing network state.

#### Scenario: Accessing NetworkObject reference

- **WHEN** a NetworkBehaviour subclass needs to access its NetworkObject
- **THEN** it SHALL use `this.NetworkObject` (inherited property)
- **AND** SHALL NOT declare a separate `[SerializeField] private NetworkObject m_NetworkObject;` field

#### Scenario: Checking ownership state

- **WHEN** a NetworkBehaviour subclass needs to check ownership
- **THEN** it SHALL use `this.IsOwner`, `this.IsServer`, `this.IsClient` (inherited properties)
- **AND** SHALL NOT cache these values in separate fields

#### Scenario: Accessing owner client ID

- **WHEN** a NetworkBehaviour subclass needs the owner's client ID
- **THEN** it SHALL use `this.OwnerClientId` (inherited property)
- **AND** SHALL NOT declare a separate `private ulong m_OwnerClientId;` field

### Requirement: NetworkBehaviour Anti-Pattern Documentation

The codebase SHALL maintain documentation of common NetworkBehaviour inheritance anti-patterns in the Serena CRITICAL memory tier.

#### Scenario: AI assistant generates NetworkBehaviour code

- **WHEN** an AI assistant is asked to create a NetworkBehaviour subclass
- **THEN** it SHALL NOT include redundant base class field declarations
- **AND** SHALL reference Serena memory for correct patterns
- **AND** generated code SHALL use inherited properties directly

#### Scenario: Code review for NetworkBehaviour subclasses

- **WHEN** code is reviewed that includes a NetworkBehaviour subclass
- **THEN** reviewer SHALL check for field names that conflict with base class properties
- **AND** flag any `m_NetworkObject`, `m_IsOwner`, `m_IsServer`, `m_IsClient`, `m_OwnerClientId` declarations as violations

### Requirement: Safe Field Naming in NetworkBehaviour Subclasses

NetworkBehaviour subclasses SHALL use unique, descriptive field names that avoid collision with base class naming conventions when declaring custom fields.

#### Scenario: Custom component references

- **WHEN** a NetworkBehaviour subclass needs to cache component references
- **THEN** it SHALL use fields with unique names like `m_Character`, `m_Animator`, `m_CustomComponent`
- **AND** these MUST NOT shadow any Unity Netcode base class properties

#### Scenario: Custom network variables

- **WHEN** a NetworkBehaviour subclass declares NetworkVariable fields
- **THEN** it SHALL use descriptive names that don't conflict with base class (e.g., `m_PlayerName`, `m_TeamId`)
- **AND** SHALL NOT use generic names like `m_NetworkVariable` that may cause future conflicts

## Reference: NetworkBehaviour Base Class Properties

The following properties are inherited from `NetworkBehaviour` and SHALL NOT be redeclared:

| Property | Type | Description |
|----------|------|-------------|
| `NetworkObject` | `NetworkObject` | The NetworkObject associated with this behaviour |
| `NetworkManager` | `NetworkManager` | Reference to the NetworkManager |
| `IsOwner` | `bool` | True if local client owns this object |
| `IsServer` | `bool` | True if running on server |
| `IsClient` | `bool` | True if running on client |
| `IsHost` | `bool` | True if running as host (server + client) |
| `IsLocalPlayer` | `bool` | True if this is the local player |
| `OwnerClientId` | `ulong` | Client ID of the owner |
| `IsSpawned` | `bool` | True if NetworkObject is spawned |
| `HasNetworkObject` | `bool` | True if NetworkObject reference is valid |

## Code Examples

### Correct Pattern

```csharp
public class NetworkPlayerController : NetworkBehaviour
{
    // ✅ Custom fields with unique names
    private Character m_Character;
    private Animator m_Animator;

    // ✅ Custom NetworkVariables with descriptive names
    private NetworkVariable<FixedString64Bytes> m_PlayerName = new();

    public override void OnNetworkSpawn()
    {
        // ✅ Use inherited properties
        if (IsOwner)
        {
            Debug.Log($"Owner spawned: {OwnerClientId}");
        }

        // ✅ Access NetworkObject via inherited property
        Debug.Log($"NetworkObjectId: {NetworkObject.NetworkObjectId}");
    }
}
```

### Anti-Pattern (DO NOT USE)

```csharp
public class BrokenController : NetworkBehaviour
{
    // ❌ WRONG: Shadows base class property - causes serialization conflict!
    [SerializeField] private NetworkObject m_NetworkObject;

    // ❌ WRONG: Redundant caching of inherited property
    private bool m_IsOwner;

    // ❌ WRONG: Duplicates base class functionality
    private ulong m_OwnerClientId;

    private void Start()
    {
        // ❌ WRONG: Using cached field instead of property
        m_IsOwner = IsOwner;
        m_OwnerClientId = OwnerClientId;
    }
}
```
