#!/usr/bin/env bash

set -o errexit
set -o nounset

ProjectDir=$1
TargetDir=$2

authHeader=()
if [ -n "${GITHUB_TOKEN:-}" ]; then
  authHeader=(-H "Authorization: token $GITHUB_TOKEN")
fi
# Portable expansion of a possibly-empty array under `set -u` (works on older bash too).
houstonUrl="$(curl --silent ${authHeader[@]+"${authHeader[@]}"} "https://api.github.com/repos/TeleIO/houston/releases/latest" | grep '"browser_download_url":' | cut -d : -f2,3 | cut -d \" -f2)"
mkonUrl="https://github.com/TeleIO/mkon/archive/master.zip"

echo "$ProjectDir"
echo "$TargetDir"

# Stage publish directory
rm -rf "$ProjectDir/../publish/GameData"

mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Parts"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/PluginData"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"

cp "$TargetDir/Telemachus.dll"      "$ProjectDir/../publish/GameData/Telemachus/Plugins/"
cp "$TargetDir/websocket-sharp.dll" "$ProjectDir/../publish/GameData/Telemachus/Plugins/"

cp "$ProjectDir/../TelemachusReborn.version" "$ProjectDir/../publish/GameData/Telemachus/"

cp -pR "$ProjectDir/../Parts/."                         "$ProjectDir/../publish/GameData/Telemachus/Parts/"
cp -pR "$ProjectDir/../Licences/."                      "$ProjectDir/../publish/GameData/Telemachus/"

# Web UI: build the modern frontend (frontend/, Deno+Svelte) when Deno is
# available; otherwise fall back to the legacy WebPages/src pages so the build
# still works on machines without Deno.
webDest="$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"
frontendDir="$ProjectDir/../frontend"
if command -v deno >/dev/null 2>&1 && [ -f "$frontendDir/deno.json" ]; then
  echo "Building modern web UI with Deno..."
  ( cd "$frontendDir" && deno install --quiet && deno task build )
  cp -pR "$frontendDir/dist/." "$webDest"
else
  echo "Deno not available — shipping legacy WebPages/src."
  cp -pR "$ProjectDir/../WebPages/WebPages/src/." "$webDest"
fi
cp     "$ProjectDir/../README.md"                       "$ProjectDir/../publish/GameData/Telemachus/"

# Download Houston
curl -LO "$houstonUrl"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/houston"
unzip Houston.zip -d "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/houston"

# Download mkon
curl -Lo mkon.zip "$mkonUrl"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/mkon"
unzip mkon.zip
cp -pR mkon-master/. "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/mkon"

rm Houston.zip mkon.zip
rm -rf mkon-master

# Extract API schema from source-generated file
schemaFile=$(find "$ProjectDir/obj" -name "TelemetrySchema.g.cs" -type f 2>/dev/null | head -1)
if [ -n "$schemaFile" ]; then
  # Extract the JSON from between the @" and "; markers, un-doubling quotes
  sed -n '/SCHEMA_JSON_BEGIN/,/SCHEMA_JSON_END/p' "$schemaFile" \
    | grep -v 'SCHEMA_JSON' \
    | sed 's/.*internal const string Json = @"//;s/";//' \
    | sed 's/""/"/g' \
    > "$ProjectDir/../publish/api-schema.json"
  echo "Extracted API schema to publish/api-schema.json"
else
  echo "Warning: TelemetrySchema.g.cs not found in obj/"
fi

# Copy to local KSP install (local dev only — skipped in CI)
kspDir="$ProjectDir/../ksp-telemachus-dev"
if [ -d "$kspDir" ]; then
  rm -rf "$kspDir/GameData/Telemachus"
  mkdir -p "$kspDir/GameData/Telemachus/Plugins/PluginData/Telemachus/test"
  cp -pR "$ProjectDir/../WebPages/WebPagesTest/src/." "$kspDir/GameData/Telemachus/Plugins/PluginData/Telemachus/test"
  cp -pR "$ProjectDir/../publish/GameData/."          "$kspDir/GameData/"
fi

ls "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"
