# Visual Scripting Specification

**Capability:** visual-scripting  
**Status:** Baseline (v1.0 - Skeleton)  
**Last Updated:** 2025-11-07

## Overview

Visual Scripting extends GameCreator's node-based system with multiplayer Instructions (Actions) and Conditions for network operations.

---

## Requirements

### Requirement: Network Instruction Execution

Instructions SHALL execute network operations (spawn, RPC, variable sync) through visual scripting.

#### Scenario: Spawn player instruction
- **WHEN** visual script executes "Network: Spawn Player" instruction
- **THEN** player SHALL spawn via NetworkObject.Spawn()
- **AND** ownership SHALL be assigned to requesting client
- **AND** instruction SHALL complete with success/failure status

#### Scenario: RPC instruction
- **WHEN** visual script executes "Network: Send RPC" instruction
- **THEN** specified RPC SHALL execute on target (server/client/all)

### Requirement: Instruction Task Signature

All network Instructions SHALL use correct Task signature without CancellationToken parameter.

#### Scenario: Instruction compilation
- **GIVEN** custom network Instruction class
- **WHEN** implementing Run() method
- **THEN** signature SHALL be `protected override Task Run(Args args)`
- **AND** SHALL NOT include CancellationToken parameter (compilation error)

### Requirement: Network Condition Evaluation

Conditions SHALL check network state (IsServer, IsClient, IsOwner, IsSpawned).

#### Scenario: Ownership condition
- **WHEN** visual script evaluates "Is Locally Owned" condition
- **THEN** condition SHALL return true if NetworkObject.IsOwner == true
- **AND** condition SHALL return false otherwise

#### Scenario: Role condition
- **WHEN** visual script evaluates "Is Server" condition
- **THEN** condition SHALL return NetworkManager.Singleton.IsServer

### Requirement: Visual Script Network Events

Triggers SHALL respond to network events (OnClientConnected, OnPlayerSpawned, OnOwnershipChanged).

#### Scenario: Player spawn trigger
- **WHEN** NetworkPlayerManager spawns player
- **THEN** "On Local Player Spawned" trigger SHALL fire
- **AND** attached instruction list SHALL execute
- **AND** Args SHALL contain spawned player reference

---

## See Also

- `.serena/memories/_INTEGRATION/GameCreator_Multiplayer_Complete_Reference.md` - Visual scripting patterns
- `claudedocs/VISUAL_SCRIPTING_MASTER_REFERENCE.md` - Complete visual scripting guide

**Note:** This is a skeleton spec. Expand as implementation progresses.
