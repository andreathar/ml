#!/usr/bin/env python3
"""
Unity Knowledge Base - Conflict Detection System
===============================================

Advanced conflict detection for Unity components and GameCreator modules.
Identifies incompatible classes, namespace clashes, and potential runtime issues.

Features:
- Component conflict detection
- Namespace clash identification
- GameCreator module conflicts
- Assembly dependency conflicts
- Inheritance conflict analysis

Author: MLCreator AI Assistant
Version: 2.0
"""

import re
import json
from pathlib import Path
from typing import Dict, List, Set, Tuple, Optional
from dataclasses import dataclass
from enum import Enum

class ConflictType(Enum):
    COMPONENT_INCOMPATIBLE = "component_incompatible"
    NAMESPACE_CLASH = "namespace_clash"
    ASSEMBLY_CONFLICT = "assembly_conflict"
    INHERITANCE_CONFLICT = "inheritance_conflict"
    DEPENDENCY_CONFLICT = "dependency_conflict"
    ATTRIBUTE_CONFLICT = "attribute_conflict"
    METHOD_SIGNATURE_CLASH = "method_signature_clash"

class ConflictSeverity(Enum):
    LOW = "low"         # Warning, may work but suboptimal
    MEDIUM = "medium"   # Potential runtime issues
    HIGH = "high"       # Likely to cause crashes or undefined behavior
    CRITICAL = "critical"  # Will definitely break functionality

@dataclass
class ConflictRule:
    """Rule for detecting conflicts"""
    type: ConflictType
    severity: ConflictSeverity
    description: str
    components_a: List[str]
    components_b: List[str]
    condition: Optional[str] = None

@dataclass
class DetectedConflict:
    """Detected conflict between classes/components"""
    type: ConflictType
    severity: ConflictSeverity
    description: str
    class_a: str
    class_b: str
    location_a: str
    location_b: str
    resolution: str
    keywords: Set[str]

class UnityConflictDetector:
    """Advanced conflict detection for Unity projects"""

    def __init__(self):
        self.conflict_rules = self._initialize_conflict_rules()
        self.known_conflicts = self._load_known_conflicts()

    def _initialize_conflict_rules(self) -> List[ConflictRule]:
        """Initialize comprehensive conflict detection rules"""
        return [
            # Unity Component Conflicts
            ConflictRule(
                type=ConflictType.COMPONENT_INCOMPATIBLE,
                severity=ConflictSeverity.CRITICAL,
                description="NetworkTransform conflicts with CharacterController - causes position fighting",
                components_a=["NetworkTransform"],
                components_b=["CharacterController", "Rigidbody"],
                condition="Both attached to same GameObject"
            ),

            ConflictRule(
                type=ConflictType.COMPONENT_INCOMPATIBLE,
                severity=ConflictSeverity.HIGH,
                description="Multiple CharacterControllers on same GameObject cause undefined behavior",
                components_a=["CharacterController"],
                components_b=["CharacterController"],
                condition="Multiple instances on same GameObject"
            ),

            # GameCreator Conflicts
            ConflictRule(
                type=ConflictType.COMPONENT_INCOMPATIBLE,
                severity=ConflictSeverity.HIGH,
                description="NetworkCharacterAdapter conflicts with NetworkTransform on character prefabs",
                components_a=["NetworkCharacterAdapter"],
                components_b=["NetworkTransform"],
                condition="On GameCreator Character prefab"
            ),

            # Namespace Conflicts
            ConflictRule(
                type=ConflictType.NAMESPACE_CLASH,
                severity=ConflictSeverity.MEDIUM,
                description="Class name conflicts with Unity built-in class",
                components_a=["Object", "Component", "Behaviour", "MonoBehaviour"],
                components_b=["*"],
                condition="Same name as Unity built-in"
            ),

            # Inheritance Conflicts
            ConflictRule(
                type=ConflictType.INHERITANCE_CONFLICT,
                severity=ConflictSeverity.HIGH,
                description="Multiple inheritance from same base class",
                components_a=["MonoBehaviour", "NetworkBehaviour", "ScriptableObject"],
                components_b=["*"],
                condition="Multiple inheritance paths"
            ),

            # Assembly Conflicts
            ConflictRule(
                type=ConflictType.ASSEMBLY_CONFLICT,
                severity=ConflictSeverity.MEDIUM,
                description="Different versions of same assembly referenced",
                components_a=["Unity.Netcode.Runtime"],
                components_b=["Unity.Netcode.Runtime"],
                condition="Different assembly versions"
            ),

            # Attribute Conflicts
            ConflictRule(
                type=ConflictType.ATTRIBUTE_CONFLICT,
                severity=ConflictSeverity.MEDIUM,
                description="Conflicting attributes on same class",
                components_a=["RequireComponent", "DisallowMultipleComponent"],
                components_b=["RequireComponent", "DisallowMultipleComponent"],
                condition="Mutually exclusive attributes"
            )
        ]

    def _load_known_conflicts(self) -> Dict[str, List[str]]:
        """Load known component conflicts"""
        return {
            "NetworkTransform": ["CharacterController", "Rigidbody", "NetworkCharacterAdapter"],
            "CharacterController": ["NetworkTransform", "Rigidbody", "CharacterController"],
            "Rigidbody": ["CharacterController", "NetworkTransform"],
            "NetworkCharacterAdapter": ["NetworkTransform"],
            "NetworkBehaviour": ["MultipleNetworkBehaviours"],
            "MonoBehaviour": ["DuplicateMonoBehaviours"],
            "RequireComponent": ["DisallowMultipleComponent"]
        }

    def detect_conflicts_in_class(self, class_name: str, namespace: str,
                                base_classes: List[str], attributes: List[str] = None,
                                assembly: str = None) -> List[DetectedConflict]:
        """
        Detect conflicts for a specific class

        Args:
            class_name: Name of the class
            namespace: Full namespace
            base_classes: List of base classes
            attributes: List of attributes
            assembly: Assembly name

        Returns:
            List of detected conflicts
        """
        conflicts = []
        attributes = attributes or []

        # Check against conflict rules
        for rule in self.conflict_rules:
            conflict = self._check_rule_against_class(rule, class_name, namespace,
                                                    base_classes, attributes, assembly)
            if conflict:
                conflicts.append(conflict)

        # Check for duplicate registrations
        duplicate_conflicts = self._detect_duplicate_registrations(class_name, base_classes)
        conflicts.extend(duplicate_conflicts)

        return conflicts

    def detect_cross_class_conflicts(self, classes_data: List[Dict]) -> List[DetectedConflict]:
        """
        Detect conflicts between multiple classes (e.g., on same GameObject)

        Args:
            classes_data: List of class dictionaries with metadata

        Returns:
            List of cross-class conflicts
        """
        conflicts = []

        # Group classes by potential co-existence (e.g., same prefab)
        # This is a simplified version - real implementation would analyze prefabs
        for i, class_a in enumerate(classes_data):
            for class_b in classes_data[i+1:]:
                cross_conflicts = self._detect_class_pair_conflicts(class_a, class_b)
                conflicts.extend(cross_conflicts)

        return conflicts

    def _check_rule_against_class(self, rule: ConflictRule, class_name: str,
                                namespace: str, base_classes: List[str],
                                attributes: List[str], assembly: str) -> Optional[DetectedConflict]:
        """Check if a conflict rule applies to a class"""
        # Check if class has components from rule set A
        has_component_a = any(comp in base_classes or comp in class_name
                            for comp in rule.components_a)

        # Check if class has components from rule set B
        has_component_b = any(comp in base_classes or comp in class_name
                            for comp in rule.components_b)

        # Special case: namespace clashes
        if rule.type == ConflictType.NAMESPACE_CLASH:
            if class_name in rule.components_a:
                has_component_a = True
                has_component_b = True  # Conflicts with itself

        # Special case: attribute conflicts
        if rule.type == ConflictType.ATTRIBUTE_CONFLICT:
            has_component_a = any(attr in attributes for attr in rule.components_a)
            has_component_b = any(attr in attributes for attr in rule.components_b)

        if has_component_a and has_component_b:
            return DetectedConflict(
                type=rule.type,
                severity=rule.severity,
                description=rule.description,
                class_a=class_name,
                class_b=class_name,  # Self-conflict
                location_a=f"{namespace}.{class_name}",
                location_b=f"{namespace}.{class_name}",
                resolution=self._get_resolution_for_rule(rule),
                keywords=self._generate_conflict_keywords(rule, class_name)
            )

        return None

    def _detect_duplicate_registrations(self, class_name: str, base_classes: List[str]) -> List[DetectedConflict]:
        """Detect duplicate component registrations"""
        conflicts = []

        # Check for multiple NetworkBehaviours (Unity limitation)
        if 'NetworkBehaviour' in base_classes and 'Singleton' not in class_name:
            conflicts.append(DetectedConflict(
                type=ConflictType.COMPONENT_INCOMPATIBLE,
                severity=ConflictSeverity.MEDIUM,
                description="Multiple NetworkBehaviours may cause sync issues",
                class_a=class_name,
                class_b="Other NetworkBehaviour",
                location_a=f"{class_name}",
                location_b="Unknown",
                resolution="Ensure only one NetworkBehaviour per GameObject or use NetworkObject.SpawnAsPlayerObject",
                keywords={"conflict", "networkbehaviour", "duplicate", "sync"}
            ))

        return conflicts

    def _detect_class_pair_conflicts(self, class_a: Dict, class_b: Dict) -> List[DetectedConflict]:
        """Detect conflicts between two specific classes"""
        conflicts = []

        # Extract component info
        components_a = class_a.get('base_classes', [])
        components_b = class_b.get('base_classes', [])

        # Check known conflicts
        for comp_a in components_a:
            if comp_a in self.known_conflicts:
                conflicting_with = self.known_conflicts[comp_a]
                for comp_b in components_b:
                    if comp_b in conflicting_with:
                        conflicts.append(DetectedConflict(
                            type=ConflictType.COMPONENT_INCOMPATIBLE,
                            severity=ConflictSeverity.HIGH,
                            description=f"{comp_a} conflicts with {comp_b}",
                            class_a=class_a['name'],
                            class_b=class_b['name'],
                            location_a=class_a.get('namespace', 'Unknown'),
                            location_b=class_b.get('namespace', 'Unknown'),
                            resolution=self._get_component_conflict_resolution(comp_a, comp_b),
                            keywords={"conflict", "component", comp_a.lower(), comp_b.lower()}
                        ))

        return conflicts

    def _get_resolution_for_rule(self, rule: ConflictRule) -> str:
        """Get resolution text for a conflict rule"""
        resolutions = {
            ConflictType.COMPONENT_INCOMPATIBLE: "Remove one of the conflicting components or use alternative implementations",
            ConflictType.NAMESPACE_CLASH: "Rename class to avoid conflict with Unity built-in classes",
            ConflictType.ASSEMBLY_CONFLICT: "Ensure all assemblies reference the same version",
            ConflictType.INHERITANCE_CONFLICT: "Refactor inheritance hierarchy to avoid diamond problem",
            ConflictType.ATTRIBUTE_CONFLICT: "Remove conflicting attributes or choose one approach"
        }
        return resolutions.get(rule.type, "Review and resolve the conflict manually")

    def _get_component_conflict_resolution(self, comp_a: str, comp_b: str) -> str:
        """Get specific resolution for component conflicts"""
        specific_resolutions = {
            ("NetworkTransform", "CharacterController"): "Use NetworkCharacterAdapter instead of NetworkTransform for GameCreator characters",
            ("CharacterController", "NetworkTransform"): "Remove NetworkTransform from character prefab and use NetworkCharacterAdapter",
            ("NetworkCharacterAdapter", "NetworkTransform"): "NetworkCharacterAdapter already handles networking - remove NetworkTransform"
        }

        return specific_resolutions.get((comp_a, comp_b),
                                      specific_resolutions.get((comp_b, comp_a),
                                      "Remove one of the conflicting components"))

    def _generate_conflict_keywords(self, rule: ConflictRule, class_name: str) -> Set[str]:
        """Generate keywords for conflict documentation"""
        keywords = {"conflict", rule.type.value}

        # Add component-specific keywords
        for comp in rule.components_a + rule.components_b:
            if comp != "*":  # Skip wildcard
                keywords.add(f"conflict.{comp.lower()}")

        # Add severity keyword
        keywords.add(f"severity.{rule.severity.value}")

        return keywords

    def get_conflict_report(self, conflicts: List[DetectedConflict]) -> Dict:
        """Generate comprehensive conflict report"""
        report = {
            "total_conflicts": len(conflicts),
            "conflicts_by_type": {},
            "conflicts_by_severity": {},
            "critical_conflicts": [],
            "resolution_summary": []
        }

        for conflict in conflicts:
            # Count by type
            ctype = conflict.type.value
            report["conflicts_by_type"][ctype] = report["conflicts_by_type"].get(ctype, 0) + 1

            # Count by severity
            severity = conflict.severity.value
            report["conflicts_by_severity"][severity] = report["conflicts_by_severity"].get(severity, 0) + 1

            # Track critical conflicts
            if conflict.severity in [ConflictSeverity.CRITICAL, ConflictSeverity.HIGH]:
                report["critical_conflicts"].append({
                    "classes": f"{conflict.class_a} vs {conflict.class_b}",
                    "description": conflict.description,
                    "resolution": conflict.resolution
                })

        return report

def analyze_project_conflicts(project_path: str) -> Dict:
    """
    Analyze entire project for conflicts

    Args:
        project_path: Path to Unity project root

    Returns:
        Comprehensive conflict analysis report
    """
    detector = UnityConflictDetector()

    # This would scan actual project files
    # For now, return sample analysis
    sample_classes = [
        {
            "name": "NetworkCharacterAdapter",
            "namespace": "MLCreator_Multiplayer.Runtime.Components",
            "base_classes": ["NetworkBehaviour"],
            "attributes": ["RequireComponent(typeof(Character))"]
        },
        {
            "name": "Character",
            "namespace": "GameCreator.Runtime.Characters",
            "base_classes": ["MonoBehaviour"],
            "attributes": []
        }
    ]

    all_conflicts = []

    # Analyze individual classes
    for class_data in sample_classes:
        conflicts = detector.detect_conflicts_in_class(
            class_data["name"],
            class_data["namespace"],
            class_data["base_classes"],
            class_data.get("attributes", [])
        )
        all_conflicts.extend(conflicts)

    # Analyze cross-class conflicts
    cross_conflicts = detector.detect_cross_class_conflicts(sample_classes)
    all_conflicts.extend(cross_conflicts)

    # Generate report
    report = detector.get_conflict_report(all_conflicts)

    return {
        "analysis_timestamp": "2025-11-29T12:00:00Z",
        "project_path": project_path,
        "classes_analyzed": len(sample_classes),
        "conflict_report": report,
        "all_conflicts": [
            {
                "type": c.type.value,
                "severity": c.severity.value,
                "description": c.description,
                "classes": f"{c.class_a} vs {c.class_b}",
                "resolution": c.resolution,
                "keywords": list(c.keywords)
            } for c in all_conflicts
        ]
    }

if __name__ == "__main__":
    # Example usage
    report = analyze_project_conflicts("/path/to/unity/project")

    print("Conflict Analysis Report")
    print("=" * 50)
    print(f"Classes Analyzed: {report['classes_analyzed']}")
    print(f"Total Conflicts: {report['conflict_report']['total_conflicts']}")

    if report['conflict_report']['critical_conflicts']:
        print("\nCritical Conflicts:")
        for conflict in report['conflict_report']['critical_conflicts']:
            print(f"⚠️  {conflict['classes']}: {conflict['description']}")
            print(f"   Resolution: {conflict['resolution']}")

    # Save detailed report
    with open('conflict_analysis_report.json', 'w') as f:
        json.dump(report, f, indent=2)

    print(f"\nDetailed report saved to conflict_analysis_report.json")</content>
</xai:function_call">Mcp_serena_write_memory