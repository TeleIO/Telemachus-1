# Telemachus Reborn

[![Build](https://github.com/TeleIO/Telemachus-1/actions/workflows/build.yml/badge.svg)](https://github.com/TeleIO/Telemachus-1/actions/workflows/build.yml)
[![GitHub Downloads](https://img.shields.io/github/downloads/TeleIO/Telemachus-1/total.svg)](https://github.com/TeleIO/Telemachus-1/releases)
[![Stars](https://img.shields.io/github/stars/TeleIO/Telemachus-1)](https://github.com/TeleIO/Telemachus-1/stargazers)
[![Discord](https://img.shields.io/discord/695052938095231016.svg?label=discord)](https://discord.gg/nkbauw7)

A KSP plugin that exposes live telemetry data over HTTP and WebSocket, letting you build external dashboards, flight instruments, and ground control applications for your vessels.

---

## Features

- **HTTP REST + WebSocket API** on port 8085 — poll or stream any telemetry value in real time
- **Built-in web UI** — flight controls, navigation, orbital data, map, console, SmartASS, and more, accessible from any browser on your network
- **MechJeb integration** — read and command SmartASS autopilot modes via the API
- **Antenna parts** — three antenna models (Blade, Fustek, Yagi) with configurable up/downlink rates; the plugin also runs "partless" if needed
- **Sensor support** — temperature, pressure, gravity, acceleration
- **Resource monitoring** — query any resource by name
- Supports **KSP 1.6.0 – 1.9.x**

---

## Installation

1. Download the latest `GameData.zip` from the [Releases](https://github.com/TeleIO/Telemachus-1/releases) page.
2. Extract and merge the `GameData/` folder into your KSP installation directory.
3. Launch KSP. The Telemachus server starts automatically when a vessel with a Telemachus antenna is active, or when running in partless mode.
4. Open a browser and navigate to `http://localhost:8085/` to access the built-in UI.

> The default port is **8085**. It can be changed in the in-game settings window.

---

## API

The API is accessible at `http://<host>:8085/telemachus/datalink`.

### HTTP (polling)

Request one or more values by passing their keys as query parameters:

```
GET /telemachus/datalink?v.altitude&v.lat&v.long
```

```json
{ "v.altitude": 12345.6, "v.lat": -0.0974, "v.long": 285.4 }
```

### WebSocket

Connect to `ws://<host>:8085/datalink` and send a JSON subscription message:

```json
{ "+": ["v.altitude", "v.lat", "v.long"], "rate": 500 }
```

The server will push updates at the requested interval (milliseconds). Use `"+"` to subscribe and `"-"` to unsubscribe.

### Key reference

| Prefix | Data |
|--------|------|
| `v.*`  | Vessel — altitude, latitude, longitude, name, body, velocity |
| `o.*`  | Orbit — SMA, eccentricity, inclination, LAN, period, epoch |
| `n.*`  | Navigation — heading, pitch, roll (raw and surface-relative) |
| `f.*`  | Flight controls — throttle, SAS, RCS, gear, brakes, lights, staging, abort |
| `r.*`  | Resources — query any resource by name |
| `s.*`  | Sensors — temperature, pressure, gravity, acceleration |
| `b.*`  | Target body — name, radius, SOI, orbital parameters |
| `p.*`  | Pause state |
| `t.*`  | Time controls — pause / unpause |
| `mj.*` | MechJeb — SmartASS modes (requires MechJeb) |
| `a.*`  | API meta — version, IP |

Full API documentation is available on the [wiki](https://github.com/TeleIO/Telemachus-1/wiki).

---

## Building

Dependencies (Unity and KSP DLLs) are pre-committed to `references/`. No KSP installation is required to build.

### With Nix (recommended)

```sh
nix develop
msbuild Telemachus.sln /p:Configuration=Release
```

The post-build script stages the output to `publish/GameData/` and downloads the [Houston](https://github.com/TeleIO/houston) and [mkon](https://github.com/TeleIO/mkon) companion apps automatically.

### Without Nix

Install [Mono](https://www.mono-project.com/) (which includes MSBuild), then:

```sh
msbuild Telemachus.sln /p:Configuration=Release
```

---

## Contributing

Contributions are welcome — see [CONTRIBUTORS.md](CONTRIBUTORS.md) for the full list of people who have helped build this.

- Bug reports and feature requests: [open an issue](https://github.com/TeleIO/Telemachus-1/issues)
- Chat: [Discord server](https://discord.gg/nkbauw7)
- Pull requests: please follow the [conventional commits](https://www.conventionalcommits.org/) format (`feat:`, `fix:`, `docs:`, etc.). A commit-msg hook is installed automatically when you enter the Nix dev shell.

Please follow the [Code of Conduct](https://github.com/TeleIO/Telemachus-1/wiki/Code-of-Conduct).

---

## License

See [LICENSE](LICENSE) and [Licences/](Licences/) for third-party licenses.
