# Unity Knowledge Base v2 - Consolidated Guide

**Priority:** P0 CRITICAL - Core Development Infrastructure
**Last Updated:** 2025-11-29
**Status:** ✅ **PRODUCTION READY - 77,914 indexed items**

---

## 🎯 **CURRENT SYSTEM - KB v2 (Qdrant + Dashboard)**

**Location**: `/d/DockerDesktopWSL/unity-kb-v2/`
**Dashboard**: http://localhost:6333/dashboard#/collections
**Collection**: `unity_project_kb`
**Points**: **77,914** (code + docs + metadata)

---

## 🚀 **How to Use the Unity KB (Simple)**

### **Method 1: Dashboard (Easiest)**
1. Open: http://localhost:6333/dashboard#/collections
2. Select collection: `unity_project_kb`
3. Use search bar for queries
4. Browse results directly

### **Method 2: Serena Integration (Automatic)**
- Serena automatically searches KB for code patterns
- Results enhance all Unity/GameCreator queries
- No manual action needed

### **Method 3: Python API (Programmatic)**
```python
from qdrant_client import QdrantClient
from src.infrastructure.vector_stores.qdrant_store import QdrantVectorStore
from src.core.domain.models import SearchQuery

# Connect to KB
client = QdrantClient(host='localhost', port=6333)
store = QdrantVectorStore(client, 'unity_project_kb')

# Search for classes with "character"
query = SearchQuery(query='character', kind='class', limit=10)
results = store.scroll_symbols(query, 0, 10)
```

---

## 📊 **Current Statistics**

| Metric | Value | Details |
|--------|-------|---------|
| **Total Points** | 77,914 | All indexed items |
| **Collection Status** | Green | Fully operational |
| **Query Performance** | <100ms | Fast responses |
| **Data Types** | Code + Docs | C# symbols + documentation |
| **Last Indexed** | 2025-11-29 | Up to date |

---

## 🔍 **Search Examples**

### **Find Character Classes**
- **Query**: `character`
- **Filter**: `kind=class`
- **Results**: All character-related classes in your project

### **Network Multiplayer Code**
- **Query**: `NetworkCharacterAdapter`
- **Results**: Complete implementation details

### **GameCreator Instructions**
- **Query**: `InstructionCharacter`
- **Results**: All visual scripting character instructions

---

## 🏗️ **Architecture (Simplified)**

```
┌─────────────────────────────────────┐
│         Qdrant Dashboard            │ ← You access this
│     http://localhost:6333           │
├─────────────────────────────────────┤
│   unity_project_kb Collection       │ ← 77,914 points
│   ├── Code symbols                  │
│   ├── Documentation                 │
│   └── Metadata                      │
├─────────────────────────────────────┤
│   Python API Layer                  │ ← Programmatic access
│   ├── Type-safe models              │
│   ├── Clean interfaces              │
│   └── Protocol-based design         │
└─────────────────────────────────────┘
```

---

## 🤖 **Serena Integration**

Serena automatically uses the KB for:

### **Code Generation**
- Searches existing patterns before creating new code
- References optimization patterns from `code_optimizations_2025_11.md`
- Validates against established architecture

### **Documentation Queries**
- Provides verified answers from indexed docs
- Cites sources from official Unity/GameCreator guides
- Never guesses - always uses indexed knowledge

### **Architecture Validation**
- Checks new code against established patterns
- Ensures consistency across the codebase
- Prevents duplication and architectural drift

---

## 🔧 **Maintenance**

### **Check Status**
```bash
# Dashboard: http://localhost:6333/dashboard#/collections
# Look for: unity_project_kb, 77,914 points, Status: Green
```

### **Re-index (if needed)**
```bash
cd /d/DockerDesktopWSL/unity-kb-v2
# Run indexer when code changes significantly
python scripts/index.py /d/GithubRepos/MLcreator
```

### **Troubleshooting**
- **Can't access dashboard?** Check if Qdrant container is running
- **Wrong point count?** Re-index may be needed
- **Search not working?** Check collection status in dashboard

---

## ⚠️ **DEPRECATED - DO NOT USE**

### **KB v1 (REMOVED)**
- ❌ `localhost:8100` endpoints
- ❌ MCP server approach
- ❌ Old `/d/DockerDesktopWSL/unity-knowledge-base/` location
- ❌ 29,466 point limit

### **Old Instructions (IGNORED)**
- ❌ Curl commands to localhost:8100
- ❌ MCP protocol usage
- ❌ Legacy documentation references

---

## 📚 **What the KB Contains**

### **Code Symbols (Primary)**
- All C# classes, methods, properties, fields
- Assembly organization and dependencies
- Inheritance hierarchies and interfaces
- File locations and line numbers

### **Documentation (Secondary)**
- Unity 6 editor guides and shortcuts
- GameCreator module documentation
- Best practices and patterns
- Troubleshooting guides

### **Metadata**
- Performance optimization patterns
- Code relationships and cross-references
- Search rankings and relevance scores

---

## 🎯 **Bottom Line**

**USE THE DASHBOARD**: http://localhost:6333/dashboard#/collections

**Your KB has 77,914 indexed items** and is fully operational. Search directly in the dashboard or let Serena handle the integration automatically.

**NO MORE AMBIGUITY** - This is the single source of truth for Unity KB usage.

---

**Created**: 2025-11-29
**Consolidated**: Removed all deprecated KB v1 references
**Status**: ✅ **PRODUCTION READY**
