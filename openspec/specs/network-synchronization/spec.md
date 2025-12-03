# Network Synchronization Specification

**Capability:** network-synchronization  
**Status:** Baseline (v1.0 - Skeleton)  
**Last Updated:** 2025-11-07

## Overview

Network Synchronization provides state replication between server and clients using NetworkVariable, RPC, and NetworkTransform patterns.

---

## Requirements

### Requirement: NetworkVariable State Replication

The system SHALL replicate variable state changes from server to clients using NetworkVariable<T>.

#### Scenario: Health synchronization
- **WHEN** server updates player health NetworkVariable
- **THEN** all clients SHALL receive the updated value
- **AND** clients SHALL trigger OnValueChanged callbacks

### Requirement: RPC Remote Invocation

The system SHALL support ClientRpc and ServerRpc methods with proper naming convention.

#### Scenario: Server RPC execution
- **WHEN** client calls [ServerRpc] method
- **THEN** method SHALL execute on server
- **AND** server SHALL validate ownership if RequireOwnership=true

#### Scenario: Client RPC execution
- **WHEN** server calls [ClientRpc] method
- **THEN** method SHALL execute on all clients (or targeted clients)

### Requirement: Custom Type Serialization

Custom structs and classes SHALL implement INetworkSerializable for network transmission.

#### Scenario: Struct serialization
- **WHEN** RPC parameter uses custom struct implementing INetworkSerializable
- **THEN** struct SHALL serialize and deserialize correctly across network

### Requirement: GameCreator Variable Sync

GameCreator Local Variables SHALL sync via NetworkVariablesSync component.

#### Scenario: Variable sync configuration
- **WHEN** NetworkVariablesSync configured with variable names
- **THEN** specified variables SHALL replicate to clients
- **AND** sync mode (Everyone/OwnerOnly/ServerOnly) SHALL be respected

---

## See Also

- `.serena/memories/_CRITICAL/Unity_Netcode_Complete_API.md` - Complete Netcode API
- `.serena/memories/_INTEGRATION/GameCreator_Multiplayer_Complete_Reference.md` - Integration patterns

**Note:** This is a skeleton spec. Expand as implementation progresses.
