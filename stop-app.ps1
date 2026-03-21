param (
    [switch]$PortsOnly,   # Only release ports, don't kill all dotnet/node
    [switch]$Force        # No confirmation prompt
)

# ── Banner ──────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    GHCAA Platform  |  Stop All Services" -ForegroundColor Cyan
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host ""

if (-not $Force -and -not $PortsOnly) {
    $confirm = Read-Host "  Stop all GHCAA services? [Y/n]"
    if ($confirm -ne "" -and $confirm -notmatch "^[Yy]") {
        Write-Host "  Aborted." -ForegroundColor DarkGray
        exit 0
    }
}

# ── Helper ──────────────────────────────────────────────────────────────────
function Stop-ByName([string]$name, [string]$label) {
    $procs = Get-Process $name -ErrorAction SilentlyContinue
    if ($procs) {
        $procs | Stop-Process -Force -ErrorAction SilentlyContinue
        Write-Host "  [OK] Stopped $label ($($procs.Count) process(es))" -ForegroundColor Green
    } else {
        Write-Host "  [--] No $label processes running" -ForegroundColor DarkGray
    }
}

function Release-Port([int]$port) {
    $pids = netstat -ano 2>$null |
        Select-String ":$port\s" |
        ForEach-Object { ($_ -split '\s+')[-1] } |
        Where-Object { $_ -match '^\d+$' } |
        Sort-Object -Unique

    foreach ($p in $pids) {
        try {
            Stop-Process -Id $p -Force -ErrorAction Stop
            Write-Host "  [OK] Released port $port (PID $p)" -ForegroundColor Green
        } catch {
            # process may have already gone
        }
    }
    if (-not $pids) {
        Write-Host "  [--] Port $port was not in use" -ForegroundColor DarkGray
    }
}

# ── Stop Processes ──────────────────────────────────────────────────────────
if (-not $PortsOnly) {
    Write-Host "  Stopping .NET processes (GHCAA API)..." -ForegroundColor Yellow
    Stop-ByName "dotnet" ".NET API"

    Write-Host "  Stopping Node processes (Angular frontend)..." -ForegroundColor Yellow
    Stop-ByName "node" "Node/Angular"

    Start-Sleep -Milliseconds 500
}

# ── Release Ports ───────────────────────────────────────────────────────────
Write-Host "  Releasing application ports..." -ForegroundColor Yellow
Release-Port 7214   # GHCAA API (HTTPS)
Release-Port 5087   # GHCAA API (HTTP fallback)
Release-Port 4200   # Angular frontend

# ── Done ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    All GHCAA services stopped." -ForegroundColor Green
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    Run .\run-app.ps1 or run-app.bat to restart." -ForegroundColor DarkGray
Write-Host ""
