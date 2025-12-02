# Change: Fix Hotspot and Interaction Conflicts in Multiplayer

## Why

Multiple conflicting patterns exist in the multiplayer interaction system that cause hotspots and triggers to fail silently or behave unexpectedly. These conflicts were introduced over time as different components were added without proper integration review.

**User Impact:** XP pickups, door triggers, and other hotspot-based interactions don't work correctly in multiplayer - they either fail completely, only work for host, or cause duplicate triggers.

## What Changes

### **BREAKING** Architecture Changes

1. **Remove NetworkTransform from static interaction objects** - NetworkTransform is for moving objects, not static pickups/triggers
2. **Unify Trigger vs NetworkHotspotExtension patterns** - Currently conflicting when used together
3. **Fix LocalPlayerResolver hotspot patching** - Reflection-based patching doesn't work with PropertyGetGameObject
4. **Create NetworkTriggerAdapter** - New component to bridge GC Triggers with multiplayer properly
5. **Document clear patterns** - Which approach to use for each scenario

### Non-Breaking Fixes

1. Add validation warnings in Editor for conflicting setups
2. Fix NetworkHotspotExtension to work with GC Trigger's OnTriggerEnter
3. Ensure "Local Player" target in Trigger correctly resolves to network owner

## Impact

- **Affected specs:** `multiplayer-interactions` (new capability spec)
- **Affected code:**
  - `NetworkHotspotExtension.cs` - Major refactor
  - `LocalPlayerResolver.cs` - Fix hotspot patching
  - `NetworkTriggerAdapter.cs` - New file
  - Editor validation scripts
  - Scene objects using triggers/hotspots

## Root Cause Analysis

### Conflict 1: NetworkTransform on Static Objects
- **Problem:** Adding NetworkTransform to XP pickups (static scene objects)
- **Why it breaks:** NetworkTransform requires NetworkObject, adds unnecessary bandwidth, and can cause ownership conflicts
- **Solution:** Static pickups should NOT have NetworkObject/NetworkTransform - use scene-authority pattern

### Conflict 2: Trigger + NetworkHotspotExtension Clash
- **Problem:** GC Trigger fires on physics OnTriggerEnter, NetworkHotspotExtension expects explicit RequestInteraction()
- **Why it breaks:** Two systems responding to same event with different logic
- **Solution:** NetworkHotspotExtension must intercept and gate the GC Trigger execution

### Conflict 3: "Local Player" Target Resolution
- **Problem:** PropertyGetGameObject with "Local Player" uses GetGameObjectPlayer which relies on tag lookup
- **Why it breaks:** Tag management timing conflicts with network spawn order
- **Solution:** Inject actual local player reference at spawn time, not rely on tags

### Conflict 4: LocalPlayerResolver Reflection Patching
- **Problem:** UpdateHotspotTarget() sets m_Target field to GameObject, but it's a PropertyGetGameObject
- **Why it breaks:** Type mismatch - PropertyGetGameObject is not a GameObject
- **Solution:** Create proper PropertyGetGameObject instances or use a different injection pattern

## Recommended Patterns After Fix

| Use Case | Components | Notes |
|----------|------------|-------|
| XP Pickup (local only) | Trigger + Collider (NO NetworkObject) | Each client collects independently |
| XP Pickup (synced, one-time) | Trigger + NetworkTriggerAdapter | Server validates, destroys for all |
| Door (any player opens) | Trigger + NetworkTriggerAdapter (AnyPlayer) | Synced state |
| Interactive Object | Hotspot + NetworkHotspotExtension | Full interaction system |
