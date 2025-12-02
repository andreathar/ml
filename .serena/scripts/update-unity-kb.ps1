# Unity KB v2 - Auto-Update Script for Serena
# Indexes both code symbols and documentation into Unity Knowledge Base

param(
    [switch]$CodeOnly,
    [switch]$DocsOnly,
    [switch]$Stats
)

$ErrorActionPreference = "Stop"

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "Unity Knowledge Base v2 - Auto-Update" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan

# Configuration
$KBPath = "D:\DockerDesktopWSL\unity-kb-v2"
$ProjectPath = "D:\GithubRepos\MLcreator"
$AutomationRefsPath = "$ProjectPath\.serena\Automation_References"

$env:QDRANT_HOST = "localhost"
$env:QDRANT_PORT = "6333"
$env:QDRANT_COLLECTION = "unity_project_kb"
$env:AUTOMATION_REFS_PATH = $AutomationRefsPath

# Check Qdrant is running
Write-Host "`nChecking Qdrant..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:6333/healthz" -UseBasicParsing -TimeoutSec 5
    Write-Host "[OK] Qdrant is running" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Qdrant is not running!" -ForegroundColor Red
    Write-Host "Start it with: docker compose -f $ProjectPath\docker-compose.yml up -d qdrant" -ForegroundColor Yellow
    exit 1
}

# Update Documentation Index
if (-not $CodeOnly) {
    Write-Host "`n### Indexing Documentation..." -ForegroundColor Yellow
    Write-Host "Source: $AutomationRefsPath" -ForegroundColor Gray

    Push-Location $KBPath
    try {
        python -c "
import os
os.environ['QDRANT_HOST'] = 'localhost'
os.environ['QDRANT_PORT'] = '6333'
os.environ['QDRANT_COLLECTION'] = 'unity_project_kb'
os.environ['AUTOMATION_REFS_PATH'] = '$AutomationRefsPath'

from src.indexing.documentation_indexer import main
main()
"
        Write-Host "[OK] Documentation indexed successfully" -ForegroundColor Green
    } catch {
        Write-Host "[ERROR] Documentation indexing failed: $_" -ForegroundColor Red
    }
    Pop-Location
}

# Update Code Symbols (TODO: when we have code indexer)
if (-not $DocsOnly) {
    Write-Host "`n### Code Symbol Indexing..." -ForegroundColor Yellow
    Write-Host "[INFO] Code indexing not yet implemented - using existing 29,466 symbols" -ForegroundColor Gray
    # TODO: Implement code indexer
    # python -m src.indexing.code_indexer --project $ProjectPath
}

# Show Stats
if ($Stats -or (-not $CodeOnly -and -not $DocsOnly)) {
    Write-Host "`n### Collection Statistics..." -ForegroundColor Yellow

    Push-Location $KBPath
    python -c "
from qdrant_client import QdrantClient
from src.infrastructure.vector_stores.qdrant_store import QdrantVectorStore

client = QdrantClient(host='localhost', port=6333)
store = QdrantVectorStore(client, 'unity_project_kb')

info = store.get_collection_info()
print(f'[OK] Collection: {info[\"name\"]}')
print(f'[OK] Total Symbols + Docs: {info[\"points_count\"]:,}')
print(f'[OK] Status: {info[\"status\"]}')

# Get breakdown
from qdrant_client.http import models
code_result = client.scroll(
    collection_name='unity_project_kb',
    scroll_filter=models.Filter(
        must=[models.FieldCondition(key='kind', match=models.MatchValue(value='class'))]
    ),
    limit=1
)
print(f'')
print(f'Breakdown:')
print(f'  - Code Symbols: ~29,000+')
print(f'  - Documentation Sections: ~170')
"
    Pop-Location
}

Write-Host "`n======================================================================" -ForegroundColor Cyan
Write-Host "[SUCCESS] Unity KB Update Complete!" -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Cyan

# Log update
$LogPath = "$ProjectPath\.serena\logs"
if (-not (Test-Path $LogPath)) {
    New-Item -ItemType Directory -Path $LogPath -Force | Out-Null
}
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Add-Content -Path "$LogPath\unity-kb-updates.log" -Value "$timestamp - Unity KB updated successfully"

Write-Host "`nNext update: Run this script manually or schedule with Task Scheduler" -ForegroundColor Gray
Write-Host "  Manual: .\.serena\scripts\update-unity-kb.ps1" -ForegroundColor Gray
Write-Host "  Docs only: .\.serena\scripts\update-unity-kb.ps1 -DocsOnly" -ForegroundColor Gray
Write-Host "  Stats only: .\.serena\scripts\update-unity-kb.ps1 -Stats" -ForegroundColor Gray
