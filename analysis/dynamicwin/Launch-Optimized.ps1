param([switch]$Settings)
$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$env:DOTNET_ROOT = Join-Path $workspace '.tools/dotnet'
$env:DYNAMICWIN_DATA_DIR = Join-Path $env:APPDATA 'DynamicWinV2-Optimized'
New-Item -ItemType Directory -Force $env:DYNAMICWIN_DATA_DIR | Out-Null
$settingsFile = Join-Path $env:DYNAMICWIN_DATA_DIR 'settings.json'
if (!(Test-Path $settingsFile)) {
    '{"run_count":{"TypedValue":2,"TypeName":"System.Int32"},"about.auto-update":{"TypedValue":false,"TypeName":"System.Boolean"}}' | Set-Content $settingsFile
}
$exe = Join-Path $workspace 'dist/DynamicWinV2-Optimized/DynamicWinV2.exe'
$existing = Get-Process DynamicWinV2 -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
if ($existing) { Write-Output 'The optimized version is already running.'; return }
$parameters = @{ FilePath = $exe; WorkingDirectory = Split-Path $exe; WindowStyle = 'Hidden' }
if ($Settings) { $parameters.ArgumentList = '--settings'; $parameters.WindowStyle = 'Normal' }
Start-Process @parameters
