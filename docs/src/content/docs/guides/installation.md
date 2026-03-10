---
title: Installation
description: How to install Telemachus Reborn in KSP.
---

## Requirements

- Kerbal Space Program 1.12.x
- .NET Framework 4.7.2 (ships with KSP)

## Install via CKAN

The easiest way to install Telemachus Reborn is through [CKAN](https://github.com/KSP-CKAN/CKAN):

1. Open CKAN
2. Search for "Telemachus Reborn"
3. Install and launch KSP

## Manual Install

1. Download the latest release from [GitHub Releases](https://github.com/TeleIO/Telemachus-1/releases)
2. Extract the zip into your KSP `GameData/` folder
3. Your directory should look like:

```
GameData/
  Telemachus/
    Plugins/
      Telemachus.dll
      websocket-sharp.dll
      PluginData/
        Telemachus/
          ...web files...
    Parts/
      telemachus.cfg
```

## Verify Installation

1. Launch KSP
2. In the VAB/SPH, search for "Telemachus" in the parts list
3. Add the antenna part to your vessel
4. Launch the vessel
5. Open `http://localhost:8085` in your browser

You should see the Telemachus web interface.

## Configuration

The server port and other settings can be configured in-game through the Telemachus part's right-click menu, or by editing the config file at `GameData/Telemachus/PluginData/Telemachus/config.xml`.
