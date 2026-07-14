param(
    [ValidateRange(1, 65535)]
    [int]$FrontendPort = 5168,

    [ValidateRange(1, 65535)]
    [int]$ApiPort = 5090,

    [ValidateRange(10, 300)]
    [int]$StartupTimeoutSeconds = 120
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$frontendProject = Join-Path $root "APO-BOT\APO-BOT.csproj"
$apiProject = Join-Path $root "demo\APO-BOT.DemoApi\APO-BOT.DemoApi.csproj"
$apiUrl = "http://localhost:$ApiPort"
$frontendUrl = "http://localhost:$FrontendPort"
$healthUrl = "$apiUrl/health"
$apiLog = Join-Path $env:TEMP "apobot-demo-api.log"
$apiErrorLog = Join-Path $env:TEMP "apobot-demo-api-error.log"
$startedApi = $null

function Test-ApoBotApi {
    try {
        $health = Invoke-RestMethod -Uri $healthUrl -TimeoutSec 2
        return $health.status -eq "ok" -and $health.database -eq "sqlite"
    }
    catch {
        return $false
    }
}

function Stop-ProcessTree {
    param([int]$ProcessId)

    $children = Get-CimInstance Win32_Process -Filter "ParentProcessId = $ProcessId" -ErrorAction SilentlyContinue
    foreach ($child in $children) {
        Stop-ProcessTree -ProcessId $child.ProcessId
    }

    Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
}

if (-not (Test-Path -LiteralPath $frontendProject)) {
    throw "No se encuentra el proyecto frontend: $frontendProject"
}

if (-not (Test-Path -LiteralPath $apiProject)) {
    throw "No se encuentra la API demo: $apiProject"
}

$dotnet = Get-Command dotnet -ErrorAction Stop
$existingFrontend = Get-NetTCPConnection -LocalPort $FrontendPort -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($existingFrontend) {
    throw "El puerto $FrontendPort ya esta ocupado. Cierra la instancia anterior o usa -FrontendPort con otro valor."
}

$existingApi = Get-NetTCPConnection -LocalPort $ApiPort -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($existingApi -and -not (Test-ApoBotApi)) {
    throw "El puerto $ApiPort esta ocupado por otro servicio que no es la API demo de APObot."
}

if (-not $existingApi) {
    Remove-Item $apiLog, $apiErrorLog -Force -ErrorAction SilentlyContinue
    $startedApi = Start-Process -FilePath $dotnet.Source `
        -ArgumentList @("run", "--no-launch-profile", "--project", "demo\APO-BOT.DemoApi\APO-BOT.DemoApi.csproj", "--urls", $apiUrl) `
        -WorkingDirectory $root `
        -WindowStyle Hidden `
        -RedirectStandardOutput $apiLog `
        -RedirectStandardError $apiErrorLog `
        -PassThru

    $deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if ($startedApi.HasExited) {
            break
        }

        if (Test-ApoBotApi) {
            break
        }

        Start-Sleep -Milliseconds 500
    }

    if (-not (Test-ApoBotApi)) {
        Get-Content $apiLog, $apiErrorLog -ErrorAction SilentlyContinue
        if (-not $startedApi.HasExited) {
            Stop-ProcessTree -ProcessId $startedApi.Id
        }
        throw "La API de demostracion no ha quedado disponible en $healthUrl."
    }
}

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$previousApiEnabled = $env:Api__Enabled
$previousApiBaseUrl = $env:Api__BaseUrl

try {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:Api__Enabled = "true"
    $env:Api__BaseUrl = "$apiUrl/"

    Write-Host "API demo: $apiUrl"
    Write-Host "Frontend: $frontendUrl"
    Write-Host "Pulsa Ctrl+C para detener ambos servicios."

    & $dotnet.Source run --no-launch-profile --project $frontendProject --urls $frontendUrl
    if ($LASTEXITCODE -ne 0) {
        throw "El frontend ha finalizado con el codigo $LASTEXITCODE."
    }
}
finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    $env:Api__Enabled = $previousApiEnabled
    $env:Api__BaseUrl = $previousApiBaseUrl

    if ($startedApi -and -not $startedApi.HasExited) {
        Stop-ProcessTree -ProcessId $startedApi.Id
    }
}
