#!/usr/bin/env python3
"""
Qdrant Configuration for MLcreator Unity Knowledge Base
========================================================

Optimized hybrid search configuration combining:
- Dense vectors (semantic embeddings) for conceptual similarity
- Sparse vectors (BM25/SPLADE) for keyword matching
- Payload indexing for efficient filtering

Designed for indexing:
- C# source code (classes, methods, properties)
- Documentation (markdown, XML docs)
- GameCreator visual scripting components
- Unity Netcode multiplayer patterns
"""

from dataclasses import dataclass, field
from typing import Dict, List, Optional, Any
from enum import Enum

# =============================================================================
# CONFIGURATION CONSTANTS
# =============================================================================

# Qdrant Connection
QDRANT_HOST = "localhost"
QDRANT_PORT = 6333
QDRANT_GRPC_PORT = 6334  # For faster operations

# Collection Names
COLLECTION_CODE = "mlcreator_code"           # C# code symbols
COLLECTION_DOCS = "mlcreator_docs"           # Documentation
COLLECTION_PATTERNS = "mlcreator_patterns"   # Network patterns & recipes
COLLECTION_UNIFIED = "mlcreator_unified"     # Combined hybrid search

# Vector Dimensions
DENSE_VECTOR_SIZE = 1024      # For text-embedding-3-large or similar
DENSE_VECTOR_SIZE_SMALL = 384 # For smaller models (all-MiniLM-L6-v2)

# Hybrid Search Weights (tune based on use case)
DEFAULT_DENSE_WEIGHT = 0.7    # Semantic similarity
DEFAULT_SPARSE_WEIGHT = 0.3   # Keyword matching


class CodeType(Enum):
    """Types of code artifacts"""
    CLASS = "class"
    INTERFACE = "interface"
    STRUCT = "struct"
    ENUM = "enum"
    METHOD = "method"
    PROPERTY = "property"
    FIELD = "field"
    EVENT = "event"
    DELEGATE = "delegate"
    INSTRUCTION = "instruction"  # GameCreator
    CONDITION = "condition"      # GameCreator
    GC_EVENT = "gc_event"        # GameCreator Event


class DocType(Enum):
    """Types of documentation"""
    API_REFERENCE = "api_reference"
    GUIDE = "guide"
    TUTORIAL = "tutorial"
    ARCHITECTURE = "architecture"
    MEMORY = "memory"            # Serena memories
    PATTERN = "pattern"
    EXAMPLE = "example"


class AssemblyCategory(Enum):
    """Assembly categories for filtering"""
    UNITY_CORE = "unity_core"
    UNITY_NETCODE = "unity_netcode"
    GAMECREATOR_CORE = "gamecreator_core"
    GAMECREATOR_MODULE = "gamecreator_module"
    MLCREATOR_RUNTIME = "mlcreator_runtime"
    MLCREATOR_EDITOR = "mlcreator_editor"
    PROJECT_CODE = "project_code"


# =============================================================================
# COLLECTION SCHEMAS
# =============================================================================

@dataclass
class UnifiedCollectionConfig:
    """
    Configuration for the unified hybrid search collection.

    This is the recommended collection for most queries as it combines
    all content types with optimized hybrid search.
    """

    name: str = COLLECTION_UNIFIED

    # Vector configurations
    dense_vector_name: str = "dense"
    sparse_vector_name: str = "sparse"
    dense_size: int = DENSE_VECTOR_SIZE

    # Quantization for memory efficiency (reduces storage by ~4x)
    use_scalar_quantization: bool = True
    quantization_type: str = "int8"  # int8, binary, product

    # HNSW Index parameters (tune for quality vs speed)
    hnsw_m: int = 16              # Number of connections per node
    hnsw_ef_construct: int = 100  # Construction time quality
    hnsw_full_scan_threshold: int = 10000  # When to use brute force

    # On-disk settings for large collections
    on_disk_payload: bool = False  # Keep payload in memory for speed
    on_disk_vectors: bool = False  # Keep vectors in memory for speed

    # Shard configuration
    shard_number: int = 1         # Single shard for <100k points
    replication_factor: int = 1   # No replication for local dev

    # Payload index configuration
    payload_indexes: Dict[str, Dict] = field(default_factory=lambda: {
        # Text indexes for filtering
        "content_type": {"type": "keyword"},           # code, doc, pattern
        "code_type": {"type": "keyword"},              # class, method, etc.
        "assembly_name": {"type": "keyword"},          # Assembly name
        "assembly_category": {"type": "keyword"},      # Unity, GC, MLCreator
        "namespace": {"type": "keyword"},              # Full namespace
        "name": {"type": "text", "tokenizer": "word"}, # Symbol/doc name (searchable)
        "class_name": {"type": "keyword"},             # Parent class
        "file_path": {"type": "keyword"},              # Source file path

        # Keyword matching indexes
        "keywords": {"type": "keyword", "is_array": True},  # Searchable keywords
        "tags": {"type": "keyword", "is_array": True},      # User tags
        "modifiers": {"type": "keyword", "is_array": True}, # public, static, etc.

        # Numeric indexes for range queries
        "start_line": {"type": "integer"},
        "end_line": {"type": "integer"},
        "relevance_score": {"type": "float"},  # Pre-computed relevance

        # Boolean indexes
        "is_network": {"type": "bool"},        # Network-related code
        "is_deprecated": {"type": "bool"},     # Deprecated symbols
        "has_documentation": {"type": "bool"}, # Has XML docs
    })


@dataclass
class HybridSearchConfig:
    """Configuration for hybrid search queries"""

    # Fusion method: "rrf" (Reciprocal Rank Fusion) or "dbsf" (Distribution-Based Score Fusion)
    fusion_method: str = "rrf"

    # RRF parameters
    rrf_k: int = 60  # Smoothing constant (higher = more weight to lower ranks)

    # Score normalization
    normalize_scores: bool = True

    # Prefetch configuration for multi-stage retrieval
    dense_prefetch_limit: int = 100
    sparse_prefetch_limit: int = 100

    # Final limit
    final_limit: int = 20

    # Weights (can be adjusted per query)
    dense_weight: float = DEFAULT_DENSE_WEIGHT
    sparse_weight: float = DEFAULT_SPARSE_WEIGHT

    # Query-time HNSW parameters
    hnsw_ef: int = 128  # Search quality (higher = better but slower)

    # Timeout
    timeout_seconds: int = 30


# =============================================================================
# PAYLOAD SCHEMAS
# =============================================================================

@dataclass
class CodePayload:
    """Payload schema for code symbols"""

    # Required fields
    content_type: str = "code"
    name: str = ""
    name_path: str = ""  # Full path: Namespace.Class.Method

    # Classification
    code_type: str = ""  # From CodeType enum
    assembly_name: str = ""
    assembly_category: str = ""
    namespace: str = ""
    class_name: str = ""  # Parent class (for methods/properties)

    # Source location
    file_path: str = ""
    start_line: int = 0
    end_line: int = 0

    # Signature and code
    signature: str = ""
    code_preview: str = ""  # First ~500 chars of code
    documentation: str = ""  # XML doc comments

    # Modifiers and attributes
    modifiers: List[str] = field(default_factory=list)  # public, static, async
    attributes: List[str] = field(default_factory=list)  # [Serializable], etc.

    # Inheritance
    base_classes: List[str] = field(default_factory=list)
    interfaces: List[str] = field(default_factory=list)

    # Search optimization
    keywords: List[str] = field(default_factory=list)
    tags: List[str] = field(default_factory=list)

    # Flags
    is_network: bool = False
    is_deprecated: bool = False
    has_documentation: bool = False

    # Scoring
    relevance_score: float = 1.0


@dataclass
class DocPayload:
    """Payload schema for documentation"""

    # Required fields
    content_type: str = "doc"
    name: str = ""  # Document title

    # Classification
    doc_type: str = ""  # From DocType enum
    source: str = ""    # serena, claudedocs, unity_docs, etc.

    # Content
    content: str = ""       # Full content
    summary: str = ""       # First paragraph or explicit summary

    # Location
    file_path: str = ""
    url: str = ""           # External URL if applicable

    # Organization
    section: str = ""       # Parent section
    hierarchy: List[str] = field(default_factory=list)  # Breadcrumb path

    # Search optimization
    keywords: List[str] = field(default_factory=list)
    tags: List[str] = field(default_factory=list)
    related_code: List[str] = field(default_factory=list)  # Related code symbols

    # Flags
    is_network: bool = False
    is_deprecated: bool = False

    # Scoring
    relevance_score: float = 1.0


# =============================================================================
# EMBEDDING CONFIGURATION
# =============================================================================

@dataclass
class EmbeddingConfig:
    """Configuration for generating embeddings"""

    # Model selection
    model_name: str = "text-embedding-3-large"  # OpenAI
    # Alternatives:
    # - "text-embedding-3-small" (1536 dim, cheaper)
    # - "sentence-transformers/all-MiniLM-L6-v2" (384 dim, local)
    # - "BAAI/bge-large-en-v1.5" (1024 dim, local, good quality)
    # - "intfloat/e5-large-v2" (1024 dim, local)

    # Dimension
    dimension: int = DENSE_VECTOR_SIZE

    # Chunking for long content
    max_chunk_size: int = 512  # tokens
    chunk_overlap: int = 50    # tokens

    # Batching
    batch_size: int = 32

    # Local model settings (if using sentence-transformers)
    device: str = "cuda"  # cuda, cpu, mps
    normalize_embeddings: bool = True


@dataclass
class SparseEmbeddingConfig:
    """Configuration for sparse (keyword) embeddings"""

    # Model selection
    # Options:
    # - "bm25" - Classic BM25 (requires pre-built index)
    # - "splade" - Learned sparse (SPLADE models)
    # - "tfidf" - TF-IDF based
    model_name: str = "bm25"

    # BM25 parameters
    bm25_k1: float = 1.5   # Term frequency saturation
    bm25_b: float = 0.75   # Document length normalization

    # Vocabulary
    min_token_length: int = 2
    max_token_length: int = 50

    # Stop words
    remove_stop_words: bool = True
    custom_stop_words: List[str] = field(default_factory=lambda: [
        "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
        "of", "with", "by", "from", "as", "is", "was", "are", "were", "been",
        "be", "have", "has", "had", "do", "does", "did", "will", "would",
        "could", "should", "may", "might", "must", "shall", "can", "need",
        "this", "that", "these", "those", "it", "its", "they", "them", "their",
        "he", "she", "his", "her", "we", "our", "you", "your", "i", "my", "me"
    ])

    # Code-specific tokens to preserve
    preserve_tokens: List[str] = field(default_factory=lambda: [
        "rpc", "serverrpc", "clientrpc", "networkvariable", "networkobject",
        "networkbehaviour", "networkmanager", "netcode", "spawn", "despawn",
        "gamecreator", "instruction", "condition", "trigger", "character",
        "inventory", "stats", "traits", "perception", "behavior", "dialogue"
    ])


# =============================================================================
# INDEXING CONFIGURATION
# =============================================================================

@dataclass
class IndexingConfig:
    """Configuration for content indexing"""

    # Paths to index
    code_paths: List[str] = field(default_factory=lambda: [
        "Assets/Plugins/GameCreator_Multiplayer/",
        "Assets/Game/",
        "Assets/Scripts/",
    ])

    doc_paths: List[str] = field(default_factory=lambda: [
        "claudedocs/",
        ".serena/memories/",
        "docs/",
        "openspec/",
    ])

    # File patterns
    code_patterns: List[str] = field(default_factory=lambda: [
        "*.cs",
    ])

    doc_patterns: List[str] = field(default_factory=lambda: [
        "*.md",
        "*.txt",
    ])

    # Exclude patterns
    exclude_patterns: List[str] = field(default_factory=lambda: [
        "**/bin/**",
        "**/obj/**",
        "**/.git/**",
        "**/node_modules/**",
        "**/*.meta",
        "**/Library/**",
        "**/Temp/**",
    ])

    # Assembly categorization rules
    assembly_category_rules: Dict[str, str] = field(default_factory=lambda: {
        "UnityEngine": AssemblyCategory.UNITY_CORE.value,
        "Unity.Netcode": AssemblyCategory.UNITY_NETCODE.value,
        "GameCreator.Runtime.Common": AssemblyCategory.GAMECREATOR_CORE.value,
        "GameCreator.Runtime": AssemblyCategory.GAMECREATOR_MODULE.value,
        "GameCreator.Editor": AssemblyCategory.GAMECREATOR_MODULE.value,
        "GameCreator.Multiplayer.Runtime": AssemblyCategory.MLCREATOR_RUNTIME.value,
        "GameCreator.Multiplayer.Editor": AssemblyCategory.MLCREATOR_EDITOR.value,
        "NexusConvergence": AssemblyCategory.PROJECT_CODE.value,
    })

    # Keyword extraction
    extract_code_keywords: bool = True
    extract_doc_keywords: bool = True

    # Network detection patterns
    network_patterns: List[str] = field(default_factory=lambda: [
        "Network", "Rpc", "ServerRpc", "ClientRpc", "Netcode",
        "Multiplayer", "Sync", "Replicate", "Owner", "Spawn"
    ])

    # Batch size for indexing
    index_batch_size: int = 100

    # Update strategy
    update_strategy: str = "upsert"  # upsert, replace, skip_existing


# =============================================================================
# SEARCH PRESETS
# =============================================================================

@dataclass
class SearchPresets:
    """Pre-configured search settings for common use cases"""

    @staticmethod
    def code_search() -> Dict[str, Any]:
        """Optimized for finding code symbols"""
        return {
            "dense_weight": 0.6,
            "sparse_weight": 0.4,  # Higher keyword weight for code
            "filter": {"content_type": "code"},
            "prefetch_limit": 50,
            "final_limit": 20,
        }

    @staticmethod
    def semantic_search() -> Dict[str, Any]:
        """Optimized for conceptual/semantic queries"""
        return {
            "dense_weight": 0.85,
            "sparse_weight": 0.15,
            "prefetch_limit": 100,
            "final_limit": 20,
        }

    @staticmethod
    def keyword_search() -> Dict[str, Any]:
        """Optimized for exact keyword matching"""
        return {
            "dense_weight": 0.3,
            "sparse_weight": 0.7,
            "prefetch_limit": 100,
            "final_limit": 30,
        }

    @staticmethod
    def network_code_search() -> Dict[str, Any]:
        """Optimized for finding network/multiplayer code"""
        return {
            "dense_weight": 0.5,
            "sparse_weight": 0.5,
            "filter": {
                "must": [
                    {"key": "content_type", "match": {"value": "code"}},
                    {"key": "is_network", "match": {"value": True}},
                ]
            },
            "prefetch_limit": 50,
            "final_limit": 20,
        }

    @staticmethod
    def gamecreator_search() -> Dict[str, Any]:
        """Optimized for GameCreator-specific content"""
        return {
            "dense_weight": 0.6,
            "sparse_weight": 0.4,
            "filter": {
                "should": [
                    {"key": "assembly_category", "match": {"value": "gamecreator_core"}},
                    {"key": "assembly_category", "match": {"value": "gamecreator_module"}},
                    {"key": "keywords", "match": {"any": ["gamecreator", "instruction", "condition"]}},
                ]
            },
            "prefetch_limit": 50,
            "final_limit": 20,
        }

    @staticmethod
    def documentation_search() -> Dict[str, Any]:
        """Optimized for finding documentation"""
        return {
            "dense_weight": 0.75,
            "sparse_weight": 0.25,
            "filter": {"content_type": "doc"},
            "prefetch_limit": 50,
            "final_limit": 15,
        }


# =============================================================================
# EXPORT ALL CONFIGURATIONS
# =============================================================================

def get_default_config() -> Dict[str, Any]:
    """Get complete default configuration as dictionary"""
    return {
        "connection": {
            "host": QDRANT_HOST,
            "port": QDRANT_PORT,
            "grpc_port": QDRANT_GRPC_PORT,
        },
        "collections": {
            "unified": UnifiedCollectionConfig().__dict__,
        },
        "hybrid_search": HybridSearchConfig().__dict__,
        "embedding": EmbeddingConfig().__dict__,
        "sparse_embedding": SparseEmbeddingConfig().__dict__,
        "indexing": IndexingConfig().__dict__,
        "presets": {
            "code": SearchPresets.code_search(),
            "semantic": SearchPresets.semantic_search(),
            "keyword": SearchPresets.keyword_search(),
            "network": SearchPresets.network_code_search(),
            "gamecreator": SearchPresets.gamecreator_search(),
            "docs": SearchPresets.documentation_search(),
        }
    }


if __name__ == "__main__":
    import json

    config = get_default_config()
    print(json.dumps(config, indent=2, default=str))
