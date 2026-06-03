# Telemachus bundled web UI (frontend)

A modern **Deno + Svelte + Vite** rebuild of the in-mod web UI that Telemachus
serves at `/telemachus/`. It replaces the jQuery 1.9 / CoffeeScript / dead-CDN
pages under `../WebPages/WebPages/src` while preserving the same pages, layout,
features and CSS — only the core (data client, reactivity, build) is swapped.

## Run it

```sh
deno task gen     # regenerate src/lib/schema.gen.ts from ../docs/api-schema.json
deno task dev     # Vite dev server
deno task build   # -> dist/  (static, relative-pathed, ready to serve)
```

The build emits one self-contained set of static files per page with relative
asset URLs, so it drops straight into the plugin's static server (which has no
SPA fallback and serves `.js` as `application/x-javascript`). Routing stays
multi-page; no `.mjs`/`.wasm` is emitted.

## Eyeball it locally (no KSP)

```sh
deno task mock    # builds the UI, then serves it with fake telemetry
```

Open <http://localhost:8085/telemachus/index.html>. The mock (`mock/server.ts`)
emulates the plugin: static files under `/telemachus/`, the HTTP
`datalink?alias=api` endpoint, the `/datalink` WebSocket push channel
(+/-/run/rate), and `a.api`/`a.version`/`a.ip`. Telemetry is a small coherent
flight sim, so the map marker moves, charts scroll and tables update; control
pages send real (no-op) actions. `deno task serve` serves an existing `dist/`
without rebuilding.

## Architecture (what got deduplicated)

| Shared module | Replaces |
| --- | --- |
| `lib/telemachus.ts` | the 3 legacy clients — `jKSPWAPICore.js`, console.js's `Telemachus`, touchball's `telemachus`. Exposes `TelemachusClient` (HTTP `call`/`poll`/`action`) **and** `TelemachusSocket` (the `/datalink` WebSocket push channel). |
| `lib/telemetry.ts` | per-page poll loops — one ref-counted shared connection (WS-first, poll fallback) feeding reactive `telemetry` / `signal` / `connected` stores. |
| `lib/command.ts` | the copy-pasted `command()`/`toggle()`/`execute()` handlers. |
| `lib/notify.ts` + `components/Toast.svelte` | the global `sNotify` queue. |
| `lib/format.ts` | the two legacy formatter sets. |
| `lib/maps.ts` | the 560-line `layer-defs.js` (generated layer table; bundled Leaflet). |
| `lib/chart.ts` | console.js's d3 `Chart` class (kept on d3 v3, jQuery removed). |
| `lib/strings.ts` | `ru_console.html` — it's now `Console.svelte` with `lang="ru"`. |
| `lib/pages.ts` | the page lists duplicated in index.html + information.html. |

`schema.gen.ts` is generated from the same `api-schema.json` that drives the
docs site (`a.schema`) — one source of truth for endpoint metadata.

## Pages

menu (`index`), `information`, `console`, `ru_console`, `map`, `flight-control`,
`smart-ass`, `d-pad`, `touchball-pyr`, `speech`. (`font_demo`, a static font
showcase, was dropped.)

## Deploying (wiring into the mod)

Not yet wired into `Telemachus/AfterBuild.sh` — flip it when ready by building
here and copying `dist/` into the served path instead of the legacy `src`:

```sh
(cd frontend && deno task build)
cp -pR frontend/dist/. publish/GameData/Telemachus/Plugins/PluginData/Telemachus/
```

(The CI `Build` job would need Deno added for this to run there.)

## Known simplifications vs. the legacy console

Faithful in look and data, but these console niceties are not reimplemented:
drag-to-reorder of telemetry rows, the custom-chart builder, and the chart
legend hover/click series isolation. The per-frame scroll easing is replaced by
a straight redraw.
