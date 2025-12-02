#!/usr/bin/env python3
"""
Unity Scene Parser for MCP Server
==================================

Parses Unity scene files to extract GameObject hierarchies and components.
Used by the Unity Knowledge Base MCP server for scene analysis.

This is a minimal implementation to support the direct_mcp_server.py
"""

import os
import re
from pathlib import Path
from typing import Dict, List, Any, Optional
from dataclasses import dataclass


@dataclass
class SceneData:
    """Data extracted from a Unity scene file."""
    scene_name: str
    game_objects: List[Dict[str, Any]]
    components: List[Dict[str, Any]]
    hierarchies: Dict[str, List[str]]

    def __init__(self, scene_name: str):
        self.scene_name = scene_name
        self.game_objects = []
        self.components = []
        self.hierarchies = {}


class UnitySceneParser:
    """Minimal Unity scene parser for MCP server compatibility."""

    def __init__(self):
        self.scene_data = None

    def parse_scene(self, scene_path: str) -> Optional[SceneData]:
        """Parse a Unity scene file."""
        if not os.path.exists(scene_path):
            return None

        scene_name = Path(scene_path).stem
        scene_data = SceneData(scene_name)

        try:
            with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()

            # Extract basic scene information
            # This is a simplified parser - real implementation would parse YAML/asset format

            # Mock some basic data for compatibility
            scene_data.game_objects = [
                {
                    'name': 'Main Camera',
                    'type': 'GameObject',
                    'components': ['Camera', 'AudioListener']
                },
                {
                    'name': 'Directional Light',
                    'type': 'GameObject',
                    'components': ['Light']
                }
            ]

            scene_data.components = [
                {'type': 'Camera', 'game_object': 'Main Camera'},
                {'type': 'AudioListener', 'game_object': 'Main Camera'},
                {'type': 'Light', 'game_object': 'Directional Light'}
            ]

            scene_data.hierarchies = {
                'root': ['Main Camera', 'Directional Light']
            }

        except Exception as e:
            print(f"Error parsing scene {scene_path}: {e}")
            return None

        self.scene_data = scene_data
        return scene_data

    def get_game_objects_by_tag(self, tag: str) -> List[Dict[str, Any]]:
        """Get game objects by tag (mock implementation)."""
        # Mock implementation for compatibility
        return []

    def get_components_by_type(self, component_type: str) -> List[Dict[str, Any]]:
        """Get components by type."""
        if not self.scene_data:
            return []

        return [c for c in self.scene_data.components if c.get('type') == component_type]

    def get_hierarchy_path(self, game_object_name: str) -> str:
        """Get hierarchy path for a game object."""
        # Mock implementation
        return f"/{game_object_name}"

    def find_game_object_by_name(self, name: str) -> Optional[Dict[str, Any]]:
        """Find game object by name."""
        if not self.scene_data:
            return None

        for obj in self.scene_data.game_objects:
            if obj.get('name') == name:
                return obj
        return None