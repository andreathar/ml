#!/usr/bin/env python3
"""
Qdrant Collection Setup for MLcreator Unity Knowledge Base
===========================================================

Creates optimized collections with hybrid search support:
- Dense vectors for semantic similarity
- Sparse vectors for keyword matching (BM25)
- Payload indexes for efficient filtering

Run this script to initialize or recreate collections.
"""

import sys
from pathlib import Path
from typing import Dict, List, Optional, Any
from qdrant_client import QdrantClient
from qdrant_client.http import models
from qdrant_client.models import (
    VectorParams, SparseVectorParams, Distance,
    HnswConfigDiff, OptimizersConfigDiff, QuantizationConfig,
    ScalarQuantization, ScalarQuantizationConfig, ScalarType,
    PayloadSchemaType, TokenizerType, TextIndexParams,
    IntegerIndexParams, FloatIndexParams, KeywordIndexParams,
    BoolIndexParams
)

from qdrant_config import (
    QDRANT_HOST, QDRANT_PORT, QDRANT_GRPC_PORT,
    COLLECTION_UNIFIED, DENSE_VECTOR_SIZE,
    UnifiedCollectionConfig, get_default_config
)


class QdrantSetup:
    """Setup and manage Qdrant collections for MLcreator KB"""

    def __init__(self, host: str = QDRANT_HOST, port: int = QDRANT_PORT,
                 prefer_grpc: bool = True):
        """Initialize Qdrant client"""
        self.host = host
        self.port = port

        if prefer_grpc:
            self.client = QdrantClient(
                host=host,
                port=port,
                grpc_port=QDRANT_GRPC_PORT,
                prefer_grpc=True,
                timeout=60
            )
        else:
            self.client = QdrantClient(host=host, port=port, timeout=60)

        print(f"[OK] Connected to Qdrant at {host}:{port}")

    def create_unified_collection(
        self,
        recreate: bool = False,
        config: Optional[UnifiedCollectionConfig] = None
    ) -> bool:
        """
        Create the unified hybrid search collection.

        Args:
            recreate: If True, delete existing collection first
            config: Collection configuration (uses defaults if None)

        Returns:
            True if successful
        """
        config = config or UnifiedCollectionConfig()

        print(f"\n{'='*60}")
        print(f"Setting up collection: {config.name}")
        print(f"{'='*60}")

        # Check if collection exists
        collections = self.client.get_collections().collections
        exists = any(c.name == config.name for c in collections)

        if exists:
            if recreate:
                print(f"  [!] Deleting existing collection: {config.name}")
                self.client.delete_collection(config.name)
            else:
                print(f"  [!] Collection {config.name} already exists. Use recreate=True to rebuild.")
                return False

        # Configure quantization for memory efficiency
        quantization_config = None
        if config.use_scalar_quantization:
            quantization_config = QuantizationConfig(
                scalar=ScalarQuantization(
                    type=ScalarType.INT8,
                    quantile=0.99,
                    always_ram=True  # Keep quantized vectors in RAM
                )
            )
            print(f"  [+] Using INT8 scalar quantization (~4x memory reduction)")

        # Configure HNSW index
        hnsw_config = HnswConfigDiff(
            m=config.hnsw_m,
            ef_construct=config.hnsw_ef_construct,
            full_scan_threshold=config.hnsw_full_scan_threshold,
            max_indexing_threads=0,  # Use all available threads
            on_disk=config.on_disk_vectors,
        )

        # Configure optimizers
        optimizers_config = OptimizersConfigDiff(
            indexing_threshold=20000,  # Index after this many points
            memmap_threshold=50000,    # Use memmap after this many points
        )

        # Create collection with dense and sparse vector support
        print(f"  [+] Creating collection with hybrid vectors...")
        print(f"      Dense: {config.dense_size} dimensions")
        print(f"      Sparse: BM25/SPLADE compatible")

        self.client.create_collection(
            collection_name=config.name,
            vectors_config={
                config.dense_vector_name: VectorParams(
                    size=config.dense_size,
                    distance=Distance.COSINE,
                    hnsw_config=hnsw_config,
                    quantization_config=quantization_config,
                    on_disk=config.on_disk_vectors,
                )
            },
            sparse_vectors_config={
                config.sparse_vector_name: SparseVectorParams(
                    index=models.SparseIndexParams(
                        on_disk=False,  # Keep sparse index in RAM
                    )
                )
            },
            optimizers_config=optimizers_config,
            shard_number=config.shard_number,
            replication_factor=config.replication_factor,
            on_disk_payload=config.on_disk_payload,
        )

        print(f"  [OK] Collection created")

        # Create payload indexes
        print(f"  [+] Creating payload indexes...")
        self._create_payload_indexes(config.name, config.payload_indexes)

        print(f"\n[SUCCESS] Collection {config.name} is ready for hybrid search!")
        return True

    def _create_payload_indexes(self, collection_name: str, indexes: Dict[str, Dict]):
        """Create payload indexes for efficient filtering"""

        for field_name, index_config in indexes.items():
            index_type = index_config.get("type", "keyword")

            try:
                if index_type == "keyword":
                    self.client.create_payload_index(
                        collection_name=collection_name,
                        field_name=field_name,
                        field_schema=PayloadSchemaType.KEYWORD,
                    )

                elif index_type == "text":
                    tokenizer = index_config.get("tokenizer", "word")
                    self.client.create_payload_index(
                        collection_name=collection_name,
                        field_name=field_name,
                        field_schema=TextIndexParams(
                            type="text",
                            tokenizer=TokenizerType.WORD if tokenizer == "word" else TokenizerType.PREFIX,
                            min_token_len=2,
                            max_token_len=20,
                            lowercase=True,
                        ),
                    )

                elif index_type == "integer":
                    self.client.create_payload_index(
                        collection_name=collection_name,
                        field_name=field_name,
                        field_schema=IntegerIndexParams(
                            type="integer",
                            lookup=True,
                            range=True,
                        ),
                    )

                elif index_type == "float":
                    self.client.create_payload_index(
                        collection_name=collection_name,
                        field_name=field_name,
                        field_schema=FloatIndexParams(
                            type="float",
                            lookup=False,
                            range=True,
                        ),
                    )

                elif index_type == "bool":
                    self.client.create_payload_index(
                        collection_name=collection_name,
                        field_name=field_name,
                        field_schema=PayloadSchemaType.BOOL,
                    )

                print(f"      ✓ {field_name} ({index_type})")

            except Exception as e:
                print(f"      ✗ {field_name}: {e}")

    def get_collection_info(self, collection_name: str = COLLECTION_UNIFIED) -> Dict:
        """Get detailed collection information"""
        try:
            info = self.client.get_collection(collection_name)
            return {
                "name": collection_name,
                "status": str(info.status),
                "points_count": info.points_count,
                "vectors_count": info.vectors_count,
                "indexed_vectors_count": info.indexed_vectors_count,
                "segments_count": info.segments_count,
                "config": {
                    "vectors": str(info.config.params.vectors),
                    "sparse_vectors": str(info.config.params.sparse_vectors) if info.config.params.sparse_vectors else None,
                    "shard_number": info.config.params.shard_number,
                    "replication_factor": info.config.params.replication_factor,
                }
            }
        except Exception as e:
            return {"error": str(e)}

    def list_collections(self) -> List[str]:
        """List all collections"""
        collections = self.client.get_collections().collections
        return [c.name for c in collections]

    def delete_collection(self, collection_name: str) -> bool:
        """Delete a collection"""
        try:
            self.client.delete_collection(collection_name)
            print(f"[OK] Deleted collection: {collection_name}")
            return True
        except Exception as e:
            print(f"[ERROR] Failed to delete {collection_name}: {e}")
            return False

    def optimize_collection(self, collection_name: str = COLLECTION_UNIFIED):
        """Trigger optimization for a collection"""
        print(f"[+] Optimizing collection: {collection_name}")
        self.client.update_collection(
            collection_name=collection_name,
            optimizer_config=OptimizersConfigDiff(
                indexing_threshold=10000,
            )
        )
        print(f"[OK] Optimization triggered")

    def create_snapshot(self, collection_name: str = COLLECTION_UNIFIED) -> Optional[str]:
        """Create a snapshot of the collection"""
        try:
            snapshot = self.client.create_snapshot(collection_name)
            print(f"[OK] Created snapshot: {snapshot.name}")
            return snapshot.name
        except Exception as e:
            print(f"[ERROR] Snapshot failed: {e}")
            return None


def setup_all_collections(recreate: bool = False):
    """Setup all required collections"""
    setup = QdrantSetup()

    # List existing collections
    existing = setup.list_collections()
    print(f"\nExisting collections: {existing}")

    # Create unified collection (main collection for hybrid search)
    setup.create_unified_collection(recreate=recreate)

    # Show final status
    print(f"\n{'='*60}")
    print("COLLECTION STATUS")
    print(f"{'='*60}")

    info = setup.get_collection_info(COLLECTION_UNIFIED)
    if "error" not in info:
        print(f"\n{COLLECTION_UNIFIED}:")
        print(f"  Status: {info['status']}")
        print(f"  Points: {info['points_count']}")
        print(f"  Indexed vectors: {info['indexed_vectors_count']}")
    else:
        print(f"\n{COLLECTION_UNIFIED}: {info['error']}")


def main():
    """Main entry point"""
    import argparse

    parser = argparse.ArgumentParser(description="Setup Qdrant collections for MLcreator KB")
    parser.add_argument("--recreate", action="store_true", help="Recreate existing collections")
    parser.add_argument("--info", action="store_true", help="Show collection info only")
    parser.add_argument("--delete", type=str, help="Delete a specific collection")
    parser.add_argument("--optimize", action="store_true", help="Optimize collections")

    args = parser.parse_args()

    setup = QdrantSetup()

    if args.info:
        print("\nCOLLECTION INFO")
        print("="*60)
        for collection in setup.list_collections():
            info = setup.get_collection_info(collection)
            print(f"\n{collection}:")
            for key, value in info.items():
                print(f"  {key}: {value}")
        return

    if args.delete:
        setup.delete_collection(args.delete)
        return

    if args.optimize:
        for collection in setup.list_collections():
            setup.optimize_collection(collection)
        return

    # Default: setup collections
    setup_all_collections(recreate=args.recreate)


if __name__ == "__main__":
    main()
