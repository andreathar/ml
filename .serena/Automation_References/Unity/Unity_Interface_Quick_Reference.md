# Unity 6 Editor Interface - Quick Reference

**For**: Rapid lookup during development
**See**: Unity6_Editor_Interface_Complete_Guide.md for detailed information

---

## Essential Windows at a Glance

| Window | Purpose | Key Shortcut | Critical Features |
|--------|---------|--------------|-------------------|
| **Scene View** | 3D/2D editing | `F` (frame) | Transform tools (QWERTY), gizmos, grid snap |
| **Game View** | Runtime preview | `Ctrl/Cmd + P` (play) | Play/pause/step, aspect ratio, stats |
| **Inspector** | Properties | Lock icon | Component editing, debug mode, multi-edit |
| **Hierarchy** | Scene tree | `Ctrl/Cmd + Shift + N` | Parent-child, visibility, picking |
| **Project** | Asset browser | `Ctrl/Cmd + 0` | Favorites, search filters (t:, l:) |
| **Console** | Logs/errors | `Ctrl/Cmd + Shift + C` | Error pause, stack traces, collapse |

---

## Transform Tools (Scene View)

| Tool | Shortcut | Purpose |
|------|----------|---------|
| Hand | `Q` | Pan/navigate without selecting |
| Move | `W` | Translate objects (XYZ) |
| Rotate | `E` | Rotate around pivot |
| Scale | `R` | Scale objects |
| Rect | `T` | 2D/UI positioning |
| Transform | `Y` | Combined move/rotate/scale |

---

## Scene View Navigation

| Action | Control |
|--------|---------|
| **Pan** | Middle mouse + drag OR Alt + Left + drag |
| **Orbit** | Alt + Left + drag |
| **Zoom** | Scroll wheel OR Alt + Right + drag |
| **Fly** | Right mouse + WASD (Shift = faster) |
| **Frame Selected** | `F` |
| **Frame All** | `Shift + F` |

---

## Inspector Modes

| Mode | Access | Purpose |
|------|--------|---------|
| **Normal** | Default | User-friendly properties |
| **Debug** | Three-dot → Debug | All fields (public/private) |
| **Debug-Internal** | Three-dot → Debug-Internal | Unity internal state |

---

## Search Filters (Project & Hierarchy)

| Filter | Syntax | Example |
|--------|--------|---------|
| **Type** | `t:Type` | `t:Prefab`, `t:Scene`, `t:Script` |
| **Label** | `l:Label` | `l:Character`, `l:Environment` |
| **Tag** | `tag:Tag` | `tag:Player` |
| **Layer** | `l:Layer` | `l:UI` |
| **Combine** | Multiple terms | `t:Prefab l:Character` (AND logic) |

---

## Console Message Types

| Type | Icon | API | Purpose |
|------|------|-----|---------|
| **Message** | Gray info | `Debug.Log()` | General logging |
| **Warning** | Yellow triangle | `Debug.LogWarning()` | Non-critical issues |
| **Error** | Red circle | `Debug.LogError()` | Critical problems |

---

## Hierarchy Visibility Controls

| Icon | Purpose | Shortcut |
|------|---------|----------|
| **Eye** | Scene visibility (hide/show) | `H` (hide), `Shift + H` (show all) |
| **Hand** | Scene picking (selectability) | Click icon to toggle |
| **Foldout Arrow** | Expand/collapse children | `Alt + Click` (all descendants) |

---

## Play Mode Controls

| Button | Shortcut | Behavior |
|--------|----------|----------|
| **Play** | `Ctrl/Cmd + P` | Enter Play mode (⚠️ changes lost on exit!) |
| **Pause** | `Ctrl/Cmd + Shift + P` | Freeze simulation |
| **Step** | `Ctrl/Cmd + Alt + P` | Advance one frame |

---

## Component Management (Inspector)

| Action | Method |
|--------|--------|
| **Add** | "Add Component" button → Search/browse |
| **Remove** | Three-dot menu → Remove Component |
| **Copy** | Right-click component → Copy Component |
| **Paste** | Right-click GameObject → Paste Component |
| **Reset** | Three-dot menu → Reset |
| **Reorder** | Drag component header |

---

## Workspace Layouts

| Layout | Best For |
|--------|----------|
| **Default** | General development |
| **2 by 3** | Multi-window workflows |
| **4 Split** | Quad view (Scene/Game side-by-side) |
| **Tall** | Portrait monitors |
| **Wide** | Ultrawide monitors |
| **Custom** | Save: Layout → Save Layout |

---

## Common Keyboard Shortcuts

### General
| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Save | `Ctrl + S` | `Cmd + S` |
| Undo | `Ctrl + Z` | `Cmd + Z` |
| Redo | `Ctrl + Y` | `Cmd + Shift + Z` |
| Duplicate | `Ctrl + D` | `Cmd + D` |
| Delete | `Delete` | `Delete` |
| Rename | `F2` | `F2` |
| Find | `Ctrl + F` | `Cmd + F` |
| Global Search | `Ctrl + K` | `Cmd + K` |

### GameObject
| Action | Shortcut |
|--------|----------|
| New Empty GameObject | `Ctrl/Cmd + Shift + N` |
| Frame Selected | `F` |
| Frame All | `Shift + F` |

### Scene View
| Action | Shortcut |
|--------|----------|
| Toggle Grid | `Ctrl/Cmd + \\` |
| Toggle Gizmos | `Ctrl/Cmd + Shift + G` |

---

## ML Workflow Integration

### Symbol Tracking Workflow

```
1. Unity Inspector (view component)
   ↓
2. Tools → Serena → Export All Symbols
   ↓
3. Data/Exports/symbols/export_{timestamp}.json
   ↓
4. Serena MCP imports to memory
   ↓
5. AI uses query_symbol_by_name(), etc.
```

### Component Validation Workflow

```
1. AI predicts component type (ML model)
   ↓
2. Andrew views in Unity Inspector
   ↓
3. Visual check in Scene View
   ↓
4. Console logs validation result
   ↓
5. Serena stores validation (true/false)
```

---

## Common Pitfalls

### ⚠️ Changes Lost After Play Mode
- **Problem**: Editing during Play mode
- **Solution**: ALWAYS exit Play mode before editing
- **Workaround**: Copy component → exit Play → paste values

### ⚠️ GameObject Not Visible
- **Check**: Eye icon (Hierarchy) - Scene visibility
- **Check**: Renderer enabled (Inspector)
- **Check**: Layer visibility (Scene view)

### ⚠️ Cannot Select Object in Scene
- **Check**: Hand icon (Hierarchy) - Scene picking
- **Check**: Object not locked

### ⚠️ Inspector Not Updating
- **Check**: Lock icon - unlock Inspector
- **Check**: Multi-object selection confusion

---

## Performance Tips

### ✅ DO
- Hide complex objects (eye icon) while editing
- Close unused windows
- Use Stats panel to identify bottlenecks
- Collapse unused Hierarchy sections

### ❌ DON'T
- Keep Frame Debugger open during editing
- Leave all GameObjects visible in complex scenes
- Run Profiler continuously without need

---

## Game View Display Options

| Option | Purpose |
|--------|---------|
| **Aspect Ratio** | Test different screen formats (16:9, 4:3, etc.) |
| **Scale Slider** | Zoom in/out (0.1x - 10x) |
| **VSync** | Sync to monitor refresh (reduce tearing) |
| **Stats** | Performance metrics (FPS, draw calls, etc.) |
| **Gizmos** | Toggle debug graphics in Game view |

---

## Project Window Layouts

### One Column Layout
- Single hierarchical tree
- Files and folders in same view
- Best for: Deep folder navigation

### Two Column Layout (Default)
- Left: Folder tree
- Right: Asset icons/list
- Best for: Browsing and selecting

**Toggle**: Project window → Layout dropdown

---

## Inspector Features

### Lock Feature
- **Icon**: Lock (top-right)
- **Purpose**: Keep focused on object regardless of selection
- **Use Case**: Compare properties between objects

### Multi-Object Editing
- **How**: Select multiple GameObjects (Ctrl/Cmd + click)
- **Shows**: Common properties only
- **Mixed Values**: Displays "—" for differing values

### Preset System
- **Save**: Preset icon → Save current to...
- **Apply**: Preset icon → Select preset
- **Use Case**: Reusable component configurations

---

## Console Features

### Error Pause
- **Enable**: "Error Pause" checkbox
- **Behavior**: Freezes Play mode on `Debug.LogError()`
- **Use Case**: Inspect game state at error time

### Collapse
- **Enable**: Collapse button
- **Shows**: First instance of identical messages
- **Count**: "×N" for repetitions

### Stack Traces
- **Clickable**: File:LineNumber links
- **Opens**: Script at error line
- **Verbosity**: Edit → Preferences → Stack Trace Logging

---

## Hierarchy Features

### Default Parent
- **Enable**: Right-click GameObject → Set as Default Parent
- **Effect**: New GameObjects automatically become children
- **Limit**: One per scene

### Multi-Scene View
- **How**: File → Open Scene Additive
- **Display**: Each scene as separate root
- **Active Scene**: Bold name (new objects go here)

---

## Toolbar Quick Access

| Element | Purpose |
|---------|---------|
| **Account** | Unity Account management |
| **Asset Store** | Package Manager access |
| **AI** | Unity Assistant, Generators |
| **Version Control** | Unity VCS window |
| **Play Controls** | Play, Pause, Step |
| **Cloud** | Unity Services |
| **Undo History** | Action timeline |
| **Search** | Global search (`Ctrl/Cmd + K`) |
| **Layout** | Workspace arrangements |

---

## Scene View Overlays

### Key Overlays
- **Scene View Options**: Camera, rendering settings
- **Grid and Snap**: Grid config, snap settings
- **Tool Settings**: Context-sensitive options
- **Orientation Gizmo**: 3D reference
- **Search**: Quick GameObject search

### Customization
- **Move**: Drag by header
- **Resize**: Drag edges
- **Dock**: Snap to edges
- **Toggle**: Right-click Scene → Overlays

---

## Resource Locations

### Documentation
- **Full Guide**: `Unity6_Editor_Interface_Complete_Guide.md`
- **Unity Manual**: https://docs.unity3d.com/Manual/
- **GameCreator Docs**: https://docs.gamecreator.io/

### Project Files
- **CLAUDE.md**: AI guidelines (prevent hallucinations)
- **START_HERE_ML.md**: ML system quick start
- **openspec/AGENTS.md**: OpenSpec workflow

---

**Quick Tip**: Press `Ctrl/Cmd + K` for global search when you can't find a window or setting!

**Status**: ✅ **READY FOR USE**
**Version**: 1.0
**Created**: 2025-10-29
