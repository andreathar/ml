#!/usr/bin/env python3
"""
Unity KB Memory Validator

Validates Serena memories against current Unity Knowledge Base state.
Detects outdated, missing, and new references.

Usage:
    # Validate all AUTO_GENERATED memories
    python validate_memories.py --tier AUTO_GENERATED

    # Validate specific memory
    python validate_memories.py --memory api_networkcharacteradapter

    # Validate all CRITICAL memories
    python validate_memories.py --tier CRITICAL

    # Full validation report
    python validate_memories.py --full-report
"""

import argparse
import json
import re
import sys
from datetime import datetime, timedelta
from pathlib import Path
from typing import Dict, List, Tuple

# Add project root to path
PROJECT_ROOT = Path(__file__).parent.parent.parent
sys.path.insert(0, str(PROJECT_ROOT))

try:
    from scripts.unity_kb.mcp_client import (
        search_unity_symbols,
        get_unity_class_members,
    )
    MCP_AVAILABLE = True
except ImportError:
    print("WARNING: MCP client not available")
    MCP_AVAILABLE = False


class MemoryValidator:
    """Validates Serena memories against Unity KB"""

    def __init__(self, memory_root: Path):
        self.memory_root = memory_root
        self.validation_results = {
            "valid": [],
            "outdated": [],
            "missing": [],
            "new_additions": [],
            "stale": []
        }

    def validate_memory_file(self, memory_file: Path) -> Dict:
        """
        Validate a single memory file.

        Returns:
            Dictionary with validation results
        """
        print(f"🔍 Validating: {memory_file.name}")

        content = memory_file.read_text(encoding='utf-8')

        # Extract last updated timestamp
        updated_match = re.search(r'\*\*Last Updated:\*\* (.+)', content)
        if updated_match:
            try:
                last_updated = datetime.fromisoformat(updated_match.group(1).strip())
                age_days = (datetime.now() - last_updated).days

                if age_days > 7:
                    print(f"  ⚠️ STALE: {age_days} days old (>7 days)")
                    self.validation_results["stale"].append({
                        "file": str(memory_file),
                        "age_days": age_days,
                        "last_updated": last_updated.isoformat()
                    })
            except Exception as e:
                print(f"  ⚠️ Could not parse timestamp: {e}")

        # Extract code references (class names, methods, properties)
        references = self._extract_code_references(content)

        print(f"  Found {len(references)} code references to validate")

        results = {
            "file": str(memory_file),
            "valid_refs": [],
            "outdated_refs": [],
            "missing_refs": [],
            "total_refs": len(references)
        }

        for ref in references:
            validation = self._validate_reference(ref, content)

            if validation["status"] == "valid":
                results["valid_refs"].append(ref)
            elif validation["status"] == "outdated":
                results["outdated_refs"].append(validation)
            elif validation["status"] == "missing":
                results["missing_refs"].append(validation)

        # Summary
        if results["missing_refs"]:
            print(f"  ❌ {len(results['missing_refs'])} missing references")
        if results["outdated_refs"]:
            print(f"  ⚠️ {len(results['outdated_refs'])} outdated references")
        if results["valid_refs"] and not results["missing_refs"] and not results["outdated_refs"]:
            print(f"  ✅ All {len(results['valid_refs'])} references valid")

        return results

    def _extract_code_references(self, content: str) -> List[str]:
        """
        Extract code references from memory content.

        Looks for:
        - Class names in headers
        - Method/property names in tables
        - Code blocks
        """
        references = set()

        # Extract from headers (e.g., "# NetworkCharacterAdapter API Reference")
        for match in re.finditer(r'^#+ ([A-Z][A-Za-z0-9_]+)', content, re.MULTILINE):
            references.add(match.group(1))

        # Extract from tables (|MethodName|...)
        for match in re.finditer(r'\| ([A-Z][A-Za-z0-9_]+) \|', content):
            references.add(match.group(1))

        # Extract class names from "**Assembly:**" lines
        assembly_match = re.search(r'\*\*Assembly:\*\* (.+)', content)
        if assembly_match:
            # Store assembly for context
            pass

        return list(references)

    def _validate_reference(self, ref: str, memory_content: str) -> Dict:
        """
        Validate a single code reference against Unity KB.

        Returns:
            Dictionary with validation status
        """
        # Search Unity KB for reference
        try:
            kb_results = search_unity_symbols(query=ref, limit=1)

            if not kb_results:
                return {
                    "reference": ref,
                    "status": "missing",
                    "message": "Symbol no longer exists in codebase"
                }

            symbol = kb_results[0]

            # Check if signature matches (if present in memory)
            memory_sig = self._extract_signature_from_memory(memory_content, ref)
            kb_sig = symbol.get("signature", "")

            if memory_sig and kb_sig and memory_sig != kb_sig:
                return {
                    "reference": ref,
                    "status": "outdated",
                    "memory_signature": memory_sig,
                    "actual_signature": kb_sig,
                    "location": f"{symbol.get('file_path', '')}:{symbol.get('start_line', '')}"
                }

            return {
                "reference": ref,
                "status": "valid"
            }

        except Exception as e:
            print(f"  ⚠️ Error validating {ref}: {e}")
            return {
                "reference": ref,
                "status": "error",
                "message": str(e)
            }

    def _extract_signature_from_memory(self, content: str, ref: str) -> str:
        """Extract signature for a reference from memory content"""

        # Look for signature in code blocks or tables
        pattern = rf"`([^`]*{re.escape(ref)}[^`]*)`"
        match = re.search(pattern, content)

        if match:
            return match.group(1).strip()

        return ""

    def validate_tier(self, tier_name: str) -> Dict:
        """Validate all memories in a tier"""

        tier_dir = self.memory_root / tier_name

        if not tier_dir.exists():
            print(f"❌ Tier directory not found: {tier_dir}")
            return {}

        print(f"\n📁 Validating tier: {tier_name}")

        memory_files = list(tier_dir.glob("*.md"))
        if tier_name == "AUTO_GENERATED":
            # Exclude README
            memory_files = [f for f in memory_files if f.name != "README.md"]

        print(f"Found {len(memory_files)} memory files")

        tier_results = {
            "tier": tier_name,
            "total_files": len(memory_files),
            "validations": []
        }

        for memory_file in memory_files:
            result = self.validate_memory_file(memory_file)
            tier_results["validations"].append(result)

        return tier_results

    def full_report(self) -> Dict:
        """Generate full validation report for all tiers"""

        print("🔬 Running full validation report...\n")

        tiers_to_validate = ["CRITICAL", "CORE", "INTEGRATION", "AUTO_GENERATED"]

        full_results = {
            "timestamp": datetime.now().isoformat(),
            "tiers": []
        }

        for tier in tiers_to_validate:
            tier_result = self.validate_tier(tier)
            if tier_result:
                full_results["tiers"].append(tier_result)

        # Summary
        print("\n" + "="*60)
        print("VALIDATION SUMMARY")
        print("="*60)

        total_files = sum(t["total_files"] for t in full_results["tiers"])
        print(f"Total files validated: {total_files}")

        if self.validation_results["stale"]:
            print(f"\n⚠️ STALE MEMORIES ({len(self.validation_results['stale'])}):")
            for stale in self.validation_results["stale"]:
                print(f"  - {Path(stale['file']).name} ({stale['age_days']} days old)")

        return full_results


def main():
    parser = argparse.ArgumentParser(description='Validate Serena memories against Unity KB')
    parser.add_argument('--tier', type=str, help='Validate specific tier (CRITICAL, CORE, etc.)')
    parser.add_argument('--memory', type=str, help='Validate specific memory file (by name)')
    parser.add_argument('--full-report', action='store_true', help='Generate full validation report')

    args = parser.parse_args()

    # Find memory root
    if (PROJECT_ROOT / ".serena" / "memories").exists():
        memory_root = PROJECT_ROOT / ".serena" / "memories"
    elif (Path("D:/UnityWorkspaces/MLcreator/.serena/memories")).exists():
        memory_root = Path("D:/UnityWorkspaces/MLcreator/.serena/memories")
    else:
        print("ERROR: Cannot find Serena memories directory")
        sys.exit(1)

    validator = MemoryValidator(memory_root)

    if args.full_report:
        report = validator.full_report()

        # Save report
        report_file = PROJECT_ROOT / "claudedocs" / "reports" / f"memory_validation_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        report_file.parent.mkdir(parents=True, exist_ok=True)
        report_file.write_text(json.dumps(report, indent=2), encoding='utf-8')

        print(f"\n📄 Report saved to: {report_file}")

    elif args.tier:
        validator.validate_tier(args.tier)

    elif args.memory:
        # Find memory file
        memory_file = None
        for tier_dir in memory_root.iterdir():
            if tier_dir.is_dir():
                candidate = tier_dir / f"{args.memory}.md"
                if candidate.exists():
                    memory_file = candidate
                    break

        if memory_file:
            validator.validate_memory_file(memory_file)
        else:
            print(f"❌ Memory not found: {args.memory}")

    else:
        parser.print_help()


if __name__ == "__main__":
    main()
