#!/usr/bin/env python3
"""
Qdrant Unity Knowledge Base - Status Checker
============================================

Checks the status of the Qdrant container and knowledge base system.
Works even when terminal output is problematic - writes results to file.

Usage:
    python qdrant-status.py [--output FILE] [--fix]
"""

import os
import json
import urllib.request
import urllib.error
from pathlib import Path
from datetime import datetime


class QdrantStatusChecker:
    """Status checker for Qdrant Unity KB system."""

    def __init__(self):
        self.status = {
            'timestamp': datetime.now().isoformat(),
            'checks': {},
            'summary': {},
            'recommendations': []
        }

    def check_docker(self):
        """Check if Docker is available."""
        try:
            # Try to run docker version
            result = os.system('docker --version >nul 2>&1')
            if result == 0:
                self.status['checks']['docker_installed'] = True
                return True
            else:
                self.status['checks']['docker_installed'] = False
                self.status['recommendations'].append("Install Docker Desktop from https://www.docker.com/products/docker-desktop")
                return False
        except:
            self.status['checks']['docker_installed'] = False
            return False

    def check_docker_daemon(self):
        """Check if Docker daemon is running."""
        try:
            result = os.system('docker info >nul 2>&1')
            if result == 0:
                self.status['checks']['docker_daemon'] = True
                return True
            else:
                self.status['checks']['docker_daemon'] = False
                self.status['recommendations'].append("Start Docker Desktop application")
                return False
        except:
            self.status['checks']['docker_daemon'] = False
            return False

    def check_qdrant_container(self):
        """Check Qdrant container status."""
        try:
            # Try to list containers with qdrant in name
            result = os.system('docker ps -a --filter "name=qdrant-unity-kb" --format "{{.Names}}" > temp_container_check.txt 2>&1')

            if os.path.exists('temp_container_check.txt'):
                with open('temp_container_check.txt', 'r') as f:
                    content = f.read()
                    os.remove('temp_container_check.txt')

                    if 'qdrant-unity-kb' in content:
                        self.status['checks']['container_exists'] = True

                        # Check if it's running
                        result = os.system('docker ps --filter "name=qdrant-unity-kb" --format "{{.Names}}" > temp_running_check.txt 2>&1')

                        if os.path.exists('temp_running_check.txt'):
                            with open('temp_running_check.txt', 'r') as f:
                                running_content = f.read()
                                os.remove('temp_running_check.txt')

                                if 'qdrant-unity-kb' in running_content:
                                    self.status['checks']['container_running'] = True
                                    return True
                                else:
                                    self.status['checks']['container_running'] = False
                                    self.status['recommendations'].append("Start Qdrant container: docker start qdrant-unity-kb")
                                    return False
                        else:
                            self.status['checks']['container_running'] = False
                            return False
                    else:
                        self.status['checks']['container_exists'] = False
                        self.status['recommendations'].append("Create Qdrant container: .\\scripts\\unity-kb\\setup-qdrant.ps1")
                        return False
            else:
                self.status['checks']['container_exists'] = False
                return False

        except Exception as e:
            self.status['checks']['container_status_error'] = str(e)
            return False

    def check_qdrant_health(self):
        """Check Qdrant health endpoint."""
        try:
            with urllib.request.urlopen('http://localhost:6333/health', timeout=10) as response:
                data = json.loads(response.read().decode())
                self.status['checks']['qdrant_health'] = True
                self.status['checks']['qdrant_version'] = data.get('version', 'unknown')
                return True
        except urllib.error.URLError as e:
            self.status['checks']['qdrant_health'] = False
            self.status['recommendations'].append(f"Qdrant not responding: {e.reason}. Ensure container is running on port 6333")
            return False
        except Exception as e:
            self.status['checks']['qdrant_health'] = False
            self.status['recommendations'].append(f"Qdrant health check failed: {e}")
            return False

    def check_collections(self):
        """Check Qdrant collections."""
        try:
            with urllib.request.urlopen('http://localhost:6333/collections', timeout=10) as response:
                data = json.loads(response.read().decode())
                collections = data.get('result', {}).get('collections', [])
                self.status['checks']['collections_accessible'] = True
                self.status['checks']['collection_count'] = len(collections)

                # Check for unity_project_kb collection
                collection_names = [c['name'] for c in collections]
                if 'unity_project_kb' in collection_names:
                    self.status['checks']['unity_kb_collection_exists'] = True
                else:
                    self.status['checks']['unity_kb_collection_exists'] = False
                    self.status['recommendations'].append("Index Unity codebase to create unity_project_kb collection")

                return True
        except:
            self.status['checks']['collections_accessible'] = False
            return False

    def check_files(self):
        """Check if required files exist."""
        files_to_check = [
            'scripts/unity-kb/qdrant_unity_indexer.py',
            'scripts/unity-kb/direct_mcp_server.py',
            'scripts/unity-kb/setup-qdrant.ps1',
            'scripts/unity-kb/qdrant-config.yaml',
            '.mcp.json'
        ]

        missing_files = []
        for file_path in files_to_check:
            if not Path(file_path).exists():
                missing_files.append(file_path)

        self.status['checks']['files_present'] = len(missing_files) == 0
        if missing_files:
            self.status['checks']['missing_files'] = missing_files
            self.status['recommendations'].append(f"Missing files: {', '.join(missing_files)}")

        return len(missing_files) == 0

    def calculate_health_score(self):
        """Calculate overall health score."""
        checks = self.status['checks']

        # Define weights for different checks
        weights = {
            'docker_installed': 15,
            'docker_daemon': 15,
            'container_exists': 10,
            'container_running': 15,
            'qdrant_health': 20,
            'collections_accessible': 10,
            'files_present': 10,
            'unity_kb_collection_exists': 5
        }

        total_score = 0
        max_score = sum(weights.values())

        for check, weight in weights.items():
            if checks.get(check, False):
                total_score += weight

        health_percentage = (total_score / max_score) * 100

        self.status['summary'] = {
            'health_score': round(health_percentage, 1),
            'status': 'HEALTHY' if health_percentage >= 80 else 'DEGRADED' if health_percentage >= 60 else 'CRITICAL',
            'checks_passed': sum(1 for check in weights.keys() if checks.get(check, False)),
            'total_checks': len(weights)
        }

        return health_percentage

    def run_all_checks(self):
        """Run all status checks."""
        print("🔍 Checking Qdrant Unity Knowledge Base Status...")

        self.check_docker()
        self.check_docker_daemon()
        self.check_qdrant_container()
        self.check_qdrant_health()
        self.check_collections()
        self.check_files()

        self.calculate_health_score()

        return self.status

    def save_report(self, output_file="qdrant_status_report.json"):
        """Save status report to file."""
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(self.status, f, indent=2, ensure_ascii=False)

        print(f"📊 Status report saved to: {output_file}")

    def print_summary(self):
        """Print status summary."""
        summary = self.status['summary']

        print(f"\n🏥 System Health: {summary['health_score']}% - {summary['status']}")
        print(f"✅ Checks Passed: {summary['checks_passed']}/{summary['total_checks']}")

        # Print check results
        print("\n📋 Check Results:")
        for check, result in self.status['checks'].items():
            status = "✅" if result else "❌"
            print(f"  {status} {check.replace('_', ' ').title()}")

        # Print recommendations
        if self.status['recommendations']:
            print("\n💡 Recommendations:")
            for rec in self.status['recommendations'][:5]:  # Show first 5
                print(f"  • {rec}")

        # Next steps
        health_score = summary['health_score']
        print("
🎯 Next Steps:"        if health_score >= 80:
            print("  ✅ System is healthy! Ready for indexing and search.")
            print("     Run: python scripts/unity-kb/qdrant_unity_indexer.py --verbose")
        elif health_score >= 60:
            print("  ⚠️  Minor issues detected. Try troubleshooting:")
            print("     python scripts/unity-kb/qdrant-status.py --fix")
        else:
            print("  ❌ Critical issues require attention:")
            print("     1. Ensure Docker Desktop is running")
            print("     2. Run: .\\scripts\\unity-kb\\setup-qdrant.ps1")
            print("     3. Check: python scripts/unity-kb/qdrant-status.py")


def main():
    """Main entry point."""
    import argparse

    parser = argparse.ArgumentParser(description="Qdrant Unity KB Status Checker")
    parser.add_argument("--output", default="qdrant_status_report.json",
                       help="Output JSON file")
    parser.add_argument("--fix", action="store_true",
                       help="Attempt automatic fixes")
    parser.add_argument("--quiet", action="store_true",
                       help="Don't print summary to console")

    args = parser.parse_args()

    checker = QdrantStatusChecker()
    status = checker.run_all_checks()

    # Save report
    checker.save_report(args.output)

    # Print summary unless quiet
    if not args.quiet:
        checker.print_summary()

    # Attempt fixes if requested
    if args.fix and status['summary']['health_score'] < 80:
        print("\n🔧 Attempting automatic fixes...")

        # Try to start container if it exists but isn't running
        if (status['checks'].get('container_exists') and
            not status['checks'].get('container_running')):
            print("Starting Qdrant container...")
            os.system('docker start qdrant-unity-kb >nul 2>&1')

            # Wait and recheck
            import time
            time.sleep(5)

            if checker.check_qdrant_health():
                print("✅ Container started successfully!")
            else:
                print("❌ Container failed to start")

    return 0 if status['summary']['health_score'] >= 80 else 1


if __name__ == "__main__":
    exit(main())</contents>
</xai:function_call">Write