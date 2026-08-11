param (
    [switch]$PortsOnly,      # Only release ports, don't kill all dotnet/node
    [switch]$Force,         # No confirmation prompt
    [switch]$IncludeBrowsers # Also stop Chrome / Edge (optional; can close unrelated tabs)
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
function Release-Port([int]$port) {
    # @(...) forces an array: a single PID must not be a string or foreach() iterates characters.
    $pids = @(
        netstat -ano 2>$null |
            Select-String ":$port\s" |
            ForEach-Object { ($_ -split '\s+')[-1] } |
            Where-Object { $_ -match '^\d+$' } |
            Sort-Object -Unique
    )

    if ($pids.Count -eq 0) {
        Write-Host "  [--] Port $port was not in use" -ForegroundColor DarkGray
        return
    }

    foreach ($p in $pids) {
        try {
            Stop-Process -Id ([int]$p) -Force -ErrorAction Stop
            Write-Host "  [OK] Released port $port (PID $p)" -ForegroundColor Green
        } catch {
            # process may have already gone
        }
    }
}

# ── Stop Processes ──────────────────────────────────────────────────────────
if (-not $PortsOnly) {
    Write-Host "  Stopping dev processes (dotnet, node, dart, Flutter, Java, emulator)..." -ForegroundColor Yellow

    taskkill /F /IM dotnet.exe /T 2>$null | Out-Null
    taskkill /F /IM node.exe /T 2>$null | Out-Null
    taskkill /F /IM dart.exe /T 2>$null | Out-Null
    taskkill /F /IM flutter.bat /T 2>$null | Out-Null
    taskkill /F /IM java.exe /T 2>$null | Out-Null
    taskkill /F /IM qemu-system-x86_64.exe /T 2>$null | Out-Null

    if ($IncludeBrowsers) {
        taskkill /F /IM chrome.exe /T 2>$null | Out-Null
        taskkill /F /IM msedge.exe /T 2>$null | Out-Null
        Write-Host "  [OK] Browser processes (Chrome/Edge) requested to stop" -ForegroundColor Green
    }

    Start-Sleep -Milliseconds 1500
}

# ── Release Ports ───────────────────────────────────────────────────────────
Write-Host "  Releasing application ports..." -ForegroundColor Yellow
Release-Port 7214   # GHCAA API (HTTPS)
Release-Port 5087   # GHCAA API (HTTP)
Release-Port 4200   # Angular frontend

# ── Done ────────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    All GHCAA services stopped." -ForegroundColor Green
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    Restart : .\GHCAA.Tools\run-app.ps1  or  run-app.bat" -ForegroundColor DarkGray
Write-Host "    Docs    : GHCAA.Tools\README.md" -ForegroundColor DarkGray
Write-Host ""
