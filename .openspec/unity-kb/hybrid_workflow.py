#!/usr/bin/env python3
"""
Hybrid Development Workflow

Combines Unity KB discovery with Serena memory validation for optimal workflow.

This demonstrates the query-first strategy:
1. Discovery (Unity KB) - Find relevant code patterns
2. Validation (Serena Memories) - Check compliance with rules
3. Context Building - Combine KB results + memories
4. Implementation - Guided by full context
"""

import json
import sys
from pathlib import Path
from typing import Dict, List

PROJECT_ROOT = Path(__file__).parent.parent.parent
sys.path.insert(0, str(PROJECT_ROOT))

try:
    from scripts.unity_kb.mcp_client import (
        search_unity_symbols,
        get_unity_class_members,
        find_similar_unity_code
    )
    MCP_AVAILABLE = True
except ImportError:
    MCP_AVAILABLE = False


def smart_memory_preload(task_description: str, current_file: str = None) -> Dict:
    """
    Intelligently preload relevant memories using Unity KB.

    Args:
        task_description: Description of the task
        current_file: Optional current file path for context

    Returns:
        Dictionary with preload strategy and loaded content
    """
    print(f"[INFO] Smart memory preloading for task: {task_description[:50]}...")

    preload_strategy = {
        "critical_memories": [],
        "api_memories": [],
        "pattern_memories": [],
        "integration_memories": []
    }

    if not MCP_AVAILABLE:
        print("[WARNING] MCP not available, using fallback strategy")
        return preload_strategy

    # Step 1: Semantic search for relevant symbols
    print("  [1/4] Searching Unity KB for relevant symbols...")
    symbols = search_unity_symbols(query=task_description, limit=30)
    print(f"        Found {len(symbols)} symbols")

    # Step 2: Identify assemblies/namespaces involved
    assemblies = set(s.get('assembly_name', '') for s in symbols)
    namespaces = set(s.get('namespace', '') for s in symbols)

    print(f"  [2/4] Identified {len(assemblies)} assemblies, {len(namespaces)} namespaces")

    # Step 3: Map assemblies to critical memories
    for assembly in assemblies:
        if "Multiplayer" in assembly or "Netcode" in assembly:
            preload_strategy["critical_memories"].extend([
                "CRITICAL/network_architecture_never_forget",
                "CRITICAL/gamecreator_invasive_integration"
            ])

        if "GameCreator" in assembly:
            preload_strategy["integration_memories"].append(
                "INTEGRATION/gamecreator_patterns"
            )

    # Step 4: Find related classes and add their API memories
    print("  [3/4] Mapping to API memories...")
    for symbol in symbols[:10]:  # Top 10 most relevant
        if symbol.get('kind') == 'class':
            api_memory = f"AUTO_GENERATED/api_{symbol.get('name', '').lower()}"
            preload_strategy["api_memories"].append(api_memory)

    # Step 5: Context from current file (if provided)
    if current_file:
        print("  [4/4] Adding context from current file...")
        # Would call get_unity_file_symbols(current_file) if implemented
        pass

    print(f"[OK] Preload strategy ready:")
    print(f"     - Critical memories: {len(preload_strategy['critical_memories'])}")
    print(f"     - API memories: {len(preload_strategy['api_memories'])}")
    print(f"     - Pattern memories: {len(preload_strategy['pattern_memories'])}")

    return {
        "strategy": preload_strategy,
        "symbols_found": len(symbols),
        "assemblies_involved": list(assemblies)
    }


def hybrid_development_workflow(task: str, current_file: str = None) -> Dict:
    """
    Hybrid workflow: KB query → Memory validation → Implementation

    Args:
        task: Task description
        current_file: Optional current file path

    Returns:
        Comprehensive context for implementation
    """
    print("=" * 70)
    print("HYBRID DEVELOPMENT WORKFLOW")
    print("=" * 70)

    if not MCP_AVAILABLE:
        print("[ERROR] Unity KB MCP not available")
        return {}

    # Phase 1: DISCOVERY (Unity KB)
    print("\n[PHASE 1] DISCOVERY (Unity KB)")
    print("-" * 70)

    # Discover relevant code patterns
    symbols = search_unity_symbols(query=task, limit=20)
    print(f"[OK] Found {len(symbols)} relevant symbols")

    # Find similar implementations
    # (would extract code snippet from task in real implementation)
    similar = []  # find_similar_unity_code(code_snippet, limit=10)
    print(f"[OK] Found {len(similar)} similar implementations")

    # Phase 2: VALIDATION (Serena Memories)
    print("\n[PHASE 2] VALIDATION (Serena Memories)")
    print("-" * 70)

    # Load critical rules based on symbols found
    critical_memories = load_critical_memories_for_symbols(symbols)
    print(f"[OK] Loaded {len(critical_memories)} critical rules")

    # Check for rule violations
    violations = []
    for symbol in symbols:
        for rule in critical_memories:
            if violates_rule(symbol, rule):
                violations.append({
                    "symbol": symbol.get('name'),
                    "rule": rule.get("title"),
                    "severity": rule.get("severity", "WARNING")
                })

    if violations:
        print(f"[WARNING] {len(violations)} rule violations detected:")
        for v in violations:
            print(f"          - {v['symbol']}: {v['rule']} (Severity: {v['severity']})")
    else:
        print("[OK] No rule violations detected")

    # Phase 3: CONTEXT BUILDING
    print("\n[PHASE 3] CONTEXT BUILDING")
    print("-" * 70)

    # Build comprehensive context
    context = {
        "discovery": {
            "relevant_symbols": symbols,
            "similar_implementations": similar,
            "assemblies_involved": list(set(s.get('assembly_name', '') for s in symbols))
        },
        "validation": {
            "critical_rules": critical_memories,
            "violations": violations,
            "compliance_score": 1.0 - (len(violations) / max(len(symbols), 1))
        }
    }

    print(f"[OK] Context built:")
    print(f"     - Symbols: {len(symbols)}")
    print(f"     - Critical rules: {len(critical_memories)}")
    print(f"     - Compliance score: {context['validation']['compliance_score']:.2%}")

    # Phase 4: READY FOR IMPLEMENTATION
    print("\n[PHASE 4] READY FOR IMPLEMENTATION")
    print("=" * 70)
    print("\n[OK] Full context available for guided implementation\n")

    return context


def load_critical_memories_for_symbols(symbols: List[Dict]) -> List[Dict]:
    """Load critical memories relevant to symbols"""
    # Simplified - in real implementation would read actual memory files
    critical_rules = []

    assemblies = set(s.get('assembly_name', '') for s in symbols)

    if any("Multiplayer" in a for a in assemblies):
        critical_rules.append({
            "title": "Use NetworkCharacterAdapter, not NetworkTransform",
            "severity": "CRITICAL"
        })

    if any("GameCreator" in a for a in assemblies):
        critical_rules.append({
            "title": "GameCreator Invasive Integration Pattern",
            "severity": "CRITICAL"
        })

    return critical_rules


def violates_rule(symbol: Dict, rule: Dict) -> bool:
    """Check if symbol violates a rule"""
    # Simplified violation detection
    if "NetworkTransform" in rule.get("title", ""):
        if symbol.get('name') == "NetworkTransform":
            return True

    return False


def main():
    """Test the hybrid workflow"""

    # Example 1: Character networking task
    print("\n\nEXAMPLE 1: Character Network Synchronization\n")
    task1 = "implement character network synchronization with prediction"
    context1 = hybrid_development_workflow(task1)

    # Example 2: Memory preloading
    print("\n\nEXAMPLE 2: Smart Memory Preloading\n")
    task2 = "create NetworkVariable for player health with callbacks"
    preload = smart_memory_preload(task2)


if __name__ == "__main__":
    main()
