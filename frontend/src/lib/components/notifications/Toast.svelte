<script lang="ts">
	interface Props {
		message: string;
		type?: 'info' | 'warning' | 'error' | 'success';
		onclose: () => void;
	}

	let { message, type = 'info', onclose }: Props = $props();

	const colors = {
		info: 'border-accent',
		warning: 'border-warning',
		error: 'border-danger',
		success: 'border-success'
	};

	let timeout = $state<ReturnType<typeof setTimeout>>();
	let hovered = $state(false);

	$effect(() => {
		if (!hovered) {
			timeout = setTimeout(onclose, 4000);
			return () => clearTimeout(timeout);
		}
	});
</script>

<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
	class="bg-surface border-l-2 {colors[type]} px-4 py-2 text-sm shadow-lg"
	onmouseenter={() => (hovered = true)}
	onmouseleave={() => (hovered = false)}
>
	<div class="flex items-center justify-between gap-4">
		<span class="text-text">{message}</span>
		<button class="text-text-dim hover:text-text shrink-0" onclick={onclose}>&times;</button>
	</div>
</div>
