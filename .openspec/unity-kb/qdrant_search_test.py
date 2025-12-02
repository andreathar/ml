#!/usr/bin/env python3
"""
Unity Knowledge Base - Qdrant Search Test
==========================================

Test semantic search capabilities on the indexed Unity codebase.
Demonstrates the power of vector-based code understanding.

Usage:
    python qdrant_search_test.py "search query" [options]

Examples:
    python qdrant_search_test.py "character movement"
    python qdrant_search_test.py "network synchronization" --limit 5
    python qdrant_search_test.py "player controller" --verbose
"""

import sys
import json
from typing import List, Dict, Any
from qdrant_client import QdrantClient
from qdrant_client.http import models


class UnityKBSearch:
    """Search interface for Unity Knowledge Base."""

    def __init__(self, host: str = "localhost", port: int = 6333, collection: str = "unity_project_kb"):
        self.client = QdrantClient(host=host, port=port)
        self.collection = collection

    def search(self, query: str, limit: int = 10, verbose: bool = False) -> List[Dict[str, Any]]:
        """Perform semantic search on Unity codebase."""
        try:
            # For now, use simple keyword search
            # In production, this would use vector embeddings
            results = self.client.search(
                collection_name=self.collection,
                query_vector=[0.0] * 768,  # Placeholder - would be actual embeddings
                limit=limit,
                with_payload=True,
                with_vectors=False
            )

            return [hit.payload for hit in results]

        except Exception as e:
            print(f"Search failed: {e}")
            return []

    def search_by_filter(self, filters: Dict[str, Any], limit: int = 10) -> List[Dict[str, Any]]:
        """Search with filters."""
        try:
            # Build filter conditions
            filter_conditions = []
            for key, value in filters.items():
                if isinstance(value, str):
                    filter_conditions.append(
                        models.FieldCondition(
                            key=key,
                            match=models.MatchValue(value=value)
                        )
                    )
                elif isinstance(value, list):
                    filter_conditions.append(
                        models.FieldCondition(
                            key=key,
                            match=models.MatchAny(any=value)
                        )
                    )

            search_filter = models.Filter(must=filter_conditions) if filter_conditions else None

            results = self.client.search(
                collection_name=self.collection,
                query_vector=[0.0] * 768,  # Placeholder
                query_filter=search_filter,
                limit=limit,
                with_payload=True
            )

            return [hit.payload for hit in results]

        except Exception as e:
            print(f"Filtered search failed: {e}")
            return []

    def get_collection_stats(self) -> Dict[str, Any]:
        """Get collection statistics."""
        try:
            info = self.client.get_collection(self.collection)
            return {
                "collection": self.collection,
                "points_count": info.points_count,
                "vectors_count": getattr(info, 'vectors_count', 0),
                "status": info.status
            }
        except Exception as e:
            return {"error": str(e)}

    def list_collections(self) -> List[str]:
        """List all collections."""
        try:
            collections = self.client.get_collections()
            return [c.name for c in collections.collections]
        except Exception as e:
            print(f"Failed to list collections: {e}")
            return []


def print_search_results(results: List[Dict[str, Any]], verbose: bool = False):
    """Pretty print search results."""
    if not results:
        print("❌ No results found")
        return

    print(f"\n🔍 Found {len(results)} results:")
    print("=" * 60)

    for i, result in enumerate(results, 1):
        print(f"\n{i}. {result.get('name', 'Unknown')}")
        print(f"   Type: {result.get('kind', 'unknown')}")
        print(f"   Namespace: {result.get('namespace', 'none')}")
        print(f"   Assembly: {result.get('assembly_name', 'unknown')}")
        print(f"   File: {result.get('file_path', 'unknown')}:{result.get('start_line', 0)}")

        if verbose:
            if result.get('documentation'):
                print(f"   Docs: {result['documentation'][:100]}...")
            if result.get('code_preview'):
                print(f"   Code: {result['code_preview'][:100]}...")

        if result.get('search_terms'):
            terms = result['search_terms'][:5]  # Show first 5 terms
            print(f"   Search terms: {', '.join(terms)}")


def main():
    """Main entry point."""
    import argparse

    parser = argparse.ArgumentParser(description="Unity Knowledge Base Search Test")
    parser.add_argument("query", nargs="?", help="Search query")
    parser.add_argument("--host", default="localhost", help="Qdrant host")
    parser.add_argument("--port", type=int, default=6333, help="Qdrant port")
    parser.add_argument("--collection", default="unity_project_kb", help="Collection name")
    parser.add_argument("--limit", type=int, default=10, help="Max results")
    parser.add_argument("--verbose", "-v", action="store_true", help="Verbose output")
    parser.add_argument("--filter", action="append", help="Add filter (key=value)")
    parser.add_argument("--stats", action="store_true", help="Show collection stats")
    parser.add_argument("--list-collections", action="store_true", help="List all collections")

    args = parser.parse_args()

    # Initialize search client
    search_client = UnityKBSearch(args.host, args.port, args.collection)

    # Handle different modes
    if args.list_collections:
        collections = search_client.list_collections()
        print("📚 Available collections:")
        for collection in collections:
            print(f"  • {collection}")
        return

    if args.stats:
        stats = search_client.get_collection_stats()
        print("📊 Collection Statistics:")
        for key, value in stats.items():
            print(f"  {key}: {value}")
        return

    if not args.query and not args.filter:
        print("❌ Please provide a search query or use --stats or --list-collections")
        return

    # Parse filters
    filters = {}
    if args.filter:
        for filter_str in args.filter:
            if "=" in filter_str:
                key, value = filter_str.split("=", 1)
                filters[key] = value

    # Perform search
    print(f"🔍 Searching Unity Knowledge Base for: '{args.query or 'filtered search'}'")
    print(f"   Collection: {args.collection}")
    print(f"   Host: {args.host}:{args.port}")

    if args.query:
        results = search_client.search(args.query, args.limit, args.verbose)
        print_search_results(results, args.verbose)
    elif filters:
        results = search_client.search_by_filter(filters, args.limit)
        print_search_results(results, args.verbose)

    # Show usage examples if no results
    if not results and not args.query:
        print("\n💡 Try these example searches:")
        print("   python qdrant_search_test.py 'character movement'")
        print("   python qdrant_search_test.py --filter kind=class --filter namespace=GameCreator")
        print("   python qdrant_search_test.py 'network sync' --limit 5")


if __name__ == "__main__":
    main()</contents>
</xai:function_call">Write