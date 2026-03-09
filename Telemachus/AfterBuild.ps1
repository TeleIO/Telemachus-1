param(
    [Parameter(Mandatory)][string]$ProjectDir,
    [Parameter(Mandatory)][string]$TargetDir
)

$ErrorActionPreference = 'Stop'

$root = Resolve-Path "$ProjectDir/.."
$publish = "$root/publish/GameData/Telemachus"
$pluginData = "$publish/Plugins/PluginData/Telemachus"

Write-Host "ProjectDir: $ProjectDir"
Write-Host "TargetDir:  $TargetDir"

# Stage publish directory
if (Test-Path "$root/publish/GameData") {
    Remove-Item "$root/publish/GameData" -Recurse -Force
}

New-Item -ItemType Directory -Force -Path "$publish/Plugins", "$publish/Parts", "$publish/PluginData", $pluginData | Out-Null

Copy-Item "$TargetDir/Telemachus.dll"      "$publish/Plugins/"
Copy-Item "$TargetDir/websocket-sharp.dll" "$publish/Plugins/"

Copy-Item "$root/TelemachusReborn.version" "$publish/"

Copy-Item "$root/Parts/*"                "$publish/Parts/"    -Recurse -Force
Copy-Item "$root/WebPages/WebPages/src/*" $pluginData          -Recurse -Force
Copy-Item "$root/Licences/*"             "$publish/"          -Recurse -Force
Copy-Item "$root/README.md"              "$publish/"

# Download Houston
$headers = @{}
if ($env:GITHUB_TOKEN) {
    $headers['Authorization'] = "token $env:GITHUB_TOKEN"
}

$release = Invoke-RestMethod -Uri 'https://api.github.com/repos/TeleIO/houston/releases/latest' -Headers $headers
$houstonUrl = $release.assets[0].browser_download_url

$houstonZip = Join-Path $TargetDir 'Houston.zip'
Invoke-WebRequest -Uri $houstonUrl -OutFile $houstonZip
New-Item -ItemType Directory -Force -Path "$pluginData/houston" | Out-Null
Expand-Archive -Path $houstonZip -DestinationPath "$pluginData/houston" -Force

# Download mkon
$mkonZip = Join-Path $TargetDir 'mkon.zip'
Invoke-WebRequest -Uri 'https://github.com/TeleIO/mkon/archive/master.zip' -OutFile $mkonZip
$mkonTmp = Join-Path $TargetDir 'mkon-extract'
Expand-Archive -Path $mkonZip -DestinationPath $mkonTmp -Force
New-Item -ItemType Directory -Force -Path "$pluginData/mkon" | Out-Null
Copy-Item "$mkonTmp/mkon-master/*" "$pluginData/mkon" -Recurse -Force

# Cleanup
Remove-Item $houstonZip, $mkonZip -Force -ErrorAction SilentlyContinue
Remove-Item $mkonTmp -Recurse -Force -ErrorAction SilentlyContinue

# Copy to local KSP install (local dev only)
$kspDir = "$root/ksp-telemachus-dev"
if (Test-Path $kspDir) {
    if (Test-Path "$kspDir/GameData/Telemachus") {
        Remove-Item "$kspDir/GameData/Telemachus" -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path "$kspDir/GameData/Telemachus/Plugins/PluginData/Telemachus/test" | Out-Null
    Copy-Item "$root/WebPages/WebPagesTest/src/*" "$kspDir/GameData/Telemachus/Plugins/PluginData/Telemachus/test" -Recurse -Force
    Copy-Item "$root/publish/GameData/*" "$kspDir/GameData/" -Recurse -Force
}

Get-ChildItem $pluginData
