import type { TelemachusWS } from '../api/websocket.js';

export class SubscriptionManager {
	private refCounts = new Map<string, number>();
	private ws: TelemachusWS;

	constructor(ws: TelemachusWS) {
		this.ws = ws;
	}

	subscribe(apis: string[]): void {
		const toAdd: string[] = [];
		for (const api of apis) {
			const count = this.refCounts.get(api) ?? 0;
			this.refCounts.set(api, count + 1);
			if (count === 0) toAdd.push(api);
		}
		if (toAdd.length > 0) this.ws.subscribe(toAdd);
	}

	unsubscribe(apis: string[]): void {
		const toRemove: string[] = [];
		for (const api of apis) {
			const count = this.refCounts.get(api) ?? 0;
			if (count <= 1) {
				this.refCounts.delete(api);
				toRemove.push(api);
			} else {
				this.refCounts.set(api, count - 1);
			}
		}
		if (toRemove.length > 0) this.ws.unsubscribe(toRemove);
	}

	resubscribeAll(): void {
		const all = Array.from(this.refCounts.keys());
		if (all.length > 0) this.ws.subscribe(all);
	}
}
