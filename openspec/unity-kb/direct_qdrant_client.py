#!/usr/bin/env python3
"""
Direct Qdrant Client for Unity KB

Bypasses HTTP API and connects directly to Qdrant.
"""

from qdrant_client import QdrantClient
from qdrant_client.models import Filter, FieldCondition, MatchValue, MatchText, SearchRequest
from typing import Dict, List, Optional

# Qdrant connection
QDRANT_HOST = "localhost"
QDRANT_PORT = 6333
COLLECTION_NAME = "unity_project_kb"

# Global client
_client = None


def get_client() -> QdrantClient:
    """Get or create Qdrant client"""
    global _client
    if _client is None:
        _client = QdrantClient(host=QDRANT_HOST, port=QDRANT_PORT, timeout=60)
    return _client


def search_unity_symbols(
    query: str,
    assembly_name: Optional[str] = None,
    code_type: Optional[str] = None,
    kind: Optional[str] = None,
    limit: int = 10
) -> List[Dict]:
    """
    Search for C# symbols using Qdrant directly.

    Note: This is a simplified version that doesn't use embeddings.
    For full semantic search, we'd need to embed the query first.
    For now, we'll use scroll with filters.
    """
    client = get_client()

    # Build filter conditions
    must_conditions = []

    if assembly_name:
        must_conditions.append(
            FieldCondition(key="assembly_name", match=MatchValue(value=assembly_name))
        )

    if code_type:
        must_conditions.append(
            FieldCondition(key="code_type", match=MatchValue(value=code_type))
        )

    if kind:
        must_conditions.append(
            FieldCondition(key="kind", match=MatchValue(value=kind))
        )

    # Simplified approach: scroll and filter in Python
    # This is less efficient but more reliable
    results = []

    if must_conditions:
        filter_obj = Filter(must=must_conditions)
    else:
        filter_obj = None

    # Get a larger set to filter from
    fetch_limit = min(limit * 100, 10000) if query else limit

    scroll_result = client.scroll(
        collection_name=COLLECTION_NAME,
        scroll_filter=filter_obj,
        limit=fetch_limit,
        with_payload=True,
        with_vectors=False
    )

    for point in scroll_result[0]:
        payload = point.payload

        # Filter by query if provided
        if query:
            query_lower = query.lower()
            name_match = query_lower in payload.get("name", "").lower()
            path_match = query_lower in payload.get("name_path", "").lower()
            sig_match = query_lower in payload.get("signature", "").lower()

            if not (name_match or path_match or sig_match):
                continue

        results.append({
            "name": payload.get("name", ""),
            "name_path": payload.get("name_path", ""),
            "kind": payload.get("kind", ""),
            "assembly_name": payload.get("assembly_name", ""),
            "namespace": payload.get("namespace", ""),
            "file_path": payload.get("file_path", ""),
            "start_line": payload.get("start_line", 0),
            "end_line": payload.get("end_line", 0),
            "signature": payload.get("signature", ""),
            "modifiers": payload.get("modifiers", []),
            "base_classes": payload.get("base_classes", []),
            "interfaces": payload.get("interfaces", []),
            "documentation": payload.get("documentation", ""),
            "code_preview": payload.get("code_preview", ""),
            "class_name": payload.get("class_name", ""),
            "code_type": payload.get("code_type", "")
        })

    return results[:limit]


def get_unity_class_members(
    class_name: str,
    assembly_name: Optional[str] = None
) -> List[Dict]:
    """Get all members of a class"""
    client = get_client()

    must_conditions = [
        FieldCondition(key="class_name", match=MatchText(text=class_name))
    ]

    if assembly_name:
        must_conditions.append(
            FieldCondition(key="assembly_name", match=MatchText(text=assembly_name))
        )

    filter_obj = Filter(must=must_conditions)

    scroll_result = client.scroll(
        collection_name=COLLECTION_NAME,
        scroll_filter=filter_obj,
        limit=1000,  # Get all members
        with_payload=True,
        with_vectors=False
    )

    members = []
    for point in scroll_result[0]:
        payload = point.payload
        members.append({
            "name": payload.get("name", ""),
            "kind": payload.get("kind", ""),
            "signature": payload.get("signature", ""),
            "modifiers": payload.get("modifiers", []),
            "file_path": payload.get("file_path", ""),
            "start_line": payload.get("start_line", 0),
            "documentation": payload.get("documentation", ""),
            "class_name": payload.get("class_name", "")
        })

    return members


def get_unity_kb_stats() -> Dict:
    """Get collection statistics"""
    client = get_client()

    collection_info = client.get_collection(COLLECTION_NAME)

    return {
        "total_symbols": collection_info.points_count,
        "indexed_vectors": collection_info.indexed_vectors_count,
        "status": collection_info.status
    }


def list_unity_assemblies() -> List[Dict]:
    """List all assemblies with counts"""
    client = get_client()

    # This would require aggregation which Qdrant doesn't support directly
    # For now, we'll scroll and count
    scroll_result = client.scroll(
        collection_name=COLLECTION_NAME,
        limit=10000,
        with_payload=True,
        with_vectors=False
    )

    assembly_counts = {}
    for point in scroll_result[0]:
        assembly = point.payload.get("assembly_name", "Unknown")
        assembly_counts[assembly] = assembly_counts.get(assembly, 0) + 1

    assemblies = [
        {"name": name, "symbol_count": count}
        for name, count in sorted(assembly_counts.items(), key=lambda x: x[1], reverse=True)
    ]

    return assemblies


def test_connection():
    """Test connection to Qdrant"""
    try:
        stats = get_unity_kb_stats()
        print(f"[OK] Connected to Qdrant!")
        print(f"     Total symbols: {stats['total_symbols']}")
        print(f"     Status: {stats['status']}")
        return True
    except Exception as e:
        print(f"[ERROR] Connection failed: {e}")
        return False


if __name__ == "__main__":
    # Test connection
    if test_connection():
        print("\n[TEST] Searching for NetworkCharacterAdapter...")
        results = search_unity_symbols("NetworkCharacterAdapter", kind="class")
        print(f"       Found {len(results)} results")
        for r in results[:3]:
            print(f"       - {r['name_path']} ({r['assembly_name']})")

        if results:
            print(f"\n[TEST] Getting members of {results[0]['name']}...")
            members = get_unity_class_members(
                results[0]['name'],
                results[0]['assembly_name']
            )
            print(f"       Found {len(members)} members")
            for m in members[:5]:
                print(f"       - {m['kind']}: {m['name']}")
