<#
.SYNOPSIS
    Builds the Thunderstore package into dist/.

.DESCRIPTION
    TarDistillation-<version>.zip   The one package, for players and for dedicated
                                    servers alike. Import it as a local mod in
                                    Thunderstore Mod Manager / r2modman, upload it to
                                    Thunderstore as-is, or unpack it over a server root.

    The plugin sits at BepInEx/plugins/TarDistillation/ inside the zip, which is the
    layout mod managers expect and the layout a server root already has, so the same
    archive serves both.

    BepInEx and Jotunn are dependencies and are not bundled.
#>

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

# Compress-Archive on Windows PowerShell writes entry names with backslashes, which
# unzip on a Linux server turns into one file with slashes in its name instead of a
# folder tree. So write the entries by hand with forward slashes.
function New-Zip {
    param([string]$SourceDir, [string]$Destination)

    Add-Type -AssemblyName System.IO.Compression | Out-Null
    Add-Type -AssemblyName System.IO.Compression.FileSystem | Out-Null

    if (Test-Path $Destination) { Remove-Item $Destination -Force }

    $base = (Resolve-Path $SourceDir).Path.TrimEnd('\') + '\'
    $zip = [System.IO.Compression.ZipFile]::Open($Destination, 'Create')
    try {
        foreach ($file in Get-ChildItem -Path $SourceDir -Recurse -File) {
            $name = $file.FullName.Substring($base.Length).Replace('\', '/')
            [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $name)
        }
    }
    finally {
        $zip.Dispose()
    }
}
$version = (Get-Content "$root\package\manifest.json" -Raw | ConvertFrom-Json).version_number

dotnet build "$root\TarDistillation.csproj" -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

$dll = Join-Path $root 'bin\Release\TarDistillation.dll'
if (-not (Test-Path $dll)) { throw "Missing $dll" }

$dist = Join-Path $root 'dist'
$stage = Join-Path $dist 'stage'
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $dist | Out-Null

# Thunderstore wants manifest, icon, README and CHANGELOG at the root; everything else
# mirrors the BepInEx folder.
$plugins = Join-Path $stage 'BepInEx\plugins\TarDistillation'
New-Item -ItemType Directory -Force -Path $plugins | Out-Null
Copy-Item $dll $plugins

# The models, textures and icons, read off disk at runtime. The plugin falls back to the
# vanilla look without them, so a missing folder costs the look and nothing else - but the
# package is the only place a player gets them from.
$assets = Join-Path $root 'assets'
if (-not (Test-Path $assets)) { throw "Missing $assets" }
Copy-Item $assets -Destination $plugins -Recurse
Copy-Item (Join-Path $root 'package\manifest.json') $stage
Copy-Item (Join-Path $root 'package\icon.png') $stage
Copy-Item (Join-Path $root 'package\INSTALL.md') $stage
Copy-Item (Join-Path $root 'package\CHANGELOG.md') $stage
Copy-Item (Join-Path $root 'README.md') $stage

$zip = Join-Path $dist "TarDistillation-$version.zip"
New-Zip -SourceDir $stage -Destination $zip

Remove-Item $stage -Recurse -Force

Get-Item $zip | Select-Object Name, @{ N = 'KB'; E = { [math]::Round($_.Length / 1KB, 1) } }
