#!/usr/bin/env python3
"""Inspect Qdrant collection to see what's actually in it"""

from qdrant_client import QdrantClient
import json

client = QdrantClient(host="localhost", port=6333)

# Get a few sample points
points, _ = client.scroll(
    collection_name="unity_project_kb",
    limit=5,
    with_payload=True,
    with_vectors=False
)

print("Sample symbols from collection:")
print("=" * 70)

for i, point in enumerate(points, 1):
    print(f"\nSymbol {i}:")
    print(f"  Name: {point.payload.get('name', 'N/A')}")
    print(f"  Kind: {point.payload.get('kind', 'N/A')}")
    print(f"  Assembly: {point.payload.get('assembly_name', 'N/A')}")
    print(f"  Namespace: {point.payload.get('namespace', 'N/A')}")
    print(f"  File: {point.payload.get('file_path', 'N/A')}")

# Try to find NetworkCharacterAdapter
print("\n" + "=" * 70)
print("Searching for 'NetworkCharacterAdapter'...")

from qdrant_client.models import Filter, FieldCondition, MatchText

# Try text match
try:
    results = client.scroll(
        collection_name="unity_project_kb",
        scroll_filter=Filter(
            must=[
                FieldCondition(
                    key="name",
                    match=MatchText(text="NetworkCharacterAdapter")
                )
            ]
        ),
        limit=10,
        with_payload=True
    )

    print(f"Found {len(results[0])} results with exact name match")
    for p in results[0]:
        print(f"  - {p.payload.get('name')} ({p.payload.get('assembly_name')})")
except Exception as e:
    print(f"Error: {e}")

# Try scrolling and filtering in Python
print("\nTrying Python-side filter...")
all_points, _ = client.scroll(
    collection_name="unity_project_kb",
    limit=10000,
    with_payload=True
)

matches = [p for p in all_points if "NetworkCharacterAdapter" in p.payload.get("name", "")]
print(f"Found {len(matches)} matches in {len(all_points)} symbols")
for m in matches[:5]:
    print(f"  - {m.payload.get('name')} in {m.payload.get('assembly_name', 'N/A')}")
