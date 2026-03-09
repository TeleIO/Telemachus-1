export interface APIEntry {
	name: string;
	apistring: string;
	units: string;
	plotable: boolean;
}

export interface DatalinkResponse {
	[key: string]: unknown;
	unknown?: string[];
	errors?: Record<string, string>;
}

export type ConnectionState = 'connecting' | 'open' | 'closed' | 'error';

export type WsMessage = {
	'+'?: string[];
	'-'?: string[];
	run?: string[];
	rate?: number;
	binary?: string[];
};
