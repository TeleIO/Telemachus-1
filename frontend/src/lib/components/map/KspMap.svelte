<script lang="ts">
	import { onMount } from 'svelte';
	import L from 'leaflet';
	import { getAllLayers, getDefaultLayer, type BodyLayer } from './MapLayerDefs.js';
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { formatValue } from '$lib/utils/formatters.js';

	let mapContainer: HTMLDivElement;
	let map: L.Map | null = null;
	let marker: L.Marker | null = null;
	let currentLayer: L.TileLayer | null = null;

	let allLayers = $state<BodyLayer[]>([]);
	let selectedBody = $state('kerbin');
	let selectedStyle = $state<string>('sat');
	let selectedPack = $state<string>('Stock');

	const APIS = ['v.lat', 'v.long', 'v.name', 'v.body', 'v.altitude', 'v.surfaceSpeed'];
	useSubscription(() => APIS);

	let bodies = $derived(() => {
		const set = new Set<string>();
		for (const l of allLayers) if (l.pack === selectedPack) set.add(l.body);
		return [...set];
	});

	function switchLayer() {
		if (!map) return;
		const found = allLayers.find(
			(l) => l.body === selectedBody && l.style === selectedStyle && l.pack === selectedPack
		);
		if (!found) return;
		if (currentLayer) map.removeLayer(currentLayer);
		currentLayer = found.layer;
		currentLayer.addTo(map);
	}

	onMount(() => {
		allLayers = getAllLayers();
		const defaultLayer = getDefaultLayer();

		map = L.map(mapContainer, {
			crs: L.CRS.EPSG4326,
			center: [0, 0],
			zoom: 1,
			minZoom: 0,
			maxZoom: 7
		});

		currentLayer = defaultLayer.layer;
		currentLayer.addTo(map);

		// Fix icon paths for bundled leaflet
		delete (L.Icon.Default.prototype as any)._getIconUrl;
		L.Icon.Default.mergeOptions({
			iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
			iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
			shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png'
		});

		marker = L.marker([0, 0]).addTo(map);

		const ro = new ResizeObserver(() => {
			map?.invalidateSize();
		});
		ro.observe(mapContainer);

		return () => {
			ro.disconnect();
			map?.remove();
		};
	});

	$effect(() => {
		const lat = telemetry.get('v.lat') as number | undefined;
		let lng = telemetry.get('v.long') as number | undefined;
		const name = telemetry.get('v.name') as string | undefined;
		const alt = telemetry.get('v.altitude') as number | undefined;
		const spd = telemetry.get('v.surfaceSpeed') as number | undefined;

		if (lat == null || lng == null || !marker) return;
		if (lng > 180) lng -= 360;

		marker.setLatLng([lat, lng]);
		marker.bindPopup(
			`<b>${name ?? 'Vessel'}</b><br>` +
			`Alt: ${formatValue(alt, 'distance')}<br>` +
			`Spd: ${formatValue(spd, 'velocity')}`
		);
	});

	$effect(() => {
		const body = telemetry.get('v.body') as string | undefined;
		if (body && body.toLowerCase() !== selectedBody) {
			selectedBody = body.toLowerCase();
			switchLayer();
		}
	});
</script>

<div class="flex flex-col h-full">
	<div class="flex items-center gap-2 px-3 py-1.5 border-b border-border bg-surface shrink-0">
		<select
			class="bg-surface-bright text-text text-xs border border-border rounded px-2 py-1"
			bind:value={selectedPack}
			onchange={() => switchLayer()}
		>
			<option value="Stock">Stock</option>
			<option value="JNSQ">JNSQ</option>
		</select>
		<select
			class="bg-surface-bright text-text text-xs border border-border rounded px-2 py-1"
			bind:value={selectedBody}
			onchange={() => switchLayer()}
		>
			{#each bodies() as body}
				<option value={body}>{body.charAt(0).toUpperCase() + body.slice(1)}</option>
			{/each}
		</select>
		<select
			class="bg-surface-bright text-text text-xs border border-border rounded px-2 py-1"
			bind:value={selectedStyle}
			onchange={() => switchLayer()}
		>
			<option value="sat">Satellite</option>
			<option value="biome">Biome</option>
			<option value="slope">Slope</option>
		</select>
	</div>
	<div bind:this={mapContainer} class="flex-1"></div>
</div>
