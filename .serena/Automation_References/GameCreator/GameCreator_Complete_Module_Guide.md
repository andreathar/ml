# GameCreator Complete Module Guide

**Version**: GameCreator 2.x (Unity 6 Compatible)
**Created**: 2025-10-29
**Purpose**: Comprehensive reference for all GameCreator modules, features, and visual scripting capabilities

---

## Table of Contents

1. [GameCreator Overview](#gamecreator-overview)
2. [Core Module](#core-module)
3. [Visual Scripting System](#visual-scripting-system)
4. [Variables System](#variables-system)
5. [Advanced Features](#advanced-features)
6. [Inventory Module](#inventory-module)
7. [Stats Module](#stats-module)
8. [Dialogue Module](#dialogue-module)
9. [Quests Module](#quests-module)
10. [Behavior Module](#behavior-module)
11. [Additional Modules](#additional-modules)
12. [Integration with Unity](#integration-with-unity)
13. [Multiplayer Considerations](#multiplayer-considerations)

---

## GameCreator Overview

### What is GameCreator?

GameCreator is a comprehensive visual scripting framework for Unity that enables game development without extensive coding. It provides a modular, extensible architecture with visual tools for:

- Character control and animation
- Camera management
- Visual scripting (Actions, Triggers, Conditions)
- Variables and data management
- UI systems
- Physics interactions
- Audio management
- Save/Load systems

### Architecture Philosophy

```
GameCreator Framework
├── Core (Foundation)
│   ├── Characters & Animation
│   ├── Camera Systems
│   ├── Visual Scripting
│   ├── Variables
│   └── Advanced Features
├── Extension Modules
│   ├── Inventory (Items & Equipment)
│   ├── Stats (Attributes & Progression)
│   ├── Dialogue (Conversations)
│   ├── Quests (Missions & Tasks)
│   ├── Behavior (AI)
│   ├── Perception (Awareness)
│   ├── Shooter (Ranged Combat)
│   ├── Melee (Close Combat)
│   └── Traversal (Movement)
└── Integration Systems
    ├── Unity Physics
    ├── Unity UI
    ├── Unity Input System
    └── Third-Party Assets
```

### Module Interdependencies

**Core → All Modules**: Core is required by all extension modules
**Stats → Inventory**: Stats can affect item properties
**Stats → Dialogue**: Stat checks can gate dialogue options
**Stats → Quests**: Quest requirements can check stats
**Dialogue → Quests**: Dialogue can trigger quest progression
**Inventory → Quests**: Quest tasks can require items

---

## Core Module

### Character Systems

#### Character Controller
- **Purpose**: Player and NPC movement, animation, and behavior
- **Features**:
  - Navigation (pathfinding, obstacle avoidance)
  - Animation states and gestures
  - IK (Inverse Kinematics) for foot placement and look-at
  - Ragdoll physics
  - Busy states (prevent action interruption)
  - Footstep audio (surface-based)
  - Interaction mechanics

#### Animation System
- **States**: Idle, walk, run, jump, fall, crouch, etc.
- **Gestures**: One-shot animations (wave, attack, etc.)
- **Animator Integration**: Unity Animator controller
- **Blend Shapes**: Facial expressions, morphs
- **IK Targets**: Hand/foot placement, head tracking

#### Character Properties
- **Mass**: Physics weight
- **Height**: Character size (affects cameras, collisions)
- **Radius**: Collision capsule
- **Can Jump**: Enable/disable jumping
- **Can Run**: Enable/disable running
- **Motion**: Walk/run speeds, acceleration

### Camera Systems

#### Shot Types
1. **Third Person**: Follow camera behind character
2. **First Person**: View from character's eyes
3. **Fixed Camera**: Static camera position
4. **Follow**: Camera follows target smoothly
5. **Lock-On**: Camera tracks specific target
6. **Dolly**: Camera moves along path
7. **Rail**: Camera constrained to track

#### Camera Features
- **Transitions**: Smooth blending between shots
- **Zoom**: Dynamic field of view
- **Shake**: Impact effects, explosions
- **Head Bobbing**: Walking/running motion
- **Head Leaning**: Turning corners
- **Avoidance**: Obstacle detection, clipping prevention
- **Look-At**: Target tracking

#### Camera Properties
- **Field of View (FOV)**: Viewing angle
- **Offset**: Position relative to target
- **Distance**: Camera distance from target
- **Damping**: Smoothness of movement
- **Rotation**: Camera angle constraints

---

## Visual Scripting System

### The Three Pillars

**1. Actions** (Instructions - "Do something")
**2. Triggers** (Events - "When something happens")
**3. Conditions** (Logic - "Check if something is true")

### Actions (Instructions)

Actions are the building blocks of gameplay logic. They execute specific behaviors.

#### Action Categories (20+)

##### Animation Actions
- Set Animator Parameter (bool, int, float, trigger)
- Play Animation Clip
- Set Blend Shape
- Enable/Disable Animator
- Reset Animator State

##### Character Actions
- Change Character Property (speed, height, mass)
- Change Character State (idle, walk, run, jump)
- Enable/Disable Character
- Change Character Skin
- Teleport Character
- Look At Target
- Follow Path
- Play Gesture

##### Camera Actions
- Change Camera Shot
- Change Camera Properties (FOV, distance, offset)
- Shake Camera
- Zoom Camera
- Cut to Camera (instant transition)
- Follow Target

##### Audio Actions
- Play Audio Clip
- Play Music
- Play UI Sound
- Play Speech
- Stop Audio
- Change Volume/Pitch
- Audio Mixing

##### Math Actions
- Arithmetic (add, subtract, multiply, divide)
- Random (random number, random choice)
- Clamp (constrain value)
- Lerp (linear interpolation)
- Geometry (distance, direction, angle)
- Shading (color operations)

##### Variable Actions
- Set Variable Value
- Get Variable Value
- Increment/Decrement
- List Operations (add, remove, clear, loop)
- Dictionary Operations
- Change ID (rename variable)

##### Physics Actions (2D/3D)
- Add Force
- Add Torque
- Set Velocity
- Enable/Disable Gravity
- Freeze Position/Rotation
- Check Collision/Overlap
- Raycast

##### Game Object Actions
- Instantiate GameObject
- Destroy GameObject
- Enable/Disable GameObject
- Change Transform (position, rotation, scale)
- Parent/Unparent
- Find GameObject

##### Scene Management Actions
- Load Scene
- Unload Scene
- Set Active Scene
- Load Scene Additive
- Async Scene Loading

##### UI Actions
- Change Text
- Change Image/Sprite
- Change Slider Value
- Change Toggle State
- Enable/Disable UI Element
- Show/Hide Panel
- Play UI Animation

##### Input Actions
- Change Input Map
- Enable/Disable Input
- Rebind Controls
- Save Input Preferences

##### System Actions
- Wait (delay)
- Time Scale (slow motion, pause)
- Quit Application
- Open URL/Web Page
- Change Cursor
- Debug Log

##### Storage Actions
- Save Game
- Load Game
- Delete Save
- Check Save Exists
- Get/Set PlayerPrefs

##### Visual Scripting Actions
- Call Trigger
- Call Action
- Set Condition Result
- Broadcast Message

#### Action Execution
- **Sequence**: Actions run in order (top to bottom)
- **Wait for Completion**: Action can block until finished
- **Run and Continue**: Action starts, next action runs immediately
- **Cancellation**: Interrupt action with Cancel Action

#### Action Parameters
Every Action has configurable parameters:
- **Target**: GameObject or variable reference
- **Value**: Number, text, boolean, vector
- **Duration**: Time-based actions (lerp, wait)
- **Easing**: Animation curves for smooth transitions

### Triggers (Events)

Triggers detect events and execute Actions in response.

#### Trigger Categories

##### Lifecycle Triggers
- **On Start**: When GameObject starts (first frame)
- **On Enable**: When GameObject/component enabled
- **On Disable**: When GameObject/component disabled
- **On Destroy**: When GameObject destroyed
- **On Awake**: Before Start (initialization)

##### Input Triggers
- **On Input Down**: Key/button pressed
- **On Input Up**: Key/button released
- **On Input Hold**: Key/button held
- **On Mouse Click**: Mouse button click
- **On Mouse Hover**: Mouse over object
- **On Touch**: Mobile touch input

##### Collision Triggers (2D/3D)
- **On Collision Enter**: Object collision begins
- **On Collision Stay**: Object collision continues
- **On Collision Exit**: Object collision ends
- **On Trigger Enter**: Trigger zone entered
- **On Trigger Stay**: Inside trigger zone
- **On Trigger Exit**: Trigger zone exited

##### Physics Triggers
- **On Grounded**: Character touches ground
- **On Airborne**: Character leaves ground
- **On Jump**: Character jumps
- **On Land**: Character lands after jump/fall

##### Variable Triggers
- **On Global Name Variable Change**: Global variable modified
- **On Local Name Variable Change**: Local variable modified
- **On Global List Variable Change**: Global list modified
- **On Local List Variable Change**: Local list modified

##### Time Triggers
- **On Interval**: Repeating timer
- **On Delay**: One-time delay
- **On Frame**: Every frame update

##### Custom Triggers
- **On Message**: Receive broadcast message
- **On Signal**: Custom event signal
- **On Hotspot Activated**: Interaction point triggered

#### Trigger Configuration
- **Repeat**: Trigger can fire once or multiple times
- **Cooldown**: Minimum time between trigger activations
- **Conditions**: Only fire if conditions met
- **Tags/Layers**: Filter by GameObject tags or layers

### Conditions (Logic Checks)

Conditions evaluate game state and return true/false.

#### Condition Categories

##### Character Conditions
- Is Character Grounded
- Is Character Moving
- Is Character Running
- Is Character Jumping
- Is Character Busy
- Is Character Alive
- Has Character Property (speed, height, etc.)

##### Input Conditions
- Is Input Down
- Is Input Up
- Is Input Pressed
- Is Axis Value (gamepad analog)

##### Math Conditions
- Compare Numbers (==, !=, <, >, <=, >=)
- Compare Strings
- Compare Booleans
- Is Between Range
- Is Multiple Of

##### Physics Conditions
- Is Grounded
- Is Colliding With
- Is In Trigger Zone
- Raycast Hit

##### GameObject Conditions
- Is GameObject Active
- Is GameObject Enabled
- Is Component Enabled
- GameObject Exists
- Has Component

##### Variable Conditions
- Compare Variable Value
- Variable Exists
- List Contains Item
- List Is Empty

##### Logic Conditions
- AND (all conditions true)
- OR (any condition true)
- NOT (invert condition)

#### Condition Usage
- **If/Else Branching**: Different actions based on result
- **Trigger Filtering**: Only fire trigger if conditions met
- **Action Gating**: Prevent action unless conditions satisfied

### Hotspots (Interaction Points)

Hotspots are interactive objects in the scene.

**Features**:
- **Proximity Detection**: Activate when player nearby
- **Interaction Prompt**: Display UI hint
- **Activation**: Execute Actions on interaction
- **Reusable**: Can be triggered multiple times
- **Conditional**: Only active if conditions met

**Use Cases**:
- Doors, levers, switches
- Collectibles, pickups
- NPC interactions
- Quest objectives
- Environmental puzzles

---

## Variables System

### Overview

Variables store dynamic data accessible throughout the game. GameCreator provides four variable types:

1. **Global Name Variables**: Key-value pairs accessible everywhere
2. **Global List Variables**: Collections accessible everywhere
3. **Local Name Variables**: Key-value pairs scoped to GameObject
4. **Local List Variables**: Collections scoped to GameObject

### Variable Scopes

#### Global Variables
- **Access**: Anywhere in any scene
- **Persistence**: Can be saved/loaded
- **Use Cases**:
  - Player inventory
  - Game settings
  - Quest progress
  - Unlocked achievements
  - High scores

#### Local Variables
- **Access**: Only on GameObject or children
- **Persistence**: Lost when GameObject destroyed
- **Use Cases**:
  - Enemy health
  - Puzzle state
  - Timer countdown
  - Temporary calculations

### Variable Types

#### Name Variables (Key-Value)
Store single values identified by name (key).

**Supported Data Types**:
- **Number**: Integer or decimal (float, double)
- **Boolean**: True/false
- **String**: Text
- **Color**: RGBA color
- **Vector2**: 2D coordinates (x, y)
- **Vector3**: 3D coordinates (x, y, z)
- **Quaternion**: Rotation
- **GameObject**: Reference to scene object
- **Component**: Reference to component
- **Sprite**: 2D image
- **Texture**: 2D/3D texture
- **Material**: Rendering material
- **AudioClip**: Sound file

**Example**:
```
Global Name Variable: "PlayerHealth" = 100
Local Name Variable: "DoorLocked" = true
Global Name Variable: "PlayerPosition" = Vector3(10, 0, 5)
```

#### List Variables (Collections)
Store multiple values in ordered collections.

**Features**:
- **Dynamic Size**: Add/remove items at runtime
- **Index Access**: Get item by position (0, 1, 2, ...)
- **Iteration**: Loop through all items
- **Sorting**: Alphabetically, by distance, by value
- **Filtering**: Select items matching criteria

**Example**:
```
Global List Variable: "QuestLog" = ["Quest1", "Quest2", "Quest3"]
Local List Variable: "EnemiesInRange" = [Enemy1, Enemy2, Enemy3]
```

### Variable Operations (Actions)

#### Name Variable Actions
- **Set Variable**: Assign new value
- **Get Variable**: Retrieve current value
- **Increment/Decrement**: Add/subtract from number
- **Toggle**: Flip boolean (true ↔ false)
- **Change ID**: Rename variable

#### List Variable Actions
- **Add to List**: Append item to end
- **Insert in List**: Add item at specific index
- **Remove from List**: Delete item by value or index
- **Clear List**: Empty all items
- **Move Item**: Change item position
- **Shuffle List**: Randomize order
- **Reverse List**: Flip order
- **Sort List**: Alphabetically, by distance, by value
- **Filter List**: Keep only items matching condition
- **Loop List**: Execute actions for each item
- **Collect Characters**: Fill list with all characters in scene

### Variable Events (Triggers)

#### Name Variable Events
- **On Global Name Variable Change**: Fires when global variable modified
- **On Local Name Variable Change**: Fires when local variable modified

#### List Variable Events
- **On Global List Variable Change**: Fires when global list modified (add/remove/clear)
- **On Local List Variable Change**: Fires when local list modified

### Variable Conditions

- **Compare Variable Value**: Check if variable equals/greater/less than value
- **Variable Exists**: Check if variable defined
- **List Contains**: Check if list has specific item
- **List Is Empty**: Check if list has zero items
- **List Count**: Compare list size

### Best Practices

#### Naming Conventions
✅ **DO**:
- Use descriptive names: `PlayerHealth`, `EnemiesDefeated`, `QuestActive`
- Use PascalCase: `MaxHealth`, `CurrentAmmo`
- Prefix by type: `List_Enemies`, `Bool_DoorLocked`

❌ **DON'T**:
- Use vague names: `Var1`, `Temp`, `X`
- Use abbreviations: `HP`, `Atk`, `Def` (use full words)

#### Global vs Local Decision
- **Use Global**: Data needed across scenes (player stats, inventory, settings)
- **Use Local**: Data specific to one object (enemy health, door state)

#### Performance
- **Lists**: Avoid very large lists (>1000 items) for frequent operations
- **Polling**: Don't check variables every frame unnecessarily (use events instead)

---

## Advanced Features

### Audio Systems

#### Audio Manager
- **Purpose**: Centralized audio control
- **Features**:
  - Master volume control
  - Audio categories (music, SFX, voice, UI)
  - Per-category volume/muting
  - Audio mixing profiles
  - 3D spatial audio
  - Audio occlusion/obstruction

#### Audio Actions
- **Play Audio Clip**: One-shot sound
- **Play Music**: Background music with crossfade
- **Play UI Sound**: Interface feedback sounds
- **Play Speech**: Character dialogue
- **Stop Audio**: Halt specific sound/category
- **Change Volume**: Adjust audio level
- **Change Pitch**: Speed up/slow down audio

### Data Structures

#### Dictionaries
- **Purpose**: Key-value storage (alternative to Name Variables)
- **Features**:
  - String keys, any value type
  - Dynamic add/remove
  - Existence checking
  - Enumeration

#### State Machines
- **Purpose**: Manage complex state logic
- **Features**:
  - Define states (idle, patrol, chase, attack)
  - Transitions between states
  - Entry/exit actions per state
  - Condition-based transitions
- **Use Cases**: AI behavior, UI menus, game modes

#### Spatial Hashing
- **Purpose**: Optimize proximity queries (find nearby objects)
- **Features**:
  - Fast neighbor lookup
  - Configurable cell size
  - Dynamic object registration
- **Use Cases**: Enemy awareness, particle systems, LOD management

### Variables API (Scripting)

For C# integration, GameCreator provides Variables API:

```csharp
// Global Name Variables
GlobalVariables.Get("PlayerHealth");
GlobalVariables.Set("PlayerHealth", 100);

// Global List Variables
GlobalVariables.List.Get("Inventory");
GlobalVariables.List.Add("Inventory", item);

// Local Name Variables (on GameObject)
gameObject.GetComponent<LocalNameVariables>().Get("Health");
gameObject.GetComponent<LocalNameVariables>().Set("Health", 50);

// Local List Variables (on GameObject)
gameObject.GetComponent<LocalListVariables>().Get("Enemies");
gameObject.GetComponent<LocalListVariables>().Add("Enemies", enemy);
```

### Properties System

**Purpose**: Expose C# properties to visual scripting

**How It Works**:
1. Define property in C# with `[Property]` attribute
2. Property becomes available in Actions/Conditions
3. Visual scripting can get/set property values

**Use Cases**:
- Custom component values
- Third-party asset integration
- Performance-critical values (direct access vs variables)

### Save/Load System

#### Save Data
- **Automatic**: GameCreator auto-saves variables, character states
- **Manual**: Trigger save with "Save Game" action
- **Profiles**: Multiple save slots supported
- **Format**: JSON or binary serialization

#### Custom Encryption
- **Purpose**: Protect save files from tampering
- **Configuration**: Set encryption key in settings
- **Format**: AES encryption of save data

#### Saved Data Includes
- Global Name/List Variables
- Character properties (position, rotation, state)
- Scene data (GameObject states)
- Inventory (if Inventory module installed)
- Stats (if Stats module installed)
- Quest progress (if Quests module installed)

#### Save/Load Actions
- **Save Game**: Write current state to file
- **Load Game**: Restore state from file
- **Delete Save**: Remove save file
- **Get Save Info**: Check save existence, metadata
- **Auto-Save**: Periodic automatic saving

### Tween Animations

**Purpose**: Smooth interpolation of values over time

**Features**:
- **Easing Curves**: Linear, ease-in, ease-out, bounce, elastic
- **Target Properties**: Position, rotation, scale, color, any variable
- **Duration**: Time-based animation
- **Callbacks**: Actions on start/complete

**Actions**:
- **Tween Position**: Move object smoothly
- **Tween Rotation**: Rotate object smoothly
- **Tween Scale**: Scale object smoothly
- **Tween Color**: Fade color smoothly
- **Tween Variable**: Animate any number variable

**Use Cases**:
- UI animations (fade in/out, slide, scale)
- Camera movements (cinematic shots)
- Environmental effects (floating platforms, rotating objects)
- Feedback animations (damage flash, pickup bounce)

---

## Inventory Module

### Overview

The Inventory module provides complete item management, equipment systems, crafting, and merchant interactions.

### Items

#### Item Properties
- **Name**: Display name
- **Description**: Hover tooltip text
- **Icon**: 2D sprite for UI
- **3D Model**: World representation (optional)
- **Price**: Buy/sell value
- **Weight**: Affects encumbrance
- **Stack Size**: Maximum per stack
- **Consumable**: Can be used/consumed
- **Equippable**: Can be equipped in slots
- **Droppable**: Can be dropped in world
- **Tradeable**: Can be bought/sold

#### Item Types
- **Consumables**: Potions, food, scrolls
- **Equipment**: Weapons, armor, accessories
- **Quest Items**: Key items for quests
- **Materials**: Crafting ingredients
- **Miscellaneous**: Vendor trash, collectibles

#### Item Sockets
- **Purpose**: Attachment points for upgrades/gems
- **Configuration**:
  - Socket count per item
  - Socket types (color-coded)
  - Socket compatibility rules

#### Item Actions
- **Use Item**: Consume or activate item effect
- **Equip Item**: Place in equipment slot
- **Unequip Item**: Remove from equipment slot
- **Drop Item**: Place in world
- **Give Item**: Transfer to another character
- **Destroy Item**: Delete permanently

### Equipment System

#### Equipment Slots
- **Predefined Slots**: Head, chest, legs, feet, hands, weapon, shield, accessory
- **Custom Slots**: Define your own (e.g., mount, pet, rune)
- **Slot Types**: Each slot has compatible item types
- **Multi-Slot Items**: Items can occupy multiple slots (two-handed weapons)

#### Equipment Actions
- **Equip to Slot**: Place item in specific slot
- **Unequip from Slot**: Remove item from slot
- **Swap Equipment**: Replace item in slot with another
- **Get Equipped Item**: Retrieve item from slot

#### Equipment Conditions
- **Is Slot Free**: Check if slot empty
- **Is Item Equippable**: Check if item can be equipped
- **Is Item Equipped**: Check if specific item currently equipped
- **Compare Equipped Item**: Check item in slot properties

#### Visual Equipment
- **3D Model Attachment**: Show equipped items on character
- **Bone Mapping**: Attach to character bones (hand, back, hip)
- **Skin Override**: Change character appearance based on equipped gear

### Bags (Inventory Storage)

#### Bag Properties
- **Width × Height**: Grid-based inventory (e.g., 5×4 = 20 slots)
- **Weight Limit**: Maximum carried weight
- **Type Restrictions**: Only specific item types allowed
- **Multiple Bags**: Characters can have multiple bags (backpack, pouch, bank)

#### Bag Actions
- **Add to Bag**: Place item in bag
- **Remove from Bag**: Take item out
- **Move Item**: Transfer between bags
- **Sort Bag**: Auto-organize items
- **Get Bag Content**: List all items in bag

#### Bag Conditions
- **Is Bag Full**: No space remaining
- **Is Bag Overloaded**: Weight limit exceeded
- **Has Item in Bag**: Check for specific item
- **Bag Contains Type**: Check for item type

### Crafting & Tinkering

#### Recipes
- **Ingredients**: Required items and quantities
- **Result**: Crafted item(s)
- **Requirements**: Level, skill, tool needed
- **Success Rate**: Chance to create item (can fail)
- **Byproducts**: Secondary items created

#### Crafting Actions
- **Craft Item**: Execute recipe
- **Dismantle Item**: Deconstruct into ingredients
- **Learn Recipe**: Add recipe to known list
- **Forget Recipe**: Remove recipe from known list

#### Crafting Conditions
- **Can Craft Recipe**: Has ingredients and meets requirements
- **Knows Recipe**: Recipe in learned list
- **Has Ingredients**: Sufficient materials available

#### Tinkering
- **Purpose**: Modify/upgrade existing items
- **Operations**:
  - Add socket to item
  - Embed gem in socket
  - Remove gem from socket
  - Reroll item stats

### Merchants

#### Merchant Properties
- **Inventory**: Items available for purchase
- **Buy Price Multiplier**: Markup on sold items (e.g., 1.5× = 50% markup)
- **Sell Price Multiplier**: Markdown on bought items (e.g., 0.5× = 50% markdown)
- **Stock Refresh**: Time until inventory restocks
- **Reputation**: Discount based on player standing

#### Merchant Actions
- **Buy from Merchant**: Purchase item
- **Sell to Merchant**: Sell item to merchant
- **Refresh Merchant Stock**: Reset inventory
- **Open Merchant UI**: Display shop interface

#### Merchant Conditions
- **Can Buy Item**: Has enough currency
- **Can Sell Item**: Merchant accepts item type
- **Is Item in Stock**: Merchant has item available

#### Merchant Events
- **On Purchase**: Triggered when item bought
- **On Sale**: Triggered when item sold
- **On Shop Open/Close**: UI open/close events

### Inventory UI

#### Bag UI
- **Grid Display**: Visual inventory grid
- **Drag & Drop**: Move items between slots
- **Item Tooltips**: Hover info (name, description, stats)
- **Sorting Buttons**: Auto-organize inventory
- **Filter Tabs**: Show specific item types

#### Merchant UI
- **Shop Inventory**: Merchant's items for sale
- **Player Inventory**: Player's items for selling
- **Transaction Panel**: Buy/sell confirmation
- **Currency Display**: Player's gold/money

#### Equipment UI
- **Equipment Slots**: Visual representation of slots
- **Equipped Items**: Display currently equipped gear
- **Stat Comparison**: Compare equipped vs new item
- **Quick Equip**: Click item to auto-equip

#### Crafting UI
- **Recipe List**: Available recipes
- **Ingredient Check**: Visual ingredient status (green = have, red = need)
- **Craft Button**: Execute recipe
- **Result Preview**: Show crafted item details

---

## Stats Module

### Overview

The Stats module provides character progression, attributes, status effects, and RPG-style statistics.

### Attributes

#### Core Attributes
- **Health (HP)**: Life points
- **Mana/Energy**: Ability resource
- **Stamina**: Physical exertion
- **Strength**: Physical power
- **Dexterity**: Agility and precision
- **Constitution**: Endurance and vitality
- **Intelligence**: Mental capacity
- **Wisdom**: Perception and insight
- **Charisma**: Social influence
- **Luck**: Random outcome modifier

#### Attribute Properties
- **Base Value**: Starting value
- **Current Value**: Modified by buffs/debuffs
- **Min/Max**: Value constraints
- **Regeneration**: Automatic recovery over time
- **Formula**: Calculate derived values

### Traits

**Purpose**: Character specializations, skills, perks

**Examples**:
- **Skills**: Swordsmanship, archery, lockpicking
- **Perks**: Critical hit chance, bonus damage, resistance
- **Abilities**: Unlocked powers (fireball, heal, invisibility)

**Trait Levels**:
- Each trait has experience/level
- Leveling trait improves effectiveness
- Can have level-based unlocks

### Status Effects

#### Effect Types
- **Buffs**: Positive effects (strength boost, speed increase, invincibility)
- **Debuffs**: Negative effects (poison, slow, silence, stun)
- **Persistent**: Always active (racial bonuses, equipment stats)

#### Effect Properties
- **Duration**: How long effect lasts
- **Stack Count**: Multiple instances of effect
- **Tick Interval**: How often effect applies (e.g., poison every 1 second)
- **Modify Attribute**: Change stat values
- **Cancel Conditions**: When effect ends early

#### Effect Actions
- **Apply Status Effect**: Add effect to character
- **Remove Status Effect**: Remove specific effect
- **Clear Status Effects**: Remove all effects of type
- **Extend Status Effect**: Prolong duration

#### Effect Conditions
- **Has Status Effect**: Check if effect active
- **Status Effect Stack Count**: Check number of stacks

### Formulas

**Purpose**: Calculate derived values from attributes

**Examples**:
```
MaxHealth = Constitution × 10 + Level × 5
AttackDamage = Strength × 1.5 + WeaponDamage
CriticalChance = Luck × 0.5 + Dexterity × 0.2
MovementSpeed = BaseSpeed × (1 + Dexterity * 0.01)
```

**Formula Variables**:
- Attributes (Strength, Dexterity, etc.)
- Traits (Skill levels)
- Constants (Base values)
- Math operations (+, -, ×, ÷, ^, sqrt, abs, min, max)

### Classes

**Purpose**: Character archetypes with predefined stats and growth

**Class Properties**:
- **Starting Attributes**: Initial stat values
- **Attribute Growth**: Stats per level
- **Starting Traits**: Initial skills/perks
- **Equipment Restrictions**: Allowed item types
- **Ability Unlocks**: Level-based ability access

**Examples**:
- **Warrior**: High Strength/Constitution, low Intelligence
- **Mage**: High Intelligence/Wisdom, low Strength
- **Rogue**: High Dexterity/Luck, medium all others
- **Cleric**: High Wisdom/Charisma, medium Strength

### Experience & Leveling

#### Experience System
- **Gain Experience**: From combat, quests, discoveries
- **Experience Actions**: Add/remove XP
- **Level Up**: Trigger when XP threshold reached
- **Level Up Actions**: Auto-execute on level gain (stat increase, ability unlock)

#### Level Scaling
- **Enemy Levels**: Match player level
- **Loot Quality**: Higher level = better drops
- **Quest Rewards**: Scale with player level

### Stats Actions

- **Change Attribute**: Modify stat value (damage, heal, buff, debuff)
- **Apply Formula**: Calculate and set derived value
- **Add/Remove Trait**: Grant/revoke skill/perk
- **Gain Experience**: Award XP
- **Set Level**: Directly change level
- **Apply Status Effect**: Add buff/debuff
- **Remove Status Effect**: Clear effect

### Stats Conditions

- **Compare Attribute**: Check if stat meets threshold
- **Has Trait**: Check if character has skill/perk
- **Has Status Effect**: Check if effect active
- **Compare Level**: Check character level
- **Can Level Up**: Enough XP to level

### Stats Events

- **On Attribute Change**: Triggered when stat modified
- **On Trait Gained/Lost**: Triggered when skill acquired/removed
- **On Status Effect Applied/Removed**: Effect state changes
- **On Level Up**: Character gains level
- **On Death**: Health reaches zero

---

## Dialogue Module

### Overview

The Dialogue module provides conversation systems, NPC interactions, branching narratives, and cinematic sequences.

### Conversations

#### Dialogue Structure
```
Conversation
├── Node 1: NPC speaks
│   ├── Choice A: Player option
│   │   └── Node 2: NPC responds to A
│   └── Choice B: Player option
│       └── Node 3: NPC responds to B
└── Node 4: Continuation
```

#### Node Types
- **Speech Node**: Character speaks (text + audio)
- **Choice Node**: Player selects response
- **Jump Node**: Branch to different part of conversation
- **Exit Node**: End conversation

#### Dialogue Properties
- **Speaker**: Character delivering line
- **Text**: Spoken text content
- **Audio Clip**: Voice acting (optional)
- **Duration**: How long text displays (auto or manual advance)
- **Choices**: Player response options (branching)
- **Conditions**: Only show if conditions met

### Actors

**Purpose**: Characters participating in dialogues

**Actor Properties**:
- **Name**: Display name
- **Portrait**: UI image
- **Voice**: Audio profile
- **Expression Set**: Facial expressions/emotions
- **Color**: Text color for speaker identification

### Expressions

**Purpose**: Visual emotional reactions during dialogue

**Expression Types**:
- **Facial**: Happy, sad, angry, surprised, neutral
- **Body Language**: Gesture animations
- **Camera Reactions**: Close-up on speaker

**Expression Actions**:
- **Set Expression**: Change actor's face/gesture
- **Clear Expression**: Return to neutral

### Story System

**Purpose**: Track narrative progress, branching paths, and player choices

**Story Variables**:
- Track dialogue choices made
- Track story branches taken
- Gate future dialogues based on history

**Story Actions**:
- **Mark Story Point**: Flag narrative milestone
- **Check Story Point**: Query if event occurred

### Dialogue Actions

- **Start Conversation**: Begin dialogue sequence
- **Continue Conversation**: Advance to next node
- **End Conversation**: Exit dialogue
- **Set Actor Expression**: Change emotional state
- **Skip Dialogue Line**: Player fast-forward

### Dialogue Conditions

- **Has Completed Conversation**: Check if dialogue finished before
- **Has Chosen Option**: Check specific choice made
- **Story Point Reached**: Check narrative flag

### Dialogue Events

- **On Conversation Start**: Dialogue begins
- **On Conversation End**: Dialogue finishes
- **On Line Spoken**: Each dialogue node
- **On Choice Selected**: Player picks option

### Dialogue UI

**Components**:
- **Dialogue Box**: Text display panel
- **Speaker Name**: Character name label
- **Speaker Portrait**: Character image
- **Choice Buttons**: Player response options
- **Continue Button**: Advance dialogue
- **Skip Button**: Fast-forward through dialogue

---

## Quests Module

### Overview

The Quests module provides mission tracking, objectives, journal UI, and quest progression systems.

### Quest Structure

```
Quest
├── Objectives
│   ├── Task 1: "Talk to NPC"
│   ├── Task 2: "Collect 5 items"
│   └── Task 3: "Defeat 3 enemies"
├── Rewards
│   ├── Experience: 100 XP
│   ├── Gold: 50
│   └── Item: Magic Sword
└── States
    ├── Inactive (not started)
    ├── Active (in progress)
    ├── Completed (finished, not rewarded)
    └── Rewarded (claimed rewards)
```

### Quests

#### Quest Properties
- **Title**: Display name
- **Description**: Quest summary
- **Icon**: UI representation
- **Type**: Main story, side quest, repeatable
- **Level Requirement**: Minimum level to start
- **Prerequisites**: Required completed quests
- **Time Limit**: Optional time constraint

#### Quest States
1. **Inactive**: Not yet started
2. **Active**: In progress
3. **Completed**: All objectives finished
4. **Failed**: Time limit expired or fail condition met
5. **Rewarded**: Rewards claimed, quest archived

### Tasks (Objectives)

#### Task Types
- **Talk to NPC**: Initiate dialogue with character
- **Collect Items**: Gather specific items (inventory check)
- **Defeat Enemies**: Kill specific or generic enemies
- **Reach Location**: Travel to waypoint
- **Use Item**: Consume or activate item
- **Wait Time**: Delay period
- **Custom Condition**: Any condition check

#### Task Properties
- **Description**: What player must do
- **Required Count**: How many times to complete (e.g., "Collect 5 herbs")
- **Current Count**: Progress tracker
- **Optional**: Task not required for quest completion
- **Hidden**: Not displayed in quest log until triggered

#### Task States
- **Inactive**: Not yet active (waiting for previous task)
- **Active**: In progress
- **Completed**: Finished
- **Failed**: Fail condition met

### Quest Actions

- **Start Quest**: Activate quest
- **Complete Quest**: Finish quest (all tasks done)
- **Fail Quest**: Mark quest as failed
- **Abandon Quest**: Player cancels quest
- **Update Task**: Increment task progress
- **Complete Task**: Mark task as finished
- **Award Quest Rewards**: Give XP, items, gold

### Quest Conditions

- **Is Quest Active**: Check if quest in progress
- **Is Quest Completed**: Check if quest finished
- **Is Task Active**: Check if specific task in progress
- **Is Task Completed**: Check if specific task finished
- **Has Quest**: Check if player has quest (any state)

### Quest Events

- **On Quest Started**: Quest activated
- **On Quest Completed**: All objectives finished
- **On Quest Failed**: Fail condition triggered
- **On Task Completed**: Individual objective finished
- **On Rewards Claimed**: Player took quest rewards

### Quest UI (Journal)

**Components**:
- **Quest List**: Active/completed/failed quests
- **Quest Details**: Description, objectives, rewards
- **Task Tracker**: On-screen HUD showing active objectives
- **Quest Marker**: World-space waypoint
- **Minimap Icon**: Quest location indicator

---

## Behavior Module

### Overview

The Behavior module provides AI systems, behavior trees, utility AI, and NPC decision-making.

### Behavior Trees

**Purpose**: Hierarchical AI logic for complex behaviors

**Node Types**:
- **Sequence**: Execute children in order (stop on failure)
- **Selector**: Execute children until one succeeds
- **Parallel**: Execute multiple children simultaneously
- **Decorator**: Modify child node behavior (repeat, invert, cooldown)
- **Action**: Leaf node performing actual behavior
- **Condition**: Leaf node checking game state

**Example Tree**:
```
Patrol Behavior
├── Sequence
│   ├── Condition: Is Path Valid
│   ├── Action: Move to Waypoint
│   └── Action: Wait at Waypoint
└── Selector (if patrol fails)
    ├── Action: Find New Path
    └── Action: Idle
```

### Utility AI

**Purpose**: Score-based decision-making (choose best action)

**How It Works**:
1. Evaluate all possible actions
2. Calculate score for each action (0-1)
3. Choose action with highest score
4. Execute action
5. Re-evaluate periodically

**Example**:
```
Enemy AI Considerations:
- Health low (0.9) → Flee
- Enemy nearby (0.7) → Attack
- Idle (0.3) → Patrol
Result: Flee (highest score)
```

### Perception System

**Purpose**: AI awareness of surroundings (see Additional Modules section)

---

## Additional Modules

### Perception Module

**Purpose**: AI awareness and detection systems

**Features**:
- **Senses**: Sight, hearing, touch
- **Detection**: Line-of-sight, radius-based, cone-based
- **Awareness States**: Unaware, alert, aware
- **Target Tracking**: Remember detected targets
- **Memory**: Retain information about last seen position

**Actions**:
- **Set Perception Radius**: Detection range
- **Enable/Disable Sense**: Toggle sight/hearing
- **Forget Target**: Clear detected target

**Conditions**:
- **Can See Target**: Line-of-sight check
- **Can Hear Target**: Sound detection check
- **Is Target in Range**: Distance check
- **Is Aware of Target**: Has detected target

### Shooter Module

**Purpose**: Ranged combat mechanics

**Features**:
- **Weapons**: Firearms, bows, magic projectiles
- **Projectiles**: Bullets, arrows, spells
- **Aiming**: Crosshair, aim-assist, accuracy spread
- **Recoil**: Visual and accuracy penalties
- **Reloading**: Ammo management
- **Damage**: Hit detection, headshots, critical hits

**Actions**:
- **Fire Weapon**: Shoot projectile
- **Reload Weapon**: Refill ammo
- **Aim Weapon**: Enter aiming mode
- **Change Weapon**: Switch equipped weapon

### Melee Module

**Purpose**: Close combat mechanics

**Features**:
- **Weapons**: Swords, axes, fists
- **Combos**: Chained attack sequences
- **Blocking**: Damage reduction/negation
- **Parrying**: Timed defense for counterattack
- **Stamina**: Attack cost resource

**Actions**:
- **Attack**: Execute melee strike
- **Block**: Raise defense
- **Parry**: Timed block
- **Execute Combo**: Perform attack chain

### Traversal Module

**Purpose**: Advanced movement mechanics

**Features**:
- **Climbing**: Ladders, ledges, climbable surfaces
- **Vaulting**: Hop over obstacles
- **Sliding**: Crouch-slide under barriers
- **Swimming**: Water navigation
- **Ziplines**: Rope traversal

**Actions**:
- **Climb**: Ascend/descend climbable surface
- **Vault**: Hurdle over obstacle
- **Slide**: Crouch-slide
- **Swim**: Water movement

---

## Integration with Unity

### Unity Components

GameCreator integrates seamlessly with Unity systems:

#### Physics Integration
- **Rigidbody**: Character physics
- **Colliders**: Collision detection
- **Joints**: Physics constraints
- **Raycasting**: Line-of-sight checks

#### Animation Integration
- **Animator Controller**: Animation state machines
- **Animation Clips**: Animation assets
- **Avatar**: Humanoid rigging
- **IK**: Inverse kinematics

#### UI Integration
- **Canvas**: UI layout
- **Text**: TextMeshPro support
- **Buttons**: Click interactions
- **Sliders/Toggles**: Input controls

#### Input Integration
- **Input System**: New Input System support
- **Input Actions**: Rebindable controls
- **Multiple Devices**: Keyboard, mouse, gamepad, touch

### Third-Party Integration

GameCreator works with popular Unity assets:

- **ProBuilder**: Level design
- **Cinemachine**: Advanced cameras
- **Timeline**: Cutscenes
- **Visual Effects Graph**: VFX
- **Shader Graph**: Custom shaders
- **Playmaker**: Visual scripting (bridge)
- **Bolt**: Visual scripting (bridge)

---

## Multiplayer Considerations

### Netcode for GameObjects Compatibility

**Challenge**: GameCreator designed for single-player; multiplayer requires adaptation

**Conversion Requirements**:

#### Network Variables
- Convert `Global Name Variables` → `NetworkVariable<T>`
- Convert `Global List Variables` → `NetworkList<T>`
- **Important**: Local variables remain local (no sync)

#### Network Actions
Actions that modify shared state need `ServerRpc` or `ClientRpc`:
- Character movement → NetworkTransform
- Health changes → NetworkVariable
- Inventory changes → NetworkList
- Quest progress → NetworkVariable

#### Network Triggers
Triggers that affect all players need network broadcasting:
- Global events → `ClientRpc` broadcast
- Multiplayer-specific triggers (player joined/left)

### Authority & Ownership

**Server Authority** (recommended):
- Server validates all actions
- Clients send input, server updates state
- Prevents cheating

**Client Authority**:
- Client directly updates state
- Server trusts client
- Faster response, less secure

### ML-Assisted Multiplayer Conversion

**Our Approach** (see CLAUDE.md):
1. **ML Models Predict**: Which components need multiplayer conversion
2. **Human Validates**: Andrew verifies predictions in Unity Inspector
3. **AI Generates Code**: Create NetworkBehaviour wrappers
4. **Andrew Tests**: Verify multiplayer functionality

**ML Training Data**: GameCreator component schemas → multiplayer requirements

---

## Quick Reference Tables

### Visual Scripting Quick Reference

| Element | Symbol | Purpose | Example |
|---------|--------|---------|---------|
| **Action** | ▶️ | Do something | "Move Character", "Play Sound" |
| **Trigger** | ⚡ | When event | "On Input Down", "On Collision" |
| **Condition** | ❓ | Check if true | "Is Grounded", "Has Item" |
| **Variable** | 📦 | Store data | "PlayerHealth", "QuestLog" |
| **Hotspot** | 🎯 | Interaction | "Door", "Lever", "NPC" |

### Variable Type Quick Reference

| Type | Scope | Structure | Use Case |
|------|-------|-----------|----------|
| **Global Name** | Everywhere | Key-Value | Player stats, settings |
| **Global List** | Everywhere | Collection | Quest log, inventory |
| **Local Name** | GameObject | Key-Value | Enemy health, door state |
| **Local List** | GameObject | Collection | Enemies in range |

### Module Feature Matrix

| Module | Characters | Items | Stats | Dialogue | Quests | Combat | AI |
|--------|-----------|-------|-------|----------|--------|--------|-----|
| **Core** | ✅ | ❌ | ❌ | ❌ | ❌ | ⚠️ | ⚠️ |
| **Inventory** | ⚠️ | ✅ | ⚠️ | ❌ | ⚠️ | ❌ | ❌ |
| **Stats** | ⚠️ | ⚠️ | ✅ | ⚠️ | ⚠️ | ⚠️ | ❌ |
| **Dialogue** | ⚠️ | ❌ | ⚠️ | ✅ | ⚠️ | ❌ | ❌ |
| **Quests** | ⚠️ | ⚠️ | ⚠️ | ⚠️ | ✅ | ❌ | ❌ |
| **Behavior** | ⚠️ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Shooter** | ⚠️ | ⚠️ | ⚠️ | ❌ | ❌ | ✅ | ⚠️ |
| **Melee** | ⚠️ | ⚠️ | ⚠️ | ❌ | ❌ | ✅ | ⚠️ |

✅ = Core feature | ⚠️ = Integration/optional | ❌ = Not included

---

## Best Practices

### Visual Scripting

✅ **DO**:
- Use descriptive names for Actions/Triggers
- Group related Actions into sub-Actions
- Add comments explaining complex logic
- Use Conditions to gate Actions
- Leverage Variables for dynamic behavior

❌ **DON'T**:
- Create overly long Action sequences (break into sub-Actions)
- Hard-code values (use Variables instead)
- Repeat identical logic (create reusable Actions)
- Ignore performance (optimize loops, conditions)

### Variables

✅ **DO**:
- Use Global for cross-scene data
- Use Local for object-specific data
- Name variables descriptively
- Use Lists for collections
- Subscribe to Variable Change events

❌ **DON'T**:
- Poll variables every frame (use events)
- Create duplicate variables
- Use vague names ("Var1", "Temp")
- Store large objects in variables (use references)

### Performance

✅ **DO**:
- Use Conditions to prevent unnecessary Actions
- Cache frequently accessed variables
- Use Spatial Hashing for proximity queries
- Batch similar operations
- Profile performance regularly

❌ **DON'T**:
- Execute expensive Actions every frame
- Use large Lists in tight loops
- Check many Conditions unnecessarily
- Leave debug Actions in production

---

## Resources

### Official Documentation
- **GameCreator Docs**: https://docs.gamecreator.io/
- **GameCreator Forum**: https://gamecreator.page/forum
- **GameCreator Discord**: Community support
- **GameCreator Hub**: Asset downloads

### Project Resources
- **Unity Interface Guide**: Unity6_Editor_Interface_Complete_Guide.md
- **ML Training Guide**: REAL_ML_TRAINING_GUIDE.md
- **AI Guidelines**: CLAUDE.md
- **OpenSpec Workflow**: openspec/AGENTS.md

---

**Document Status**: ✅ **COMPLETE**
**Last Updated**: 2025-10-29
**Source**: GameCreator Official Documentation
**Verified By**: Claude AI (The Brain) with WebFetch verification
**Next Review**: 2025-11-29

---

**Remember**: This documentation provides comprehensive GameCreator knowledge for AI assistants (Claude, Sonnet, Opus) to understand the framework deeply and prevent hallucinations about features.
