#!/usr/bin/env python3
"""
Project Indexer for MLcreator Unity Knowledge Base
===================================================

Indexes all code and documentation in the project into Qdrant
with both dense (semantic) and sparse (keyword) vectors.

Usage:
    python index_project.py                    # Index all
    python index_project.py --code-only        # Index code only
    python index_project.py --docs-only        # Index docs only
    python index_project.py --incremental      # Only index changed files
"""

import os
import re
import sys
import uuid
import hashlib
import json
from pathlib import Path
from typing import Dict, List, Optional, Any, Generator, Tuple
from dataclasses import dataclass, asdict
from datetime import datetime
import fnmatch

from qdrant_client import QdrantClient
from qdrant_client.models import PointStruct, SparseVector

# Add project root to path
PROJECT_ROOT = Path(__file__).parent.parent.parent
sys.path.insert(0, str(PROJECT_ROOT))

from scripts.unity_kb.qdrant_config import (
    QDRANT_HOST, QDRANT_PORT, QDRANT_GRPC_PORT,
    COLLECTION_UNIFIED, DENSE_VECTOR_SIZE,
    IndexingConfig, CodeType, DocType, AssemblyCategory,
    CodePayload, DocPayload,
)
from scripts.unity_kb.hybrid_search import BM25Tokenizer


# =============================================================================
# CODE PARSER
# =============================================================================

@dataclass
class ParsedSymbol:
    """A parsed C# code symbol"""
    name: str
    name_path: str
    code_type: str
    namespace: str
    class_name: str
    file_path: str
    start_line: int
    end_line: int
    signature: str
    code_preview: str
    documentation: str
    modifiers: List[str]
    attributes: List[str]
    base_classes: List[str]
    interfaces: List[str]
    assembly_name: str
    is_network: bool


class CSharpParser:
    """
    Simple C# parser for extracting symbols.

    For production, consider using:
    - tree-sitter with C# grammar
    - Roslyn (via dotnet)
    - OmniSharp
    """

    def __init__(self, config: IndexingConfig = None):
        self.config = config or IndexingConfig()

        # Regex patterns for C# parsing
        self.namespace_pattern = re.compile(r'namespace\s+([\w.]+)')
        self.class_pattern = re.compile(
            r'(?P<attrs>(?:\[[\w\s,()="\']+\]\s*)*)'
            r'(?P<mods>(?:public|private|protected|internal|static|abstract|sealed|partial)\s+)*'
            r'(?P<type>class|struct|interface|enum)\s+'
            r'(?P<name>\w+)'
            r'(?:\s*<[\w,\s]+>)?'  # Generics
            r'(?:\s*:\s*(?P<bases>[\w\s,.<>]+))?'
        )
        self.method_pattern = re.compile(
            r'(?P<attrs>(?:\[[\w\s,()="\']+\]\s*)*)'
            r'(?P<mods>(?:public|private|protected|internal|static|virtual|override|abstract|async)\s+)*'
            r'(?P<return>[\w<>\[\],\s?]+)\s+'
            r'(?P<name>\w+)\s*'
            r'(?:<[\w,\s]+>)?\s*'  # Generics
            r'\((?P<params>[^)]*)\)'
        )
        self.property_pattern = re.compile(
            r'(?P<mods>(?:public|private|protected|internal|static|virtual|override)\s+)*'
            r'(?P<type>[\w<>\[\],\s?]+)\s+'
            r'(?P<name>\w+)\s*'
            r'(?:\{|=>)'
        )
        self.xml_doc_pattern = re.compile(r'///\s*(.+)')

    def parse_file(self, file_path: Path) -> List[ParsedSymbol]:
        """Parse a C# file and extract all symbols"""
        try:
            content = file_path.read_text(encoding='utf-8', errors='ignore')
        except Exception as e:
            print(f"[ERROR] Failed to read {file_path}: {e}")
            return []

        symbols = []
        lines = content.split('\n')

        # Extract namespace
        namespace = ""
        ns_match = self.namespace_pattern.search(content)
        if ns_match:
            namespace = ns_match.group(1)

        # Determine assembly from path
        assembly_name = self._guess_assembly(file_path)

        # Check if file is network-related
        is_network_file = self._is_network_code(content)

        # Parse classes/structs/interfaces/enums
        current_class = ""
        for match in self.class_pattern.finditer(content):
            symbol = self._create_type_symbol(
                match, content, lines, file_path, namespace,
                assembly_name, is_network_file
            )
            if symbol:
                symbols.append(symbol)
                current_class = symbol.name

        # Parse methods (simplified - doesn't handle nesting properly)
        for match in self.method_pattern.finditer(content):
            # Skip if it's a constructor-like pattern inside a class
            symbol = self._create_method_symbol(
                match, content, lines, file_path, namespace,
                current_class, assembly_name, is_network_file
            )
            if symbol:
                symbols.append(symbol)

        return symbols

    def _create_type_symbol(
        self, match, content: str, lines: List[str],
        file_path: Path, namespace: str, assembly_name: str,
        is_network: bool
    ) -> Optional[ParsedSymbol]:
        """Create a symbol for a class/struct/interface/enum"""
        name = match.group('name')
        type_kind = match.group('type')
        mods = match.group('mods') or ""
        attrs = match.group('attrs') or ""
        bases = match.group('bases') or ""

        # Get line number
        start_pos = match.start()
        start_line = content[:start_pos].count('\n') + 1

        # Extract documentation
        doc = self._extract_xml_doc(lines, start_line - 1)

        # Parse base classes and interfaces
        base_classes = []
        interfaces = []
        if bases:
            for base in bases.split(','):
                base = base.strip()
                if base.startswith('I') and base[1].isupper():
                    interfaces.append(base)
                else:
                    base_classes.append(base)

        # Code preview
        end_line = self._find_block_end(lines, start_line - 1)
        code_preview = '\n'.join(lines[start_line-1:min(start_line+20, end_line)])

        # Map type to CodeType
        code_type_map = {
            'class': CodeType.CLASS.value,
            'struct': CodeType.STRUCT.value,
            'interface': CodeType.INTERFACE.value,
            'enum': CodeType.ENUM.value,
        }

        return ParsedSymbol(
            name=name,
            name_path=f"{namespace}.{name}" if namespace else name,
            code_type=code_type_map.get(type_kind, CodeType.CLASS.value),
            namespace=namespace,
            class_name="",
            file_path=str(file_path.relative_to(PROJECT_ROOT)),
            start_line=start_line,
            end_line=end_line,
            signature=f"{mods}{type_kind} {name}",
            code_preview=code_preview[:1000],
            documentation=doc,
            modifiers=[m.strip() for m in mods.split() if m.strip()],
            attributes=self._parse_attributes(attrs),
            base_classes=base_classes,
            interfaces=interfaces,
            assembly_name=assembly_name,
            is_network=is_network or self._is_network_symbol(name, base_classes, interfaces),
        )

    def _create_method_symbol(
        self, match, content: str, lines: List[str],
        file_path: Path, namespace: str, class_name: str,
        assembly_name: str, is_network: bool
    ) -> Optional[ParsedSymbol]:
        """Create a symbol for a method"""
        name = match.group('name')
        mods = match.group('mods') or ""
        return_type = match.group('return')
        params = match.group('params')

        # Skip if it looks like a variable declaration
        if not params and '(' not in match.group(0):
            return None

        start_pos = match.start()
        start_line = content[:start_pos].count('\n') + 1

        doc = self._extract_xml_doc(lines, start_line - 1)
        signature = f"{return_type} {name}({params})"

        # Determine code type
        code_type = CodeType.METHOD.value
        if name.endswith("ServerRpc"):
            code_type = "serverrpc"
            is_network = True
        elif name.endswith("ClientRpc"):
            code_type = "clientrpc"
            is_network = True

        return ParsedSymbol(
            name=name,
            name_path=f"{namespace}.{class_name}.{name}" if class_name else f"{namespace}.{name}",
            code_type=code_type,
            namespace=namespace,
            class_name=class_name,
            file_path=str(file_path.relative_to(PROJECT_ROOT)),
            start_line=start_line,
            end_line=start_line + 10,  # Approximate
            signature=signature,
            code_preview="",
            documentation=doc,
            modifiers=[m.strip() for m in mods.split() if m.strip()],
            attributes=[],
            base_classes=[],
            interfaces=[],
            assembly_name=assembly_name,
            is_network=is_network,
        )

    def _extract_xml_doc(self, lines: List[str], line_num: int) -> str:
        """Extract XML documentation comments above a line"""
        doc_lines = []
        i = line_num - 1
        while i >= 0 and i > line_num - 20:
            line = lines[i].strip()
            if line.startswith('///'):
                doc_lines.insert(0, self.xml_doc_pattern.sub(r'\1', line))
            elif not line or line.startswith('['):
                i -= 1
                continue
            else:
                break
            i -= 1
        return '\n'.join(doc_lines)

    def _find_block_end(self, lines: List[str], start: int) -> int:
        """Find the end of a code block"""
        brace_count = 0
        started = False
        for i in range(start, min(start + 500, len(lines))):
            line = lines[i]
            brace_count += line.count('{') - line.count('}')
            if '{' in line:
                started = True
            if started and brace_count <= 0:
                return i + 1
        return min(start + 100, len(lines))

    def _parse_attributes(self, attrs: str) -> List[str]:
        """Parse attribute annotations"""
        if not attrs:
            return []
        return re.findall(r'\[(\w+)', attrs)

    def _guess_assembly(self, file_path: Path) -> str:
        """Guess assembly name from file path"""
        path_str = str(file_path)

        # Look for .asmdef in parent directories
        for parent in file_path.parents:
            asmdef_files = list(parent.glob("*.asmdef"))
            if asmdef_files:
                # Parse asmdef to get assembly name
                try:
                    asmdef_content = asmdef_files[0].read_text()
                    asmdef_data = json.loads(asmdef_content)
                    return asmdef_data.get("name", asmdef_files[0].stem)
                except:
                    return asmdef_files[0].stem

        # Fallback: guess from path
        if "GameCreator_Multiplayer" in path_str:
            if "Editor" in path_str:
                return "GameCreator.Multiplayer.Editor"
            return "GameCreator.Multiplayer.Runtime"
        if "GameCreator" in path_str:
            return "GameCreator.Runtime"
        if "Game/" in path_str:
            return "NexusConvergence.Runtime"

        return "Unknown"

    def _is_network_code(self, content: str) -> bool:
        """Check if content contains network-related code"""
        network_patterns = [
            'NetworkBehaviour', 'NetworkObject', 'NetworkVariable',
            'ServerRpc', 'ClientRpc', 'Netcode', 'NetworkManager',
            'using Unity.Netcode'
        ]
        return any(p in content for p in network_patterns)

    def _is_network_symbol(self, name: str, bases: List[str], interfaces: List[str]) -> bool:
        """Check if a symbol is network-related"""
        all_types = [name] + bases + interfaces
        network_keywords = ['Network', 'Rpc', 'Sync', 'Replicate', 'Multiplayer']
        return any(k in t for t in all_types for k in network_keywords)


# =============================================================================
# DOCUMENTATION PARSER
# =============================================================================

@dataclass
class ParsedDoc:
    """A parsed documentation file"""
    name: str
    doc_type: str
    source: str
    content: str
    summary: str
    file_path: str
    hierarchy: List[str]
    keywords: List[str]
    is_network: bool


class DocParser:
    """Parser for documentation files (Markdown, etc.)"""

    def __init__(self, config: IndexingConfig = None):
        self.config = config or IndexingConfig()

        self.title_pattern = re.compile(r'^#\s+(.+)$', re.MULTILINE)
        self.heading_pattern = re.compile(r'^(#{1,6})\s+(.+)$', re.MULTILINE)

    def parse_file(self, file_path: Path) -> List[ParsedDoc]:
        """Parse a documentation file"""
        try:
            content = file_path.read_text(encoding='utf-8', errors='ignore')
        except Exception as e:
            print(f"[ERROR] Failed to read {file_path}: {e}")
            return []

        # Extract title
        title_match = self.title_pattern.search(content)
        title = title_match.group(1) if title_match else file_path.stem

        # Determine doc type and source
        doc_type, source = self._classify_doc(file_path)

        # Extract summary (first paragraph after title)
        summary = self._extract_summary(content)

        # Extract keywords
        keywords = self._extract_keywords(content, file_path)

        # Build hierarchy
        hierarchy = self._build_hierarchy(file_path)

        # Check if network-related
        is_network = self._is_network_doc(content)

        # Create single document (or split into sections for large docs)
        docs = [ParsedDoc(
            name=title,
            doc_type=doc_type,
            source=source,
            content=content[:10000],  # Limit content size
            summary=summary,
            file_path=str(file_path.relative_to(PROJECT_ROOT)),
            hierarchy=hierarchy,
            keywords=keywords,
            is_network=is_network,
        )]

        return docs

    def _classify_doc(self, file_path: Path) -> Tuple[str, str]:
        """Classify document type and source"""
        path_str = str(file_path).lower()

        # Determine source
        if '.serena/memories' in path_str:
            source = 'serena'
        elif 'claudedocs' in path_str:
            source = 'claudedocs'
        elif 'openspec' in path_str:
            source = 'openspec'
        elif 'docs/' in path_str:
            source = 'docs'
        else:
            source = 'project'

        # Determine type
        if 'guide' in path_str:
            doc_type = DocType.GUIDE.value
        elif 'tutorial' in path_str:
            doc_type = DocType.TUTORIAL.value
        elif 'architecture' in path_str:
            doc_type = DocType.ARCHITECTURE.value
        elif 'pattern' in path_str:
            doc_type = DocType.PATTERN.value
        elif source == 'serena':
            doc_type = DocType.MEMORY.value
        else:
            doc_type = DocType.API_REFERENCE.value

        return doc_type, source

    def _extract_summary(self, content: str) -> str:
        """Extract summary from content"""
        # Skip title and get first paragraph
        lines = content.split('\n')
        summary_lines = []
        in_paragraph = False

        for line in lines:
            line = line.strip()
            if line.startswith('#'):
                continue
            if not line:
                if in_paragraph:
                    break
                continue
            in_paragraph = True
            summary_lines.append(line)
            if len(' '.join(summary_lines)) > 500:
                break

        return ' '.join(summary_lines)[:500]

    def _extract_keywords(self, content: str, file_path: Path) -> List[str]:
        """Extract keywords from content"""
        keywords = set()

        # From filename
        keywords.update(file_path.stem.lower().replace('_', ' ').replace('-', ' ').split())

        # From headings
        for match in self.heading_pattern.finditer(content):
            heading = match.group(2).lower()
            keywords.update(heading.replace('_', ' ').replace('-', ' ').split())

        # Code-related keywords
        code_keywords = re.findall(r'`(\w+)`', content)
        keywords.update(k.lower() for k in code_keywords if len(k) > 2)

        # Filter
        keywords = {k for k in keywords if len(k) > 2 and k.isalnum()}

        return list(keywords)[:50]

    def _build_hierarchy(self, file_path: Path) -> List[str]:
        """Build breadcrumb hierarchy from path"""
        rel_path = file_path.relative_to(PROJECT_ROOT)
        return [p for p in rel_path.parts[:-1] if p not in ['.', '..']]

    def _is_network_doc(self, content: str) -> bool:
        """Check if doc is network-related"""
        network_terms = ['network', 'multiplayer', 'rpc', 'sync', 'netcode', 'server', 'client']
        content_lower = content.lower()
        return any(t in content_lower for t in network_terms)


# =============================================================================
# INDEXER
# =============================================================================

class ProjectIndexer:
    """Index project code and documentation into Qdrant"""

    def __init__(
        self,
        host: str = QDRANT_HOST,
        port: int = QDRANT_PORT,
        collection_name: str = COLLECTION_UNIFIED,
        embedding_fn=None
    ):
        self.client = QdrantClient(
            host=host,
            port=port,
            grpc_port=QDRANT_GRPC_PORT,
            prefer_grpc=True,
            timeout=120
        )
        self.collection_name = collection_name
        self.embedding_fn = embedding_fn
        self.config = IndexingConfig()
        self.code_parser = CSharpParser(self.config)
        self.doc_parser = DocParser(self.config)
        self.tokenizer = BM25Tokenizer()

        # Stats
        self.stats = {
            'code_files': 0,
            'code_symbols': 0,
            'doc_files': 0,
            'doc_entries': 0,
            'errors': 0,
        }

    def index_all(self, incremental: bool = False):
        """Index all code and documentation"""
        print("="*60)
        print("MLcreator Project Indexer")
        print("="*60)

        # Index code
        print("\n[1/2] Indexing Code...")
        self.index_code()

        # Index documentation
        print("\n[2/2] Indexing Documentation...")
        self.index_docs()

        # Print stats
        self._print_stats()

    def index_code(self):
        """Index all C# code files"""
        points = []

        for code_path in self.config.code_paths:
            full_path = PROJECT_ROOT / code_path
            if not full_path.exists():
                continue

            for pattern in self.config.code_patterns:
                for file_path in full_path.rglob(pattern):
                    if self._should_exclude(file_path):
                        continue

                    self.stats['code_files'] += 1
                    symbols = self.code_parser.parse_file(file_path)

                    for symbol in symbols:
                        point = self._symbol_to_point(symbol)
                        if point:
                            points.append(point)
                            self.stats['code_symbols'] += 1

                    # Batch upsert
                    if len(points) >= self.config.index_batch_size:
                        self._upsert_points(points)
                        points = []

        # Final batch
        if points:
            self._upsert_points(points)

        print(f"   Indexed {self.stats['code_symbols']} symbols from {self.stats['code_files']} files")

    def index_docs(self):
        """Index all documentation files"""
        points = []

        for doc_path in self.config.doc_paths:
            full_path = PROJECT_ROOT / doc_path
            if not full_path.exists():
                continue

            for pattern in self.config.doc_patterns:
                for file_path in full_path.rglob(pattern):
                    if self._should_exclude(file_path):
                        continue

                    self.stats['doc_files'] += 1
                    docs = self.doc_parser.parse_file(file_path)

                    for doc in docs:
                        point = self._doc_to_point(doc)
                        if point:
                            points.append(point)
                            self.stats['doc_entries'] += 1

                    if len(points) >= self.config.index_batch_size:
                        self._upsert_points(points)
                        points = []

        if points:
            self._upsert_points(points)

        print(f"   Indexed {self.stats['doc_entries']} documents from {self.stats['doc_files']} files")

    def _symbol_to_point(self, symbol: ParsedSymbol) -> Optional[PointStruct]:
        """Convert parsed symbol to Qdrant point"""
        # Generate ID from name_path
        point_id = self._generate_id(f"code:{symbol.name_path}:{symbol.file_path}")

        # Build text for embedding
        embed_text = f"{symbol.name} {symbol.namespace} {symbol.signature} {symbol.documentation}"

        # Generate sparse vector
        sparse_indices, sparse_values = self.tokenizer.to_sparse_vector(embed_text)

        # Generate dense vector (if embedding function available)
        dense_vector = None
        if self.embedding_fn:
            dense_vector = self.embedding_fn(embed_text)
        else:
            # Placeholder - in production, use actual embeddings
            dense_vector = [0.0] * DENSE_VECTOR_SIZE

        # Determine assembly category
        assembly_category = self._categorize_assembly(symbol.assembly_name)

        # Build keywords
        keywords = self._extract_code_keywords(symbol)

        # Build payload
        payload = {
            "content_type": "code",
            "name": symbol.name,
            "name_path": symbol.name_path,
            "code_type": symbol.code_type,
            "assembly_name": symbol.assembly_name,
            "assembly_category": assembly_category,
            "namespace": symbol.namespace,
            "class_name": symbol.class_name,
            "file_path": symbol.file_path,
            "start_line": symbol.start_line,
            "end_line": symbol.end_line,
            "signature": symbol.signature,
            "code_preview": symbol.code_preview,
            "documentation": symbol.documentation,
            "modifiers": symbol.modifiers,
            "attributes": symbol.attributes,
            "base_classes": symbol.base_classes,
            "interfaces": symbol.interfaces,
            "keywords": keywords,
            "tags": [],
            "is_network": symbol.is_network,
            "is_deprecated": "Obsolete" in symbol.attributes,
            "has_documentation": bool(symbol.documentation),
            "relevance_score": 1.0,
            "indexed_at": datetime.utcnow().isoformat(),
        }

        return PointStruct(
            id=point_id,
            vector={
                "dense": dense_vector,
                "sparse": SparseVector(indices=sparse_indices, values=sparse_values),
            },
            payload=payload,
        )

    def _doc_to_point(self, doc: ParsedDoc) -> Optional[PointStruct]:
        """Convert parsed document to Qdrant point"""
        point_id = self._generate_id(f"doc:{doc.file_path}:{doc.name}")

        # Build text for embedding
        embed_text = f"{doc.name} {doc.summary} {' '.join(doc.keywords)}"

        # Generate vectors
        sparse_indices, sparse_values = self.tokenizer.to_sparse_vector(embed_text)

        dense_vector = None
        if self.embedding_fn:
            dense_vector = self.embedding_fn(embed_text)
        else:
            dense_vector = [0.0] * DENSE_VECTOR_SIZE

        payload = {
            "content_type": "doc",
            "name": doc.name,
            "doc_type": doc.doc_type,
            "source": doc.source,
            "content": doc.content,
            "summary": doc.summary,
            "file_path": doc.file_path,
            "hierarchy": doc.hierarchy,
            "keywords": doc.keywords,
            "tags": [],
            "is_network": doc.is_network,
            "is_deprecated": False,
            "relevance_score": 1.0,
            "indexed_at": datetime.utcnow().isoformat(),
        }

        return PointStruct(
            id=point_id,
            vector={
                "dense": dense_vector,
                "sparse": SparseVector(indices=sparse_indices, values=sparse_values),
            },
            payload=payload,
        )

    def _generate_id(self, content: str) -> str:
        """Generate deterministic ID from content"""
        return hashlib.sha256(content.encode()).hexdigest()[:32]

    def _should_exclude(self, file_path: Path) -> bool:
        """Check if file should be excluded"""
        path_str = str(file_path)
        for pattern in self.config.exclude_patterns:
            if fnmatch.fnmatch(path_str, pattern):
                return True
        return False

    def _categorize_assembly(self, assembly_name: str) -> str:
        """Categorize assembly"""
        for prefix, category in self.config.assembly_category_rules.items():
            if assembly_name.startswith(prefix):
                return category
        return AssemblyCategory.PROJECT_CODE.value

    def _extract_code_keywords(self, symbol: ParsedSymbol) -> List[str]:
        """Extract keywords from code symbol"""
        keywords = set()

        # From name (split camelCase)
        name_parts = re.findall(r'[A-Z][a-z]+|[a-z]+|[A-Z]+(?=[A-Z]|$)', symbol.name)
        keywords.update(p.lower() for p in name_parts)

        # From namespace
        ns_parts = symbol.namespace.split('.')
        keywords.update(p.lower() for p in ns_parts)

        # From base classes and interfaces
        for base in symbol.base_classes + symbol.interfaces:
            parts = re.findall(r'[A-Z][a-z]+|[a-z]+', base)
            keywords.update(p.lower() for p in parts)

        # From modifiers
        keywords.update(symbol.modifiers)

        # Network keywords
        if symbol.is_network:
            keywords.add("network")
            keywords.add("multiplayer")

        # Code type
        keywords.add(symbol.code_type)

        return list(keywords)[:30]

    def _upsert_points(self, points: List[PointStruct]):
        """Upsert points to Qdrant"""
        try:
            self.client.upsert(
                collection_name=self.collection_name,
                points=points,
                wait=True,
            )
        except Exception as e:
            print(f"[ERROR] Failed to upsert {len(points)} points: {e}")
            self.stats['errors'] += 1

    def _print_stats(self):
        """Print indexing statistics"""
        print("\n" + "="*60)
        print("INDEXING COMPLETE")
        print("="*60)
        print(f"  Code files processed:  {self.stats['code_files']}")
        print(f"  Code symbols indexed:  {self.stats['code_symbols']}")
        print(f"  Doc files processed:   {self.stats['doc_files']}")
        print(f"  Doc entries indexed:   {self.stats['doc_entries']}")
        print(f"  Total indexed:         {self.stats['code_symbols'] + self.stats['doc_entries']}")
        print(f"  Errors:                {self.stats['errors']}")


# =============================================================================
# CLI
# =============================================================================

def main():
    import argparse

    parser = argparse.ArgumentParser(description="Index MLcreator project into Qdrant")
    parser.add_argument("--code-only", action="store_true", help="Index code only")
    parser.add_argument("--docs-only", action="store_true", help="Index docs only")
    parser.add_argument("--incremental", action="store_true", help="Incremental indexing")
    parser.add_argument("--collection", default=COLLECTION_UNIFIED, help="Collection name")

    args = parser.parse_args()

    indexer = ProjectIndexer(collection_name=args.collection)

    if args.code_only:
        indexer.index_code()
        indexer._print_stats()
    elif args.docs_only:
        indexer.index_docs()
        indexer._print_stats()
    else:
        indexer.index_all(incremental=args.incremental)


if __name__ == "__main__":
    main()
