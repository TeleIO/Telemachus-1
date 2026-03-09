<script lang="ts">
	import ActionButton from '$lib/components/controls/ActionButton.svelte';
	import ToggleButton from '$lib/components/controls/ToggleButton.svelte';
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { command } from '$lib/api/client.js';

	useSubscription(() => ['f.throttle', 'f.sasMode', 'f.sasEnabled']);

	let throttle = $derived(
		typeof telemetry.get('f.throttle') === 'number'
			? Math.round((telemetry.get('f.throttle') as number) * 100)
			: 0
	);

	const actionGroups = Array.from({ length: 10 }, (_, i) => ({
		label: `AG${i + 1}`,
		toggleApi: `f.ag${i + 1}`,
		valueApi: `v.ag${i + 1}Value`
	}));

	const sasModes = [
		'StabilityAssist', 'Prograde', 'Retrograde', 'Normal', 'Antinormal',
		'RadialIn', 'RadialOut', 'Target', 'AntiTarget', 'Maneuver'
	];

	let sasMode = $derived(telemetry.get('f.sasMode') as string ?? '');
</script>

<div class="flex flex-col items-center gap-6 p-6 max-w-2xl mx-auto">
	<h1 class="text-sm font-bold text-accent-bright">Flight Control</h1>

	<!-- Throttle -->
	<div class="w-full">
		<div class="text-xs text-text-dim mb-2">Throttle: {throttle}%</div>
		<div class="flex gap-2 flex-wrap">
			<ActionButton label="0%" apiString="f.throttleZero" />
			<ActionButton label="-" apiString="f.throttleDown" />
			<ActionButton label="+" apiString="f.throttleUp" />
			<ActionButton label="100%" apiString="f.throttleFull" />
		</div>
	</div>

	<!-- Staging -->
	<div class="w-full">
		<div class="text-xs text-text-dim mb-2">Staging</div>
		<ActionButton label="STAGE" apiString="f.stage" class="!bg-danger/20 !border-danger !text-danger hover:!bg-danger/30" />
	</div>

	<!-- System Toggles -->
	<div class="w-full">
		<div class="text-xs text-text-dim mb-2">Systems</div>
		<div class="flex gap-2 flex-wrap">
			<ToggleButton label="RCS" toggleApi="f.rcs" valueApi="v.rcsValue" />
			<ToggleButton label="SAS" toggleApi="f.sas" valueApi="v.sasValue" />
			<ToggleButton label="Light" toggleApi="f.light" valueApi="v.lightValue" />
			<ToggleButton label="Gear" toggleApi="f.gear" valueApi="v.gearValue" />
			<ToggleButton label="Brake" toggleApi="f.brake" valueApi="v.brakeValue" />
			<ActionButton label="ABORT" apiString="f.abort" class="!border-danger !text-danger" />
		</div>
	</div>

	<!-- SAS Mode -->
	<div class="w-full">
		<div class="text-xs text-text-dim mb-2">SAS Mode: <span class="text-accent-bright">{sasMode}</span></div>
		<div class="flex gap-2 flex-wrap">
			{#each sasModes as mode}
				<button
					class="px-2 py-1 text-xs rounded border transition-colors
						{sasMode === mode
							? 'border-accent text-accent-bright bg-accent/10'
							: 'border-border text-text-dim hover:border-accent hover:text-text'}"
					onclick={() => command(`f.setSASMode[${mode}]`)}
				>
					{mode}
				</button>
			{/each}
		</div>
	</div>

	<!-- Action Groups -->
	<div class="w-full">
		<div class="text-xs text-text-dim mb-2">Action Groups</div>
		<div class="flex gap-2 flex-wrap">
			{#each actionGroups as ag}
				<ToggleButton label={ag.label} toggleApi={ag.toggleApi} valueApi={ag.valueApi} />
			{/each}
		</div>
	</div>
</div>
