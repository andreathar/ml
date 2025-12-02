#!/usr/bin/env python3
"""
Unity Knowledge Base - Documentation Retrieval System
====================================================

Intelligent documentation retrieval for Unity and GameCreator.
Provides context-aware documentation based on code analysis and keywords.

Features:
- Unity official documentation lookup
- GameCreator module documentation
- Best practices and troubleshooting guides
- Cross-referenced examples and tutorials

Author: MLCreator AI Assistant
Version: 2.0
"""

import re
import json
from pathlib import Path
from typing import Dict, List, Set, Optional, Tuple
from dataclasses import dataclass
from enum import Enum

class DocumentationType(Enum):
    UNITY_API = "unity_api"
    UNITY_GUIDE = "unity_guide"
    UNITY_TUTORIAL = "unity_tutorial"
    UNITY_TROUBLESHOOTING = "unity_troubleshooting"
    GC_API = "gc_api"
    GC_GUIDE = "gc_guide"
    GC_TUTORIAL = "gc_tutorial"
    GC_BEST_PRACTICE = "gc_best_practice"
    MULTIPLAYER_GUIDE = "multiplayer_guide"
    NETWORKING_TUTORIAL = "networking_tutorial"

class DocumentationPriority(Enum):
    CRITICAL = "critical"  # Must-read for functionality
    HIGH = "high"         # Important for best practices
    MEDIUM = "medium"     # Useful additional information
    LOW = "low"          # Nice-to-know background info

@dataclass
class DocumentationReference:
    """Reference to documentation"""
    title: str
    type: DocumentationType
    priority: DocumentationPriority
    url: Optional[str]
    summary: str
    keywords: Set[str]
    related_topics: Set[str]

@dataclass
class DocumentationQuery:
    """Query for documentation retrieval"""
    keywords: Set[str]
    context: str  # e.g., "character_movement", "network_sync"
    user_intent: str  # e.g., "how_to", "troubleshooting", "api_reference"

class UnityDocumentationRetriever:
    """Intelligent documentation retrieval system"""

    def __init__(self):
        self.documentation_index = self._build_documentation_index()
        self.keyword_mappings = self._build_keyword_mappings()

    def _build_documentation_index(self) -> Dict[str, DocumentationReference]:
        """Build comprehensive documentation index"""
        return {
            # Unity Core Documentation
            "unity.transform": DocumentationReference(
                title="Transform Component",
                type=DocumentationType.UNITY_API,
                priority=DocumentationPriority.HIGH,
                url="https://docs.unity3d.com/ScriptReference/Transform.html",
                summary="Position, rotation, and scale manipulation",
                keywords={"unity", "transform", "position", "rotation", "scale"},
                related_topics={"unity.gameobject", "unity.component"}
            ),

            "unity.monobehaviour": DocumentationReference(
                title="MonoBehaviour Class",
                type=DocumentationType.UNITY_API,
                priority=DocumentationPriority.CRITICAL,
                url="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html",
                summary="Base class for all Unity scripts",
                keywords={"unity", "monobehaviour", "script", "lifecycle"},
                related_topics={"unity.update", "unity.start", "unity.awake"}
            ),

            # Unity Networking Documentation
            "unity.netcode": DocumentationReference(
                title="Unity Netcode for GameObjects",
                type=DocumentationType.UNITY_GUIDE,
                priority=DocumentationPriority.CRITICAL,
                url="https://docs-multiplayer.unity3d.com/netcode/current/about/index.html",
                summary="Official networking solution for Unity",
                keywords={"unity", "networking", "netcode", "multiplayer"},
                related_topics={"unity.networkbehaviour", "unity.networkobject"}
            ),

            "unity.networkbehaviour": DocumentationReference(
                title="NetworkBehaviour",
                type=DocumentationType.UNITY_API,
                priority=DocumentationPriority.CRITICAL,
                url="https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkBehaviour.html",
                summary="Base class for network-synchronized scripts",
                keywords={"unity", "networkbehaviour", "sync", "rpc", "networkvariable"},
                related_topics={"unity.serverrpc", "unity.clientrpc", "unity.networkvariable"}
            ),

            "unity.serverrpc": DocumentationReference(
                title="ServerRpc Attribute",
                type=DocumentationType.UNITY_GUIDE,
                priority=DocumentationPriority.HIGH,
                url="https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc.html",
                summary="Server-authoritative remote procedure calls",
                keywords={"unity", "serverrpc", "rpc", "server", "authority"},
                related_topics={"unity.clientrpc", "unity.networkbehaviour"}
            ),

            # GameCreator Documentation
            "gc.character": DocumentationReference(
                title="Character System - GameCreator",
                type=DocumentationType.GC_GUIDE,
                priority=DocumentationPriority.CRITICAL,
                url="https://docs.gamecreator.io/gamecreator/characters/",
                summary="Complete character movement and animation system",
                keywords={"gc", "character", "movement", "animation", "controller"},
                related_topics={"gc.character.movement", "gc.character.animation"}
            ),

            "gc.inventory": DocumentationReference(
                title="Inventory System - GameCreator",
                type=DocumentationType.GC_GUIDE,
                priority=DocumentationPriority.HIGH,
                url="https://docs.gamecreator.io/gamecreator/inventory/",
                summary="Item management, equipment, and crafting",
                keywords={"gc", "inventory", "item", "equipment", "crafting"},
                related_topics={"gc.inventory.item", "gc.inventory.equipment"}
            ),

            "gc.stats": DocumentationReference(
                title="Stats & Attributes - GameCreator",
                type=DocumentationType.GC_GUIDE,
                priority=DocumentationPriority.HIGH,
                url="https://docs.gamecreator.io/gamecreator/stats/",
                summary="Character attributes, modifiers, and calculations",
                keywords={"gc", "stats", "attribute", "modifier", "formula"},
                related_topics={"gc.stats.attribute", "gc.stats.modifier"}
            ),

            # Multiplayer-Specific Documentation
            "multiplayer.sync": DocumentationReference(
                title="Network Synchronization Patterns",
                type=DocumentationType.MULTIPLAYER_GUIDE,
                priority=DocumentationPriority.CRITICAL,
                url="https://docs-multiplayer.unity3d.com/netcode/current/basics/networkobject/index.html",
                summary="Best practices for network synchronization",
                keywords={"multiplayer", "sync", "network", "state", "prediction"},
                related_topics={"multiplayer.interpolation", "multiplayer.reconciliation"}
            ),

            "network.conflicts": DocumentationReference(
                title="Resolving Network Component Conflicts",
                type=DocumentationType.NETWORKING_TUTORIAL,
                priority=DocumentationPriority.HIGH,
                url="https://docs-multiplayer.unity3d.com/netcode/current/components/networktransform/index.html",
                summary="Avoiding conflicts between network components",
                keywords={"network", "conflict", "transform", "character", "controller"},
                related_topics={"network.networktransform", "network.charactercontroller"}
            ),

            # Best Practices
            "bestpractice.monobehaviour": DocumentationReference(
                title="MonoBehaviour Best Practices",
                type=DocumentationType.UNITY_GUIDE,
                priority=DocumentationPriority.MEDIUM,
                url="https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html",
                summary="Performance optimization for Unity scripts",
                keywords={"unity", "performance", "optimization", "bestpractice"},
                related_topics={"unity.update", "unity.fixedupdate"}
            ),

            "gc.bestpractice": DocumentationReference(
                title="GameCreator Best Practices",
                type=DocumentationType.GC_BEST_PRACTICE,
                priority=DocumentationPriority.HIGH,
                url="https://docs.gamecreator.io/gamecreator/getting-started/best-practices/",
                summary="Optimization and performance guidelines",
                keywords={"gc", "bestpractice", "optimization", "performance"},
                related_topics={"gc.performance", "gc.optimization"}
            )
        }

    def _build_keyword_mappings(self) -> Dict[str, Set[str]]:
        """Build keyword to documentation mapping"""
        return {
            # Unity keywords
            "unity": {"unity.transform", "unity.monobehaviour", "bestpractice.monobehaviour"},
            "network": {"unity.netcode", "unity.networkbehaviour", "multiplayer.sync"},
            "monobehaviour": {"unity.monobehaviour", "bestpractice.monobehaviour"},
            "transform": {"unity.transform"},
            "serverrpc": {"unity.serverrpc"},
            "networkvariable": {"unity.networkbehaviour"},

            # GameCreator keywords
            "gc": {"gc.character", "gc.inventory", "gc.stats", "gc.bestpractice"},
            "character": {"gc.character"},
            "inventory": {"gc.inventory"},
            "stats": {"gc.stats"},

            # Multiplayer keywords
            "multiplayer": {"unity.netcode", "multiplayer.sync"},
            "sync": {"multiplayer.sync", "unity.networkbehaviour"},
            "conflict": {"network.conflicts"},

            # Best practices
            "bestpractice": {"bestpractice.monobehaviour", "gc.bestpractice"},
            "performance": {"bestpractice.monobehaviour", "gc.bestpractice"},
            "optimization": {"bestpractice.monobehaviour", "gc.bestpractice"}
        }

    def get_documentation_for_query(self, query: DocumentationQuery) -> List[DocumentationReference]:
        """
        Retrieve relevant documentation for a query

        Args:
            query: DocumentationQuery with keywords and context

        Returns:
            List of relevant documentation references
        """
        relevant_docs = set()

        # Direct keyword matching
        for keyword in query.keywords:
            if keyword in self.keyword_mappings:
                relevant_docs.update(self.keyword_mappings[keyword])

        # Context-based recommendations
        context_docs = self._get_context_recommendations(query.context, query.user_intent)
        relevant_docs.update(context_docs)

        # Intent-based prioritization
        prioritized_docs = self._prioritize_by_intent(
            [self.documentation_index[doc_id] for doc_id in relevant_docs if doc_id in self.documentation_index],
            query.user_intent
        )

        return prioritized_docs[:10]  # Return top 10 most relevant

    def get_documentation_for_class(self, class_name: str, namespace: str,
                                  keywords: Set[str]) -> List[DocumentationReference]:
        """
        Get documentation relevant to a specific class

        Args:
            class_name: Name of the class
            namespace: Full namespace
            keywords: Keywords associated with the class

        Returns:
            List of relevant documentation
        """
        query = DocumentationQuery(
            keywords=keywords,
            context=self._infer_context_from_class(class_name, namespace),
            user_intent="api_reference"
        )

        return self.get_documentation_for_query(query)

    def _infer_context_from_class(self, class_name: str, namespace: str) -> str:
        """Infer documentation context from class information"""
        context = "general"

        # Unity contexts
        if namespace.startswith("UnityEngine"):
            if "Network" in class_name:
                context = "networking"
            elif "UI" in namespace:
                context = "ui"
            elif "AI" in namespace:
                context = "ai"
            else:
                context = "core"

        # GameCreator contexts
        elif namespace.startswith("GameCreator"):
            if "Character" in namespace:
                context = "character"
            elif "Inventory" in namespace:
                context = "inventory"
            elif "Stats" in namespace:
                context = "stats"
            elif "VisualScripting" in namespace:
                context = "visualscripting"

        # MLCreator contexts
        elif namespace.startswith("MLCreator"):
            if "Multiplayer" in namespace:
                context = "multiplayer"
            elif "VisualScripting" in namespace:
                context = "visualscripting"

        return context

    def _get_context_recommendations(self, context: str, intent: str) -> Set[str]:
        """Get documentation recommendations based on context and intent"""
        recommendations = set()

        # Context-based recommendations
        context_mappings = {
            "character": {"gc.character", "unity.monobehaviour"},
            "inventory": {"gc.inventory", "gc.stats"},
            "networking": {"unity.netcode", "unity.networkbehaviour", "multiplayer.sync"},
            "multiplayer": {"unity.netcode", "multiplayer.sync", "network.conflicts"},
            "visualscripting": {"gc.character", "gc.inventory"},  # VS docs would go here
            "ui": {"unity.transform"},  # UI docs would go here
            "ai": {"unity.monobehaviour"}  # AI docs would go here
        }

        if context in context_mappings:
            recommendations.update(context_mappings[context])

        # Intent-based additions
        intent_mappings = {
            "how_to": {"unity.netcode", "gc.character"},
            "troubleshooting": {"network.conflicts", "bestpractice.monobehaviour"},
            "api_reference": {"unity.monobehaviour", "unity.networkbehaviour"},
            "best_practice": {"bestpractice.monobehaviour", "gc.bestpractice"}
        }

        if intent in intent_mappings:
            recommendations.update(intent_mappings[intent])

        return recommendations

    def _prioritize_by_intent(self, docs: List[DocumentationReference],
                            intent: str) -> List[DocumentationReference]:
        """Prioritize documentation based on user intent"""
        # Sort by priority first, then by relevance to intent
        def sort_key(doc: DocumentationReference) -> Tuple[int, int]:
            # Priority score (higher = more important)
            priority_scores = {
                DocumentationPriority.CRITICAL: 4,
                DocumentationPriority.HIGH: 3,
                DocumentationPriority.MEDIUM: 2,
                DocumentationPriority.LOW: 1
            }

            # Intent relevance score
            intent_relevance = 0
            if intent == "troubleshooting" and "conflict" in doc.keywords:
                intent_relevance = 3
            elif intent == "how_to" and doc.type in [DocumentationType.UNITY_GUIDE, DocumentationType.GC_GUIDE]:
                intent_relevance = 2
            elif intent == "api_reference" and doc.type in [DocumentationType.UNITY_API, DocumentationType.GC_API]:
                intent_relevance = 2

            return (priority_scores[doc.priority], intent_relevance)

        return sorted(docs, key=sort_key, reverse=True)

    def search_documentation(self, search_term: str, doc_type: Optional[DocumentationType] = None,
                           max_results: int = 5) -> List[DocumentationReference]:
        """
        Search documentation by text content

        Args:
            search_term: Term to search for
            doc_type: Optional type filter
            max_results: Maximum results to return

        Returns:
            List of matching documentation
        """
        results = []
        search_lower = search_term.lower()

        for doc_id, doc in self.documentation_index.items():
            # Type filter
            if doc_type and doc.type != doc_type:
                continue

            # Text matching
            searchable_text = f"{doc.title} {doc.summary} {' '.join(doc.keywords)}".lower()

            if search_lower in searchable_text:
                results.append(doc)

        # Sort by priority
        priority_order = [DocumentationPriority.CRITICAL, DocumentationPriority.HIGH,
                         DocumentationPriority.MEDIUM, DocumentationPriority.LOW]

        results.sort(key=lambda x: priority_order.index(x.priority))

        return results[:max_results]

def demonstrate_documentation_retrieval():
    """Demonstrate the documentation retrieval system"""
    retriever = UnityDocumentationRetriever()

    print("Unity Documentation Retrieval System Demo")
    print("=" * 50)

    # Example 1: Class-based retrieval
    print("\n1. Documentation for NetworkCharacterAdapter:")
    docs = retriever.get_documentation_for_class(
        "NetworkCharacterAdapter",
        "MLCreator_Multiplayer.Runtime.Components",
        {"network", "character", "sync", "multiplayer"}
    )

    for i, doc in enumerate(docs[:3], 1):
        print(f"   {i}. {doc.title} ({doc.type.value})")
        print(f"      {doc.summary}")
        if doc.url:
            print(f"      URL: {doc.url}")

    # Example 2: Keyword search
    print("\n2. Search for 'character movement':")
    query = DocumentationQuery(
        keywords={"character", "movement"},
        context="character",
        user_intent="how_to"
    )

    docs = retriever.get_documentation_for_query(query)
    for i, doc in enumerate(docs[:3], 1):
        print(f"   {i}. {doc.title} ({doc.priority.value} priority)")

    # Example 3: Text search
    print("\n3. Text search for 'network':")
    docs = retriever.search_documentation("network", max_results=3)
    for i, doc in enumerate(docs, 1):
        print(f"   {i}. {doc.title} - {doc.summary[:50]}...")

if __name__ == "__main__":
    demonstrate_documentation_retrieval()</content>
</xai:function_call">Mcp_serena_write_memory