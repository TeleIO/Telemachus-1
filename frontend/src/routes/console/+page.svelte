<script lang="ts">
	import DashboardGrid from '$lib/components/dashboard/DashboardGrid.svelte';
	import LayoutManager from '$lib/components/dashboard/LayoutManager.svelte';
	import WidgetConfigEditor from '$lib/components/dashboard/WidgetConfigEditor.svelte';
</script>

<div class="flex flex-col h-full">
	<LayoutManager />
	<div class="flex-1 min-h-0 overflow-auto">
		<DashboardGrid />
	</div>
	<WidgetConfigEditor />
</div>
