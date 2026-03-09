<script lang="ts">
	import { base } from '$app/paths';

	const pages = [
		{ href: '/console', label: 'Console', desc: 'Real-time telemetry dashboard with charts' },
		{ href: '/map', label: 'Map', desc: 'Interactive map with vessel tracking' },
		{ href: '/flight-control', label: 'Flight Control', desc: 'Throttle, staging, and system toggles' },
		{ href: '/d-pad', label: 'D-Pad', desc: '6-DOF attitude and translation control' },
		{ href: '/touchball', label: 'Touchball', desc: 'Analog pitch/yaw/roll controller' },
		{ href: '/smart-ass', label: 'SmartASS', desc: 'MechJeb2 attitude control' },
		{ href: '/speech', label: 'Speech', desc: 'Voice command interface' },
		{ href: '/info', label: 'Info', desc: 'API browser and version info' }
	];

	const external = [
		{ href: '/telemachus/houston/index.html', label: 'Houston', desc: 'Mission control interface' },
		{ href: '/telemachus/mkon/index.html', label: 'MKON', desc: 'Modular dashboard' }
	];
</script>

<div class="flex flex-col items-center justify-center min-h-full p-8 gap-8">
	<img src="{base}/logo.png" alt="Telemachus" class="w-48 opacity-80" />

	<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 w-full max-w-4xl">
		{#each pages as p}
			<a
				href="{base}{p.href}"
				class="group border border-border rounded p-4 hover:border-accent transition-colors bg-surface"
			>
				<div class="text-accent-bright font-bold text-sm group-hover:text-white">{p.label}</div>
				<div class="text-text-dim text-xs mt-1">{p.desc}</div>
			</a>
		{/each}
	</div>

	<div class="flex gap-3">
		{#each external as e}
			<a
				href={e.href}
				class="border border-border rounded px-4 py-2 text-xs text-text-dim hover:text-text hover:border-accent transition-colors"
				target="_blank"
			>
				{e.label} &nearr;
			</a>
		{/each}
	</div>
</div>
