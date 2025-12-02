#!/usr/bin/env python3
"""
Unity Knowledge Base - Advanced Keyword Mapper
===============================================

Maps comprehensive keywords to C# classes for intelligent indexing and search.
Implements the taxonomy from keyword_taxonomy_unity_kb.md for automated:
- GameCreator module detection
- Visual scripting automation
- Conflict detection
- Documentation retrieval

Author: MLCreator AI Assistant
Version: 2.0
"""

import re
import json
from pathlib import Path
from typing import Dict, List, Set, Optional, Tuple
from dataclasses import dataclass, field
from enum import Enum

class ComponentType(Enum):
    MONOBEHAVIOUR = "monobehaviour"
    NETWORKBEHAVIOUR = "networkbehaviour"
    SCRIPTABLEOBJECT = "scriptableobject"
    EDITORWINDOW = "editorwindow"
    CUSTOMEDITOR = "customeditor"
    PROPERTYDRAWER = "propertydrawer"
    CONDITION = "condition"
    ACTION = "action"
    INSTRUCTION = "instruction"

class UnityNamespace(Enum):
    UNITY_ENGINE = "unity.engine"
    UNITY_NETCODE = "unity.networking"
    UNITY_EDITOR = "unity.editor"
    UNITY_UI = "unity.ui"
    UNITY_PHYSICS = "unity.physics"
    UNITY_AI = "unity.ai"

class GameCreatorModule(Enum):
    CORE = "gc.core"
    CHARACTER = "gc.character"
    INVENTORY = "gc.inventory"
    STATS = "gc.stats"
    SHOOTER = "gc.shooter"
    QUESTS = "gc.quests"
    DIALOGUE = "gc.dialogue"
    CAMERA = "gc.camera"
    VARIABLES = "gc.variables"
    SAVE = "gc.save"
    TWEENS = "gc.tweens"
    AUDIO = "gc.audio"
    UI = "gc.ui"
    FACTIONS = "gc.factions"
    PERCEPTION = "gc.perception"
    BEHAVIOR = "gc.behavior"
    MAILBOX = "gc.mailbox"
    TACTILE = "gc.tactile"
    PLATFORMING = "gc.platforming"

@dataclass
class KeywordMetadata:
    """Metadata for keyword-mapped classes"""
    name: str
    namespace: str
    keywords: Set[str] = field(default_factory=set)
    component_type: Optional[ComponentType] = None
    gc_module: Optional[GameCreatorModule] = None
    conflicts: Set[str] = field(default_factory=set)
    documentation_refs: Set[str] = field(default_factory=set)

class UnityKeywordMapper:
    """Advanced keyword mapper for Unity KB indexing"""

    def __init__(self):
        self.conflicting_components = {
            'NetworkTransform': ['CharacterController', 'Rigidbody'],
            'CharacterController': ['NetworkTransform', 'Rigidbody'],
            'Rigidbody': ['CharacterController', 'NetworkTransform']
        }

        self.gc_module_patterns = {
            GameCreatorModule.CHARACTER: [
                r'.*Character.*', r'.*Movement.*', r'.*Animation.*',
                r'.*Controller.*', r'.*Pivot.*', r'.*Driver.*'
            ],
            GameCreatorModule.INVENTORY: [
                r'.*Inventory.*', r'.*Item.*', r'.*Equipment.*',
                r'.*Container.*', r'.*Slot.*', r'.*Merchant.*'
            ],
            GameCreatorModule.STATS: [
                r'.*Stat.*', r'.*Attribute.*', r'.*Modifier.*',
                r'.*Formula.*', r'.*Calculation.*'
            ],
            GameCreatorModule.SHOOTER: [
                r'.*Shooter.*', r'.*Weapon.*', r'.*Projectile.*',
                r'.*Ammo.*', r'.*Damage.*', r'.*Recoil.*'
            ],
            GameCreatorModule.VARIABLES: [
                r'.*Variable.*', r'.*Property.*', r'.*Constant.*'
            ]
        }

    def map_class_keywords(self, class_name: str, namespace: str,
                          base_classes: Optional[List[str]] = None,
                          interfaces: Optional[List[str]] = None,
                          methods: Optional[List[str]] = None) -> KeywordMetadata:
        """
        Map comprehensive keywords to a C# class

        Args:
            class_name: The class name
            namespace: Full namespace
            base_classes: List of base classes
            interfaces: List of implemented interfaces
            methods: List of method names

        Returns:
            KeywordMetadata with comprehensive keyword mapping
        """
        metadata = KeywordMetadata(name=class_name, namespace=namespace)
        base_classes = base_classes or []
        interfaces = interfaces or []
        methods = methods or []

        # 1. Component Type Keywords
        metadata.component_type = self._detect_component_type(class_name, base_classes)
        if metadata.component_type:
            metadata.keywords.add(metadata.component_type.value)

        # 2. Unity Namespace Keywords
        unity_ns = self._map_unity_namespace(namespace)
        if unity_ns:
            metadata.keywords.add(unity_ns.value)

        # 3. GameCreator Module Keywords
        metadata.gc_module = self._detect_gc_module(class_name, namespace)
        if metadata.gc_module:
            metadata.keywords.add(metadata.gc_module.value)

        # 4. Multiplayer Keywords
        if self._is_multiplayer_class(class_name, base_classes, methods):
            metadata.keywords.update([
                'network', 'multiplayer', 'sync', 'rpc'
            ])

        # 5. Visual Scripting Keywords
        if self._is_visual_scripting_class(class_name, base_classes):
            metadata.keywords.update([
                'visualscript', 'action', 'condition', 'instruction'
            ])

        # 6. Conflict Detection Keywords
        conflicts = self._detect_conflicts(class_name, base_classes)
        if conflicts:
            metadata.conflicts.update(conflicts)
            metadata.keywords.add('conflict')

        # 7. Documentation Keywords
        docs = self._get_documentation_refs(class_name, namespace, metadata.gc_module)
        if docs:
            metadata.documentation_refs.update(docs)

        # 8. Functional Keywords
        functional = self._extract_functional_keywords(class_name, methods)
        metadata.keywords.update(functional)

        return metadata

    def _detect_component_type(self, class_name: str, base_classes: List[str]) -> Optional[ComponentType]:
        """Detect Unity component type from inheritance"""
        base_class_map = {
            'MonoBehaviour': ComponentType.MONOBEHAVIOUR,
            'NetworkBehaviour': ComponentType.NETWORKBEHAVIOUR,
            'ScriptableObject': ComponentType.SCRIPTABLEOBJECT,
            'EditorWindow': ComponentType.EDITORWINDOW,
            'Condition': ComponentType.CONDITION,
            'Action': ComponentType.ACTION,
            'Instruction': ComponentType.INSTRUCTION
        }

        for base in base_classes:
            if base in base_class_map:
                return base_class_map[base]

        # Name-based detection for custom editors
        if 'Editor' in class_name:
            return ComponentType.CUSTOMEDITOR

        return None

    def _map_unity_namespace(self, namespace: str) -> Optional[UnityNamespace]:
        """Map namespace to Unity category"""
        ns_map = {
            'UnityEngine': UnityNamespace.UNITY_ENGINE,
            'Unity.Netcode': UnityNamespace.UNITY_NETCODE,
            'UnityEditor': UnityNamespace.UNITY_EDITOR,
            'UnityEngine.UI': UnityNamespace.UNITY_UI,
            'UnityEngine.Physics': UnityNamespace.UNITY_PHYSICS,
            'UnityEngine.AI': UnityNamespace.UNITY_AI
        }

        for ns_prefix, unity_ns in ns_map.items():
            if namespace.startswith(ns_prefix):
                return unity_ns

        return None

    def _detect_gc_module(self, class_name: str, namespace: str) -> Optional[GameCreatorModule]:
        """Detect GameCreator module from namespace and patterns"""
        if not namespace.startswith('GameCreator'):
            return None

        # Namespace-based detection
        ns_module_map = {
            'GameCreator.Runtime.Character': GameCreatorModule.CHARACTER,
            'GameCreator.Runtime.Inventory': GameCreatorModule.INVENTORY,
            'GameCreator.Runtime.Stats': GameCreatorModule.STATS,
            'GameCreator.Runtime.Shooter': GameCreatorModule.SHOOTER,
            'GameCreator.Runtime.Quests': GameCreatorModule.QUESTS,
            'GameCreator.Runtime.Dialogue': GameCreatorModule.DIALOGUE,
            'GameCreator.Runtime.Camera': GameCreatorModule.CAMERA,
            'GameCreator.Runtime.Variables': GameCreatorModule.VARIABLES,
            'GameCreator.Runtime.Save': GameCreatorModule.SAVE,
            'GameCreator.Runtime.Tweens': GameCreatorModule.TWEENS,
            'GameCreator.Runtime.Audio': GameCreatorModule.AUDIO,
            'GameCreator.Runtime.UI': GameCreatorModule.UI,
            'GameCreator.Runtime.Factions': GameCreatorModule.FACTIONS,
            'GameCreator.Runtime.Perception': GameCreatorModule.PERCEPTION,
            'GameCreator.Runtime.Behavior': GameCreatorModule.BEHAVIOR,
            'GameCreator.Runtime.Mailbox': GameCreatorModule.MAILBOX,
            'GameCreator.Runtime.Tactile': GameCreatorModule.TACTILE,
            'GameCreator.Runtime.Platforming': GameCreatorModule.PLATFORMING
        }

        for ns_pattern, module in ns_module_map.items():
            if namespace.startswith(ns_pattern):
                return module

        # Pattern-based detection for cross-cutting classes
        for module, patterns in self.gc_module_patterns.items():
            for pattern in patterns:
                if re.search(pattern, class_name, re.IGNORECASE):
                    return module

        return GameCreatorModule.CORE

    def _is_multiplayer_class(self, class_name: str, base_classes: List[str], methods: List[str]) -> bool:
        """Check if class is multiplayer-related"""
        # Base class check
        if 'NetworkBehaviour' in base_classes:
            return True

        # Method pattern check
        multiplayer_patterns = [
            r'.*ServerRpc.*', r'.*ClientRpc.*', r'.*NetworkVariable.*',
            r'.*Spawn.*', r'.*Despawn.*', r'.*Sync.*'
        ]

        for method in methods:
            for pattern in multiplayer_patterns:
                if re.search(pattern, method):
                    return True

        # Name pattern check
        if re.search(r'.*(Network|Multiplayer|Sync|Rpc).*', class_name, re.IGNORECASE):
            return True

        return False

    def _is_visual_scripting_class(self, class_name: str, base_classes: List[str]) -> bool:
        """Check if class is visual scripting related"""
        vs_bases = ['Condition', 'Action', 'Instruction', 'Trigger', 'Event']
        if any(base in base_classes for base in vs_bases):
            return True

        if re.search(r'.*(Condition|Action|Instruction).*', class_name):
            return True

        return False

    def _detect_conflicts(self, class_name: str, base_classes: List[str]) -> Set[str]:
        """Detect potential conflicts with other components"""
        conflicts = set()

        # Check against known conflicting components
        for component, conflicting_with in self.conflicting_components.items():
            if component in base_classes or component in class_name:
                conflicts.update(conflicting_with)

        # Check for duplicate MonoBehaviours
        if 'MonoBehaviour' in base_classes and 'Singleton' not in class_name:
            conflicts.add('duplicate_monobehaviour')

        return conflicts

    def _get_documentation_refs(self, class_name: str, namespace: str,
                               gc_module: Optional[GameCreatorModule]) -> Set[str]:
        """Get documentation references for the class"""
        docs = set()

        # Unity documentation
        if namespace.startswith('UnityEngine'):
            docs.add('unity.api')
            if 'Network' in class_name:
                docs.add('unity.multiplayer')
        elif namespace.startswith('Unity.Netcode'):
            docs.add('unity.netcode')

        # GameCreator documentation
        if gc_module:
            docs.add(f'gc.{gc_module.value.split(".")[1]}')
            docs.add('gc.api')

        return docs

    def _extract_functional_keywords(self, class_name: str, methods: List[str]) -> Set[str]:
        """Extract functional keywords from class name and methods"""
        keywords = set()

        # Name-based keywords
        name_lower = class_name.lower()

        functional_patterns = {
            'movement': ['move', 'motion', 'velocity', 'direction'],
            'animation': ['anim', 'pose', 'gesture', 'transition'],
            'physics': ['collision', 'rigidbody', 'force', 'gravity'],
            'ui': ['canvas', 'button', 'panel', 'menu'],
            'ai': ['agent', 'path', 'navigation', 'behavior'],
            'audio': ['sound', 'music', 'effect', 'volume'],
            'input': ['key', 'mouse', 'touch', 'controller'],
            'save': ['persist', 'serialize', 'load', 'data']
        }

        for category, patterns in functional_patterns.items():
            if any(pattern in name_lower for pattern in patterns):
                keywords.add(category)

        # Method-based keywords
        for method in methods:
            method_lower = method.lower()
            if 'serverrpc' in method_lower:
                keywords.add('serverrpc')
            elif 'clientrpc' in method_lower:
                keywords.add('clientrpc')
            elif 'networkvariable' in method_lower:
                keywords.add('networkvariable')
            elif 'async' in method_lower or 'await' in method_lower:
                keywords.add('async')

        return keywords

def apply_keywords_to_kb_data(kb_data_path: str, output_path: str):
    """
    Apply keyword mapping to existing KB data

    Args:
        kb_data_path: Path to existing KB data
        output_path: Path to save enhanced data
    """
    mapper = UnityKeywordMapper()

    # Load existing KB data (this would be from Qdrant export)
    # For now, create sample processing

    sample_classes = [
        {
            'name': 'NetworkCharacterAdapter',
            'namespace': 'MLCreator_Multiplayer.Runtime.Components',
            'base_classes': ['NetworkBehaviour'],
            'methods': ['MoveServerRpc', 'SyncPosition', 'Update']
        },
        {
            'name': 'ConditionIsClientPlayer',
            'namespace': 'MLCreator_Multiplayer.Runtime.VisualScripting.Conditions',
            'base_classes': ['Condition'],
            'methods': ['Run']
        },
        {
            'name': 'Character',
            'namespace': 'GameCreator.Runtime.Characters',
            'base_classes': ['MonoBehaviour'],
            'methods': ['MoveToDirection', 'SetMotion', 'Update']
        }
    ]

    enhanced_data = []

    for class_data in sample_classes:
        metadata = mapper.map_class_keywords(
            class_data['name'],
            class_data['namespace'],
            class_data.get('base_classes', []),
            class_data.get('interfaces', []),
            class_data.get('methods', [])
        )

        enhanced_item = {
            'name': metadata.name,
            'namespace': metadata.namespace,
            'keywords': list(metadata.keywords),
            'component_type': metadata.component_type.value if metadata.component_type else None,
            'gc_module': metadata.gc_module.value if metadata.gc_module else None,
            'conflicts': list(metadata.conflicts),
            'documentation_refs': list(metadata.documentation_refs)
        }

        enhanced_data.append(enhanced_item)

    # Save enhanced data
    with open(output_path, 'w') as f:
        json.dump(enhanced_data, f, indent=2)

    print(f"Enhanced {len(enhanced_data)} classes with keyword metadata")
    return enhanced_data

if __name__ == '__main__':
    # Example usage
    enhanced_data = apply_keywords_to_kb_data(
        'path/to/kb_data.json',
        'enhanced_kb_data.json'
    )

    # Print sample results
    for item in enhanced_data[:3]:
        print(f"\n{item['name']}:")
        print(f"  Keywords: {', '.join(str(k) for k in item['keywords'])}")
        if item['component_type']:
            print(f"  Component: {item['component_type']}")
        if item['gc_module']:
            print(f"  GC Module: {item['gc_module']}")
        if item['conflicts']:
            print(f"  Conflicts: {', '.join(str(c) for c in item['conflicts'])}")
    enhanced_data_json = json.dumps(enhanced_data)
    print(enhanced_data_json)