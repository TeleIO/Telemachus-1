class TelemetryStore {
	data: Record<string, unknown> = $state({});
	paused: number = $state(0);
	missionTime: number = $state(0);
	universalTime: number = $state(0);
	lastUpdate: number = $state(0);

	update(incoming: Record<string, unknown>): void {
		this.lastUpdate = Date.now();
		for (const [key, value] of Object.entries(incoming)) {
			if (key === 'unknown' || key === 'errors') continue;
			this.data[key] = value;
		}
		if ('p.paused' in incoming) this.paused = incoming['p.paused'] as number;
		if ('v.missionTime' in incoming) this.missionTime = incoming['v.missionTime'] as number;
		if ('t.universalTime' in incoming) this.universalTime = incoming['t.universalTime'] as number;
	}

	get(api: string): unknown {
		return this.data[api];
	}
}

export const telemetry = new TelemetryStore();
