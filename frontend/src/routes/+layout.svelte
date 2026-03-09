<script lang="ts">
	import '../app.css';
	import AppShell from '$lib/components/layout/AppShell.svelte';
	import { connection } from '$lib/stores/connection.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { PAUSE_MESSAGES } from '$lib/api/constants.js';
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { addToast } from '$lib/components/notifications/toasts.svelte.js';
	import type { Snippet } from 'svelte';

	interface Props {
		children: Snippet;
	}

	let { children }: Props = $props();

	let prevPaused = $state(-1);

	$effect(() => {
		connection.connect();
		return () => connection.disconnect();
	});

	useSubscription(() => ['p.paused', 'v.missionTime', 't.universalTime']);

	$effect(() => {
		const p = telemetry.paused;
		if (prevPaused !== -1 && p !== prevPaused && p in PAUSE_MESSAGES) {
			addToast(PAUSE_MESSAGES[p], p === 0 ? 'success' : 'warning');
		}
		prevPaused = p;
	});
</script>

<AppShell>
	{@render children()}
</AppShell>
