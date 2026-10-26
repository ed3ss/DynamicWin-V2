$ErrorActionPreference = 'Stop'
$dotnet = Join-Path $PSScriptRoot '.tools/dotnet/dotnet.exe'
if (!(Test-Path $dotnet)) { $dotnet = (Get-Command dotnet -ErrorAction Stop).Source }
& $dotnet run --project (Join-Path $PSScriptRoot 'analysis/dynamicwin/performance-tests/PerformanceTests.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'V2 tests failed.' }
