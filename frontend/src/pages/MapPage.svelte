<script lang="ts">
  // Kerbal Maps. Uses bundled Leaflet (not the dead unpkg/proj4 CDNs) and the
  // shared telemetry store — the marker tracks `v.lat/v.long` from the same
  // connection every other data page uses.
  import { onMount, onDestroy } from "svelte";
  import L from "leaflet";
  import "leaflet/dist/leaflet.css";
  import { baseLayers, defaultIcon } from "../lib/maps.ts";
  import { useTelemetry, telemetry } from "../lib/telemetry.ts";

  const KEYS = ["v.long", "v.lat", "v.name", "v.altitude", "v.surfaceVelocity", "v.body"];

  let mapEl: HTMLDivElement;
  let map: L.Map;
  let marker: L.Marker;
  let stopKeys: (() => void) | undefined;
  let unsub: (() => void) | undefined;

  function updateMarker(v: Record<string, unknown>) {
    const lat = Number(v["v.lat"]);
    const long = Number(v["v.long"]);
    if (Number.isNaN(lat) || Number.isNaN(long)) return;
    marker.setLatLng([lat, long > 180 ? long - 360 : long]);
    const alt = Number(v["v.altitude"]);
    const spd = Number(v["v.surfaceVelocity"]);
    const altStr = alt > 10000 ? `${(alt / 1000).toFixed(1)} km` : `${alt.toFixed()} m`;
    marker.bindPopup(`${v["v.name"] ?? "Vessel"}<br>Altitude: ${altStr}<br>Surface Velocity: ${spd.toFixed()} m/s`);
  }

  onMount(() => {
    const layers = baseLayers();
    map = L.map(mapEl, { crs: L.CRS.EPSG4326, center: [0, 0], zoom: 0, maxZoom: 7 });
    L.control.layers(layers).addTo(map);
    (layers["Kerbin Satellite (Stock)"] ?? Object.values(layers)[0]).addTo(map);
    map.fitWorld();
    marker = L.marker([0, 0], { icon: defaultIcon() }).addTo(map);

    stopKeys = useTelemetry(KEYS);
    unsub = telemetry.subscribe(updateMarker);
  });

  onDestroy(() => {
    stopKeys?.();
    unsub?.();
    map?.remove();
  });
</script>

<div bind:this={mapEl} id="map"></div>

<style>
  #map {
    position: absolute;
    inset: 0;
    margin: 0;
    padding: 0;
  }
</style>
