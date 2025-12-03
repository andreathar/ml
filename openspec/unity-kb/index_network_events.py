#!/usr/bin/env python3
"""
Network Event & Trigger Indexer for Unity KB
=============================================

Indexes all network-related events, RPCs, triggers, and sync mechanisms
into the Qdrant Knowledge Base for unified event registry auditing.

Categories indexed:
- Event declarations (event Action<>)
- Event triggers (.Invoke(), EventBus.Fire())
- RPC definitions (ServerRpc, ClientRpc)
- Network spawn chains (OnNetworkSpawn/Despawn)
- Static events (public static event)

Author: MLCreator AI Assistant
Version: 1.0
"""

import os
import re
import json
import hashlib
from pathlib import Path
from typing import Dict, List, Optional, Any
from dataclasses import dataclass, asdict
from datetime import datetime

try:
    from qdrant_client import QdrantClient
    from qdrant_client.models import PointStruct, VectorParams, Distance
    QDRANT_AVAILABLE = True
except ImportError:
    QDRANT_AVAILABLE = False
    print("Warning: qdrant_client not installed. Will export to JSON only.")

# Configuration
QDRANT_HOST = "localhost"
QDRANT_PORT = 6333
EVENTS_COLLECTION = "mlcreator_network_events"
PROJECT_ROOT = Path(__file__).parent.parent.parent
MULTIPLAYER_PATH = PROJECT_ROOT / "Assets" / "Plugins" / "GameCreator" / "Packages" / "MLCreator_Multiplayer"


@dataclass
class EventDeclaration:
    """Represents an event declaration"""
    event_name: str
    event_type: str  # instance, static
    parameters: str
    file_path: str
    line_number: int
    class_name: str
    namespace: str
    category: str  # spawn, connection, input, sync, rpc, etc.
    full_signature: str


@dataclass
class EventTrigger:
    """Represents an event trigger/fire location"""
    event_name: str
    trigger_type: str  # invoke, eventbus_fire, direct_call
    file_path: str
    line_number: int
    method_name: str
    class_name: str
    context: str  # surrounding code context
    is_authority: bool  # whether this is the designated authority


@dataclass
class RPCDefinition:
    """Represents an RPC method definition"""
    method_name: str
    rpc_type: str  # ServerRpc, ClientRpc
    file_path: str
    line_number: int
    class_name: str
    parameters: str
    attributes: str
    namespace: str


@dataclass
class NetworkSpawnChain:
    """Represents OnNetworkSpawn/Despawn override"""
    method_name: str  # OnNetworkSpawn or OnNetworkDespawn
    file_path: str
    line_number: int
    class_name: str
    fires_events: List[str]
    namespace: str


class NetworkEventIndexer:
    """Indexes network events and triggers into Qdrant"""

    def __init__(self):
        self.events: List[EventDeclaration] = []
        self.triggers: List[EventTrigger] = []
        self.rpcs: List[RPCDefinition] = []
        self.spawn_chains: List[NetworkSpawnChain] = []
        self.client: Optional[QdrantClient] = None

        if QDRANT_AVAILABLE:
            try:
                self.client = QdrantClient(host=QDRANT_HOST, port=QDRANT_PORT, timeout=60)
            except Exception as e:
                print(f"Warning: Could not connect to Qdrant: {e}")

    def scan_directory(self, root_path: Path = None):
        """Scan all C# files in the directory"""
        if root_path is None:
            root_path = MULTIPLAYER_PATH

        if not root_path.exists():
            print(f"Error: Path does not exist: {root_path}")
            return

        print(f"Scanning: {root_path}")
        cs_files = list(root_path.rglob("*.cs"))
        print(f"Found {len(cs_files)} C# files")

        for cs_file in cs_files:
            self._process_file(cs_file)

        print(f"\nScan Results:")
        print(f"  Event Declarations: {len(self.events)}")
        print(f"  Event Triggers: {len(self.triggers)}")
        print(f"  RPC Definitions: {len(self.rpcs)}")
        print(f"  Network Spawn Chains: {len(self.spawn_chains)}")

    def _process_file(self, file_path: Path):
        """Process a single C# file"""
        try:
            content = file_path.read_text(encoding='utf-8')
        except Exception:
            try:
                content = file_path.read_text(encoding='utf-8-sig')
            except Exception as e:
                print(f"  Warning: Could not read {file_path}: {e}")
                return

        relative_path = str(file_path.relative_to(PROJECT_ROOT))
        lines = content.split('\n')

        # Extract namespace
        namespace = self._extract_namespace(content)

        # Extract class name(s)
        class_names = self._extract_classes(content)
        current_class = class_names[0] if class_names else "Unknown"

        # Find event declarations
        self._find_event_declarations(lines, relative_path, namespace, current_class)

        # Find event triggers
        self._find_event_triggers(lines, relative_path, current_class)

        # Find RPC definitions
        self._find_rpc_definitions(lines, relative_path, namespace, current_class)

        # Find network spawn chains
        self._find_spawn_chains(lines, relative_path, namespace, current_class)

    def _extract_namespace(self, content: str) -> str:
        """Extract namespace from C# file"""
        match = re.search(r'namespace\s+([\w.]+)', content)
        return match.group(1) if match else "Global"

    def _extract_classes(self, content: str) -> List[str]:
        """Extract class names from C# file"""
        matches = re.findall(r'(?:public|internal|private)?\s*(?:partial\s+)?class\s+(\w+)', content)
        return matches if matches else ["Unknown"]

    def _categorize_event(self, event_name: str) -> str:
        """Categorize event based on name patterns"""
        name_lower = event_name.lower()
        if any(x in name_lower for x in ['spawn', 'despawn', 'player']):
            return "spawn"
        elif any(x in name_lower for x in ['connect', 'disconnect', 'join', 'left']):
            return "connection"
        elif any(x in name_lower for x in ['input', 'move', 'look', 'action']):
            return "input"
        elif any(x in name_lower for x in ['sync', 'variable', 'change']):
            return "sync"
        elif any(x in name_lower for x in ['rpc', 'message']):
            return "rpc"
        elif any(x in name_lower for x in ['health', 'mana', 'stat', 'damage']):
            return "stats"
        elif any(x in name_lower for x in ['inventory', 'item', 'equip']):
            return "inventory"
        elif any(x in name_lower for x in ['collision', 'trigger']):
            return "physics"
        elif any(x in name_lower for x in ['gesture', 'animation']):
            return "animation"
        else:
            return "general"

    def _find_event_declarations(self, lines: List[str], file_path: str, namespace: str, class_name: str):
        """Find all event declarations"""
        # Pattern for instance events
        instance_pattern = re.compile(
            r'public\s+event\s+(Action<[^>]+>|Action)\s*\??\s+(\w+)'
        )
        # Pattern for static events
        static_pattern = re.compile(
            r'public\s+static\s+event\s+(System\.Action<[^>]+>|System\.Action|Action<[^>]+>|Action)\s+(\w+)'
        )

        for i, line in enumerate(lines):
            line_num = i + 1

            # Check instance events
            match = instance_pattern.search(line)
            if match:
                params = match.group(1)
                name = match.group(2)
                self.events.append(EventDeclaration(
                    event_name=name,
                    event_type="instance",
                    parameters=params,
                    file_path=file_path,
                    line_number=line_num,
                    class_name=class_name,
                    namespace=namespace,
                    category=self._categorize_event(name),
                    full_signature=line.strip()
                ))
                continue

            # Check static events
            match = static_pattern.search(line)
            if match:
                params = match.group(1)
                name = match.group(2)
                self.events.append(EventDeclaration(
                    event_name=name,
                    event_type="static",
                    parameters=params,
                    file_path=file_path,
                    line_number=line_num,
                    class_name=class_name,
                    namespace=namespace,
                    category=self._categorize_event(name),
                    full_signature=line.strip()
                ))

    def _find_event_triggers(self, lines: List[str], file_path: str, class_name: str):
        """Find all event trigger locations"""
        # Pattern for .Invoke() calls
        invoke_pattern = re.compile(r'(\w+)\?\s*\.Invoke\((.*?)\)')
        # Pattern for EventBus.Fire()
        eventbus_pattern = re.compile(r'EventBus\.Fire\((.*?)\)')
        # Pattern for direct event invocation
        direct_pattern = re.compile(r'(\w+)\s*\?\.\s*Invoke\((.*?)\)')

        for i, line in enumerate(lines):
            line_num = i + 1

            # Get context (surrounding code)
            start = max(0, i - 2)
            end = min(len(lines), i + 3)
            context = '\n'.join(lines[start:end])

            # Extract method name from context
            method_name = self._extract_current_method(lines, i)

            # Check .Invoke() calls
            match = invoke_pattern.search(line)
            if match:
                event_name = match.group(1)
                self.triggers.append(EventTrigger(
                    event_name=event_name,
                    trigger_type="invoke",
                    file_path=file_path,
                    line_number=line_num,
                    method_name=method_name,
                    class_name=class_name,
                    context=context,
                    is_authority=False  # Will be determined during analysis
                ))
                continue

            # Check EventBus.Fire()
            match = eventbus_pattern.search(line)
            if match:
                event_data = match.group(1)
                # Extract event type from constructor
                event_type_match = re.search(r'new\s+(\w+)', event_data)
                event_name = event_type_match.group(1) if event_type_match else "Unknown"
                self.triggers.append(EventTrigger(
                    event_name=event_name,
                    trigger_type="eventbus_fire",
                    file_path=file_path,
                    line_number=line_num,
                    method_name=method_name,
                    class_name=class_name,
                    context=context,
                    is_authority=False
                ))

    def _find_rpc_definitions(self, lines: List[str], file_path: str, namespace: str, class_name: str):
        """Find all RPC method definitions"""
        server_rpc_pattern = re.compile(r'\[ServerRpc[^\]]*\]')
        client_rpc_pattern = re.compile(r'\[ClientRpc[^\]]*\]')
        method_pattern = re.compile(r'(?:private|public|protected)?\s*(?:void|Task|async\s+Task)\s+(\w+)\s*\((.*?)\)')

        for i, line in enumerate(lines):
            line_num = i + 1

            # Check for ServerRpc
            if server_rpc_pattern.search(line):
                # Look for method on next lines
                for j in range(i + 1, min(i + 3, len(lines))):
                    method_match = method_pattern.search(lines[j])
                    if method_match:
                        self.rpcs.append(RPCDefinition(
                            method_name=method_match.group(1),
                            rpc_type="ServerRpc",
                            file_path=file_path,
                            line_number=j + 1,
                            class_name=class_name,
                            parameters=method_match.group(2),
                            attributes=line.strip(),
                            namespace=namespace
                        ))
                        break

            # Check for ClientRpc
            if client_rpc_pattern.search(line):
                # Look for method on next lines
                for j in range(i + 1, min(i + 3, len(lines))):
                    method_match = method_pattern.search(lines[j])
                    if method_match:
                        self.rpcs.append(RPCDefinition(
                            method_name=method_match.group(1),
                            rpc_type="ClientRpc",
                            file_path=file_path,
                            line_number=j + 1,
                            class_name=class_name,
                            parameters=method_match.group(2),
                            attributes=line.strip(),
                            namespace=namespace
                        ))
                        break

    def _find_spawn_chains(self, lines: List[str], file_path: str, namespace: str, class_name: str):
        """Find OnNetworkSpawn/Despawn override chains"""
        spawn_pattern = re.compile(r'public\s+override\s+void\s+(OnNetworkSpawn|OnNetworkDespawn)')

        for i, line in enumerate(lines):
            line_num = i + 1

            match = spawn_pattern.search(line)
            if match:
                method_name = match.group(1)

                # Look for events fired within this method (next ~50 lines)
                fired_events = []
                brace_count = 0
                in_method = False

                for j in range(i, min(i + 100, len(lines))):
                    method_line = lines[j]

                    if '{' in method_line:
                        brace_count += method_line.count('{')
                        in_method = True
                    if '}' in method_line:
                        brace_count -= method_line.count('}')

                    if in_method and brace_count == 0:
                        break

                    # Look for event fires
                    if 'EventBus.Fire' in method_line:
                        event_match = re.search(r'new\s+(\w+)', method_line)
                        if event_match:
                            fired_events.append(event_match.group(1))
                    if '.Invoke(' in method_line:
                        invoke_match = re.search(r'(\w+)\?\s*\.Invoke', method_line)
                        if invoke_match:
                            fired_events.append(invoke_match.group(1))

                self.spawn_chains.append(NetworkSpawnChain(
                    method_name=method_name,
                    file_path=file_path,
                    line_number=line_num,
                    class_name=class_name,
                    fires_events=fired_events,
                    namespace=namespace
                ))

    def _extract_current_method(self, lines: List[str], current_line: int) -> str:
        """Extract the method name containing the current line"""
        method_pattern = re.compile(r'(?:private|public|protected|internal)?\s*(?:static\s+)?(?:async\s+)?(?:void|Task|bool|int|string|[\w<>]+)\s+(\w+)\s*\(')

        for i in range(current_line, -1, -1):
            match = method_pattern.search(lines[i])
            if match:
                return match.group(1)
        return "Unknown"

    def generate_id(self, item_type: str, item_data: dict) -> str:
        """Generate unique ID for an item"""
        key = f"{item_type}:{item_data.get('file_path', '')}:{item_data.get('line_number', 0)}"
        return hashlib.md5(key.encode()).hexdigest()

    def export_to_json(self, output_path: Path = None):
        """Export all indexed data to JSON"""
        if output_path is None:
            output_path = PROJECT_ROOT / "claudedocs" / "reports" / "network_events_index.json"

        output_path.parent.mkdir(parents=True, exist_ok=True)

        data = {
            "generated_at": datetime.now().isoformat(),
            "summary": {
                "total_events": len(self.events),
                "total_triggers": len(self.triggers),
                "total_rpcs": len(self.rpcs),
                "total_spawn_chains": len(self.spawn_chains)
            },
            "categories": self._get_category_summary(),
            "event_declarations": [asdict(e) for e in self.events],
            "event_triggers": [asdict(t) for t in self.triggers],
            "rpc_definitions": [asdict(r) for r in self.rpcs],
            "spawn_chains": [asdict(s) for s in self.spawn_chains],
            "duplicate_trigger_analysis": self._analyze_duplicate_triggers()
        }

        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2)

        print(f"\nExported to: {output_path}")
        return output_path

    def _get_category_summary(self) -> Dict[str, int]:
        """Get summary of events by category"""
        categories = {}
        for event in self.events:
            cat = event.category
            categories[cat] = categories.get(cat, 0) + 1
        return categories

    def _analyze_duplicate_triggers(self) -> Dict[str, Any]:
        """Analyze which events are triggered from multiple locations"""
        trigger_sources = {}

        for trigger in self.triggers:
            name = trigger.event_name
            if name not in trigger_sources:
                trigger_sources[name] = []
            trigger_sources[name].append({
                "file": trigger.file_path,
                "line": trigger.line_number,
                "method": trigger.method_name,
                "type": trigger.trigger_type
            })

        duplicates = {
            name: sources
            for name, sources in trigger_sources.items()
            if len(sources) > 1
        }

        return {
            "events_with_multiple_triggers": len(duplicates),
            "duplicates": duplicates
        }

    def index_to_qdrant(self):
        """Index all data to Qdrant"""
        if not self.client:
            print("Qdrant client not available. Skipping Qdrant indexing.")
            return

        print(f"\nIndexing to Qdrant collection: {EVENTS_COLLECTION}")

        # Check if collection exists, create if not
        collections = self.client.get_collections().collections
        collection_names = [c.name for c in collections]

        if EVENTS_COLLECTION not in collection_names:
            print(f"Creating collection: {EVENTS_COLLECTION}")
            # Using dummy vector since we're doing metadata-based filtering
            self.client.create_collection(
                collection_name=EVENTS_COLLECTION,
                vectors_config=VectorParams(size=4, distance=Distance.COSINE)
            )

        points = []

        # Index event declarations
        for event in self.events:
            point_id = self.generate_id("event", asdict(event))
            points.append(PointStruct(
                id=point_id,
                vector=[0.0, 0.0, 0.0, 0.0],  # Dummy vector
                payload={
                    "type": "event_declaration",
                    "event_name": event.event_name,
                    "event_type": event.event_type,
                    "parameters": event.parameters,
                    "file_path": event.file_path,
                    "line_number": event.line_number,
                    "class_name": event.class_name,
                    "namespace": event.namespace,
                    "category": event.category,
                    "full_signature": event.full_signature,
                    "keywords": f"event {event.category} {event.event_name} {event.class_name}"
                }
            ))

        # Index event triggers
        for trigger in self.triggers:
            point_id = self.generate_id("trigger", asdict(trigger))
            points.append(PointStruct(
                id=point_id,
                vector=[0.0, 0.0, 0.0, 0.0],
                payload={
                    "type": "event_trigger",
                    "event_name": trigger.event_name,
                    "trigger_type": trigger.trigger_type,
                    "file_path": trigger.file_path,
                    "line_number": trigger.line_number,
                    "method_name": trigger.method_name,
                    "class_name": trigger.class_name,
                    "is_authority": trigger.is_authority,
                    "keywords": f"trigger fire invoke {trigger.event_name} {trigger.class_name}"
                }
            ))

        # Index RPC definitions
        for rpc in self.rpcs:
            point_id = self.generate_id("rpc", asdict(rpc))
            points.append(PointStruct(
                id=point_id,
                vector=[0.0, 0.0, 0.0, 0.0],
                payload={
                    "type": "rpc_definition",
                    "method_name": rpc.method_name,
                    "rpc_type": rpc.rpc_type,
                    "file_path": rpc.file_path,
                    "line_number": rpc.line_number,
                    "class_name": rpc.class_name,
                    "parameters": rpc.parameters,
                    "namespace": rpc.namespace,
                    "keywords": f"rpc {rpc.rpc_type} {rpc.method_name} {rpc.class_name} network"
                }
            ))

        # Index spawn chains
        for chain in self.spawn_chains:
            point_id = self.generate_id("spawn", asdict(chain))
            points.append(PointStruct(
                id=point_id,
                vector=[0.0, 0.0, 0.0, 0.0],
                payload={
                    "type": "spawn_chain",
                    "method_name": chain.method_name,
                    "file_path": chain.file_path,
                    "line_number": chain.line_number,
                    "class_name": chain.class_name,
                    "fires_events": chain.fires_events,
                    "namespace": chain.namespace,
                    "keywords": f"spawn despawn network lifecycle {chain.class_name} {' '.join(chain.fires_events)}"
                }
            ))

        # Upload in batches
        batch_size = 100
        for i in range(0, len(points), batch_size):
            batch = points[i:i + batch_size]
            self.client.upsert(
                collection_name=EVENTS_COLLECTION,
                points=batch
            )
            print(f"  Indexed {min(i + batch_size, len(points))}/{len(points)} points")

        print(f"\nTotal points indexed: {len(points)}")

    def print_duplicate_analysis(self):
        """Print analysis of duplicate triggers"""
        analysis = self._analyze_duplicate_triggers()

        print("\n" + "=" * 60)
        print("DUPLICATE TRIGGER ANALYSIS")
        print("=" * 60)

        if not analysis["duplicates"]:
            print("No duplicate triggers found.")
            return

        print(f"\nEvents with multiple trigger sources: {analysis['events_with_multiple_triggers']}")
        print("-" * 60)

        for event_name, sources in analysis["duplicates"].items():
            print(f"\n{event_name} ({len(sources)} sources):")
            for src in sources:
                print(f"  - {src['file']}:{src['line']}")
                print(f"    Method: {src['method']}, Type: {src['type']}")


def main():
    """Main entry point"""
    print("=" * 60)
    print("Network Event & Trigger Indexer")
    print("=" * 60)

    indexer = NetworkEventIndexer()

    # Scan multiplayer code
    indexer.scan_directory()

    # Print duplicate analysis
    indexer.print_duplicate_analysis()

    # Export to JSON
    json_path = indexer.export_to_json()

    # Index to Qdrant
    indexer.index_to_qdrant()

    print("\n" + "=" * 60)
    print("Indexing Complete!")
    print("=" * 60)


if __name__ == "__main__":
    main()
