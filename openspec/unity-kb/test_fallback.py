#!/usr/bin/env python3
"""
Test script for Unity KB fallback functionality
"""

import sys
import os
sys.path.append(os.path.dirname(__file__))

from direct_mcp_server import KnowledgeBase, Config

def test_fallback():
    """Test the JSON fallback functionality."""
    print("Testing Unity KB fallback functionality...", flush=True)

    # Create config
    config = Config()
    print(f"Config: host={config.qdrant_host}, port={config.qdrant_port}", flush=True)

    # Create knowledge base
    kb = KnowledgeBase(config)
    print("KnowledgeBase created", flush=True)

    # Try to connect (should fallback to JSON)
    print("Attempting to connect...", flush=True)
    connected = kb.connect()
    print(f"Connect result: {connected}", flush=True)

    if connected:
        print("✅ Connected successfully", flush=True)
        print(f"Using fallback: {kb.use_fallback}", flush=True)

        # Test stats
        print("\nTesting stats...", flush=True)
        stats = kb.get_stats()
        print(f"Stats: {stats}", flush=True)

        # Test search
        print("\nTesting search for 'Character'...", flush=True)
        results = kb.search_by_filter({"query": "Character", "kind": "class"}, limit=3)
        print(f"Found {len(results)} results:", flush=True)
        for result in results[:3]:
            print(f"  - {result.get('class_name', 'Unknown')}", flush=True)

        # Test class members
        if results:
            class_name = results[0].get('class_name')
            print(f"\nTesting class members for '{class_name}'...", flush=True)
            members = kb.get_class_members(class_name)
            print(f"Found {len(members)} members", flush=True)

        # Test namespace analysis
        print("\nTesting namespace analysis...", flush=True)
        analysis = kb.analyze_namespace("GameCreator", include_sub_namespaces=True)
        print(f"Analysis: {analysis.get('total_classes', 0)} classes found", flush=True)

    else:
        print("❌ Failed to connect", flush=True)
        print("Checking JSON fallback directly...", flush=True)

        # Test JSON fallback directly
        from direct_mcp_server import JSONKnowledgeBase
        json_kb = JSONKnowledgeBase()
        if json_kb.load_data():
            print("JSON fallback loaded successfully", flush=True)
            stats = json_kb.get_stats()
            print(f"JSON Stats: {stats}", flush=True)
        else:
            print("JSON fallback failed to load", flush=True)

if __name__ == "__main__":
    test_fallback()