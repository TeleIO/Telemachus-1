#!/usr/bin/env bash

set -o errexit
set -o nounset

ProjectDir=$1
TargetDir=$2
houstonUrl="$(curl --silent "https://api.github.com/repos/TeleIO/houston/releases/latest" | grep '"browser_download_url":' | cut -d : -f2,3 | cut -d \" -f2)"
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

cp -ra "$ProjectDir/../Parts/."                         "$ProjectDir/../publish/GameData/Telemachus/Parts/"
cp -ra "$ProjectDir/../WebPages/WebPages/src/."         "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"
cp -ra "$ProjectDir/../Licences/."                      "$ProjectDir/../publish/GameData/Telemachus/"
cp     "$ProjectDir/../readme.md"                       "$ProjectDir/../publish/GameData/Telemachus/"

# Download Houston
curl -LO "$houstonUrl"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/houston"
unzip Houston.zip -d "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/houston"

# Download mkon
curl -Lo mkon.zip "$mkonUrl"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/mkon"
unzip mkon.zip
cp -ra mkon-master/. "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/mkon"

rm Houston.zip mkon.zip
rm -rf mkon-master

# Copy to local KSP install (local dev only — skipped in CI)
kspDir="$ProjectDir/../ksp-telemachus-dev"
if [ -d "$kspDir" ]; then
  rm -rf "$kspDir/GameData/Telemachus"
  mkdir -p "$kspDir/GameData/Telemachus/Plugins/PluginData/Telemachus/test"
  cp -ra "$ProjectDir/../WebPages/WebPagesTest/src/." "$kspDir/GameData/Telemachus/Plugins/PluginData/Telemachus/test"
  cp -ra "$ProjectDir/../publish/GameData/."          "$kspDir/GameData/"
fi

ls "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"
