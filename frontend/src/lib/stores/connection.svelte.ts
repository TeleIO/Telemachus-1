import type { ConnectionState } from '../api/types.js';
import { TelemachusWS } from '../api/websocket.js';
import { SubscriptionManager } from './subscriptions.svelte.js';
import { telemetry } from './telemetry.svelte.js';

class ConnectionStore {
	state: ConnectionState = $state('closed');
	ws: TelemachusWS;
	subscriptions: SubscriptionManager;

	constructor() {
		this.ws = new TelemachusWS();
		this.subscriptions = new SubscriptionManager(this.ws);

		this.ws.onData = (data) => telemetry.update(data);
		this.ws.onStateChange = (s) => {
			this.state = s;
			if (s === 'open') this.subscriptions.resubscribeAll();
		};
	}

	connect(): void {
		this.ws.connect();
	}

	disconnect(): void {
		this.ws.disconnect();
	}
}

export const connection = new ConnectionStore();
