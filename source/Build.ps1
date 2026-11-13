param([string]$OutputDirectory = (Join-Path $PSScriptRoot '..\app'))
$ErrorActionPreference = 'Stop'
if (-not (Test-Path (Join-Path $PSScriptRoot 'app.ico'))) { & (Join-Path $PSScriptRoot 'Create-Icon.ps1') }
& dotnet restore (Join-Path $PSScriptRoot 'SpotifyGlassSidebar.csproj') --configfile (Join-Path $PSScriptRoot 'NuGet.Config')
if ($LASTEXITCODE -ne 0) { throw 'Nie udało się pobrać zależności.' }
& dotnet publish (Join-Path $PSScriptRoot 'SpotifyGlassSidebar.csproj') -c Release --no-restore -o $OutputDirectory
if ($LASTEXITCODE -ne 0) { throw 'Kompilacja nie powiodła się.' }
Write-Host "Gotowe: $OutputDirectory\SpotifyGlassSidebar.exe"