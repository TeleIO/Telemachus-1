<script lang="ts">
	import { command } from '$lib/api/client.js';

	let ball: HTMLDivElement;
	let tracking = $state(false);
	let pitch = $state(0);
	let yaw = $state(0);
	let roll = $state(0);
	let interval: ReturnType<typeof setInterval> | null = null;

	function ease(v: number): number {
		return Math.pow(v, 3);
	}

	function onPointerDown(e: PointerEvent) {
		tracking = true;
		ball.setPointerCapture(e.pointerId);
		updateFromEvent(e);
		interval = setInterval(sendState, 100);
	}

	function onPointerMove(e: PointerEvent) {
		if (!tracking) return;
		updateFromEvent(e);
	}

	function onPointerUp() {
		tracking = false;
		pitch = 0;
		yaw = 0;
		roll = 0;
		if (interval) { clearInterval(interval); interval = null; }
		command('v.setFbW[0]');
	}

	function updateFromEvent(e: PointerEvent) {
		const rect = ball.getBoundingClientRect();
		const cx = rect.left + rect.width / 2;
		const cy = rect.top + rect.height / 2;
		const radius = rect.width / 2;

		let dx = (e.clientX - cx) / radius;
		let dy = (e.clientY - cy) / radius;

		dx = Math.max(-1, Math.min(1, dx));
		dy = Math.max(-1, Math.min(1, dy));

		if (e.buttons === 2 || e.pointerType === 'touch') {
			pitch = ease(-dy);
			roll = ease(dx);
			yaw = 0;
		} else {
			pitch = ease(-dy);
			yaw = ease(dx);
			roll = 0;
		}
	}

	async function sendState() {
		if (!tracking) return;
		await command('v.setFbW[1]');
		await command(`v.setPitchYawRollXYZ[${pitch},${yaw},${roll},0,0,0]`);
	}
</script>

<div class="flex flex-col items-center justify-center h-full gap-6 p-6">
	<h1 class="text-sm font-bold text-accent-bright">Touchball Controller</h1>
	<p class="text-xs text-text-dim">Click/touch and drag. Left = pitch/yaw. Right = pitch/roll.</p>

	<!-- svelte-ignore a11y_no_static_element_interactions -->
	<div
		bind:this={ball}
		class="w-64 h-64 rounded-full border-2 border-border bg-surface-bright cursor-crosshair
			relative select-none touch-none
			{tracking ? 'border-accent shadow-lg shadow-accent/20' : ''}"
		onpointerdown={onPointerDown}
		onpointermove={onPointerMove}
		onpointerup={onPointerUp}
		onpointercancel={onPointerUp}
		oncontextmenu={(e) => e.preventDefault()}
	>
		<!-- Crosshair -->
		<div class="absolute inset-0 flex items-center justify-center pointer-events-none">
			<div class="w-px h-full bg-border/30 absolute"></div>
			<div class="h-px w-full bg-border/30 absolute"></div>
		</div>
		<!-- Deflection indicator -->
		{#if tracking}
			<div
				class="absolute w-3 h-3 rounded-full bg-accent"
				style="left: calc(50% + {yaw || roll} * 45% - 6px); top: calc(50% + {-pitch} * 45% - 6px)"
			></div>
		{/if}
	</div>

	<div class="text-xs text-text-dim tabular-nums">
		P: {pitch.toFixed(2)} | Y: {yaw.toFixed(2)} | R: {roll.toFixed(2)}
	</div>
</div>
