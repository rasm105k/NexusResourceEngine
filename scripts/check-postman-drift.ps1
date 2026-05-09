param(
    [string]$ApiUrl = "http://localhost:5299",
    [switch]$NoStart = $false
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$PostmanFile = Join-Path $ProjectRoot "NexusResourceEngine.postman_collection.json"

# --- Step 1: ensure API is running ---
if (-not $NoStart) {
    Write-Host "Starting API on $ApiUrl ..." -ForegroundColor Cyan
    $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/Presentation --urls `"$ApiUrl`"" -WorkingDirectory $ProjectRoot -NoNewWindow -PassThru
    $started = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $req = [System.Net.WebRequest]::CreateHttp("$ApiUrl/openapi/v1.json")
            $req.Timeout = 2000
            $req.GetResponse().Dispose()
            $started = $true
            break
        } catch {
            # not ready yet
        }
    }
    if (-not $started) {
        Write-Host "ERROR: API failed to start within 30 seconds." -ForegroundColor Red
        if ($apiProcess) { $apiProcess.Kill() }
        exit 1
    }
    Write-Host "API ready." -ForegroundColor Green
}

# --- Step 2: fetch OpenAPI spec ---
Write-Host "Fetching OpenAPI spec..." -ForegroundColor Cyan
try {
    $openapi = Invoke-RestMethod -Uri "$ApiUrl/openapi/v1.json" -TimeoutSec 10
} catch {
    Write-Host "ERROR: Could not fetch OpenAPI spec from $ApiUrl/openapi/v1.json" -ForegroundColor Red
    if (-not $NoStart -and $apiProcess) { $apiProcess.Kill() }
    exit 1
}

# Extract paths from OpenAPI: "POST /auth/register", etc.
$openapiEndpoints = [System.Collections.Generic.HashSet[string]]::new()
foreach ($path in $openapi.paths.PSObject.Properties) {
    $pathName = $path.Name -replace '\{\w+\}', '{id}'
    foreach ($method in $path.Value.PSObject.Properties) {
        $openapiEndpoints.Add("$($method.Name.ToUpperInvariant()) $pathName") | Out-Null
    }
}

# --- Step 3: parse Postman collection ---
Write-Host "Parsing Postman collection..." -ForegroundColor Cyan
if (-not (Test-Path $PostmanFile)) {
    Write-Host "ERROR: Postman collection not found at $PostmanFile" -ForegroundColor Red
    if (-not $NoStart -and $apiProcess) { $apiProcess.Kill() }
    exit 1
}

$postmanJson = Get-Content $PostmanFile -Raw | ConvertFrom-Json

$postmanEndpoints = [System.Collections.Generic.HashSet[string]]::new()

function Get-PostmanPath {
    param($url)
    if ($url -is [string]) {
        if ($url -match "{{baseUrl}}(/.*?)(\?|$)") { return $matches[1] }
        return $null
    }
    if ($url.path) {
        $parts = @($url.path)
        return "/" + ($parts -join "/")
    }
    if ($url.raw) {
        if ($url.raw -match "{{baseUrl}}(/.*?)(\?|$)") { return $matches[1] }
    }
    return $null
}

function Extract-PostmanItems {
    param($items)
    foreach ($item in $items) {
        if ($item.request) {
            $method = $item.request.method.ToUpperInvariant()
            $path = Get-PostmanPath -url $item.request.url
            if ($path -ne $null -and $path -ne "") {
                # Normalize Postman {{var}} to generic {id} for comparison
                $normalized = $path -replace '\{\{\w+\}\}', '{id}'
                $postmanEndpoints.Add("$method $normalized") | Out-Null
            }
        }
        if ($item.item) {
            Extract-PostmanItems -items $item.item
        }
    }
}

Extract-PostmanItems -items $postmanJson.item

# --- Step 4: compare ---
Write-Host "`nChecking for drift..." -ForegroundColor Cyan
$missingFromPostman = [System.Collections.Generic.List[string]]::new()
$missingFromOpenApi = [System.Collections.Generic.List[string]]::new()

# Skip dev/internal routes
$skipPrefixes = @("/dev/", "/openapi/", "/scalar")

foreach ($ep in $openapiEndpoints) {
    $skip = $false
    foreach ($prefix in $skipPrefixes) {
        if ($ep -match [regex]::Escape($prefix)) { $skip = $true; break }
    }
    if (-not $skip -and -not $postmanEndpoints.Contains($ep)) {
        $missingFromPostman.Add($ep)
    }
}

foreach ($ep in $postmanEndpoints) {
    if (-not $openapiEndpoints.Contains($ep)) {
        $missingFromOpenApi.Add($ep)
    }
}

$exitCode = 0

if ($missingFromPostman.Count -gt 0) {
    Write-Host "`nMissing from Postman collection:" -ForegroundColor Yellow
    foreach ($ep in $missingFromPostman) {
        $parts = $ep -split ' ', 2
        Write-Host "  $($parts[0].PadRight(7)) $($parts[1])" -ForegroundColor Yellow
    }
    $exitCode = 1
}

if ($missingFromOpenApi.Count -gt 0) {
    Write-Host "`nWARNING - in Postman but not in OpenAPI (renamed/removed?):" -ForegroundColor Magenta
    foreach ($ep in $missingFromOpenApi) {
        $parts = $ep -split ' ', 2
        Write-Host "  $($parts[0].PadRight(7)) $($parts[1])" -ForegroundColor Magenta
    }
    $exitCode = 1
}

if ($exitCode -eq 0) {
    Write-Host "`nNo drift detected. Postman collection is in sync." -ForegroundColor Green
} else {
    Write-Host "`nDrift detected. Update the Postman collection and re-run." -ForegroundColor Red
}

# Cleanup
if (-not $NoStart -and $apiProcess) {
    $apiProcess.Kill()
    Write-Host "API stopped." -ForegroundColor Gray
}

exit $exitCode
