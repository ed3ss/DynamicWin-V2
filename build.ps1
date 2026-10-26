param([switch]$Test)
& (Join-Path $PSScriptRoot 'build-dynamicwin-v2.ps1') -Test:$Test
