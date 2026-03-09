<script lang="ts">
	import { page } from '$app/state';
	import { base } from '$app/paths';

	const links = [
		{ href: '/', label: 'Home' },
		{ href: '/console', label: 'Console' },
		{ href: '/map', label: 'Map' },
		{ href: '/flight-control', label: 'Flight' },
		{ href: '/d-pad', label: 'D-Pad' },
		{ href: '/touchball', label: 'Touchball' },
		{ href: '/smart-ass', label: 'SmartASS' },
		{ href: '/speech', label: 'Speech' },
		{ href: '/info', label: 'Info' }
	];
</script>

<nav class="flex items-center gap-1 px-4 py-2 bg-surface border-b border-border overflow-x-auto">
	<a href="{base}/" class="mr-4 font-bold text-accent-bright tracking-wider text-sm">TELEMACHUS</a>

	{#each links as link}
		<a
			href="{base}{link.href}"
			class="px-3 py-1 text-xs rounded transition-colors
				{page.url.pathname === base + link.href
					? 'bg-accent text-white'
					: 'text-text-dim hover:text-text hover:bg-surface-bright'}"
		>
			{link.label}
		</a>
	{/each}
</nav>
