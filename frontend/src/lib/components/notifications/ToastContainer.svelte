<script lang="ts">
	import Toast from './Toast.svelte';
	import { toasts, removeToast } from './toasts.svelte.js';
</script>

<div class="fixed top-4 right-4 z-50 flex flex-col gap-2 w-80">
	{#each toasts.items as toast (toast.id)}
		<Toast message={toast.message} type={toast.type} onclose={() => removeToast(toast.id)} />
	{/each}
</div>
