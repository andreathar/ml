from qdrant_client import QdrantClient
from sentence_transformers import SentenceTransformer

# Configuration
QDRANT_HOST = "host.docker.internal"
QDRANT_PORT = 6333
COLLECTION_NAME = "unity_docs"
MODEL_NAME = "all-MiniLM-L6-v2"
QUERY = "Game Creator Inventory Module Visual Scripting Actions Conditions Triggers"

def main():
    try:
        client = QdrantClient(host=QDRANT_HOST, port=QDRANT_PORT)
        model = SentenceTransformer(MODEL_NAME)
        
        vector = model.encode(QUERY).tolist()
        
        # Trying query_points as search is missing
        results = client.query_points(
            collection_name=COLLECTION_NAME,
            query=vector,
            limit=5
        ).points
        
        print(f"Found {len(results)} relevant documents:\n")
        for hit in results:
            print(f"--- Document: {hit.payload['filename']} ---")
            print(hit.payload['content'][:2000]) # Print first 2000 chars
            print("\n")
            
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
