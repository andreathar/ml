# MLCreator Integration Health Check Script
# Checks all AI integration systems: OpenSpec, Serena, Unity MCP, Unity-KB

param(
    [switch]$Verbose,
    [switch]$Fix
)

$ErrorActionPreference = "SilentlyContinue"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  MLCreator Integration Health Check" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$issues = @()
$warnings = @()

# ============================================
# 1. OpenSpec Check
# ============================================
Write-Host "[1/4] OpenSpec System" -ForegroundColor Yellow
Write-Host "---------------------"

# Check openspec directory exists
if (Test-Path "openspec") {
    Write-Host "  [OK] openspec/ directory exists" -ForegroundColor Green
    
    # Check AGENTS.md
    if (Test-Path "openspec/AGENTS.md") {
        Write-Host "  [OK] AGENTS.md present" -ForegroundColor Green
    } else {
        $issues += "openspec/AGENTS.md missing"
        Write-Host "  [FAIL] AGENTS.md missing" -ForegroundColor Red
    }
    
    # Check project.md
    if (Test-Path "openspec/project.md") {
        $content = Get-Content "openspec/project.md" -Raw
        if ($content.Length -gt 1000) {
            Write-Host "  [OK] project.md populated ($($content.Length) chars)" -ForegroundColor Green
        } else {
            $warnings += "openspec/project.md may be incomplete"
            Write-Host "  [WARN] project.md may be incomplete" -ForegroundColor Yellow
        }
    } else {
        $issues += "openspec/project.md missing"
        Write-Host "  [FAIL] project.md missing" -ForegroundColor Red
    }
    
    # Check for active changes
    $changes = Get-ChildItem "openspec/changes" -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne "archive" }
    Write-Host "  [INFO] Active changes: $($changes.Count)" -ForegroundColor Cyan
    
    # Check for specs
    $specs = Get-ChildItem "openspec/specs" -Directory -ErrorAction SilentlyContinue
    Write-Host "  [INFO] Specifications: $($specs.Count)" -ForegroundColor Cyan
    
} else {
    $issues += "openspec/ directory missing - run 'openspec init'"
    Write-Host "  [FAIL] openspec/ directory missing" -ForegroundColor Red
}

# Check for legacy .openspec directory
if (Test-Path ".openspec") {
    $warnings += "Legacy .openspec/ directory exists - consider removing after migration"
    Write-Host "  [WARN] Legacy .openspec/ exists" -ForegroundColor Yellow
}

Write-Host ""

# ============================================
# 2. Serena Check
# ============================================
Write-Host "[2/4] Serena Memory System" -ForegroundColor Yellow
Write-Host "--------------------------"

if (Test-Path ".serena") {
    Write-Host "  [OK] .serena/ directory exists" -ForegroundColor Green
    
    # Check config.yaml
    if (Test-Path ".serena/config.yaml") {
        Write-Host "  [OK] config.yaml present" -ForegroundColor Green
    } else {
        $issues += ".serena/config.yaml missing"
        Write-Host "  [FAIL] config.yaml missing" -ForegroundColor Red
    }
    
    # Check project.yml
    if (Test-Path ".serena/project.yml") {
        Write-Host "  [OK] project.yml present" -ForegroundColor Green
    } else {
        $warnings += ".serena/project.yml missing"
        Write-Host "  [WARN] project.yml missing" -ForegroundColor Yellow
    }
    
    # Check memory tiers
    $memoryTiers = @("CRITICAL", "CONTEXT", "INTEGRATION", "TOOLS")
    foreach ($tier in $memoryTiers) {
        $tierPath = ".serena/memories/$tier"
        if (Test-Path $tierPath) {
            $files = Get-ChildItem $tierPath -File -ErrorAction SilentlyContinue
            Write-Host "  [OK] $tier tier: $($files.Count) files" -ForegroundColor Green
        } else {
            if ($tier -eq "CRITICAL") {
                $issues += "CRITICAL memory tier missing"
                Write-Host "  [FAIL] $tier tier missing" -ForegroundColor Red
            } else {
                Write-Host "  [INFO] $tier tier: not present" -ForegroundColor Cyan
            }
        }
    }
    
} else {
    $issues += ".serena/ directory missing"
    Write-Host "  [FAIL] .serena/ directory missing" -ForegroundColor Red
}

Write-Host ""

# ============================================
# 3. Unity MCP Server Check
# ============================================
Write-Host "[3/4] Unity MCP Server" -ForegroundColor Yellow
Write-Host "----------------------"

# Check MCP server executable
$mcpPath = "Library/mcp-server/win-x64/unity-mcp-server.exe"
if (Test-Path $mcpPath) {
    Write-Host "  [OK] Unity MCP server executable found" -ForegroundColor Green
} else {
    $issues += "Unity MCP server executable missing at $mcpPath"
    Write-Host "  [FAIL] Unity MCP server not found at $mcpPath" -ForegroundColor Red
}

# Check .mcp.json
if (Test-Path ".mcp.json") {
    $mcpConfig = Get-Content ".mcp.json" -Raw | ConvertFrom-Json
    Write-Host "  [OK] .mcp.json present" -ForegroundColor Green
    
    # Check Unity-MCP configuration
    if ($mcpConfig.mcpServers.'Unity-MCP') {
        Write-Host "  [OK] Unity-MCP configured" -ForegroundColor Green
    } else {
        $issues += "Unity-MCP not configured in .mcp.json"
        Write-Host "  [FAIL] Unity-MCP not in .mcp.json" -ForegroundColor Red
    }
    
    # Check filesystem-ml configuration
    if ($mcpConfig.mcpServers.'filesystem-ml') {
        Write-Host "  [OK] filesystem-ml configured" -ForegroundColor Green
    } else {
        $warnings += "filesystem-ml not configured in .mcp.json"
        Write-Host "  [WARN] filesystem-ml not in .mcp.json" -ForegroundColor Yellow
    }
} else {
    $issues += ".mcp.json missing"
    Write-Host "  [FAIL] .mcp.json missing" -ForegroundColor Red
}

# Check Claude settings
if (Test-Path ".claude/settings.local.json") {
    Write-Host "  [OK] Claude settings present" -ForegroundColor Green
} else {
    $warnings += ".claude/settings.local.json missing"
    Write-Host "  [WARN] Claude settings missing" -ForegroundColor Yellow
}

Write-Host ""

# ============================================
# 4. Unity Knowledge Base Check
# ============================================
Write-Host "[4/4] Unity Knowledge Base (Qdrant)" -ForegroundColor Yellow
Write-Host "------------------------------------"

# Check Qdrant health
try {
    $qdrantHealth = Invoke-RestMethod -Uri "http://localhost:6333/healthz" -TimeoutSec 5
    Write-Host "  [OK] Qdrant is running" -ForegroundColor Green
    
    # Check collection
    try {
        $collection = Invoke-RestMethod -Uri "http://localhost:6333/collections/unity_project_kb" -TimeoutSec 5
        $pointCount = $collection.result.points_count
        
        if ($pointCount -ge 70000) {
            Write-Host "  [OK] KB collection: $pointCount symbols" -ForegroundColor Green
        } elseif ($pointCount -gt 0) {
            $warnings += "KB collection has only $pointCount symbols (expected 70000+)"
            Write-Host "  [WARN] KB collection: $pointCount symbols (expected 70000+)" -ForegroundColor Yellow
        } else {
            $issues += "KB collection is empty"
            Write-Host "  [FAIL] KB collection is empty" -ForegroundColor Red
        }
    } catch {
        $issues += "KB collection 'unity_project_kb' not found"
        Write-Host "  [FAIL] Collection 'unity_project_kb' not found" -ForegroundColor Red
    }
} catch {
    $warnings += "Qdrant not running (KB queries unavailable)"
    Write-Host "  [WARN] Qdrant not running at localhost:6333" -ForegroundColor Yellow
    Write-Host "  [INFO] Start with: docker start qdrant" -ForegroundColor Cyan
}

# Check KB scripts
if (Test-Path "openspec/unity-kb") {
    $kbScripts = Get-ChildItem "openspec/unity-kb" -Filter "*.py" -ErrorAction SilentlyContinue
    Write-Host "  [OK] KB scripts present: $($kbScripts.Count) files" -ForegroundColor Green
} elseif (Test-Path ".openspec/unity-kb") {
    Write-Host "  [INFO] KB scripts in legacy .openspec/unity-kb/" -ForegroundColor Cyan
} else {
    $warnings += "KB utility scripts not found"
    Write-Host "  [WARN] KB scripts not found" -ForegroundColor Yellow
}

Write-Host ""

# ============================================
# Summary
# ============================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if ($issues.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "All systems operational!" -ForegroundColor Green
} else {
    if ($issues.Count -gt 0) {
        Write-Host "ISSUES ($($issues.Count)):" -ForegroundColor Red
        foreach ($issue in $issues) {
            Write-Host "  - $issue" -ForegroundColor Red
        }
        Write-Host ""
    }
    
    if ($warnings.Count -gt 0) {
        Write-Host "WARNINGS ($($warnings.Count)):" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "  - $warning" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n----------------------------------------"
Write-Host "Run with -Verbose for detailed output"
Write-Host "Run with -Fix to attempt auto-fixes"
Write-Host "----------------------------------------`n"

# Return exit code
if ($issues.Count -gt 0) {
    exit 1
} else {
    exit 0
}
