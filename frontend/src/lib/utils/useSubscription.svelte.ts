import { connection } from '../stores/connection.svelte.js';

export function useSubscription(getApis: () => string[]): void {
	$effect(() => {
		const apis = getApis();
		if (apis.length === 0) return;
		connection.subscriptions.subscribe(apis);
		return () => {
			connection.subscriptions.unsubscribe(apis);
		};
	});
}
