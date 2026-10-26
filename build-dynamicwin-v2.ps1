param([switch]$Test)
$ErrorActionPreference = 'Stop'
$dotnet = Join-Path $PSScriptRoot '.tools/dotnet/dotnet.exe'
$analysis = Join-Path $PSScriptRoot 'analysis/dynamicwin'
$destination = Join-Path $PSScriptRoot 'dist/DynamicWinV2-Optimized'
if (!(Test-Path $dotnet)) {
    $dotnet = (Get-Command dotnet -ErrorAction Stop).Source
}
$env:DOTNET_ROOT = Split-Path $dotnet
$vendor = Join-Path $analysis 'v2.0.3'
if (!(Test-Path (Join-Path $vendor 'DynamicWinV2.deps.json'))) {
    $archive = Join-Path $analysis 'DynamicWinPortable.zip'
    Invoke-WebRequest 'https://github.com/FlorianButz/DynamicWin/releases/download/v2.0.3/DynamicWinPortable.zip' -OutFile $archive
    if ((Get-FileHash $archive -Algorithm SHA256).Hash -ne '9904C5848BEB90D4A1E131260AF3A2721D34A0163C83D6F178149EFB69C614E6') {
        throw 'The upstream archive checksum does not match.'
    }
    Expand-Archive -LiteralPath $archive -DestinationPath $vendor -Force
}
& $dotnet build (Join-Path $analysis 'optimized-v2/DynamicWinV2.csproj') -c Release -v quiet
if ($LASTEXITCODE -ne 0) { throw 'V2 build failed.' }
if ($Test) {
    & $dotnet run --project (Join-Path $analysis 'performance-tests/PerformanceTests.csproj') -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Performance tests failed.' }
}
if (Get-Process DynamicWinV2 -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq (Join-Path $destination 'DynamicWinV2.exe') }) {
    throw 'Close the optimized app before packaging. The compiled sources are ready in bin/Release.'
}
New-Item -ItemType Directory -Force $destination | Out-Null
Copy-Item (Join-Path $analysis 'v2.0.3/*') $destination -Recurse -Force
Copy-Item (Join-Path $analysis 'optimized-v2/bin/Release/net9.0-windows/*') $destination -Recurse -Force
# Preserve vendor dependencies (including native libraries), adding new NuGet packages.
$vendorDeps = Get-Content (Join-Path $analysis 'v2.0.3/DynamicWinV2.deps.json') -Raw | ConvertFrom-Json -AsHashtable
$builtDeps = Get-Content (Join-Path $analysis 'optimized-v2/bin/Release/net9.0-windows/DynamicWinV2.deps.json') -Raw | ConvertFrom-Json -AsHashtable
$vendorTarget = $vendorDeps.targets[$vendorDeps.runtimeTarget.name]
$builtTarget = $builtDeps.targets[$builtDeps.runtimeTarget.name]
foreach ($entry in $builtDeps.libraries.GetEnumerator()) {
    if ($entry.Value.type -ne 'package') { continue }
    $parts = $entry.Key.Split('/')
    if (!$vendorDeps.libraries.Contains($entry.Key)) {
        $vendorDeps.libraries[$entry.Key] = $entry.Value
        $vendorTarget[$entry.Key] = $builtTarget[$entry.Key]
    }
    $vendorTarget['DynamicWinV2/1.0.0'].dependencies[$parts[0]] = $parts[1]
}
$vendorDeps | ConvertTo-Json -Depth 100 | Set-Content (Join-Path $destination 'DynamicWinV2.deps.json')
Copy-Item (Join-Path $analysis 'Launch-Optimized.ps1') $destination -Force
Copy-Item (Join-Path $analysis 'PERFORMANCES.md') $destination -Force
Copy-Item (Join-Path $analysis 'INTEGRATIONS.md') $destination -Force
Copy-Item (Join-Path $analysis 'LICENSE-DynamicWin.txt') (Join-Path $destination 'LICENSE-DynamicWin.txt') -Force
Write-Output "Ready: $destination"

