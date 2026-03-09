<script lang="ts">
	import { command } from '$lib/api/client.js';
	import { addToast } from '$lib/components/notifications/toasts.svelte.js';

	let heading = $state(90);
	let pitch = $state(0);
	let roll = $state(0);

	const modes: { label: string; api: string }[] = [
		{ label: 'Off', api: 'mj.smartassoff' },
		{ label: 'Node', api: 'mj.node' },
		{ label: 'Prograde', api: 'mj.prograde' },
		{ label: 'Retrograde', api: 'mj.retrograde' },
		{ label: 'Normal+', api: 'mj.normalplus' },
		{ label: 'Normal-', api: 'mj.normalminus' },
		{ label: 'Radial+', api: 'mj.radialplus' },
		{ label: 'Radial-', api: 'mj.radialminus' },
		{ label: 'Target+', api: 'mj.targetplus' },
		{ label: 'Target-', api: 'mj.targetminus' },
		{ label: 'Relative+', api: 'mj.relativeplus' },
		{ label: 'Relative-', api: 'mj.relativeminus' },
		{ label: 'Parallel+', api: 'mj.parallelplus' },
		{ label: 'Parallel-', api: 'mj.parallelminus' }
	];

	async function setMode(api: string) {
		const ret = await command(api);
		if (ret === 5) addToast('MechJeb2 not found', 'error');
	}

	async function executeSurface() {
		const ret = await command(`mj.surface2[${heading},${pitch},${roll}]`);
		if (ret === 5) addToast('MechJeb2 not found', 'error');
	}
</script>

<div class="flex flex-col items-center gap-6 p-6 max-w-xl mx-auto">
	<h1 class="text-sm font-bold text-accent-bright">SmartASS (MechJeb2)</h1>

	<div class="w-full">
		<div class="text-xs text-text-dim mb-2">Attitude Presets</div>
		<div class="flex gap-2 flex-wrap">
			{#each modes as mode}
				<button
					class="px-3 py-2 text-xs font-bold rounded border border-border bg-surface-bright
						hover:border-accent hover:text-accent-bright active:bg-accent/20 transition-colors"
					onclick={() => setMode(mode.api)}
				>
					{mode.label}
				</button>
			{/each}
		</div>
	</div>

	<div class="w-full border-t border-border pt-4">
		<div class="text-xs text-text-dim mb-3">Custom Surface Attitude</div>

		<div class="flex flex-col gap-3">
			<label class="flex items-center gap-3">
				<span class="text-xs text-text-dim w-16">Heading</span>
				<input type="range" min="0" max="360" step="1" bind:value={heading} class="flex-1 accent-accent" />
				<span class="text-xs text-text tabular-nums w-12">{heading}&deg;</span>
			</label>
			<label class="flex items-center gap-3">
				<span class="text-xs text-text-dim w-16">Pitch</span>
				<input type="range" min="-90" max="90" step="1" bind:value={pitch} class="flex-1 accent-accent" />
				<span class="text-xs text-text tabular-nums w-12">{pitch}&deg;</span>
			</label>
			<label class="flex items-center gap-3">
				<span class="text-xs text-text-dim w-16">Roll</span>
				<input type="range" min="0" max="360" step="1" bind:value={roll} class="flex-1 accent-accent" />
				<span class="text-xs text-text tabular-nums w-12">{roll}&deg;</span>
			</label>
		</div>

		<button
			class="mt-4 px-6 py-2 text-xs font-bold rounded bg-accent text-white hover:bg-accent-bright transition-colors"
			onclick={executeSurface}
		>
			Execute
		</button>
	</div>
</div>
