#!/usr/bin/env bash

set -o errexit
set -o nounset

ProjectDir=$1
TargetDir=$2
houstonUrl="$(curl --silent "https://api.github.com/repos/TeleIO/houston/releases/latest" | grep '"browser_download_url":'  | cut -d : -f2,3 | cut -d \" -f2)"
mkonUrl="https://github.com/TeleIO/mkon/archive/master.zip" 
echo $ProjectDir
echo $TargetDir

rm -r  "$ProjectDir/../publish/GameData"
rm -r "$ProjectDir/../ksp-telemachus-dev/GameData/Telemachus"

mkdir -p  "$ProjectDir/../publish/GameData/Telemachus/Plugins"
mkdir -p  "$ProjectDir/../publish/GameData/Telemachus/Parts"
mkdir -p  "$ProjectDir/../publish/GameData/Telemachus/PluginData"
mkdir -p "$ProjectDir/../ksp-telemachus-dev/GameData/Telemachus"
mkdir -p "$ProjectDir/../ksp-telemachus-dev/GameData/Telemachus/Plugins/PluginData/Telemachus/test"
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"

#cp "$TargetDir/Servers.dll" "$ProjectDir/../publish/GameData/Telemachus/Plugins/"
cp "$TargetDir/Telemachus.dll" "$ProjectDir/../publish/GameData/Telemachus/Plugins/"
cp "$TargetDir/websocket-sharp.dll" "$ProjectDir/../publish/GameData/Telemachus/Plugins/"

cp "$ProjectDir/../dependencies/MiniAVC.dll" "$ProjectDir/../publish/GameData/Telemachus/"
cp "$ProjectDir/../TelemachusReborn.version" "$ProjectDir/../publish/GameData/Telemachus/"

cp -ra "$ProjectDir/../Parts/." "$ProjectDir/../publish/GameData/Telemachus/Parts/" 
cp -ra "$ProjectDir/../WebPages/WebPages/src/." "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"
cp -ra "$ProjectDir/../Licences/." "$ProjectDir/../publish/GameData/Telemachus/"
cp "$ProjectDir/../readme.md" "$ProjectDir/../publish/GameData/Telemachus/"

cp -ra "$ProjectDir/../WebPages/WebPagesTest/src/." "$ProjectDir/../ksp-telemachus-dev/GameData/Telemachus/Plugins/PluginData/Telemachus/test"
cp -ra "$ProjectDir/../publish/GameData/."  "$ProjectDir/../ksp-telemachus-dev/GameData/"

curl -LO $houstonUrl
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/houston"
unzip Houston.zip -d "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/houston"

curl -Lo mkon.zip $mkonUrl      
mkdir -p "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/mkon"
unzip mkon.zip
cp -ra mkon-master/. "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/mkon"

rm Houston.zip
rm mkon.zip
rm -r mkon-master
 

ls "$ProjectDir/../publish/GameData/Telemachus/Plugins/PluginData/Telemachus/"
