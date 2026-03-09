<script lang="ts">
	import NavBar from './NavBar.svelte';
	import StatusBar from './StatusBar.svelte';
	import ToastContainer from '../notifications/ToastContainer.svelte';
	import type { Snippet } from 'svelte';

	interface Props {
		children: Snippet;
	}

	let { children }: Props = $props();
</script>

<div class="flex flex-col h-screen">
	<NavBar />
	<main class="flex-1 min-h-0 overflow-auto">
		{@render children()}
	</main>
	<StatusBar />
	<ToastContainer />
</div>
