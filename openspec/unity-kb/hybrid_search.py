#!/usr/bin/env python3
"""
Hybrid Search Implementation for MLcreator Unity Knowledge Base
================================================================

Combines dense (semantic) and sparse (keyword) search for optimal results.

Features:
- Reciprocal Rank Fusion (RRF) for score combination
- Query expansion for better recall
- Pre-filtering for efficient search
- Search presets for common use cases
"""

import re
from typing import Dict, List, Optional, Any, Tuple
from dataclasses import dataclass
from collections import defaultdict
import hashlib

from qdrant_client import QdrantClient
from qdrant_client.http import models
from qdrant_client.models import (
    Filter, FieldCondition, MatchValue, MatchAny, MatchText,
    Range, SearchRequest, SearchParams, NamedVector, NamedSparseVector,
    SparseVector, QueryRequest, Prefetch, FusionQuery, Fusion,
)

from qdrant_config import (
    QDRANT_HOST, QDRANT_PORT, QDRANT_GRPC_PORT,
    COLLECTION_UNIFIED, DENSE_VECTOR_SIZE,
    HybridSearchConfig, SearchPresets,
    DEFAULT_DENSE_WEIGHT, DEFAULT_SPARSE_WEIGHT,
)


@dataclass
class SearchResult:
    """A single search result"""
    id: str
    score: float
    payload: Dict[str, Any]
    vector_name: str = ""  # Which vector matched


@dataclass
class HybridSearchResponse:
    """Response from hybrid search"""
    results: List[SearchResult]
    total_found: int
    query: str
    search_time_ms: float
    config_used: Dict[str, Any]


class BM25Tokenizer:
    """
    Simple BM25-compatible tokenizer for generating sparse vectors.

    For production, consider using:
    - SPLADE models (neural sparse)
    - Fastembed with sparse models
    """

    def __init__(self, k1: float = 1.5, b: float = 0.75):
        self.k1 = k1
        self.b = b

        # Code-specific token patterns
        self.camel_case_pattern = re.compile(r'(?<!^)(?=[A-Z])')
        self.special_chars = re.compile(r'[^\w\s]')

        # Stop words (minimal for code)
        self.stop_words = {
            'the', 'a', 'an', 'and', 'or', 'but', 'in', 'on', 'at', 'to',
            'for', 'of', 'with', 'by', 'from', 'as', 'is', 'was', 'be',
            'this', 'that', 'it', 'they', 'we', 'you', 'i', 'my', 'your'
        }

        # Important code tokens to boost
        self.boost_tokens = {
            'network': 2.0, 'rpc': 2.0, 'serverrpc': 2.5, 'clientrpc': 2.5,
            'networkvariable': 2.0, 'networkobject': 2.0, 'networkbehaviour': 2.0,
            'gamecreator': 2.0, 'instruction': 1.5, 'condition': 1.5,
            'trigger': 1.5, 'character': 1.5, 'inventory': 1.5,
            'spawn': 1.5, 'despawn': 1.5, 'owner': 1.5,
        }

    def tokenize(self, text: str) -> List[str]:
        """Tokenize text for BM25"""
        # Lowercase
        text = text.lower()

        # Split camelCase
        text = self.camel_case_pattern.sub(' ', text)

        # Remove special characters (keep underscores for code)
        text = re.sub(r'[^\w\s_]', ' ', text)

        # Split on whitespace and underscores
        tokens = re.split(r'[\s_]+', text)

        # Filter and clean
        tokens = [
            t for t in tokens
            if len(t) >= 2 and t not in self.stop_words
        ]

        return tokens

    def to_sparse_vector(self, text: str) -> Tuple[List[int], List[float]]:
        """
        Convert text to sparse vector format (indices and values).

        Returns:
            Tuple of (indices, values) for Qdrant SparseVector
        """
        tokens = self.tokenize(text)

        # Count term frequencies
        tf = defaultdict(int)
        for token in tokens:
            tf[token] += 1

        # Convert to indices and values
        # Using hash for reproducible indices
        indices = []
        values = []

        for token, count in tf.items():
            # Hash token to index (32-bit positive integer)
            token_hash = int(hashlib.md5(token.encode()).hexdigest()[:8], 16)
            indices.append(token_hash)

            # BM25-like term weight with boost
            boost = self.boost_tokens.get(token, 1.0)
            weight = (count * boost) / (count + self.k1)
            values.append(weight)

        return indices, values


class HybridSearchEngine:
    """
    Hybrid search engine combining dense and sparse vectors.

    Uses Qdrant's native fusion capabilities for efficient hybrid search.
    """

    def __init__(
        self,
        host: str = QDRANT_HOST,
        port: int = QDRANT_PORT,
        collection_name: str = COLLECTION_UNIFIED,
        embedding_fn=None  # Function to generate dense embeddings
    ):
        self.client = QdrantClient(
            host=host,
            port=port,
            grpc_port=QDRANT_GRPC_PORT,
            prefer_grpc=True,
            timeout=60
        )
        self.collection_name = collection_name
        self.embedding_fn = embedding_fn
        self.tokenizer = BM25Tokenizer()
        self.config = HybridSearchConfig()

    def search(
        self,
        query: str,
        dense_vector: Optional[List[float]] = None,
        preset: Optional[str] = None,
        filter_dict: Optional[Dict] = None,
        limit: int = 20,
        dense_weight: float = DEFAULT_DENSE_WEIGHT,
        sparse_weight: float = DEFAULT_SPARSE_WEIGHT,
        with_payload: bool = True,
    ) -> HybridSearchResponse:
        """
        Perform hybrid search combining dense and sparse vectors.

        Args:
            query: Search query text
            dense_vector: Pre-computed dense embedding (optional)
            preset: Search preset name (code, semantic, keyword, network, gamecreator, docs)
            filter_dict: Additional filter conditions
            limit: Maximum results to return
            dense_weight: Weight for dense (semantic) search
            sparse_weight: Weight for sparse (keyword) search
            with_payload: Include payload in results

        Returns:
            HybridSearchResponse with results and metadata
        """
        import time
        start_time = time.time()

        # Apply preset if specified
        if preset:
            preset_config = self._get_preset_config(preset)
            dense_weight = preset_config.get("dense_weight", dense_weight)
            sparse_weight = preset_config.get("sparse_weight", sparse_weight)
            if "filter" in preset_config and not filter_dict:
                filter_dict = preset_config["filter"]
            limit = preset_config.get("final_limit", limit)

        # Generate dense vector if not provided
        if dense_vector is None and self.embedding_fn:
            dense_vector = self.embedding_fn(query)

        # Generate sparse vector
        sparse_indices, sparse_values = self.tokenizer.to_sparse_vector(query)

        # Build filter
        qdrant_filter = self._build_filter(filter_dict) if filter_dict else None

        # Perform hybrid search using Qdrant's fusion
        results = self._hybrid_search_with_fusion(
            dense_vector=dense_vector,
            sparse_indices=sparse_indices,
            sparse_values=sparse_values,
            dense_weight=dense_weight,
            sparse_weight=sparse_weight,
            filter=qdrant_filter,
            limit=limit,
            with_payload=with_payload,
        )

        search_time = (time.time() - start_time) * 1000

        return HybridSearchResponse(
            results=results,
            total_found=len(results),
            query=query,
            search_time_ms=search_time,
            config_used={
                "dense_weight": dense_weight,
                "sparse_weight": sparse_weight,
                "preset": preset,
                "limit": limit,
            }
        )

    def _hybrid_search_with_fusion(
        self,
        dense_vector: Optional[List[float]],
        sparse_indices: List[int],
        sparse_values: List[float],
        dense_weight: float,
        sparse_weight: float,
        filter: Optional[Filter],
        limit: int,
        with_payload: bool,
    ) -> List[SearchResult]:
        """
        Perform hybrid search using Qdrant's native fusion.

        Uses Reciprocal Rank Fusion (RRF) for combining results.
        """
        prefetch_queries = []

        # Dense search prefetch
        if dense_vector:
            prefetch_queries.append(
                Prefetch(
                    query=dense_vector,
                    using="dense",
                    limit=self.config.dense_prefetch_limit,
                    filter=filter,
                )
            )

        # Sparse search prefetch
        if sparse_indices:
            prefetch_queries.append(
                Prefetch(
                    query=SparseVector(
                        indices=sparse_indices,
                        values=sparse_values,
                    ),
                    using="sparse",
                    limit=self.config.sparse_prefetch_limit,
                    filter=filter,
                )
            )

        # If no vectors available, fall back to scroll
        if not prefetch_queries:
            return self._fallback_scroll_search(filter, limit, with_payload)

        # Execute hybrid query with RRF fusion
        try:
            results = self.client.query_points(
                collection_name=self.collection_name,
                prefetch=prefetch_queries,
                query=FusionQuery(fusion=Fusion.RRF),
                limit=limit,
                with_payload=with_payload,
            )

            return [
                SearchResult(
                    id=str(point.id),
                    score=point.score,
                    payload=point.payload or {},
                )
                for point in results.points
            ]

        except Exception as e:
            print(f"[WARNING] Fusion search failed: {e}")
            # Fall back to dense-only or sparse-only
            return self._fallback_single_search(
                dense_vector, sparse_indices, sparse_values,
                filter, limit, with_payload
            )

    def _fallback_single_search(
        self,
        dense_vector: Optional[List[float]],
        sparse_indices: List[int],
        sparse_values: List[float],
        filter: Optional[Filter],
        limit: int,
        with_payload: bool,
    ) -> List[SearchResult]:
        """Fallback to single-vector search if fusion fails"""

        if dense_vector:
            results = self.client.search(
                collection_name=self.collection_name,
                query_vector=NamedVector(name="dense", vector=dense_vector),
                query_filter=filter,
                limit=limit,
                with_payload=with_payload,
            )
        elif sparse_indices:
            results = self.client.search(
                collection_name=self.collection_name,
                query_vector=NamedSparseVector(
                    name="sparse",
                    vector=SparseVector(indices=sparse_indices, values=sparse_values)
                ),
                query_filter=filter,
                limit=limit,
                with_payload=with_payload,
            )
        else:
            return []

        return [
            SearchResult(
                id=str(point.id),
                score=point.score,
                payload=point.payload or {},
            )
            for point in results
        ]

    def _fallback_scroll_search(
        self,
        filter: Optional[Filter],
        limit: int,
        with_payload: bool,
    ) -> List[SearchResult]:
        """Scroll-based search when no vectors available"""
        results, _ = self.client.scroll(
            collection_name=self.collection_name,
            scroll_filter=filter,
            limit=limit,
            with_payload=with_payload,
            with_vectors=False,
        )

        return [
            SearchResult(
                id=str(point.id),
                score=1.0,  # No score for scroll
                payload=point.payload or {},
            )
            for point in results
        ]

    def _get_preset_config(self, preset: str) -> Dict[str, Any]:
        """Get configuration for a search preset"""
        presets = {
            "code": SearchPresets.code_search(),
            "semantic": SearchPresets.semantic_search(),
            "keyword": SearchPresets.keyword_search(),
            "network": SearchPresets.network_code_search(),
            "gamecreator": SearchPresets.gamecreator_search(),
            "docs": SearchPresets.documentation_search(),
        }
        return presets.get(preset, {})

    def _build_filter(self, filter_dict: Dict) -> Filter:
        """Build Qdrant filter from dictionary"""
        must = []
        should = []
        must_not = []

        # Simple key-value filters become must conditions
        for key, value in filter_dict.items():
            if key == "must":
                for condition in value:
                    must.append(self._build_condition(condition))
            elif key == "should":
                for condition in value:
                    should.append(self._build_condition(condition))
            elif key == "must_not":
                for condition in value:
                    must_not.append(self._build_condition(condition))
            else:
                # Direct key-value
                must.append(FieldCondition(
                    key=key,
                    match=MatchValue(value=value)
                ))

        return Filter(
            must=must if must else None,
            should=should if should else None,
            must_not=must_not if must_not else None,
        )

    def _build_condition(self, condition: Dict) -> FieldCondition:
        """Build a single filter condition"""
        key = condition.get("key")
        match = condition.get("match", {})

        if "value" in match:
            return FieldCondition(key=key, match=MatchValue(value=match["value"]))
        elif "any" in match:
            return FieldCondition(key=key, match=MatchAny(any=match["any"]))
        elif "text" in match:
            return FieldCondition(key=key, match=MatchText(text=match["text"]))
        else:
            return FieldCondition(key=key, match=MatchValue(value=True))

    def search_code(self, query: str, **kwargs) -> HybridSearchResponse:
        """Shortcut for code search"""
        return self.search(query, preset="code", **kwargs)

    def search_docs(self, query: str, **kwargs) -> HybridSearchResponse:
        """Shortcut for documentation search"""
        return self.search(query, preset="docs", **kwargs)

    def search_network(self, query: str, **kwargs) -> HybridSearchResponse:
        """Shortcut for network/multiplayer code search"""
        return self.search(query, preset="network", **kwargs)

    def search_gamecreator(self, query: str, **kwargs) -> HybridSearchResponse:
        """Shortcut for GameCreator-specific search"""
        return self.search(query, preset="gamecreator", **kwargs)

    def keyword_search(self, query: str, **kwargs) -> HybridSearchResponse:
        """Shortcut for keyword-heavy search"""
        return self.search(query, preset="keyword", **kwargs)

    def semantic_search(self, query: str, **kwargs) -> HybridSearchResponse:
        """Shortcut for semantic/conceptual search"""
        return self.search(query, preset="semantic", **kwargs)


# =============================================================================
# CONVENIENCE FUNCTIONS
# =============================================================================

_search_engine: Optional[HybridSearchEngine] = None


def get_search_engine(embedding_fn=None) -> HybridSearchEngine:
    """Get or create the default search engine"""
    global _search_engine
    if _search_engine is None:
        _search_engine = HybridSearchEngine(embedding_fn=embedding_fn)
    return _search_engine


def hybrid_search(
    query: str,
    preset: str = None,
    limit: int = 20,
    **kwargs
) -> HybridSearchResponse:
    """
    Convenience function for hybrid search.

    Examples:
        # General search
        results = hybrid_search("NetworkCharacterAdapter movement sync")

        # Code-specific search
        results = hybrid_search("RPC damage calculation", preset="code")

        # Network code search
        results = hybrid_search("spawn player prefab", preset="network")

        # Documentation search
        results = hybrid_search("how to setup multiplayer", preset="docs")
    """
    engine = get_search_engine()
    return engine.search(query, preset=preset, limit=limit, **kwargs)


def search_code(query: str, limit: int = 20) -> List[Dict]:
    """Search for code symbols"""
    response = hybrid_search(query, preset="code", limit=limit)
    return [r.payload for r in response.results]


def search_docs(query: str, limit: int = 15) -> List[Dict]:
    """Search for documentation"""
    response = hybrid_search(query, preset="docs", limit=limit)
    return [r.payload for r in response.results]


def search_network_patterns(query: str, limit: int = 20) -> List[Dict]:
    """Search for network/multiplayer patterns"""
    response = hybrid_search(query, preset="network", limit=limit)
    return [r.payload for r in response.results]


# =============================================================================
# CLI
# =============================================================================

def main():
    """Interactive search CLI"""
    import argparse

    parser = argparse.ArgumentParser(description="Hybrid search for MLcreator KB")
    parser.add_argument("query", nargs="?", help="Search query")
    parser.add_argument("--preset", "-p", choices=["code", "docs", "network", "gamecreator", "keyword", "semantic"],
                       help="Search preset")
    parser.add_argument("--limit", "-l", type=int, default=10, help="Result limit")
    parser.add_argument("--interactive", "-i", action="store_true", help="Interactive mode")

    args = parser.parse_args()

    engine = HybridSearchEngine()

    if args.interactive:
        print("MLcreator KB Hybrid Search (type 'quit' to exit)")
        print("Presets: code, docs, network, gamecreator, keyword, semantic")
        print("-" * 60)

        while True:
            try:
                query = input("\nQuery: ").strip()
                if query.lower() in ('quit', 'exit', 'q'):
                    break

                # Check for preset prefix
                preset = args.preset
                if query.startswith("/"):
                    parts = query.split(" ", 1)
                    preset = parts[0][1:]
                    query = parts[1] if len(parts) > 1 else ""

                if not query:
                    continue

                response = engine.search(query, preset=preset, limit=args.limit)

                print(f"\nFound {response.total_found} results ({response.search_time_ms:.1f}ms)")
                print("-" * 60)

                for i, result in enumerate(response.results, 1):
                    name = result.payload.get("name", "Unknown")
                    content_type = result.payload.get("content_type", "")
                    namespace = result.payload.get("namespace", "")
                    score = result.score

                    print(f"{i}. [{content_type}] {name}")
                    if namespace:
                        print(f"   Namespace: {namespace}")
                    print(f"   Score: {score:.4f}")

            except KeyboardInterrupt:
                break
            except Exception as e:
                print(f"Error: {e}")

    elif args.query:
        response = engine.search(args.query, preset=args.preset, limit=args.limit)

        print(f"Query: {args.query}")
        print(f"Preset: {args.preset or 'default'}")
        print(f"Found: {response.total_found} results ({response.search_time_ms:.1f}ms)")
        print("-" * 60)

        for i, result in enumerate(response.results, 1):
            payload = result.payload
            print(f"\n{i}. {payload.get('name', 'Unknown')} (score: {result.score:.4f})")
            print(f"   Type: {payload.get('content_type', '')} / {payload.get('code_type', payload.get('doc_type', ''))}")
            if payload.get('namespace'):
                print(f"   Namespace: {payload['namespace']}")
            if payload.get('file_path'):
                print(f"   File: {payload['file_path']}")

    else:
        parser.print_help()


if __name__ == "__main__":
    main()
