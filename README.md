<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="docs/public/logo.svg">
    <source media="(prefers-color-scheme: light)" srcset="docs/public/logo.svg">
    <img alt="Telemachus Reborn" src="docs/public/logo.svg" width="480">
  </picture>
</p>

<p align="center">

[![Build](https://github.com/TeleIO/Telemachus-1/actions/workflows/build.yml/badge.svg)](https://github.com/TeleIO/Telemachus-1/actions/workflows/build.yml)
[![GitHub Downloads](https://img.shields.io/github/downloads/TeleIO/Telemachus-1/total.svg)](https://github.com/TeleIO/Telemachus-1/releases)
[![Stars](https://img.shields.io/github/stars/TeleIO/Telemachus-1)](https://github.com/TeleIO/Telemachus-1/stargazers)
[![Discord](https://img.shields.io/discord/695052938095231016.svg?label=discord)](https://discord.gg/nkbauw7)

</p>

A KSP plugin that exposes live telemetry data over HTTP and WebSocket, letting you build external dashboards, flight instruments, and ground control applications for your vessels.

---

## Features

- **HTTP REST + WebSocket API** on port 8085 — poll or stream any telemetry value in real time
- **Camera streaming** — capture in-game camera views as JPEG snapshots over HTTP
- **Built-in web UI** — flight controls, navigation, orbital data, map, console, SmartASS, and more, accessible from any browser on your network
- **Flight control** — throttle, staging, SAS modes, RCS, action groups, and fly-by-wire attitude control
- **Delta-V readouts** — per-stage and total delta-V, TWR, ISP, burn time, and mass breakdown
- **Maneuver node management** — create, update, and delete maneuver nodes via the API
- **Target tracking** — set targets, read distance/relative velocity, docking alignment angles
- **Landing prediction** — impact time, suicide burn countdown, predicted coordinates and slope *(WIP — in testing)*
- **Thermal monitoring** — hottest part/engine temperatures, heat shield flux, overheat warnings *(WIP — in testing)*
- **Science & career** — experiment data, funds, reputation, science points, game mode *(WIP — in testing)*
- **CommNet telemetry** — signal strength, control state, signal delay *(WIP — in testing)*
- **Alarm clock integration** — read KSP alarm clock entries
- **Antenna parts** — three antenna models (Blade, Fustek, Yagi) with configurable up/downlink rates; also runs "partless" if needed
- **Resource & sensor monitoring** — query any resource or sensor by name
- **Plugin API** — other mods can register custom telemetry handlers

### Mod integrations

| Mod | Prefix | Description | Status |
|-----|--------|-------------|--------|
| [MechJeb](https://github.com/MuMech/MechJeb2) | `mj.*` | SmartASS autopilot orientation commands | Released |
| [FAR](https://github.com/dkavolis/Ferram-Aerospace-Research) | `far.*` | Aerodynamic coefficients, flaps, spoilers, stall detection | Released |
| [RealChute](https://github.com/StupidChris/RealChute) | `rc.*` | Parachute deployment status, safety, arm/deploy/cut | Released |
| [Astrogator](https://github.com/HebaruSan/Astrogator) | `astg.*` | Transfer window planning — delta-V, burn times, maneuver creation | Released |
| [Kerbalism](https://github.com/Kerbalism/Kerbalism) | `kerbalism.*` | Life support, radiation, habitat, crew health, space weather | Released |
| [Principia](https://github.com/mockingbirdnest/Principia) | `principia.*`, `o.mean.*` | N-body orbit analysis — mean elements, flight plan burns, recurrence | In testing |

All mod integrations are soft dependencies via reflection — no mod DLLs are required at build time. Query `a.mods` at runtime to check which mods are detected, or `a.physicsMode` to check if Principia's N-body integrator is active (`"n_body"` vs `"patched_conics"`).

---

## Installation

1. Download the latest `GameData.zip` from the [Releases](https://github.com/TeleIO/Telemachus-1/releases) page.
2. Extract and merge the `GameData/` folder into your KSP installation directory.
3. Launch KSP. The Telemachus server starts automatically when a vessel with a Telemachus antenna is active, or when running in partless mode.
4. Open a browser and navigate to `http://localhost:8085/` to access the built-in UI.

> The default port is **8085** and binds to `0.0.0.0` (all interfaces). Both can be changed in the plugin configuration file.

---

## API overview

### Endpoints

| Endpoint | Protocol | Purpose |
|----------|----------|---------|
| `/telemachus/datalink` | HTTP GET/POST | Poll telemetry values |
| `/datalink` | WebSocket | Stream telemetry in real time |
| `/telemachus/cameras` | HTTP GET | List available cameras |
| `/telemachus/cameras/<name>` | HTTP GET | Capture a camera frame (JPEG) |
| `/telemachus/*` | HTTP GET | Built-in web UI (static files) |

### HTTP polling

Query parameters use **key=value** format, where the key is an arbitrary label and the value is the API string:

```
GET /telemachus/datalink?alt=v.altitude&lat=v.lat&lon=v.long
```

```json
{ "alt": 12345.6, "lat": -0.0974, "lon": 285.4 }
```

Multiple values in a single request:

```
GET /telemachus/datalink?alt=v.altitude&pe=o.PeA&ap=o.ApA&throttle=f.throttle
```

You can also POST a JSON body instead of query parameters:

```json
POST /telemachus/datalink
Content-Type: application/json

{ "alt": "v.altitude", "pe": "o.PeA", "ap": "o.ApA" }
```

**Actions** (throttle, staging, SAS, etc.) are invoked the same way — the server executes them on the next game tick:

```
GET /telemachus/datalink?x=f.setThrottle[0.5]&y=f.stage
```

### WebSocket streaming

Connect to `ws://<host>:8085/datalink` and send JSON commands:

```json
{ "+": ["v.altitude", "o.PeA", "f.throttle"], "rate": 500 }
```

The server pushes a JSON object with current values at the requested interval (milliseconds).

| Command | Description |
|---------|-------------|
| `"+"` | Subscribe to API keys (array) |
| `"-"` | Unsubscribe from API keys (array) |
| `"run"` | One-shot evaluation — included in the next update only (array) |
| `"rate"` | Set update interval in milliseconds (integer, default 500) |
| `"binary"` | Subscribe to binary stream — values sent as big-endian float32 (array) |

**Binary packet format:** byte 0 is `0x01` (marker), followed by 4-byte IEEE 754 big-endian floats in subscription order. Sent alongside the JSON text frame each tick.

### Error handling

Unknown API keys are returned in an `"unknown"` array. Exceptions are returned in an `"errors"` object:

```json
{
  "alt": 12345.6,
  "unknown": ["v.bogus"],
  "errors": { "v.broken": "System.NullReferenceException: ..." }
}
```

### Parameter syntax

Some API keys accept parameters in bracket notation:

```
r.resource[ElectricCharge]        — resource by name
b.name[3]                         — celestial body by index
o.orbitalSpeedAt[1200.0]          — speed at orbit time
mj.surface[90,45]                 — MechJeb heading + pitch
v.setPitchYawRollXYZ[0,0,0,1,0,0] — FBW attitude + translation
```

---

### OpenAPI spec

A machine-readable [OpenAPI 3.1 spec](docs/openapi.yaml) is auto-generated from the source code. It covers all endpoints below, including mod-specific ones (tagged with `x-requires-mod`). *(The OpenAPI spec and schema generation pipeline are not yet released — in testing.)*

## API reference

### `v.*` — Vessel

<details><summary>Position & altitude</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `v.altitude` | Altitude above sea level | m |
| `v.heightFromTerrain` | Height above terrain | m |
| `v.heightFromSurface` | Height above surface | m |
| `v.terrainHeight` | Terrain altitude | m |
| `v.pqsAltitude` | PQS terrain altitude | m |
| `v.lat` | Latitude | deg |
| `v.long` | Longitude (normalized -180 to 180) | deg |

</details>

<details><summary>Velocity</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `v.surfaceVelocity` | Surface velocity magnitude | m/s |
| `v.surfaceVelocityx` | Surface velocity X | m/s |
| `v.surfaceVelocityy` | Surface velocity Y | m/s |
| `v.surfaceVelocityz` | Surface velocity Z | m/s |
| `v.orbitalVelocity` | Orbital velocity magnitude | m/s |
| `v.orbitalVelocityx` | Orbital velocity X | m/s |
| `v.orbitalVelocityy` | Orbital velocity Y | m/s |
| `v.orbitalVelocityz` | Orbital velocity Z | m/s |
| `v.surfaceSpeed` | Horizontal surface speed | m/s |
| `v.verticalSpeed` | Vertical speed | m/s |
| `v.speed` | Total speed | m/s |
| `v.srfSpeed` | Surface speed (direct) | m/s |
| `v.obtSpeed` | Orbital speed (direct) | m/s |
| `v.angularVelocity` | Angular velocity magnitude | rad/s |
| `v.angularVelocityx/y/z` | Angular velocity components | rad/s |

</details>

<details><summary>Acceleration & forces</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `v.geeForce` | G-force (averaged) | G |
| `v.geeForceImmediate` | G-force (instantaneous) | G |
| `v.acceleration` | Total acceleration magnitude | m/s^2 |
| `v.accelerationx/y/z` | Acceleration components | m/s^2 |
| `v.specificAcceleration` | Thrust / mass | m/s^2 |
| `v.perturbation` | Orbital perturbation magnitude | m/s^2 |
| `v.perturbationx/y/z` | Perturbation components | m/s^2 |

</details>

<details><summary>Mass & inertia</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `v.mass` | Total vessel mass | kg |
| `v.angularMomentum` | Angular momentum magnitude | — |
| `v.angularMomentumx/y/z` | Angular momentum components | — |
| `v.momentOfInertia` | Moment of inertia | Vector3d |
| `v.CoM` | Center of mass position | Vector3d |

</details>

<details><summary>Atmosphere & environment</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `v.atmosphericDensity` | Air density | kg/m^3 |
| `v.dynamicPressurekPa` | Dynamic pressure | kPa |
| `v.dynamicPressure` | Dynamic pressure | Pa |
| `v.staticPressurekPa` | Static pressure | kPa |
| `v.staticPressure` | Static pressure (FlightGlobals) | kPa |
| `v.atmosphericPressurePa` | Atmospheric pressure | Pa |
| `v.atmosphericPressure` | Atmospheric pressure | atm |
| `v.atmosphericTemperature` | Air temperature | K |
| `v.externalTemperature` | External temperature | K |
| `v.mach` | Mach number | — |
| `v.speedOfSound` | Speed of sound | m/s |
| `v.indicatedAirSpeed` | Indicated airspeed | m/s |
| `v.directSunlight` | In direct sunlight | bool |
| `v.distanceToSun` | Distance to sun | m |
| `v.solarFlux` | Solar flux | — |

</details>

<details><summary>Situation & state</summary>

| Key | Description | Type |
|-----|-------------|------|
| `v.name` | Vessel name | string |
| `v.body` | Celestial body name | string |
| `v.situation` | Situation enum | string |
| `v.situationString` | Human-readable situation | string |
| `v.vesselType` | Vessel type | string |
| `v.landed` | Is landed | bool |
| `v.splashed` | Is splashed | bool |
| `v.landedOrSplashed` | Is landed or splashed | bool |
| `v.landedAt` | Biome / location name | string |
| `v.isEVA` | Is EVA | bool |
| `v.isActiveVessel` | Is active vessel | bool |
| `v.isControllable` | Is controllable | bool |
| `v.isCommandable` | Is commandable | bool |
| `v.loaded` | Vessel is loaded | bool |
| `v.packed` | Vessel is on rails | bool |
| `v.currentStage` | Current stage number | int |
| `v.missionTime` | Mission elapsed time | s |
| `v.missionTimeString` | Formatted MET | string |
| `v.launchTime` | Launch time (UT) | s |
| `v.crewCount` | Number of crew | int |
| `v.crewCapacity` | Crew capacity | int |
| `v.crew` | List of crew names | string[] |
| `v.angleToPrograde` | Angle to orbital prograde | deg |

</details>

<details><summary>Action group state (read)</summary>

| Key | Description |
|-----|-------------|
| `v.sasValue` | SAS state |
| `v.rcsValue` | RCS state |
| `v.lightValue` | Lights state |
| `v.brakeValue` | Brakes state |
| `v.gearValue` | Gear state |
| `v.abortValue` | Abort state |
| `v.precisionControlValue` | Precision mode state |
| `v.ag1Value` … `v.ag10Value` | Custom action group states |

</details>

### `o.*` — Orbit

<details><summary>Keplerian elements & apsides</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `o.ApA` | Apoapsis altitude | m |
| `o.PeA` | Periapsis altitude | m |
| `o.ApR` | Apoapsis radius | m |
| `o.PeR` | Periapsis radius | m |
| `o.timeToAp` | Time to apoapsis | s |
| `o.timeToPe` | Time to periapsis | s |
| `o.sma` | Semi-major axis | m |
| `o.semiMinorAxis` | Semi-minor axis | m |
| `o.semiLatusRectum` | Semi-latus rectum | m |
| `o.eccentricity` | Eccentricity | — |
| `o.inclination` | Inclination | deg |
| `o.lan` | Longitude of ascending node | deg |
| `o.argumentOfPeriapsis` | Argument of periapsis | deg |
| `o.period` | Orbital period | s |
| `o.epoch` | Epoch | — |
| `o.referenceBody` | Reference body name | string |

</details>

<details><summary>Anomalies</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `o.trueAnomaly` | True anomaly | deg |
| `o.meanAnomaly` | Mean anomaly | deg |
| `o.eccentricAnomaly` | Eccentric anomaly | deg |
| `o.maae` | Mean anomaly at epoch | — |
| `o.timeOfPeriapsisPassage` | Time of periapsis passage (UT) | s |
| `o.orbitPercent` | Orbit completion percentage | — |

</details>

<details><summary>Velocity, position & energy</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `o.relativeVelocity` | Relative velocity magnitude | m/s |
| `o.orbitalSpeed` | Orbital speed | m/s |
| `o.vel` | Orbital velocity vector | Vector3d |
| `o.radius` | Orbital radius | m |
| `o.pos` | Orbital position vector | Vector3d |
| `o.orbitalEnergy` | Specific orbital energy | — |
| `o.orbitNormal` | Orbit normal vector | Vector3d |
| `o.eccVec` | Eccentricity vector | Vector3d |
| `o.anVec` | Ascending node vector | Vector3d |
| `o.h` | Specific angular momentum | Vector3d |

</details>

<details><summary>Parameterized queries</summary>

| Key | Description |
|-----|-------------|
| `o.orbitalSpeedAt[time]` | Speed at orbit time (m/s) |
| `o.orbitalSpeedAtDistance[dist]` | Speed at distance (m/s) |
| `o.radiusAtTrueAnomaly[deg]` | Radius at true anomaly (m) |
| `o.trueAnomalyAtRadius[m]` | True anomaly at radius (deg) |

</details>

<details><summary>Orbit patches & transitions</summary>

| Key | Description |
|-----|-------------|
| `o.orbitPatches` | All orbit patches (complex object) |
| `o.timeToTransition1` | Time to patch transition 1 (s) |
| `o.timeToTransition2` | Time to patch transition 2 (s) |
| `o.patchStartTransition` | Patch start transition type |
| `o.patchEndTransition` | Patch end transition type |
| `o.StartUT` / `o.EndUT` | Patch start/end UT |
| `o.UTsoi` | UT of SOI transition |
| `o.closestEncounterBody` | Closest encounter body name |
| `o.closestTgtApprUT` | Closest target approach UT |
| `o.trueAnomalyAtUTForOrbitPatch[patch,ut]` | True anomaly for patch at UT |
| `o.UTForTrueAnomalyForOrbitPatch[patch,deg]` | UT for true anomaly in patch |
| `o.relativePositionAtTrueAnomalyForOrbitPatch[patch,deg]` | Position at anomaly |
| `o.relativePositionAtUTForOrbitPatch[patch,ut]` | Position at UT |

</details>

<details><summary>Maneuver nodes</summary>

| Key | Description |
|-----|-------------|
| `o.maneuverNodes` | All maneuver nodes (complex object) |
| `o.maneuverNodes.count` | Number of maneuver nodes |
| `o.maneuverNodes.deltaV[id]` | Delta-V vector (Vector3d) |
| `o.maneuverNodes.deltaVMagnitude[id]` | Delta-V magnitude (m/s) |
| `o.maneuverNodes.UT[id]` | Maneuver UT |
| `o.maneuverNodes.timeTo[id]` | Time until maneuver (s) |
| `o.maneuverNodes.burnVector[id]` | Burn vector in world space (Vector3d) |
| `o.maneuverNodes.orbitPatches[id]` | Orbit patches after maneuver |
| `o.addManeuverNode[ut,x,y,z]` | **Action:** add maneuver node |
| `o.updateManeuverNode[id,ut,x,y,z]` | **Action:** update maneuver node |
| `o.removeManeuverNode[id]` | **Action:** remove maneuver node |

</details>

### `n.*` — Navigation / navball

| Key | Description | Unit |
|-----|-------------|------|
| `n.heading` / `n.pitch` / `n.roll` | From root part | deg |
| `n.rawheading` / `n.rawpitch` / `n.rawroll` | Raw from root part | deg |
| `n.heading2` / `n.pitch2` / `n.roll2` | From center of mass | deg |
| `n.rawheading2` / `n.rawpitch2` / `n.rawroll2` | Raw from center of mass | deg |

### `f.*` — Flight control

<details><summary>Throttle</summary>

| Key | Description |
|-----|-------------|
| `f.throttle` | Current throttle (0–1) |
| `f.setThrottle[float]` | **Action:** set throttle |
| `f.throttleUp` | **Action:** increase 10% |
| `f.throttleDown` | **Action:** decrease 10% |
| `f.throttleZero` | **Action:** set to 0 |
| `f.throttleFull` | **Action:** set to 1 |

</details>

<details><summary>Control inputs (read)</summary>

| Key | Description | Range |
|-----|-------------|-------|
| `f.pitchInput` / `f.yawInput` / `f.rollInput` | Rotation axes | -1 to 1 |
| `f.xInput` / `f.yInput` / `f.zInput` | RCS translation axes | -1 to 1 |
| `f.pitchTrim` / `f.yawTrim` / `f.rollTrim` | Trim values | — |
| `f.isNeutral` | All controls neutral | bool |
| `f.killRot` | SAS kill rotation active | bool |

</details>

<details><summary>Trim (set)</summary>

| Key | Description |
|-----|-------------|
| `f.setPitchTrim[float]` | **Action:** set pitch trim |
| `f.setYawTrim[float]` | **Action:** set yaw trim |
| `f.setRollTrim[float]` | **Action:** set roll trim |

</details>

<details><summary>SAS & autopilot</summary>

| Key | Description |
|-----|-------------|
| `f.sasEnabled` | SAS autopilot enabled |
| `f.sasMode` | Current SAS mode (string) |
| `f.setSASMode[mode]` | **Action:** set SAS mode |
| `f.precisionControl` | Precision mode enabled |

SAS modes: `StabilityAssist`, `Prograde`, `Retrograde`, `Normal`, `Antinormal`, `RadialIn`, `RadialOut`, `Target`, `AntiTarget`, `Maneuver`

</details>

<details><summary>Action groups</summary>

| Key | Description |
|-----|-------------|
| `f.sas` | **Action:** toggle SAS (optional `[bool]`) |
| `f.rcs` | **Action:** toggle RCS |
| `f.light` | **Action:** toggle lights |
| `f.gear` | **Action:** toggle landing gear |
| `f.brake` | **Action:** toggle brakes |
| `f.abort` | **Action:** trigger abort |
| `f.ag1` … `f.ag10` | **Action:** custom action groups |
| `f.stage` | **Action:** activate next stage |

</details>

<details><summary>Fly-by-wire (FBW)</summary>

| Key | Description |
|-----|-------------|
| `v.setFbW[1]` | **Action:** enable FBW (0 to disable) |
| `v.setPitch[float]` | **Action:** set pitch (-1 to 1) |
| `v.setYaw[float]` | **Action:** set yaw (-1 to 1) |
| `v.setRoll[float]` | **Action:** set roll (-1 to 1) |
| `v.setAttitude[pitch,yaw,roll]` | **Action:** set rotation |
| `v.setTranslation[x,y,z]` | **Action:** set RCS translation |
| `v.setPitchYawRollXYZ[p,y,r,x,y,z]` | **Action:** set all axes at once |

</details>

### `t.*` — Time & warp

| Key | Description |
|-----|-------------|
| `t.universalTime` | Current UT (seconds) |
| `t.deltaTime` | Frame delta time |
| `t.currentRate` | Current warp rate multiplier |
| `t.currentRateIndex` | Current warp rate index |
| `t.warpMode` | `HIGH` or `LOW` |
| `t.maxPhysicsRate` | Max physics warp rate |
| `t.isPaused` | Game is paused |
| `t.timeWarp[index]` | **Action:** set time warp rate |
| `t.pause` | **Action:** pause game |
| `t.unpause` | **Action:** unpause game |

### `tar.*` — Target

<details><summary>Target info</summary>

| Key | Description |
|-----|-------------|
| `tar.name` | Target name |
| `tar.type` | Target type |
| `tar.distance` | Distance to target (m) |
| `tar.o.relativeVelocity` | Relative velocity (m/s) |
| `tar.o.velocity` | Target velocity (m/s) |
| `tar.o.PeA` / `tar.o.ApA` | Target apsides (m) |
| `tar.o.inclination` | Target inclination (deg) |
| `tar.o.eccentricity` | Target eccentricity |
| `tar.o.period` | Target period (s) |
| `tar.o.sma` / `tar.o.lan` | Target SMA / LAN |
| `tar.o.trueAnomaly` | Target true anomaly (deg) |
| `tar.o.orbitingBody` | Target reference body |
| `tar.o.orbitPatches` | Target orbit patches |
| `tar.setTargetBody[index]` | **Action:** set target to body |
| `tar.setTargetVessel[index]` | **Action:** set target to vessel |
| `tar.clearTarget` | **Action:** clear target |

</details>

### `dock.*` — Docking alignment

| Key | Description | Unit |
|-----|-------------|------|
| `dock.ax` | Docking X angle | deg |
| `dock.ay` | Docking Y angle / relative pitch | deg |
| `dock.az` | Docking Z angle | deg |
| `dock.x` | Target X distance | m |
| `dock.y` | Target Y distance | m |

### `dv.*` — Delta-V

<details><summary>Totals</summary>

| Key | Description |
|-----|-------------|
| `dv.ready` | Calculator is ready |
| `dv.totalDVVac` | Total delta-V vacuum (m/s) |
| `dv.totalDVASL` | Total delta-V at sea level (m/s) |
| `dv.totalDVActual` | Total delta-V current atmosphere (m/s) |
| `dv.totalBurnTime` | Total burn time (s) |
| `dv.stageCount` | Number of stages |
| `dv.stages` | All stages (complex object) |

</details>

<details><summary>Per-stage (indexed by stage number)</summary>

| Key | Description |
|-----|-------------|
| `dv.stageDVVac[n]` | Stage delta-V vacuum (m/s) |
| `dv.stageDVASL[n]` | Stage delta-V ASL (m/s) |
| `dv.stageDVActual[n]` | Stage delta-V actual (m/s) |
| `dv.stageTWRVac[n]` | Stage TWR vacuum |
| `dv.stageTWRASL[n]` | Stage TWR ASL |
| `dv.stageTWRActual[n]` | Stage TWR actual |
| `dv.stageISPVac[n]` | Stage ISP vacuum (s) |
| `dv.stageISPASL[n]` | Stage ISP ASL (s) |
| `dv.stageISPActual[n]` | Stage ISP actual (s) |
| `dv.stageThrustVac[n]` | Stage thrust vacuum (kN) |
| `dv.stageThrustASL[n]` | Stage thrust ASL (kN) |
| `dv.stageThrustActual[n]` | Stage thrust actual (kN) |
| `dv.stageBurnTime[n]` | Stage burn time (s) |
| `dv.stageMass[n]` | Stage total mass (kg) |
| `dv.stageDryMass[n]` | Stage dry mass (kg) |
| `dv.stageFuelMass[n]` | Stage fuel mass (kg) |
| `dv.stageStartMass[n]` | Stage start mass (kg) |
| `dv.stageEndMass[n]` | Stage end mass (kg) |

</details>

### `b.*` — Celestial bodies

All body queries take a body index parameter: `b.name[0]` (Kerbol), `b.name[1]` (Kerbin), etc.

<details><summary>Properties</summary>

| Key | Description |
|-----|-------------|
| `b.number` | Total number of bodies |
| `b.name[i]` | Body name |
| `b.description[i]` | Body description |
| `b.radius[i]` | Radius (m) |
| `b.mass[i]` | Mass (kg) |
| `b.geeASL[i]` | Surface gravity (G) |
| `b.soi[i]` | Sphere of influence (m) |
| `b.hillSphere[i]` | Hill sphere radius (m) |
| `b.rotationPeriod[i]` | Rotation period (s) |
| `b.rotationAngle[i]` | Current rotation angle (deg) |
| `b.tidallyLocked[i]` | Tidally locked (bool) |
| `b.atmosphere[i]` | Has atmosphere (bool) |
| `b.maxAtmosphere[i]` | Atmosphere depth (m) |
| `b.atmosphereContainsOxygen[i]` | Has oxygen (bool) |
| `b.ocean[i]` | Has ocean (bool) |
| `b.referenceBody[i]` | Reference body name |
| `b.orbitingBodies[i]` | Names of orbiting bodies |

</details>

<details><summary>Body orbital parameters</summary>

| Key | Description |
|-----|-------------|
| `b.o.gravParameter[i]` | Gravitational parameter |
| `b.o.PeA[i]` / `b.o.ApA[i]` | Apsides (m) |
| `b.o.inclination[i]` | Inclination (deg) |
| `b.o.eccentricity[i]` | Eccentricity |
| `b.o.period[i]` | Orbital period (s) |
| `b.o.sma[i]` / `b.o.lan[i]` | SMA / LAN |
| `b.o.trueAnomaly[i]` | True anomaly (deg) |
| `b.o.phaseAngle[i]` | Phase angle to vessel (deg) |
| `b.o.truePositionAtUT[i,ut]` | Position at UT (Vector3d) |

</details>

### `r.*` — Resources

| Key | Description |
|-----|-------------|
| `r.resource[name]` | Resource amount across all parts |
| `r.resourceMax[name]` | Resource max capacity |
| `r.resourceCurrent[name]` | Resource amount in current stage |
| `r.resourceCurrentMax[name]` | Resource max in current stage |
| `r.resourceNameList` | List of all resource names |

Example: `r.resource[ElectricCharge]`, `r.resource[LiquidFuel]`, `r.resource[Oxidizer]`

### `s.*` — Sensors

| Key | Description |
|-----|-------------|
| `s.sensor[type]` | Sensor data by type |
| `s.sensor.temp` | Temperature sensors |
| `s.sensor.pres` | Pressure sensors |
| `s.sensor.grav` | Gravity sensors |
| `s.sensor.acc` | Acceleration sensors |

### `alarm.*` — Alarm clock

| Key | Description |
|-----|-------------|
| `alarm.count` | Number of active alarms |
| `alarm.list` | All alarms (complex object) |
| `alarm.nextAlarm` | Next alarm to trigger |
| `alarm.timeToNext` | Time until next alarm (s) |

### `m.*` — Map view

| Key | Description |
|-----|-------------|
| `m.mapIsEnabled` | Map view is active |
| `m.toggleMapView` | **Action:** toggle map view |
| `m.enterMapView` | **Action:** enter map view |
| `m.exitMapView` | **Action:** exit map view |

### `mj.*` — MechJeb (requires MechJeb)

| Key | Description |
|-----|-------------|
| `mj.smartassoff` | **Action:** disable SmartASS |
| `mj.node` | **Action:** point to maneuver node |
| `mj.prograde` / `mj.retrograde` | **Action:** orbital prograde/retrograde |
| `mj.normalplus` / `mj.normalminus` | **Action:** normal/anti-normal |
| `mj.radialplus` / `mj.radialminus` | **Action:** radial in/out |
| `mj.targetplus` / `mj.targetminus` | **Action:** toward/away from target |
| `mj.relativeplus` / `mj.relativeminus` | **Action:** along/against relative velocity |
| `mj.parallelplus` / `mj.parallelminus` | **Action:** parallel/anti-parallel to target |
| `mj.surface[heading,pitch]` | **Action:** surface orientation |
| `mj.surface2[heading,pitch,roll]` | **Action:** surface with roll |
| `mj.stagingInfo` | Staging simulation data |

### `p.*` — Pause state

| Key | Value | Meaning |
|-----|-------|---------|
| `p.paused` | `0` | Flight scene OK |
| | `1` | Paused |
| | `2` | No power |
| | `3` | Off |
| | `4` | Antenna not found |
| | `5` | Not in flight scene |

### `a.*` — API meta

| Key | Description |
|-----|-------------|
| `a.version` | Telemachus version string |
| `a.ip` | Server IP addresses |
| `a.api` | Full API listing |
| `a.apiSubSet[key1,key2,...]` | Subset of API listing |
| `a.mods` | Detected mod integrations (object) |
| `a.physicsMode` | `"patched_conics"` or `"n_body"` (Principia) |
| `a.schema` | Full API schema as JSON |

### Camera API

```
GET /telemachus/cameras
```

Returns a JSON array of available cameras:

```json
[
  { "name": "FlightCamera", "type": "Flight", "url": "/telemachus/cameras/FlightCamera" }
]
```

Fetch a JPEG snapshot:

```
GET /telemachus/cameras/FlightCamera
```

Returns `image/jpeg`. Returns 503 if the camera hasn't rendered yet, 404 if the camera name is unknown.

### `far.*` — FAR aerodynamics (requires FAR)

<details><summary>Aerodynamic data & controls</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `far.available` | FAR is installed | bool |
| `far.liftCoeff` | Lift coefficient (Cl) | — |
| `far.dragCoeff` | Drag coefficient (Cd) | — |
| `far.refArea` | Reference area | m^2 |
| `far.ballisticCoeff` | Ballistic coefficient | — |
| `far.dynPres` | Dynamic pressure (FAR) | kPa |
| `far.termVel` | Terminal velocity | m/s |
| `far.tsfc` | Thrust specific fuel consumption | — |
| `far.aoa` | Angle of attack | deg |
| `far.sideslip` | Sideslip angle | deg |
| `far.stallFrac` | Stall fraction (0–1) | — |
| `far.flapSetting` | Flap deflection level (0–3, -1 if none) | — |
| `far.spoiler` | Spoilers active | bool |
| `far.voxelized` | Vessel has valid voxelization | bool |
| `far.increaseFlaps` | **Action:** increase flap deflection | |
| `far.decreaseFlaps` | **Action:** decrease flap deflection | |
| `far.setSpoilers[bool]` | **Action:** set spoilers | |

</details>

### `rc.*` — RealChute (requires RealChute)

| Key | Description |
|-----|-------------|
| `rc.available` | RealChute is installed |
| `rc.count` | Number of RealChute parts |
| `rc.anyDeployed` | Any chutes deployed |
| `rc.safeState` | Deployment safety (SAFE / RISKY / DANGEROUS) |
| `rc.chutes` | All chute status (object) |
| `rc.deploy` | **Action:** deploy all chutes |
| `rc.cut` | **Action:** cut all chutes |
| `rc.arm` | **Action:** arm all chutes |
| `rc.disarm` | **Action:** disarm all chutes |

### `astg.*` — Astrogator (requires Astrogator)

<details><summary>Transfer planning</summary>

| Key | Description |
|-----|-------------|
| `astg.available` | Astrogator is installed |
| `astg.active` | Instance is active |
| `astg.errorCondition` | Transfer calculation error |
| `astg.retrogradeOrbit` | Retrograde orbit detected |
| `astg.hyperbolicOrbit` | Hyperbolic orbit detected |
| `astg.transferCount` | Number of available transfers |
| `astg.transfers` | All transfer opportunities (object) |
| `astg.transfer[index]` | Transfer by index (object) |
| `astg.activeTransfer` | Transfer with active maneuver node |
| `astg.nextDeltaV` | Next transfer total delta-V (m/s) |
| `astg.nextDestination` | Next transfer destination name |
| `astg.nextBurnTime` | Next transfer burn time (UT) |
| `astg.nextBurnCountdown` | Seconds until next transfer burn |
| `astg.createManeuver[index]` | **Action:** create maneuver for transfer |
| `astg.warpToBurn[index]` | **Action:** warp to transfer burn |

</details>

### `kerbalism.*` — Kerbalism life support & radiation (requires Kerbalism)

<details><summary>Availability & features</summary>

| Key | Description | Type |
|-----|-------------|------|
| `kerbalism.available` | Kerbalism is installed | bool |
| `kerbalism.features` | Enabled feature flags (radiation, habitat, etc.) | object |

</details>

<details><summary>Radiation</summary>

| Key | Description | Type |
|-----|-------------|------|
| `kerbalism.radiationEnabled` | Radiation system enabled | bool |
| `kerbalism.radiation` | Environment radiation | rad/h |
| `kerbalism.habitatRadiation` | Habitat radiation | rad/h |
| `kerbalism.magnetosphere` | Inside magnetosphere | bool |
| `kerbalism.innerBelt` | Inside inner radiation belt | bool |
| `kerbalism.outerBelt` | Inside outer radiation belt | bool |
| `kerbalism.stellarActivity` | Stellar activity level (0–1) | double |

</details>

<details><summary>Habitat & life support</summary>

| Key | Description | Type |
|-----|-------------|------|
| `kerbalism.habitatVolume` | Habitat volume | m³ |
| `kerbalism.habitatSurface` | Habitat surface area | m² |
| `kerbalism.habitatPressure` | Habitat pressure (0–1) | double |
| `kerbalism.co2Level` | CO₂ poisoning level | double |
| `kerbalism.radiationShielding` | Radiation shielding (0–1) | double |
| `kerbalism.habitatLivingSpace` | Living space comfort factor | double |
| `kerbalism.habitatComfort` | Overall habitat comfort | double |

</details>

<details><summary>Environment</summary>

| Key | Description | Type |
|-----|-------------|------|
| `kerbalism.envTemperature` | Environment temperature | K |
| `kerbalism.envTempDiff` | Temp difference from survival | double |
| `kerbalism.envStormRadiation` | Storm radiation dose | double |
| `kerbalism.breathable` | Atmosphere breathable | bool |
| `kerbalism.inAtmosphere` | Inside atmosphere | bool |
| `kerbalism.solarExposure` | Solar panel average exposure (0–1) | double |

</details>

<details><summary>Comms & data</summary>

| Key | Description | Type |
|-----|-------------|------|
| `kerbalism.connectionLinked` | Signal connected | bool |
| `kerbalism.connectionRate` | Data rate | MB/s |
| `kerbalism.connectionTransmitting` | Files transmitting | int |
| `kerbalism.connection` | Full connection info | object |
| `kerbalism.drivesFreeSpace` | Drive free space | MB |
| `kerbalism.drivesCapacity` | Drive total capacity | MB |

</details>

<details><summary>Space weather & crew</summary>

| Key | Description | Type |
|-----|-------------|------|
| `kerbalism.stellarStormState` | Storm state (0=none, 1=incoming, 2=active) | int |
| `kerbalism.stellarStormIncoming` | Storm incoming | bool |
| `kerbalism.stellarStormInProgress` | Storm in progress | bool |
| `kerbalism.stellarStormDuration` | Storm duration | s |
| `kerbalism.stellarStormStartTime` | Storm start time (UT) | s |
| `kerbalism.malfunction` | Part malfunction active | bool |
| `kerbalism.critical` | Critical failure active | bool |
| `kerbalism.crew` | Crew health summary (per-kerbal) | object |
| `kerbalism.experimentRunning[id]` | Experiment is running | bool |

</details>

### `principia.*` / `o.mean.*` — Principia N-body physics (requires Principia) *(WIP — in testing)*

When [Principia](https://github.com/mockingbirdnest/Principia) is installed, KSP's patched-conic orbit propagator is replaced with an N-body integrator. Stock `o.*` values become osculating (instantaneous Keplerian) snapshots. Principia's orbit analyser computes **mean elements** averaged over many orbits — these are available under `o.mean.*`.

> All data is read via reflection from Principia's runtime state. No Principia DLLs are needed at build time, and Principia's (removed) external API is not used.

<details><summary>Detection & status</summary>

| Key | Description |
|-----|-------------|
| `principia.available` | Principia is installed |
| `principia.version` | Principia assembly version |
| `principia.active` | Principia plugin is running |
| `principia.analysisProgress` | Orbit analysis progress (0–1) |
| `principia.missionDuration` | Analysis mission duration (s) |

</details>

<details><summary>Mean orbital elements</summary>

These mirror the stock `o.*` keys but report mean (time-averaged) values from Principia's orbit analyser. Angles are in degrees, distances are altitudes (body radius subtracted).

| Key | Description | Unit |
|-----|-------------|------|
| `o.mean.sma` | Mean semi-major axis | m |
| `o.mean.eccentricity` | Mean eccentricity | — |
| `o.mean.inclination` | Mean inclination | deg |
| `o.mean.lan` | Mean longitude of ascending node | deg |
| `o.mean.argumentOfPeriapsis` | Mean argument of periapsis | deg |
| `o.mean.PeA` | Mean periapsis altitude | m |
| `o.mean.ApA` | Mean apoapsis altitude | m |

</details>

<details><summary>Element ranges (min/max from N-body integration)</summary>

Each returns `{ "min": number, "max": number }`.

| Key | Description |
|-----|-------------|
| `o.mean.smaRange` | SMA range |
| `o.mean.eccentricityRange` | Eccentricity range |
| `o.mean.inclinationRange` | Inclination range (deg) |
| `o.mean.PeARange` | Periapsis altitude range (m) |
| `o.mean.ApARange` | Apoapsis altitude range (m) |

</details>

<details><summary>Periods, precession & recurrence</summary>

| Key | Description | Unit |
|-----|-------------|------|
| `o.mean.siderealPeriod` | Sidereal period | s |
| `o.mean.nodalPeriod` | Nodal period | s |
| `o.mean.anomalisticPeriod` | Anomalistic period | s |
| `o.mean.nodalPrecession` | Nodal precession rate | rad/s |
| `o.mean.recurrence` | Orbit recurrence info (object) | — |

</details>

<details><summary>Flight plan</summary>

| Key | Description |
|-----|-------------|
| `principia.plan.count` | Number of planned burns |
| `principia.plan.guidance` | Navball guidance active |
| `principia.plan.burns` | All planned burns (array of objects) |
| `principia.plan.burn[index]` | Single burn by index |
| `principia.analysis` | Complete orbit analysis dump (object) |

Each burn object contains `{ tangent, normal, binormal, initial_time, duration }` in m/s and seconds.

</details>

### `land.*` — Landing prediction *(WIP — in testing)*

| Key | Description | Unit |
|-----|-------------|------|
| `land.timeToImpact` | Estimated seconds to impact | s |
| `land.speedAtImpact` | Predicted speed at impact (current thrust) | m/s |
| `land.bestSpeedAtImpact` | Predicted speed at impact (max thrust) | m/s |
| `land.suicideBurnCountdown` | Seconds until suicide burn start | s |
| `land.predictedLat` | Predicted landing latitude | deg |
| `land.predictedLon` | Predicted landing longitude | deg |
| `land.predictedAlt` | Predicted landing terrain altitude | m |
| `land.slopeAngle` | Terrain slope angle under vessel | deg |

### `therm.*` — Thermal monitoring *(WIP — in testing)*

| Key | Description | Unit |
|-----|-------------|------|
| `therm.hottestPartTemp` | Hottest part temperature | C |
| `therm.hottestPartTempKelvin` | Hottest part temperature | K |
| `therm.hottestPartMaxTemp` | Hottest part max temperature | K |
| `therm.hottestPartTempRatio` | Hottest part temp ratio (0–1) | — |
| `therm.hottestPartName` | Hottest part name | string |
| `therm.hottestEngineTemp` | Hottest engine temperature | K |
| `therm.hottestEngineMaxTemp` | Hottest engine max temperature | K |
| `therm.hottestEngineTempRatio` | Hottest engine temp ratio (0–1) | — |
| `therm.anyEnginesOverheating` | Any engine near overheat (>90%) | bool |
| `therm.heatShieldTemp` | Heat shield temperature | K |
| `therm.heatShieldTempCelsius` | Heat shield temperature | C |
| `therm.heatShieldFlux` | Heat shield thermal flux | kW |

### `sci.*` / `career.*` / `comm.*` — Science, career & comms *(WIP — in testing)*

| Key | Description |
|-----|-------------|
| `sci.count` | Number of science experiments aboard |
| `sci.dataAmount` | Total science data aboard |
| `sci.experiments` | Experiments with data (object) |
| `career.funds` | Available funds |
| `career.reputation` | Current reputation |
| `career.science` | Available science points |
| `career.mode` | Game mode (CAREER / SCIENCE / SANDBOX) |
| `comm.connected` | CommNet is connected |
| `comm.signalStrength` | CommNet signal strength (0–1) |
| `comm.controlState` | CommNet control state (0=none, 1=partial, 2=full) |
| `comm.controlStateName` | CommNet control state name |
| `comm.signalDelay` | CommNet signal delay (s) |

---

## Building

The project targets .NET Framework 4.7.2 via an SDK-style csproj and builds with the .NET SDK. Unity and KSP reference DLLs are pre-committed to `references/` — no KSP installation is required to build.

### With Nix (recommended)

```sh
nix develop
dotnet build Telemachus/Telemachus.csproj
```

### Without Nix

Install the [.NET SDK 8+](https://dotnet.microsoft.com/download) (or Mono), then:

```sh
dotnet build Telemachus/Telemachus.csproj
```

The post-build script stages the output to `publish/GameData/` and downloads the [Houston](https://github.com/TeleIO/houston) and [mkon](https://github.com/TeleIO/mkon) companion apps automatically.

### OpenAPI spec

A [source generator](Telemachus.DocGen/) extracts all `[TelemetryAPI]` attributes at build time into `publish/api-schema.json`. To regenerate the OpenAPI spec and docs:

```sh
bun tools/generate-openapi.ts
```

This merges the auto-generated schema with `tools/manual-apis.json` (for constructor-registered APIs) and writes `docs/openapi.yaml`.

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
