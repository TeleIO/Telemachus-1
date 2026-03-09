import type { ConnectionState, WsMessage } from './types.js';
import { DEFAULT_PORT } from './constants.js';

const RECONNECT_BASE = 1000;
const RECONNECT_MAX = 30000;

export class TelemachusWS {
	private ws: WebSocket | null = null;
	private url: string;
	private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
	private reconnectDelay = RECONNECT_BASE;
	private queue: WsMessage[] = [];
	private intentionalClose = false;

	onData: ((data: Record<string, unknown>) => void) | null = null;
	onBinary: ((data: ArrayBuffer) => void) | null = null;
	onStateChange: ((state: ConnectionState) => void) | null = null;

	constructor(host?: string, port?: number) {
		const h = host ?? (typeof location !== 'undefined' ? location.hostname : 'localhost');
		const p = port ?? DEFAULT_PORT;
		this.url = `ws://${h}:${p}/datalink`;
	}

	connect(): void {
		this.intentionalClose = false;
		this.onStateChange?.('connecting');

		const ws = new WebSocket(this.url);
		ws.binaryType = 'arraybuffer';

		ws.onopen = () => {
			this.reconnectDelay = RECONNECT_BASE;
			this.onStateChange?.('open');
			this.drainQueue();
		};

		ws.onclose = () => {
			this.ws = null;
			if (!this.intentionalClose) {
				this.onStateChange?.('closed');
				this.scheduleReconnect();
			}
		};

		ws.onerror = () => {
			this.onStateChange?.('error');
		};

		ws.onmessage = (event) => {
			if (event.data instanceof ArrayBuffer) {
				this.onBinary?.(event.data);
			} else {
				try {
					const data = JSON.parse(event.data);
					this.onData?.(data);
				} catch {
					// ignore malformed messages
				}
			}
		};

		this.ws = ws;
	}

	disconnect(): void {
		this.intentionalClose = true;
		if (this.reconnectTimer) {
			clearTimeout(this.reconnectTimer);
			this.reconnectTimer = null;
		}
		this.ws?.close();
		this.ws = null;
		this.onStateChange?.('closed');
	}

	subscribe(apis: string[]): void {
		if (apis.length > 0) this.send({ '+': apis });
	}

	unsubscribe(apis: string[]): void {
		if (apis.length > 0) this.send({ '-': apis });
	}

	run(apis: string[]): void {
		if (apis.length > 0) this.send({ run: apis });
	}

	setRate(ms: number): void {
		this.send({ rate: ms });
	}

	setBinary(apis: string[]): void {
		this.send({ binary: apis });
	}

	private send(msg: WsMessage): void {
		if (this.ws?.readyState === WebSocket.OPEN) {
			this.ws.send(JSON.stringify(msg));
		} else {
			this.queue.push(msg);
		}
	}

	private drainQueue(): void {
		while (this.queue.length > 0 && this.ws?.readyState === WebSocket.OPEN) {
			const msg = this.queue.shift()!;
			this.ws!.send(JSON.stringify(msg));
		}
	}

	private scheduleReconnect(): void {
		this.reconnectTimer = setTimeout(() => {
			this.reconnectTimer = null;
			this.connect();
		}, this.reconnectDelay);
		this.reconnectDelay = Math.min(this.reconnectDelay * 2, RECONNECT_MAX);
	}
}
