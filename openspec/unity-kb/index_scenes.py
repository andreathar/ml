#!/usr/bin/env python3
"""
Unity Scene Batch Indexer
==========================
Batch index Unity scenes into the Qdrant knowledge base.

Usage:
    python index_scenes.py [options] [scene_paths...]

Options:
    --all-scenes       Index all .unity files in Assets/ directory
    --recursive        Recursively find scenes in subdirectories
    --exclude PATTERN  Exclude files matching pattern (can be used multiple times)
    --dry-run          Show what would be indexed without actually indexing
    --force            Re-index scenes that are already indexed
    --verbose          Show detailed progress information

Examples:
    # Index all scenes in Assets/
    python index_scenes.py --all-scenes

    # Index specific scenes
    python index_scenes.py Assets/Scenes/MainMenu.unity Assets/Scenes/Gameplay.unity

    # Index scenes excluding samples
    python index_scenes.py --all-scenes --exclude "*/Samples/*"

    # Dry run to see what would be indexed
    python index_scenes.py --all-scenes --dry-run
"""

import os
import sys
import glob
import argparse
from pathlib import Path
from typing import List, Set

# Add parent directory to path for imports
sys.path.append(os.path.dirname(__file__))

from scene_parser import UnitySceneParser
from direct_mcp_server import KnowledgeBase, Config


class SceneIndexer:
    """Batch indexer for Unity scenes."""

    def __init__(self, kb: KnowledgeBase, verbose: bool = False):
        self.kb = kb
        self.verbose = verbose
        self.indexed_scenes: Set[str] = set()

    def index_scene(self, scene_path: str, force: bool = False, dry_run: bool = False) -> bool:
        """Index a single scene file."""
        scene_path = os.path.abspath(scene_path)

        if not os.path.exists(scene_path):
            print(f"⚠️  Scene file not found: {scene_path}")
            return False

        scene_name = Path(scene_path).stem

        if scene_name in self.indexed_scenes and not force:
            if self.verbose:
                print(f"⏭️  Skipping already indexed scene: {scene_name}")
            return True

        if dry_run:
            print(f"📋 Would index: {scene_path}")
            return True

        if self.verbose:
            print(f"🔄 Indexing scene: {scene_name}")

        success = self.kb.index_scene_file(scene_path)

        if success:
            self.indexed_scenes.add(scene_name)
            print(f"✅ Indexed: {scene_name}")
            return True
        else:
            print(f"❌ Failed to index: {scene_name}")
            return False

    def find_scenes(self, base_path: str = "Assets", recursive: bool = True,
                   exclude_patterns: List[str] = None) -> List[str]:
        """Find all .unity files in the specified path."""
        if exclude_patterns is None:
            exclude_patterns = []

        pattern = "**/*.unity" if recursive else "*.unity"
        search_path = os.path.join(base_path, pattern)

        scenes = glob.glob(search_path, recursive=recursive)

        # Apply exclusions
        filtered_scenes = []
        for scene in scenes:
            excluded = False
            for pattern in exclude_patterns:
                if pattern in scene:
                    excluded = True
                    break
            if not excluded:
                filtered_scenes.append(scene)

        return sorted(filtered_scenes)


def main():
    parser = argparse.ArgumentParser(
        description="Batch index Unity scenes into Qdrant knowledge base",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__
    )

    parser.add_argument(
        'scenes', nargs='*',
        help='Specific scene files to index'
    )

    parser.add_argument(
        '--all-scenes', action='store_true',
        help='Index all .unity files in Assets/ directory'
    )

    parser.add_argument(
        '--recursive', action='store_true', default=True,
        help='Recursively find scenes in subdirectories'
    )

    parser.add_argument(
        '--exclude', action='append', default=[],
        help='Exclude files matching pattern (can be used multiple times)'
    )

    parser.add_argument(
        '--dry-run', action='store_true',
        help='Show what would be indexed without actually indexing'
    )

    parser.add_argument(
        '--force', action='store_true',
        help='Re-index scenes that are already indexed'
    )

    parser.add_argument(
        '--verbose', '-v', action='store_true',
        help='Show detailed progress information'
    )

    args = parser.parse_args()

    # Validate arguments
    if not args.all_scenes and not args.scenes:
        parser.error("Either specify --all-scenes or provide specific scene paths")

    if args.all_scenes and args.scenes:
        parser.error("Cannot specify both --all-scenes and specific scene paths")

    # Initialize knowledge base
    print("🔌 Connecting to Qdrant knowledge base...")
    config = Config.from_env()
    kb = KnowledgeBase(config)

    if not kb.connect():
        print("❌ Failed to connect to Qdrant. Check your configuration.")
        sys.exit(1)

    indexer = SceneIndexer(kb, args.verbose)

    # Determine which scenes to index
    if args.all_scenes:
        print("🔍 Finding all Unity scenes...")
        scenes_to_index = indexer.find_scenes(
            base_path="Assets",
            recursive=args.recursive,
            exclude_patterns=args.exclude
        )

        if not scenes_to_index:
            print("⚠️  No scenes found to index.")
            return

        print(f"📋 Found {len(scenes_to_index)} scenes to index:")
        for scene in scenes_to_index[:10]:  # Show first 10
            print(f"   {scene}")
        if len(scenes_to_index) > 10:
            print(f"   ... and {len(scenes_to_index) - 10} more")

    else:
        scenes_to_index = args.scenes

    # Index scenes
    print(f"\n🚀 {'Dry run: ' if args.dry_run else ''}Starting indexing...")
    successful = 0
    failed = 0

    for scene_path in scenes_to_index:
        if indexer.index_scene(scene_path, args.force, args.dry_run):
            successful += 1
        else:
            failed += 1

    # Summary
    if args.dry_run:
        print(f"\n📋 Dry run complete: {successful} scenes would be indexed")
    else:
        print("\n📊 Indexing complete:")
        print(f"   ✅ Successful: {successful}")
        if failed > 0:
            print(f"   ❌ Failed: {failed}")

        if successful > 0:
            print("\n🔍 You can now search indexed scenes using:")
            print("   - search_unity_scenes() - Search across all indexed scenes")
            print("   - get_scene_components() - Analyze specific scene components")
            print("   - get_kb_health_status() - Check indexing statistics")


if __name__ == "__main__":
    main()