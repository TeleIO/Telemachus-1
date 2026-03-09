<script lang="ts">
	import { command } from '$lib/api/client.js';

	let power = $state(0.5);
	let active: Record<string, number> = $state({});

	const axes = { pitch: 0, yaw: 0, roll: 0, x: 0, y: 0, z: 0 };
	type Axis = keyof typeof axes;

	const buttons: { label: string; axis: Axis; dir: 1 | -1 }[] = [
		{ label: 'Roll L', axis: 'roll', dir: -1 },
		{ label: 'Pitch +', axis: 'pitch', dir: 1 },
		{ label: 'Roll R', axis: 'roll', dir: 1 },
		{ label: 'Yaw L', axis: 'yaw', dir: -1 },
		{ label: 'Pitch -', axis: 'pitch', dir: -1 },
		{ label: 'Yaw R', axis: 'yaw', dir: 1 },
		{ label: 'Fwd', axis: 'z', dir: 1 },
		{ label: 'Up', axis: 'y', dir: 1 },
		{ label: 'Right', axis: 'x', dir: 1 },
		{ label: 'Back', axis: 'z', dir: -1 },
		{ label: 'Down', axis: 'y', dir: -1 },
		{ label: 'Left', axis: 'x', dir: -1 }
	];

	async function sendState() {
		const p = (active['pitch'] ?? 0) * power;
		const y = (active['yaw'] ?? 0) * power;
		const r = (active['roll'] ?? 0) * power;
		const tx = (active['x'] ?? 0) * power;
		const ty = (active['y'] ?? 0) * power;
		const tz = (active['z'] ?? 0) * power;

		const anyActive = Object.values(active).some((v) => v !== 0);
		if (anyActive) {
			await command('v.setFbW[1]');
			await command(`v.setPitchYawRollXYZ[${p},${y},${r},${tx},${ty},${tz}]`);
		} else {
			await command('v.setFbW[0]');
		}
	}

	function press(axis: Axis, dir: number) {
		active = { ...active, [axis]: dir };
		sendState();
	}

	function release(axis: Axis) {
		active = { ...active, [axis]: 0 };
		sendState();
	}
</script>

<div class="flex flex-col items-center justify-center h-full gap-6 p-6">
	<h1 class="text-sm font-bold text-accent-bright">D-Pad Controller</h1>

	<!-- Rotation -->
	<div class="text-xs text-text-dim">Rotation</div>
	<div class="grid grid-cols-3 gap-2 w-64">
		{#each buttons.slice(0, 6) as btn}
			<button
				class="px-2 py-4 text-xs font-bold rounded border border-border bg-surface-bright
					hover:border-accent active:bg-accent/20 select-none touch-none"
				onpointerdown={() => press(btn.axis, btn.dir)}
				onpointerup={() => release(btn.axis)}
				onpointerleave={() => release(btn.axis)}
			>
				{btn.label}
			</button>
		{/each}
	</div>

	<!-- Translation -->
	<div class="text-xs text-text-dim">Translation</div>
	<div class="grid grid-cols-3 gap-2 w-64">
		{#each buttons.slice(6) as btn}
			<button
				class="px-2 py-4 text-xs font-bold rounded border border-border bg-surface-bright
					hover:border-accent active:bg-accent/20 select-none touch-none"
				onpointerdown={() => press(btn.axis, btn.dir)}
				onpointerup={() => release(btn.axis)}
				onpointerleave={() => release(btn.axis)}
			>
				{btn.label}
			</button>
		{/each}
	</div>

	<!-- Power slider -->
	<div class="flex items-center gap-3 w-64">
		<span class="text-xs text-text-dim">Power</span>
		<input
			type="range"
			min="0"
			max="1"
			step="0.05"
			bind:value={power}
			class="flex-1 accent-accent"
		/>
		<span class="text-xs text-text tabular-nums w-8">{(power * 100).toFixed(0)}%</span>
	</div>
</div>
