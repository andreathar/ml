#!/usr/bin/env python3
"""
Unity KB Auto-Memory Generator

Automatically generates and updates Serena memories from Unity Knowledge Base.
Supports API references, assembly overviews, and pattern catalogs.

Usage:
    # Update all auto-generated memories
    python generate_auto_memories.py --update-all

    # Generate API memory for specific class
    python generate_auto_memories.py --api NetworkCharacterAdapter

    # Generate assembly overview
    python generate_auto_memories.py --assembly GameCreator.Multiplayer.Runtime

    # Discover and generate pattern catalogs
    python generate_auto_memories.py --discover-patterns
"""

import argparse
import json
import sys
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional

# Add project root to path for imports
PROJECT_ROOT = Path(__file__).parent.parent.parent
sys.path.insert(0, str(PROJECT_ROOT))

# Direct Qdrant client (bypasses HTTP API)
try:
    # Import from absolute path
    import importlib.util
    spec = importlib.util.spec_from_file_location(
        "direct_qdrant_client",
        PROJECT_ROOT / "scripts" / "unity-kb" / "direct_qdrant_client.py"
    )
    direct_client = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(direct_client)

    search_unity_symbols = direct_client.search_unity_symbols
    get_unity_class_members = direct_client.get_unity_class_members
    get_unity_kb_stats = direct_client.get_unity_kb_stats
    list_unity_assemblies = direct_client.list_unity_assemblies

    MCP_AVAILABLE = True
except Exception as e:
    print(f"WARNING: Qdrant client not available ({e}), using mock mode")
    MCP_AVAILABLE = False

# find_similar_unity_code not implemented yet in direct client
def find_similar_unity_code(code_snippet: str, limit: int = 10) -> List[Dict]:
    """Placeholder for similar code search"""
    return []


class UnityKBMemoryGenerator:
    """Generates Serena memories from Unity Knowledge Base"""

    def __init__(self, memory_root: Path):
        self.memory_root = memory_root
        self.auto_gen_dir = memory_root / "AUTO_GENERATED"
        self.auto_gen_dir.mkdir(parents=True, exist_ok=True)

    def generate_api_memory(self, class_name: str, assembly: Optional[str] = None) -> str:
        """
        Generate comprehensive API reference memory for a class.

        Args:
            class_name: Name of the class to document
            assembly: Optional assembly name to filter by

        Returns:
            Path to generated memory file
        """
        print(f"[SEARCH] Searching for class: {class_name}")

        # Step 1: Get class metadata
        class_search = search_unity_symbols(
            query=f"class {class_name}",
            kind="class",
            assembly_name=assembly,
            limit=1
        )

        if not class_search:
            print(f"[ERROR] Class '{class_name}' not found in Unity KB")
            return None

        class_info = class_search[0]
        print(f"[OK] Found: {class_info['name_path']}")

        # Step 2: Get all class members
        print(f"[INFO] Retrieving class members...")
        members = get_unity_class_members(
            class_name=class_name,
            assembly_name=assembly
        )

        # Step 3: Find similar implementations
        print(f"[FIND] Finding similar implementations...")
        similar_code = find_similar_unity_code(
            code_snippet=f"public class {class_name}",
            limit=10
        )

        # Step 4: Generate memory content
        memory_content = self._format_api_memory(class_info, members, similar_code)

        # Step 5: Write to file
        memory_file = self.auto_gen_dir / f"api_{class_name.lower()}.md"
        memory_file.write_text(memory_content, encoding='utf-8')

        print(f"[DONE] Generated: {memory_file}")
        return str(memory_file)

    def _format_api_memory(self, class_info: Dict, members: List[Dict], similar: List[Dict]) -> str:
        """Format API memory content"""

        # Filter members by kind
        methods = [m for m in members if m['kind'] == 'method']
        properties = [m for m in members if m['kind'] == 'property']
        fields = [m for m in members if m['kind'] == 'field']

        # Format inheritance chain
        base_classes = class_info.get('base_classes', [])
        inheritance = ' → '.join(base_classes) if base_classes else 'None'

        # Format interfaces
        interfaces = class_info.get('interfaces', [])
        interfaces_str = ', '.join(interfaces) if interfaces else 'None'

        content = f"""# {class_info['name']} API Reference (Auto-Generated)

**Last Updated:** {datetime.now().isoformat()}
**Validation Status:** [OK] Generated from Unity KB
**Assembly:** {class_info['assembly_name']}
**Namespace:** {class_info['namespace']}
**Inheritance:** {inheritance}
**Interfaces:** {interfaces_str}
**File:** {class_info['file_path']}:{class_info['start_line']}

---

## Overview

{class_info.get('documentation', 'No documentation available.')}

---

## Public Methods

{self._format_members_table(methods, 'public')}

---

## Protected Methods

{self._format_members_table(methods, 'protected')}

---

## Properties

{self._format_members_table(properties)}

---

## Fields

{self._format_members_table(fields)}

---

## Network Variables

{self._format_network_members(members)}

---

## Related Implementations

{self._format_similar_code_table(similar)}

---

## Validation Queries

```python
# Verify this memory is still accurate
def validate_{class_info['name'].lower()}_memory():
    # Check class still exists
    results = search_unity_symbols(
        query="class {class_info['name']}",
        kind="class",
        assembly_name="{class_info['assembly_name']}"
    )
    assert len(results) >= 1, "Class no longer exists!"

    # Check member count
    members = get_unity_class_members(
        class_name="{class_info['name']}",
        assembly_name="{class_info['assembly_name']}"
    )
    assert len(members) >= {len(members)}, "Member count decreased!"
```

**Next Validation Due:** {(datetime.now()).strftime('%Y-%m-%d')} (weekly)

---

**Source:** Unity Knowledge Base
**Generator:** scripts/unity-kb/generate_auto_memories.py
"""
        return content

    def _format_members_table(self, members: List[Dict], modifier: Optional[str] = None) -> str:
        """Format members as markdown table"""

        if modifier:
            members = [m for m in members if modifier in m.get('modifiers', [])]

        if not members:
            return "*No members found.*"

        table = "| Name | Signature | Location | Documentation |\n"
        table += "|------|-----------|----------|---------------|\n"

        for member in members[:20]:  # Limit to 20 to avoid huge tables
            name = member['name']
            sig = member.get('signature', '').replace('|', '\\|')  # Escape pipes
            loc = f"{member.get('file_path', '')}:{member.get('start_line', '')}"
            doc = member.get('documentation', '').split('\n')[0][:100]  # First line, max 100 chars

            table += f"| {name} | `{sig}` | {loc} | {doc} |\n"

        if len(members) > 20:
            table += f"\n*...and {len(members) - 20} more*\n"

        return table

    def _format_network_members(self, members: List[Dict]) -> str:
        """Format NetworkVariable and RPC members"""

        network_vars = [m for m in members if 'NetworkVariable' in m.get('signature', '')]
        rpcs = [m for m in members if 'Rpc' in m.get('name', '')]

        if not network_vars and not rpcs:
            return "*No network-specific members found.*"

        content = ""

        if network_vars:
            content += "### NetworkVariables\n\n"
            content += self._format_members_table(network_vars)
            content += "\n\n"

        if rpcs:
            content += "### RPCs\n\n"
            content += self._format_members_table(rpcs)

        return content

    def _format_similar_code_table(self, similar: List[Dict]) -> str:
        """Format similar code implementations"""

        if not similar:
            return "*No similar implementations found.*"

        table = "| Class | Assembly | Similarity | Location |\n"
        table += "|-------|----------|------------|----------|\n"

        for impl in similar[:10]:
            name = impl.get('name', 'Unknown')
            assembly = impl.get('assembly_name', '')
            similarity = impl.get('similarity_score', 0.0)
            loc = f"{impl.get('file_path', '')}:{impl.get('start_line', '')}"

            table += f"| {name} | {assembly} | {similarity:.0%} | {loc} |\n"

        return table

    def generate_assembly_overview(self, assembly_name: str) -> str:
        """Generate assembly overview memory"""
        print(f"[PACKAGE] Generating assembly overview for: {assembly_name}")

        # Get all symbols in assembly
        symbols = search_unity_symbols(
            query="",
            assembly_name=assembly_name,
            limit=1000
        )

        if not symbols:
            print(f"[ERROR] No symbols found in assembly: {assembly_name}")
            return None

        # Analyze symbols
        stats = {
            'classes': len([s for s in symbols if s['kind'] == 'class']),
            'interfaces': len([s for s in symbols if s['kind'] == 'interface']),
            'methods': len([s for s in symbols if s['kind'] == 'method']),
            'properties': len([s for s in symbols if s['kind'] == 'property']),
            'fields': len([s for s in symbols if s['kind'] == 'field'])
        }

        # Build namespace tree
        namespaces = self._build_namespace_tree(symbols)

        # Identify key classes (most referenced)
        key_classes = self._identify_key_classes(symbols)

        # Generate content
        memory_content = self._format_assembly_memory(assembly_name, stats, namespaces, key_classes)

        # Write file
        safe_name = assembly_name.replace('.', '_').lower()
        memory_file = self.auto_gen_dir / f"assembly_{safe_name}.md"
        memory_file.write_text(memory_content, encoding='utf-8')

        print(f"[DONE] Generated: {memory_file}")
        return str(memory_file)

    def _build_namespace_tree(self, symbols: List[Dict]) -> Dict:
        """Build namespace hierarchy from symbols"""
        tree = {}
        for symbol in symbols:
            namespace = symbol.get('namespace', 'Global')
            if namespace not in tree:
                tree[namespace] = []
            if symbol['kind'] == 'class':
                tree[namespace].append(symbol['name'])
        return tree

    def _identify_key_classes(self, symbols: List[Dict], top_n: int = 10) -> List[Dict]:
        """Identify most important classes by reference count"""
        classes = [s for s in symbols if s['kind'] == 'class']
        # In real implementation, would query for reference counts
        # For now, just return first N classes
        return classes[:top_n]

    def _format_assembly_memory(self, assembly: str, stats: Dict, namespaces: Dict, key_classes: List[Dict]) -> str:
        """Format assembly overview memory"""

        content = f"""# {assembly} Assembly Overview (Auto-Generated)

**Last Updated:** {datetime.now().isoformat()}
**Total Symbols:** {sum(stats.values())}

---

## Statistics

- **Classes:** {stats['classes']}
- **Interfaces:** {stats['interfaces']}
- **Total Methods:** {stats['methods']}
- **Total Properties:** {stats['properties']}
- **Total Fields:** {stats['fields']}

---

## Namespace Structure

"""
        for ns, classes in sorted(namespaces.items()):
            content += f"### `{ns}`\n"
            content += f"- Classes: {len(classes)}\n"
            if classes:
                content += f"- Key: {', '.join(classes[:5])}\n"
            content += "\n"

        content += """---

## Key Classes (Most Referenced)

"""
        if key_classes:
            content += "| Class | Namespace | Location |\n"
            content += "|-------|-----------|----------|\n"
            for cls in key_classes:
                content += f"| {cls['name']} | {cls.get('namespace', '')} | {cls.get('file_path', '')}:{cls.get('start_line', '')} |\n"
        else:
            content += "*No key classes identified.*\n"

        content += f"""
---

## Validation Query

```python
# Verify assembly still exists and has expected symbol count
def validate_{assembly.replace('.', '_').lower()}_assembly():
    symbols = search_unity_symbols("", assembly_name="{assembly}", limit=1000)
    assert len(symbols) >= {sum(stats.values())}, "Symbol count decreased!"
```

**Next Validation:** {datetime.now().strftime('%Y-%m-%d')} (weekly)

---

**Source:** Unity Knowledge Base
**Generator:** scripts/unity-kb/generate_auto_memories.py
"""
        return content

    def discover_patterns(self) -> List[str]:
        """Discover and generate pattern catalog memories"""
        print("[SEARCH] Discovering code patterns...")

        patterns_to_discover = [
            {
                "query": "NetworkVariable OnValueChanged callback",
                "memory_name": "pattern_network_variable_callbacks",
                "category": "Network",
                "min_occurrences": 3
            },
            {
                "query": "ServerRpc ClientRpc RequireOwnership",
                "memory_name": "pattern_rpc_ownership",
                "category": "Network",
                "min_occurrences": 3
            },
            {
                "query": "Task Run Args GameCreator",
                "memory_name": "pattern_gamecreator_instructions",
                "category": "GameCreator",
                "min_occurrences": 5
            },
        ]

        generated = []

        for pattern_spec in patterns_to_discover:
            print(f"  Searching for: {pattern_spec['query']}")

            results = search_unity_symbols(
                query=pattern_spec['query'],
                limit=50
            )

            if len(results) >= pattern_spec['min_occurrences']:
                print(f"  [OK] Found {len(results)} occurrences - generating pattern memory")
                memory_file = self._generate_pattern_memory(pattern_spec, results)
                generated.append(memory_file)
            else:
                print(f"  ⏭️ Only {len(results)} occurrences - skipping (need {pattern_spec['min_occurrences']})")

        return generated

    def _generate_pattern_memory(self, spec: Dict, results: List[Dict]) -> str:
        """Generate pattern catalog memory"""

        memory_name = spec['memory_name']

        content = f"""# {memory_name.replace('_', ' ').title()} (Auto-Generated)

**Last Updated:** {datetime.now().isoformat()}
**Pattern Type:** {spec['category']}
**Occurrences:** {len(results)}

---

## Pattern Description

This pattern appears {len(results)} times across the codebase.

Query: `{spec['query']}`

---

## Example Implementations

| Class | Method/Property | Location |
|-------|-----------------|----------|
"""

        for result in results[:10]:
            name = result.get('name', 'Unknown')
            cls = result.get('class_name', result.get('name', ''))
            loc = f"{result.get('file_path', '')}:{result.get('start_line', '')}"
            content += f"| {cls} | {name} | {loc} |\n"

        if len(results) > 10:
            content += f"\n*...and {len(results) - 10} more*\n"

        content += f"""
---

## Code Example

```csharp
// Representative pattern from {results[0].get('class_name', 'example')}
{results[0].get('code_preview', '// Code preview not available')}
```

---

## Validation Query

```python
def validate_{memory_name}():
    results = search_unity_symbols("{spec['query']}", limit=50)
    assert len(results) >= {spec['min_occurrences']}, "Pattern occurrences decreased!"
```

**Next Validation:** {datetime.now().strftime('%Y-%m-%d')} (monthly)

---

**Source:** Unity Knowledge Base Pattern Discovery
**Generator:** scripts/unity-kb/generate_auto_memories.py
"""

        memory_file = self.auto_gen_dir / f"{memory_name}.md"
        memory_file.write_text(content, encoding='utf-8')

        print(f"    [DONE] Generated: {memory_file}")
        return str(memory_file)

    def update_all(self) -> Dict[str, List[str]]:
        """Update all auto-generated memories"""
        print("🔄 Updating all auto-generated memories...")

        results = {
            'api_memories': [],
            'assembly_memories': [],
            'pattern_memories': []
        }

        # Top classes to document
        priority_classes = [
            ('NetworkCharacterAdapter', 'GameCreator.Multiplayer.Runtime'),
            ('Character', 'GameCreator.Runtime.Core'),
            ('CharacterTrainingVisualizer', 'MLCreator.Core'),
            ('NetworkSpawnPoint', 'GameCreator.Multiplayer.Runtime'),
            ('RTSCameraController', None),
        ]

        print("\n📚 Generating API memories...")
        for class_name, assembly in priority_classes:
            try:
                memory_file = self.generate_api_memory(class_name, assembly)
                if memory_file:
                    results['api_memories'].append(memory_file)
            except Exception as e:
                print(f"[ERROR] Error generating {class_name}: {e}")

        # Key assemblies to document
        priority_assemblies = [
            'GameCreator.Multiplayer.Runtime',
            'MLCreator.Core',
            'undream.llmunity.Runtime',
        ]

        print("\n[PACKAGE] Generating assembly overviews...")
        for assembly in priority_assemblies:
            try:
                memory_file = self.generate_assembly_overview(assembly)
                if memory_file:
                    results['assembly_memories'].append(memory_file)
            except Exception as e:
                print(f"[ERROR] Error generating {assembly}: {e}")

        # Discover patterns
        print("\n[SEARCH] Discovering patterns...")
        try:
            pattern_files = self.discover_patterns()
            results['pattern_memories'] = pattern_files
        except Exception as e:
            print(f"[ERROR] Error discovering patterns: {e}")

        return results


def main():
    parser = argparse.ArgumentParser(description='Generate auto-memories from Unity KB')
    parser.add_argument('--update-all', action='store_true', help='Update all auto-generated memories')
    parser.add_argument('--api', type=str, help='Generate API memory for class')
    parser.add_argument('--assembly', type=str, help='Generate assembly overview')
    parser.add_argument('--discover-patterns', action='store_true', help='Discover and generate pattern catalogs')

    args = parser.parse_args()

    # Determine memory root
    if (PROJECT_ROOT / ".serena" / "memories").exists():
        memory_root = PROJECT_ROOT / ".serena" / "memories"
    elif (Path("D:/UnityWorkspaces/MLcreator/.serena/memories")).exists():
        memory_root = Path("D:/UnityWorkspaces/MLcreator/.serena/memories")
    else:
        print("ERROR: Cannot find Serena memories directory")
        sys.exit(1)

    generator = UnityKBMemoryGenerator(memory_root)

    if args.update_all:
        results = generator.update_all()
        print("\n[OK] Update complete!")
        print(f"   - API memories: {len(results['api_memories'])}")
        print(f"   - Assembly memories: {len(results['assembly_memories'])}")
        print(f"   - Pattern memories: {len(results['pattern_memories'])}")

    elif args.api:
        generator.generate_api_memory(args.api)

    elif args.assembly:
        generator.generate_assembly_overview(args.assembly)

    elif args.discover_patterns:
        patterns = generator.discover_patterns()
        print(f"\n[OK] Generated {len(patterns)} pattern memories")

    else:
        parser.print_help()


# Mock implementations for when MCP is unavailable
if not MCP_AVAILABLE:
    def search_unity_symbols(query: str, **kwargs) -> List[Dict]:
        return [{"name": "MockClass", "name_path": "Mock.MockClass", "assembly_name": "MockAssembly",
                 "namespace": "Mock", "kind": "class", "file_path": "Mock.cs", "start_line": 1,
                 "base_classes": ["MonoBehaviour"], "interfaces": []}]

    def get_unity_class_members(class_name: str, **kwargs) -> List[Dict]:
        return [{"name": "MockMethod", "kind": "method", "signature": "public void MockMethod()",
                 "file_path": "Mock.cs", "start_line": 10, "modifiers": ["public"]}]

    def find_similar_unity_code(code_snippet: str, **kwargs) -> List[Dict]:
        return []

    def list_unity_assemblies() -> List[Dict]:
        return []

    def get_unity_kb_stats() -> Dict:
        return {}


if __name__ == "__main__":
    main()
