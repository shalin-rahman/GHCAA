# Quick login test - PowerShell 5.1 compatible (no SkipCertificateCheck param)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$body = '{"username":"shalin","password":"shalin"}'
try {
    $r = Invoke-WebRequest -Uri 'https://localhost:7214/api/auth/login' -Method POST `
        -ContentType 'application/json' -Body $body -UseBasicParsing
    Write-Host "SUCCESS:" $r.StatusCode
    Write-Host $r.Content
} catch {
    Write-Host "ERROR:" $_.Exception.Message
    Write-Host "BODY:" $_.ErrorDetails.Message
    # try HTTP fallback
    try {
        $r2 = Invoke-WebRequest -Uri 'http://localhost:5087/api/auth/login' -Method POST `
            -ContentType 'application/json' -Body $body -UseBasicParsing
        Write-Host "HTTP SUCCESS:" $r2.StatusCode
        Write-Host $r2.Content
    } catch {
        Write-Host "HTTP also failed:" $_.Exception.Message
        Write-Host "HTTP BODY:" $_.ErrorDetails.Message
    }
}
