param(
    [int]$FrontendPort = 5168,
    [int]$ApiPort = 5090
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$frontendProject = Join-Path $root "APO-BOT\APO-BOT.csproj"
$apiProject = Join-Path $root "demo\APO-BOT.DemoApi\APO-BOT.DemoApi.csproj"
$apiLog = Join-Path $env:TEMP "apobot-demo-api.log"
$apiErrorLog = Join-Path $env:TEMP "apobot-demo-api-error.log"
$startedApi = $null

$existingApi = Get-NetTCPConnection -LocalPort $ApiPort -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $existingApi)
{
    $startedApi = Start-Process -FilePath "dotnet" `
        -ArgumentList @("run", "--project", $apiProject, "--urls", "http://localhost:$ApiPort") `
        -WindowStyle Hidden `
        -RedirectStandardOutput $apiLog `
        -RedirectStandardError $apiErrorLog `
        -PassThru

    for ($attempt = 0; $attempt -lt 30; $attempt++)
    {
        Start-Sleep -Milliseconds 250
        if (Get-NetTCPConnection -LocalPort $ApiPort -State Listen -ErrorAction SilentlyContinue)
        {
            break
        }
    }

    if (-not (Get-NetTCPConnection -LocalPort $ApiPort -State Listen -ErrorAction SilentlyContinue))
    {
        Get-Content $apiLog, $apiErrorLog -ErrorAction SilentlyContinue
        throw "La API de demostracion no ha podido arrancar."
    }
}

try
{
    Write-Host "API demo: http://localhost:$ApiPort"
    Write-Host "Frontend: http://localhost:$FrontendPort"
    & dotnet run --project $frontendProject --urls "http://localhost:$FrontendPort"
}
finally
{
    if ($startedApi -and -not $startedApi.HasExited)
    {
        Stop-Process -Id $startedApi.Id
    }
}
