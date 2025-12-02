# Unity 6 Editor Interface - Complete Reference Guide

**Version**: Unity 6000.2
**Created**: 2025-10-29
**Purpose**: Comprehensive documentation of all Unity Editor interface elements, windows, tabs, and UI components for ML-assisted development

---

## Table of Contents

1. [Editor Overview](#editor-overview)
2. [Primary Windows](#primary-windows)
3. [Toolbar](#toolbar)
4. [Scene View](#scene-view)
5. [Game View](#game-view)
6. [Inspector Window](#inspector-window)
7. [Hierarchy Window](#hierarchy-window)
8. [Project Window](#project-window)
9. [Console Window](#console-window)
10. [Workspace Customization](#workspace-customization)
11. [Additional Windows](#additional-windows)

---

## Editor Overview

The Unity Editor is the primary development environment for creating games and interactive content. It provides a comprehensive set of windows, panels, and tools organized in a customizable workspace.

### Core Philosophy
- **Modular Interface**: All windows can be rearranged, docked, and customized
- **Tab-Based Organization**: Windows organize into tab groups for efficient workflow
- **Context-Sensitive**: Interface adapts based on selected objects and current mode
- **Non-Destructive**: Changes can be undone through comprehensive history tracking

---

## Primary Windows

### Window Hierarchy
```
Unity Editor
├── Toolbar (top, fixed position)
├── Scene View (3D/2D environment visualization)
├── Game View (runtime preview)
├── Inspector (object/asset properties)
├── Hierarchy (scene object tree)
├── Project (asset browser)
├── Console (messages, warnings, errors)
└── Additional Windows (background tasks, undo history, etc.)
```

---

## Toolbar

**Location**: Top of Unity Editor (fixed, cannot be repositioned)

### Components (Left to Right)

#### 1. Account Management
- **Purpose**: Access Unity Account settings
- **Features**:
  - Sign in/out
  - Account preferences
  - License management
  - Profile settings

#### 2. Asset Store / Package Manager
- **Purpose**: Browse and manage Unity assets and packages
- **Features**:
  - Asset Store browsing
  - Package Manager access
  - Dependency management
  - Asset downloads

#### 3. AI Tools Menu
- **Purpose**: Access Unity's AI-powered development tools
- **Features**:
  - Unity Assistant (AI help)
  - AI Generators (content creation)
  - Machine learning tools
  - Automated workflows

#### 4. Version Control
- **Purpose**: File versioning and collaboration
- **Features**:
  - Unity Version Control window
  - Git integration
  - Change tracking
  - Collaboration tools

#### 5. Play Mode Controls
- **Play Button**: Enter Play mode (simulates game runtime)
- **Pause Button**: Pause simulation (inspect state during runtime)
- **Step Button**: Advance one frame at a time (frame-by-frame debugging)

**Keyboard Shortcuts**:
- Play: `Ctrl/Cmd + P`
- Pause: `Ctrl/Cmd + Shift + P`
- Step: `Ctrl/Cmd + Alt + P`

#### 6. Cloud Services
- **Purpose**: Access Unity cloud-based services
- **Features**:
  - Cloud Build
  - Analytics
  - Multiplayer services
  - Remote configuration

#### 7. Undo History
- **Purpose**: View, undo, or redo Editor actions
- **Features**:
  - Complete action history
  - Visual timeline
  - Selective undo
  - Branch navigation

#### 8. Global Search
- **Purpose**: Quick search across entire project
- **Keyboard Shortcut**: `Ctrl/Cmd + K`
- **Features**:
  - Asset search
  - Component search
  - Settings search
  - Command search

#### 9. Layout Manager
- **Purpose**: Save and load workspace arrangements
- **Features**:
  - Built-in layouts (Default, 2 by 3, 4 Split, Tall, Wide)
  - Custom layout saving
  - Layout import/export
  - Layout deletion

---

## Scene View

**Purpose**: Visualize and interact with the game environment during development

### Navigation Controls

#### Mouse Navigation
- **Pan**: Middle mouse button + drag OR Alt + left mouse + drag
- **Orbit**: Alt + left mouse + drag (around selection)
- **Zoom**: Scroll wheel OR Alt + right mouse + drag
- **Fly-Through**: Right mouse button + WASD keys (first-person camera)

#### Keyboard Shortcuts
- **Frame Selected**: `F` (focus camera on selected object)
- **Frame All**: `Shift + F` (fit all objects in view)
- **Toggle Grid**: `Ctrl/Cmd + \\`
- **Toggle Gizmos**: `Ctrl/Cmd + Shift + G`

### Tools & Controls

#### Transform Tools (Toolbar)
1. **Hand Tool (Q)**: Pan and navigate without selecting objects
2. **Move Tool (W)**: Translate objects in 3D space
3. **Rotate Tool (E)**: Rotate objects around pivot point
4. **Scale Tool (R)**: Scale objects uniformly or per-axis
5. **Rect Tool (T)**: 2D layout and UI positioning
6. **Transform Tool (Y)**: Combined move/rotate/scale

#### Gizmo Controls
- **Purpose**: Toggle visual overlays and debug graphics
- **Access**: Top-right dropdown menu in Scene view
- **Categories**:
  - Built-in components (cameras, lights, colliders)
  - Scripts with `OnDrawGizmos()`
  - Audio sources
  - Particle systems
  - Navigation meshes

#### Grid Snapping
- **Purpose**: Precisely align GameObjects to grid
- **Enable**: `Ctrl/Cmd + \\` toggles grid visibility
- **Snap Settings**: Edit → Grid and Snap Settings
- **Hold Snap**: Hold `Ctrl/Cmd` while moving objects
- **Features**:
  - Customizable grid size
  - Multiple snap modes (vertex, grid, surface)
  - Incremental rotation snapping

### View Modes

#### Shading Modes (Dropdown)
1. **Shaded**: Default rendering with materials
2. **Wireframe**: Mesh structure only
3. **Shaded Wireframe**: Combined mesh + materials
4. **Shadow Cascades**: Visualize shadow map regions
5. **Render Paths**: Show rendering pipeline paths
6. **Alpha Channel**: Visualize transparency
7. **Overdraw**: Highlight rendering overlap
8. **Mipmaps**: Show texture mipmap levels
9. **Sprite Mask**: Preview sprite masking

#### Draw Modes (2D)
- **Textured**: Full sprite rendering
- **Alpha Channel**: Transparency visualization
- **Overdraw**: Rendering layer overlap

### Scene View Camera

#### Camera Configuration
- **Purpose**: Control Scene view perspective independently of game cameras
- **Settings**:
  - Field of view
  - Clipping planes (near/far)
  - Orthographic vs. Perspective
  - Rendering path

#### First-Person Navigation
- **Enable**: Right mouse button + WASD/QE keys
- **Movement**:
  - W/S: Forward/Backward
  - A/D: Left/Right
  - Q/E: Down/Up
- **Speed Modifier**: Hold Shift to move faster

### Visibility Management

#### Scene Visibility
- **Purpose**: Hide/show GameObjects without disabling them
- **Access**: Eye icon in Hierarchy window
- **Shortcuts**:
  - `H`: Hide selected objects
  - `Shift + H`: Show all objects
- **Isolated View**: Select object → click eye icon → shows only that object

#### Pickability Settings
- **Purpose**: Control which objects can be selected in Scene view
- **Access**: Hand icon in Hierarchy window
- **Use Case**: Prevent accidental selection of background objects

### Overlays

**Purpose**: Persistent, customizable panels with Scene view authoring tools

#### Built-in Overlays
1. **Scene View Options**: Camera settings, rendering toggles
2. **Grid and Snap**: Grid configuration, snap settings
3. **Tool Settings**: Context-sensitive tool options
4. **Orientation Gizmo**: 3D orientation reference
5. **Search Overlay**: Quick GameObject search in scene

#### Customization
- **Move**: Drag overlay by header
- **Resize**: Drag overlay edges
- **Dock**: Snap to Scene view edges
- **Hide/Show**: Right-click Scene view → Overlays menu

### Lighting Controls

- **Scene Lighting Toggle**: Enable/disable real-time lighting
- **Skybox Toggle**: Show/hide environment skybox
- **Fog Toggle**: Enable/disable atmospheric fog
- **Flares Toggle**: Show/hide lens flares

### Audio Controls

- **Audio Toggle**: Enable/disable audio playback in Scene view
- **Use Case**: Preview audio sources while editing

---

## Game View

**Purpose**: Preview how the game appears during runtime (Play mode)

### Play Mode Controls

#### Primary Buttons
1. **Play**: Switch Editor to Play mode
   - **Shortcut**: `Ctrl/Cmd + P`
   - **Behavior**: Compiles scripts, initializes game systems
   - **Warning**: Changes made in Play mode are lost when exiting

2. **Pause**: Freeze simulation
   - **Shortcut**: `Ctrl/Cmd + Shift + P`
   - **Use Case**: Inspect game state mid-runtime

3. **Step**: Advance one frame
   - **Shortcut**: `Ctrl/Cmd + Alt + P`
   - **Use Case**: Frame-by-frame debugging

#### Play Mode Options

**Play Focused** (Default)
- Focuses editor input on Game view
- Captures keyboard/mouse input for game

**Play Maximized**
- Expands Game view to fill entire editor window
- Simulates full-screen experience

**Play Unfocused**
- Maintains normal editor focus distribution
- Allows editing while game runs (advanced use)

### Camera Settings

#### Display Dropdown
- **Purpose**: Select which camera to display in Game view
- **Multi-Camera Support**: Switch between multiple cameras
- **Camera Assignment**: Set in Camera component → Target Display

#### Camera Controls
- **Main Camera**: Default rendering camera
- **Picture-in-Picture**: Stack multiple camera views
- **Split-Screen**: Multiple viewports for local multiplayer

### Display Options

#### Aspect Ratio
- **Purpose**: Test different screen formats
- **Options**:
  - Free Aspect (default, matches Game view size)
  - 16:9, 16:10, 4:3 (common ratios)
  - Custom aspect ratios
  - iPhone, iPad, Android presets

#### Low Resolution Aspect Ratios
- **Purpose**: Emulate lower-resolution displays
- **Use Case**: Test pixel art games, retro aesthetics

#### Scale Slider
- **Purpose**: Zoom in/out of Game view
- **Range**: 0.1x to 10x
- **Use Case**: Examine UI details, test at different resolutions

#### VSync Toggle
- **Purpose**: Synchronize rendering to monitor refresh rate
- **Impact**: Reduces screen tearing, affects frame timing

### Statistics & Debugging

#### Stats Button
- **Purpose**: Display performance metrics
- **Information**:
  - **FPS** (Frames Per Second)
  - **CPU Time**: Main thread and render thread
  - **Batches**: Draw calls sent to GPU
  - **Triangles**: Total triangle count
  - **Vertices**: Total vertex count
  - **SetPass Calls**: Shader state changes
  - **Shadow Casters**: Objects casting shadows
  - **Visible Skinned Meshes**: Animated mesh count
  - **Animations**: Active animation count
  - **Audio Sources**: Active audio count
  - **Audio Memory**: Audio clip memory usage
  - **GC Allocations**: Garbage collection allocations per frame

#### Frame Debugger
- **Purpose**: Detailed rendering analysis
- **Access**: Stats button → Frame Debugger
- **Features**:
  - Step through rendering events
  - Inspect draw calls
  - View shader passes
  - Analyze GPU usage
  - Identify rendering bottlenecks

#### Gizmos Menu
- **Purpose**: Control visibility of debug graphics in Game view
- **Options**:
  - Same as Scene view gizmos
  - Typically disabled for final gameplay preview

---

## Inspector Window

**Purpose**: Display and edit properties of selected GameObjects, assets, or components

### Core Features

#### Property Display Modes

**Normal Mode** (Default)
- Shows public variables and serialized fields
- User-friendly property names
- Organized by component

**Debug Mode**
- Shows all fields (public, private, protected)
- Raw variable names
- Exposes internal Unity data
- **Access**: Three-dot menu → Debug

**Debug-Internal Mode**
- Ultra-detailed internal Unity state
- For advanced debugging only

#### Component Management

**Adding Components**
- **Button**: "Add Component" at bottom of Inspector
- **Search**: Type component name or category
- **Categories**:
  - Mesh (MeshFilter, MeshRenderer, etc.)
  - Effects (Particle System, Trail Renderer, etc.)
  - Physics (Rigidbody, Colliders, Joints, etc.)
  - Physics 2D (Rigidbody2D, Collider2D, etc.)
  - Navigation (NavMesh Agent, Obstacle, etc.)
  - Audio (Audio Source, Audio Listener, etc.)
  - Video (Video Player)
  - Rendering (Camera, Light, etc.)
  - Tilemap (Tilemap, Tilemap Renderer, etc.)

**Removing Components**
- **Method**: Three-dot menu on component → Remove Component
- **Undo**: `Ctrl/Cmd + Z` to restore

**Reordering Components**
- **Method**: Drag component header up/down
- **Use Case**: Organize for readability

**Copying/Pasting Components**
- **Copy Component**: Right-click component → Copy Component
- **Paste**: Right-click another GameObject → Paste Component
- **Paste Values**: Update existing component with copied values

#### Component Context Menu
- **Reset**: Restore component to default values
- **Move Up/Down**: Reorder in Inspector
- **Copy/Paste Component Values**: Transfer settings
- **Remove Component**: Delete component

### Property Types

#### Common Property Types

**Numeric Fields**
- **Integer**: Whole numbers
- **Float**: Decimal numbers
- **Slider**: Range-constrained values
- **Range**: Min/max clamped values

**Text Fields**
- **String**: Single-line text
- **TextArea**: Multi-line text
- **Multiline**: Expandable text area

**Boolean Fields**
- **Toggle**: Checkbox for true/false
- **Use Case**: Feature enable/disable

**Vector Fields**
- **Vector2**: 2D coordinates (x, y)
- **Vector3**: 3D coordinates (x, y, z)
- **Vector4**: 4D values (x, y, z, w)
- **Color**: RGBA color picker

**Reference Fields**
- **GameObject**: Drag GameObject from Hierarchy
- **Component**: Drag component from another object
- **Asset**: Drag asset from Project window
- **Scene**: Reference scene assets

**Enum Fields**
- **Dropdown**: Select from predefined options
- **Flags**: Multiple selection (bitwise)

**Array/List Fields**
- **Size**: Number of elements
- **Elements**: Individual items in collection
- **Add/Remove**: Plus/minus buttons

#### Advanced Property Features

**Property Drawers**
- Custom UI for specific property types
- Example: ColorUsage, GradientUsage, DelayedAttribute

**Property Attributes**
- **[Header("Text")]**: Section header in Inspector
- **[Tooltip("Text")]**: Hover tooltip for property
- **[Range(min, max)]**: Slider with bounds
- **[Space(pixels)]**: Vertical spacing
- **[SerializeField]**: Make private field visible
- **[HideInInspector]**: Hide public field

### Lock Feature

**Purpose**: Keep Inspector focused on specific object regardless of selection

**How to Use**:
1. Select object
2. Click lock icon (top-right of Inspector)
3. Inspector remains fixed on locked object
4. Select other objects without changing Inspector view

**Use Cases**:
- Compare properties between objects
- Reference one object while editing another
- Keep settings visible during multi-object workflow

### Multi-Object Editing

**Purpose**: Edit multiple selected objects simultaneously

**How It Works**:
1. Select multiple GameObjects (`Ctrl/Cmd + click`)
2. Inspector shows shared properties
3. Changes apply to all selected objects
4. **Mixed Values**: Shows "—" for differing values

**Limitations**:
- Only common components shown
- Cannot add/remove components for multiple objects

### Preset System

**Purpose**: Save and reuse component configurations

**Creating Presets**:
1. Configure component to desired state
2. Click Preset icon (top-right of component)
3. Choose "Save current to..." or create new preset

**Applying Presets**:
1. Click Preset icon on component
2. Select from saved presets
3. Component updates to preset values

**Use Cases**:
- Standardize camera settings across scenes
- Apply consistent light configurations
- Reuse physics material properties

### Icon Assignment

**Purpose**: Assign visual markers to GameObjects in Scene view

**How to Use**:
1. Click icon square (top-left of Inspector, next to GameObject name)
2. Choose from built-in icons or custom textures
3. Icon appears above GameObject in Scene view

**Use Cases**:
- Mark spawn points, waypoints, triggers
- Identify important objects in crowded scenes
- Visual organization without affecting gameplay

---

## Hierarchy Window

**Purpose**: Display and organize all GameObjects in the current scene

### Organization Features

#### GameObject Display
- **Tree Structure**: Parent-child relationships visualized
- **Indentation**: Child objects indented under parents
- **Foldout Arrows**: Expand/collapse child hierarchies
- **Name Display**: GameObject names (editable via `F2` or right-click → Rename)

#### Creation Order
- **Default**: GameObjects listed by creation time
- **Manual Reorder**: Drag up/down to change order
- **Note**: Order affects sibling index (used in scripting)

### Parenting System

#### Creating Parent-Child Relationships

**Method 1: Drag and Drop**
1. Drag child GameObject
2. Drop onto intended parent
3. Child indents under parent

**Method 2: Context Menu**
1. Right-click child GameObject
2. Select "Create Empty Parent"
3. New empty parent created with child

**Method 3: Paste As Child**
1. Copy GameObject (`Ctrl/Cmd + C`)
2. Select parent GameObject
3. Right-click → Paste As Child
4. Maintains world position

#### Parent-Child Behavior
- **Transform Inheritance**: Children inherit parent's position, rotation, scale
- **Local Transform**: Child position relative to parent
- **World Transform**: Absolute position in scene
- **Example**: Moving parent moves all children together

#### Nested Hierarchies
- **Unlimited Depth**: Parents can have parents (grandparents)
- **Scene Root**: Top-level objects have no parent
- **Performance**: Deep hierarchies can impact performance

### Visibility Controls

#### Scene Visibility (Eye Icon)
- **Purpose**: Hide/show GameObjects in Scene view only
- **Does NOT**:
  - Disable GameObject
  - Affect Game view
  - Change active state
- **Shortcuts**:
  - Click eye icon to toggle
  - `Alt + click`: Toggle all descendants
  - `H`: Hide selected
  - `Shift + H`: Show all

#### Scene Picking (Hand Icon)
- **Purpose**: Control selectability in Scene view
- **Use Case**: Prevent accidental selection of backgrounds
- **Behavior**:
  - Disabled: Cannot select in Scene view
  - Still editable via Hierarchy
  - Still runs in Play mode

### Search & Filter

#### Search Bar
- **Location**: Top of Hierarchy window
- **Functionality**:
  - Search by GameObject name
  - Supports partial matches
  - Case-insensitive
  - Real-time filtering

#### Advanced Filters
- **Search by Type**: `t:` prefix
  - Example: `t:Camera` finds all Cameras
  - Example: `t:Light` finds all Lights
- **Search by Tag**: `tag:` prefix
  - Example: `tag:Player`
- **Search by Layer**: `l:` prefix
  - Example: `l:UI`

### Default Parent

**Purpose**: Automatically parent new GameObjects

**How to Use**:
1. Right-click GameObject
2. Select "Set as Default Parent"
3. GameObject highlighted with icon
4. New GameObjects automatically become children

**Limitations**:
- Only one default parent per scene
- Persists until changed or scene reloaded

### Multi-Scene Hierarchy

**Purpose**: View multiple scenes simultaneously

**How It Works**:
- Load scenes additively (`File → Open Scene Additive`)
- Each scene shows as separate root in Hierarchy
- Scenes can be loaded/unloaded independently
- **Bold Scene Name**: Active scene (new objects go here)

**Use Cases**:
- Divide large worlds into scene chunks
- Separate static environment from dynamic objects
- Team collaboration (different people work on different scenes)

### Context Menu Options

**Right-Click GameObject**:
- **Create Empty**: New empty GameObject
- **Create Empty Child**: New child under selected
- **Create 3D Object**: Primitives (Cube, Sphere, etc.)
- **Create 2D Object**: Sprites, Tilemaps
- **Create UI Element**: Canvas, Button, Text, etc.
- **Create Light**: Directional, Point, Spot, Area
- **Create Camera**: Perspective or Orthographic
- **Copy**: `Ctrl/Cmd + C`
- **Paste**: `Ctrl/Cmd + V`
- **Paste As Child**: Paste under selected object
- **Duplicate**: `Ctrl/Cmd + D`
- **Delete**: `Delete` key
- **Rename**: `F2`
- **Set as Default Parent**
- **Properties**: Jump to Inspector

---

## Project Window

**Purpose**: Main asset navigation and management interface

### Layout Modes

#### One Column Layout
- **Structure**: Single hierarchical folder tree
- **Display**: Folders and files in same view
- **Best For**: Navigating deep folder structures

#### Two Column Layout (Default)
- **Left Panel**: Folder hierarchy (tree view)
- **Right Panel**: Asset contents (icon/list view)
- **Best For**: Browsing and selecting assets
- **Resizable**: Drag divider to adjust panel sizes

### Asset Browsing

#### Folder Structure
- **Assets Folder**: Project root (visible in Unity)
- **Packages Folder**: Installed packages and dependencies
- **Hierarchy**: Nested folders organized by user
- **Foldout Arrows**: Expand/collapse folders

#### Asset Display
- **Icon View**: Large thumbnails with asset icons
- **List View**: Compact list with type icons
- **Slider**: Adjust icon size (bottom-right)
- **Hover Tooltip**: Shows asset path and type

#### Asset Types
Visual indicators for:
- **Scripts** (.cs, .js, .boo): Code icon
- **Scenes** (.unity): Scene icon
- **Prefabs** (.prefab): Blue cube icon
- **Materials** (.mat): Sphere icon
- **Textures** (.png, .jpg, etc.): Image preview
- **Models** (.fbx, .obj, etc.): 3D mesh icon
- **Audio** (.mp3, .wav, etc.): Waveform icon
- **Animations** (.anim): Animation icon
- **Fonts**: Typography icon

### Favorites Section

**Purpose**: Quick access to frequently-used items

**Location**: Above project structure list

**Adding Items**:
- Drag folders from project structure
- Drag individual assets
- Drag search queries (saved searches)

**Organization**:
- Reorder by dragging
- Remove by dragging out
- Persists across sessions

### Search & Filter System

#### Basic Search
- **Search Bar**: Top of Project window
- **Real-Time**: Filters as you type
- **Scope**: Current folder or entire project

#### Type Filtering
- **Syntax**: `t:AssetType`
- **Examples**:
  - `t:Mesh`: All mesh assets
  - `t:Prefab`: All prefabs
  - `t:Scene`: All scene files
  - `t:Script`: All scripts
  - `t:Material`: All materials
  - `t:Texture2D`: All 2D textures
  - `t:AudioClip`: All audio files
  - `t:AnimationClip`: All animations

#### Label Filtering
- **Syntax**: `l:LabelName`
- **Examples**:
  - `l:Character`: Assets labeled "Character"
  - `l:Environment`: Assets labeled "Environment"
  - `l:WIP`: Work-in-progress assets

#### Combined Filters
- **Type OR Type**: `t:Mesh t:Prefab` (meshes OR prefabs)
- **Label OR Label**: `l:Character l:Enemy` (either label)
- **Type AND Label**: `t:Prefab l:Character` (character prefabs only)

#### Saved Searches
1. Perform search with filters
2. Drag search query to Favorites
3. Named automatically (rename by clicking)
4. Click to re-run search

### Import Settings

**Access**: Select asset → Inspector window

**Common Settings**:
- **Textures**: Compression, max size, format
- **Models**: Scale, meshes, animations, materials
- **Audio**: Quality, compression, 3D settings
- **Videos**: Codec, resolution, transcode

**Apply Button**: Required after changing settings

### Package Display

#### Package Management
- **Packages**: External dependencies (UPM, Asset Store)
- **Hidden Packages Toggle**: Show/hide package contents
- **Package Manager**: Full package interface (`Window → Package Manager`)

#### Package Types
- **Unity Packages**: Built-in Unity features
- **Asset Store Packages**: Downloaded assets
- **Custom Packages**: Local or Git packages
- **Scoped Registries**: Third-party package sources

### Context Menu Options

**Right-Click Asset**:
- **Show in Explorer**: Open file location on disk
- **Open**: Open asset in default application
- **Delete**: Move to OS trash/recycle bin
- **Rename**: `F2` to rename
- **Copy Path**: Copy relative asset path
- **Find References In Scene**: Locate usage in current scene
- **Select Dependencies**: Select all dependencies
- **Reimport**: Force reimport from source
- **Reimport All**: Reimport entire folder
- **Export Package**: Create .unitypackage file
- **Create**: Add new asset/folder

**Right-Click Folder**:
- **Create**: Add subfolders or assets
- **Show in Explorer**: Open folder on disk
- **Delete**: Remove folder and contents
- **Rename**: `F2` to rename
- **Find References In Scene**: Search folder contents usage
- **Reimport**: Reimport all assets in folder
- **Export Package**: Package folder contents

---

## Console Window

**Purpose**: Display messages, warnings, errors, and logs from Unity and scripts

### Message Types

#### Messages (Info)
- **Icon**: Gray info icon
- **Purpose**: General logging output
- **API**: `Debug.Log()`, `Debug.LogFormat()`
- **Use Case**: Tracking game state, debugging information

#### Warnings
- **Icon**: Yellow warning triangle
- **Purpose**: Non-critical issues requiring attention
- **API**: `Debug.LogWarning()`, `Debug.LogWarningFormat()`
- **Use Case**: Deprecated APIs, potential problems

#### Errors
- **Icon**: Red error circle
- **Purpose**: Critical problems requiring immediate attention
- **API**: `Debug.LogError()`, `Debug.LogException()`
- **Use Case**: Null references, missing assets, exceptions

### Filtering & Display

#### Toggle Buttons
- **Messages Toggle**: Show/hide info messages
- **Warnings Toggle**: Show/hide warnings
- **Errors Toggle**: Show/hide errors
- **Combinations**: Any combination of types can be active

#### Message Count
- **Display**: Badge on each toggle shows count
- **Updates**: Real-time as messages arrive
- **Clear**: Resets counts

#### Collapse Feature
- **Purpose**: Show only first instance of identical messages
- **Toggle**: Collapse button (icon: stacked lines)
- **Count**: Shows "×N" for repeated messages
- **Expanded View**: Uncheck to see all instances

### Search & Filter

#### Search Bar
- **Location**: Top of Console window
- **Functionality**:
  - Keyword-based filtering
  - Real-time as you type
  - Case-insensitive
  - Exact match only (no regex)
- **Limitation**: Cannot combine multiple search terms

### Stack Traces

#### Stack Trace Display
- **Location**: Detail area below message list
- **Content**:
  - File names with line numbers
  - Method call hierarchy
  - Parameter information

#### Clickable References
- **Click**: Opens script at error line
- **Format**: `FileName.cs:LineNumber`
- **Example**: `PlayerController.cs:45`

#### Stack Trace Logging Settings
- **Access**: Edit → Preferences → Stack Trace Logging
- **Options**:
  - **Script Only**: Only script code in stack trace
  - **Full**: Include Unity engine code
  - **None**: No stack trace
- **Per Message Type**: Configure separately for messages, warnings, errors

### Error Handling

#### Error Pause
- **Purpose**: Freeze Play mode when error occurs
- **Enable**: Checkbox "Error Pause" in Console toolbar
- **Behavior**:
  - Play mode pauses on `Debug.LogError()`
  - Allows inspecting game state at error time
  - Resume with Play button

#### Clear Options
- **Clear Button**: Manual clear (icon: trash can)
- **Clear on Play**: Auto-clear when entering Play mode
- **Clear on Build**: Auto-clear when building project
- **Clear on Recompile**: Auto-clear when scripts recompile

### Log Management

#### Editor Log
- **Purpose**: Full Unity Editor activity log
- **Access**: Console menu → Open Editor Log
- **Location**: OS-dependent
  - **Windows**: `%LOCALAPPDATA%\Unity\Editor\Editor.log`
  - **Mac**: `~/Library/Logs/Unity/Editor.log`
  - **Linux**: `~/.config/unity3d/Editor.log`

#### Player Log
- **Purpose**: Standalone build runtime logs
- **Access**: Console menu → Open Player Log
- **Location**: Platform-dependent
  - **Windows**: `%USERPROFILE%\AppData\LocalLow\CompanyName\GameName\output_log.txt`
  - **Mac**: `~/Library/Logs/Company Name/Game Name/Player.log`
  - **Linux**: `~/.config/unity3d/CompanyName/GameName/Player.log`

### Remote Device Logging

#### Attach to Player
- **Purpose**: View logs from builds running on remote devices
- **Access**: "Attach to Player" dropdown in Console toolbar
- **Requirements**:
  - Development Build enabled
  - Autoconnect Profiler enabled
  - Device on same network

#### Supported Platforms
- Android devices (USB or network)
- iOS devices (USB or network)
- Console platforms (devkits)
- Standalone builds (network)

### Context Menu Options

**Right-Click Message**:
- **Copy**: Copy message text to clipboard
- **Go to source code**: Open script at error line (if applicable)
- **Stack Trace Logging**: Quick access to verbosity settings

---

## Workspace Customization

**Purpose**: Tailor Unity Editor interface to workflow preferences

### Tab Management

#### Tab Operations
- **Open New Tab**: Window menu → Select window type
- **Close Tab**: Right-click tab → Close Tab OR middle-click
- **Maximize Tab**: Right-click tab → Maximize OR double-click
- **Add Tab**: Right-click tab bar → Add Tab → Select window

#### Moving Tabs

**Within Main Window**:
1. Drag tab by title
2. Editor shows outline preview (blue highlight)
3. Release at desired location
4. **Docking Zones**:
   - Center: Create tab group
   - Edges: Dock to edge (top, bottom, left, right)
   - Corners: Create corner split

**To Floating Window**:
1. Drag tab outside main window
2. Release to create independent window
3. Floating windows can contain multiple tabs
4. **Return to Main**: Drag back into main window

### Layout Presets

#### Built-in Layouts
- **Default**: Balanced multi-window view
- **2 by 3**: Grid layout with six sections
- **4 Split**: Quad-split screen (Scene, Game, Hierarchy, Inspector)
- **Tall**: Vertical emphasis (better for portrait monitors)
- **Wide**: Horizontal emphasis (better for ultrawide monitors)

#### Custom Layouts

**Saving Layouts**:
1. Arrange workspace to desired configuration
2. Layout menu → Save Layout
3. Enter name
4. **Options**:
   - Save as new
   - Overwrite existing (including built-ins)

**Loading Layouts**:
- Layout menu → Select saved layout
- Editor rearranges to match saved configuration

**Exporting Layouts**:
1. Layout menu → Save layout to file
2. Choose location (`.wlt` file)
3. **Use Case**: Share layouts with team, backup configurations

**Importing Layouts**:
1. Layout menu → Load layout from file
2. Select `.wlt` file
3. Layout added to Layout menu

**Deleting Layouts**:
1. Layout menu → Delete Layout
2. Select layout to remove
3. **Note**: Can delete built-in layouts (restore with "Reset all Layouts")

**Resetting Layouts**:
- Layout menu → Reset all Layouts
- Restores all built-in layouts to defaults
- Deletes all custom modifications
- **Warning**: Cannot be undone

### Window Sizing & Positioning

#### Resizing Windows
- **Drag Divider**: Between docked windows
- **Proportional**: Other windows adjust automatically
- **Minimum Size**: Windows enforce minimum dimensions

#### Floating Windows
- **Independent**: Can move outside main window
- **Multi-Monitor**: Place on secondary monitors
- **Always on Top**: Optional setting per window
- **Close**: Removes from workspace (reopen from Window menu)

### Preferences

**Access**: Edit → Preferences (Windows/Linux) or Unity → Preferences (Mac)

#### Key Preference Categories

**General**:
- Editor skin (Light/Dark)
- Script editor selection
- Asset serialization mode
- Editor analytics

**External Tools**:
- External script editor (VS Code, Visual Studio, Rider)
- Image application
- Revision control diff/merge tools

**Colors**:
- Playmode tint (helps identify Play mode)
- Animation recording tint
- Custom UI colors

**Keys**:
- Keyboard shortcuts customization
- Conflict detection
- Export/import shortcut profiles

**2D**:
- Grid opacity
- Grid rendering
- Sprite packer settings

**Analysis**:
- Code coverage
- Performance reporting

**Asset Pipeline**:
- Import worker count
- Cache server configuration
- Asset database refresh

**GI Cache**:
- Lightmap cache size and location
- Cache compression

**Scene View**:
- Camera settings
- Create objects at origin
- Grid and snap defaults

**Search**:
- Search engine selection
- Indexing options
- Provider settings

### Project Settings

**Access**: Edit → Project Settings

**Differs from Preferences**:
- **Preferences**: Editor behavior (per-machine)
- **Project Settings**: Game/project configuration (version controlled)

#### Key Project Settings

**Audio**:
- Global volume
- DSP buffer size
- Virtual voice count
- Real voice count

**Editor**:
- Asset serialization
- Default behavior mode (2D/3D)
- Sprite packer mode
- Enter Play Mode settings

**Graphics**:
- Rendering pipeline
- Shader stripping
- Built-in shader settings
- Tier settings per platform

**Input Manager**:
- Axis definitions
- Button mappings
- Joystick configuration

**Physics**:
- Gravity
- Bounce threshold
- Sleep threshold
- Layer collision matrix

**Physics 2D**:
- 2D gravity
- Velocity thresholds
- 2D layer collision matrix

**Player**:
- Company name
- Product name
- Version
- Icon
- Splash screen
- Platform-specific settings

**Quality**:
- Quality levels (Low, Medium, High, etc.)
- Shadows
- Texture quality
- Anti-aliasing
- V-Sync

**Tags and Layers**:
- Custom tags
- Sorting layers
- Physics layers

**Time**:
- Fixed timestep
- Maximum allowed timestep
- Time scale

---

## Additional Windows

### Background Tasks Window

**Purpose**: Monitor ongoing Unity operations

**Access**: Window → General → Background Tasks

**Features**:
- Asset import progress
- Lightmap baking status
- Package installation
- Script compilation
- Asset bundle builds

**Information Displayed**:
- Task name
- Progress percentage
- Time elapsed
- Estimated time remaining
- Cancel button (for cancelable tasks)

### Undo History Window

**Purpose**: Visual timeline of Editor actions

**Access**: Window → General → Undo History

**Features**:
- Complete action history
- Jump to any previous state
- Branch visualization (when undoing and making new changes)
- Search through history
- Clear history

**Use Cases**:
- Selective undo (skip intermediate steps)
- Compare different approaches
- Recover from complex multi-step mistakes

### Profiler Window

**Purpose**: Performance analysis and optimization

**Access**: Window → Analysis → Profiler

**Features**:
- CPU usage breakdown
- GPU usage
- Memory allocation
- Rendering statistics
- Audio performance
- Physics simulation time
- Network statistics

### Frame Debugger

**Purpose**: Step-by-step rendering analysis

**Access**: Window → Analysis → Frame Debugger

**Features**:
- Render event timeline
- Draw call inspection
- Shader pass visualization
- Texture preview
- Mesh rendering
- Performance impact analysis

### Lighting Window

**Purpose**: Configure scene lighting and baking

**Access**: Window → Rendering → Lighting

**Features**:
- Skybox settings
- Ambient lighting
- Realtime lighting settings
- Mixed lighting settings
- Baked lighting settings
- Lightmap settings
- Generate lighting button

### Occlusion Culling Window

**Purpose**: Optimize rendering by culling hidden objects

**Access**: Window → Rendering → Occlusion Culling

**Features**:
- Occlusion area configuration
- Bake occlusion data
- Visualization modes
- Statistics

### Navigation Window

**Purpose**: Configure AI pathfinding

**Access**: Window → AI → Navigation

**Features**:
- NavMesh baking
- Agent configuration
- Area settings
- Obstacle settings

### Animator Window

**Purpose**: Create and edit animation state machines

**Access**: Window → Animation → Animator

**Features**:
- State machine graph
- Transitions between states
- Blend trees
- Animation layers
- Parameters
- Avatar configuration

### Animation Window

**Purpose**: Create and edit animation clips

**Access**: Window → Animation → Animation

**Features**:
- Keyframe editing
- Curve editing
- Event markers
- Dopesheet view
- Curves view

### Package Manager

**Purpose**: Manage Unity packages and dependencies

**Access**: Window → Package Manager

**Features**:
- Browse Unity Registry packages
- Asset Store packages
- Custom packages
- Install/uninstall
- Update packages
- Package dependencies

### Test Runner

**Purpose**: Execute automated tests

**Access**: Window → General → Test Runner

**Features**:
- Play Mode tests
- Edit Mode tests
- Run all or selected tests
- Test results
- Code coverage integration

### Version Control Window

**Purpose**: Manage version control integration

**Access**: Window → Version Control (if enabled)

**Features**:
- Pending changes
- Changelist management
- File history
- Diff viewer
- Branch switching
- Merge conflicts

---

## Keyboard Shortcuts Reference

### General

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Save | `Ctrl + S` | `Cmd + S` |
| Undo | `Ctrl + Z` | `Cmd + Z` |
| Redo | `Ctrl + Y` | `Cmd + Shift + Z` |
| Select All | `Ctrl + A` | `Cmd + A` |
| Duplicate | `Ctrl + D` | `Cmd + D` |
| Find | `Ctrl + F` | `Cmd + F` |
| Global Search | `Ctrl + K` | `Cmd + K` |

### Play Mode

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Play | `Ctrl + P` | `Cmd + P` |
| Pause | `Ctrl + Shift + P` | `Cmd + Shift + P` |
| Step | `Ctrl + Alt + P` | `Cmd + Alt + P` |

### Scene View

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Hand Tool | `Q` | `Q` |
| Move Tool | `W` | `W` |
| Rotate Tool | `E` | `E` |
| Scale Tool | `R` | `R` |
| Rect Tool | `T` | `T` |
| Transform Tool | `Y` | `Y` |
| Frame Selected | `F` | `F` |
| Frame All | `Shift + F` | `Shift + F` |
| Toggle Grid | `Ctrl + \\` | `Cmd + \\` |

### GameObject

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| New Empty GameObject | `Ctrl + Shift + N` | `Cmd + Shift + N` |
| Rename | `F2` | `F2` |
| Delete | `Delete` | `Delete` |

---

## Interface Integration for ML Systems

### Component Detection Workflow

**For GameCreator ML Analysis**:

1. **Scene View**: Visual inspection of component placement
2. **Hierarchy**: Navigate GameObject structure
3. **Inspector**: Read component properties and values
4. **Project**: Locate prefabs and assets
5. **Console**: Monitor ML prediction logs

### Symbol Tracking Integration

**Unity Interface → Serena Memory**:

```
Unity Inspector Component
↓
SerenaSymbolExporter.cs (Editor Script)
↓
Data/Exports/symbols/export_{timestamp}.json
↓
Serena MCP Memory (symbol_schemas.md)
↓
AI Query Tools (query_symbol_by_name, etc.)
```

### ML Validation Workflow

**Human-AI Collaboration**:

1. **AI Prediction**: ML model predicts component type
2. **Unity Inspector**: Andrew verifies component details
3. **Visual Confirmation**: Scene view shows component behavior
4. **Validation Logging**: Console confirms/rejects prediction
5. **Serena Update**: Store validation result in memory

---

## Best Practices for Interface Usage

### Organization

✅ **DO**:
- Use consistent naming conventions (PascalCase for GameObjects)
- Group related objects under parent GameObjects
- Assign icons to important GameObjects
- Use tags and layers meaningfully
- Save custom layouts for specific tasks

❌ **DON'T**:
- Leave unnamed GameObjects ("GameObject (1)", "GameObject (2)")
- Flatten all objects to scene root (no hierarchy)
- Ignore visibility controls (clutter Scene view)
- Mix unrelated objects without organization

### Performance

✅ **DO**:
- Use Scene visibility to hide complex objects while editing
- Collapse unused Hierarchy sections
- Filter Project window when browsing large asset folders
- Close unused Editor windows
- Use Stats panel to identify bottlenecks

❌ **DON'T**:
- Keep all GameObjects visible in complex scenes
- Leave Frame Debugger open during editing
- Run profiler continuously without need

### Workflow

✅ **DO**:
- Lock Inspector when comparing objects
- Use favorites for frequently accessed assets
- Save searches for common queries
- Use keyboard shortcuts for speed
- Test in Game view regularly

❌ **DON'T**:
- Switch between Inspector and Project constantly
- Manually navigate deep folder structures repeatedly
- Rely only on mouse for all operations
- Edit only in Scene view (Game view validation essential)

---

## Troubleshooting Common Interface Issues

### Inspector Not Updating
- **Cause**: Inspector locked
- **Solution**: Click lock icon to unlock

### Cannot Select GameObject in Scene
- **Cause**: Scene picking disabled
- **Solution**: Enable hand icon in Hierarchy

### GameObject Hidden in Scene View
- **Cause**: Scene visibility off
- **Solution**: Enable eye icon in Hierarchy OR press `Shift + H`

### Changes Lost After Play Mode
- **Cause**: Editing in Play mode
- **Solution**: **ALWAYS** exit Play mode before making changes
- **Workaround**: Copy component, exit Play mode, paste values

### Layout Reset Unexpectedly
- **Cause**: Unity crash or forced close
- **Solution**: Layout menu → Load custom layout OR reset to default

### Console Flooded with Messages
- **Cause**: Repeated errors in loop
- **Solution**: Enable "Collapse" to see unique messages

---

## Resources & Further Reading

### Official Unity Documentation
- Unity Manual: https://docs.unity3d.com/Manual/
- Unity Scripting API: https://docs.unity3d.com/ScriptReference/
- Unity Tutorials: https://learn.unity.com/

### GameCreator Documentation
- GameCreator Docs: https://docs.gamecreator.io/
- GameCreator Forum: https://gamecreator.page/forum

### Project-Specific Resources
- **START_HERE_ML.md**: ML system quick start
- **REAL_ML_TRAINING_GUIDE.md**: Training workflow
- **CLAUDE.md**: AI assistant guidelines
- **openspec/AGENTS.md**: OpenSpec workflow

---

**Document Status**: ✅ **COMPLETE**
**Last Updated**: 2025-10-29
**Source**: Unity 6000.2 Official Documentation
**Verified By**: Claude AI (The Brain) with Ref MCP integration
**Next Review**: 2025-11-29 (or when Unity 6000.3 releases)

---

**Remember**: This documentation prevents hallucinations by providing verified, authoritative information about Unity's interface. Always reference this document before making claims about Unity Editor features.
