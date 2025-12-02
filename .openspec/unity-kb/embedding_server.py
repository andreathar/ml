"""
Simple Embedding Server for Unity KB Query Extension
Uses all-MiniLM-L6-v2 (384 dimensions) to match the indexed vectors

Usage:
    python embedding_server.py

Then configure VS Code:
    "unity-kb.embeddingUrl": "http://localhost:8765/embed"
"""

from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer
import logging

# Configuration
HOST = "0.0.0.0"
PORT = 8765
MODEL_NAME = "all-MiniLM-L6-v2"

# Setup logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize Flask app
app = Flask(__name__)

# Load model (cached after first load)
logger.info(f"Loading model: {MODEL_NAME}...")
model = SentenceTransformer(MODEL_NAME)
logger.info(f"Model loaded! Vector size: {model.get_sentence_embedding_dimension()}")


@app.route('/embed', methods=['POST'])
def embed():
    """Generate embedding for text"""
    try:
        data = request.get_json()
        text = data.get('text', '')

        if not text:
            return jsonify({'error': 'No text provided'}), 400

        # Generate embedding
        embedding = model.encode(text).tolist()

        logger.info(f"Embedded: '{text[:50]}...' -> {len(embedding)} dims")

        return jsonify({
            'embedding': embedding,
            'model': MODEL_NAME,
            'dimensions': len(embedding)
        })

    except Exception as e:
        logger.error(f"Error: {e}")
        return jsonify({'error': str(e)}), 500


@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({
        'status': 'ok',
        'model': MODEL_NAME,
        'dimensions': model.get_sentence_embedding_dimension()
    })


if __name__ == '__main__':
    print(f"""
=============================================================
  Unity KB Embedding Server
  Model: {MODEL_NAME}
  Dimensions: 384
  URL: http://localhost:{PORT}/embed
-------------------------------------------------------------
  Configure VS Code:
  "unity-kb.embeddingUrl": "http://localhost:{PORT}/embed"
=============================================================
    """)
    app.run(host=HOST, port=PORT, debug=False)
