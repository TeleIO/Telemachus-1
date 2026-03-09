import type { APIEntry, DatalinkResponse } from './types.js';

function datalinkUrl(): string {
	return `/telemachus/datalink`;
}

export async function datalink(params: Record<string, string>): Promise<DatalinkResponse> {
	const query = new URLSearchParams(params).toString();
	const resp = await fetch(`${datalinkUrl()}?${query}`);
	if (!resp.ok) throw new Error(`Datalink error: ${resp.status}`);
	return resp.json();
}

export async function command(apiString: string): Promise<unknown> {
	const data = await datalink({ ret: apiString });
	return data.ret;
}

export async function getAPI(): Promise<APIEntry[]> {
	const data = await datalink({ api: 'a.api' });
	return data.api as APIEntry[];
}

export async function getVersion(): Promise<string> {
	const data = await datalink({ version: 'a.version' });
	return data.version as string;
}
