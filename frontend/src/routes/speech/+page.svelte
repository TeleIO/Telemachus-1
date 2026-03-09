<script lang="ts">
	import { command } from '$lib/api/client.js';
	import { addToast } from '$lib/components/notifications/toasts.svelte.js';

	let listening = $state(false);
	let transcript = $state('');
	let recognition: any = null;

	const commands: Record<string, string> = {
		stage: 'f.stage',
		engage: 'f.throttleFull',
		disengage: 'f.throttleZero',
		abort: 'f.abort',
		one: 'f.ag1', two: 'f.ag2', three: 'f.ag3', four: 'f.ag4', five: 'f.ag5',
		six: 'f.ag6', seven: 'f.ag7', eight: 'f.ag8', nine: 'f.ag9', ten: 'f.ag10',
		'1': 'f.ag1', '2': 'f.ag2', '3': 'f.ag3', '4': 'f.ag4', '5': 'f.ag5',
		'6': 'f.ag6', '7': 'f.ag7', '8': 'f.ag8', '9': 'f.ag9', '10': 'f.ag10',
		lights: 'f.light',
		gear: 'f.gear',
		brakes: 'f.brake'
	};

	function startListening() {
		const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
		if (!SpeechRecognition) {
			addToast('Speech recognition not supported in this browser', 'error');
			return;
		}

		recognition = new SpeechRecognition();
		recognition.continuous = true;
		recognition.interimResults = true;
		recognition.lang = 'en-US';

		recognition.onresult = (event: any) => {
			let final = '';
			for (let i = event.resultIndex; i < event.results.length; i++) {
				const result = event.results[i];
				if (result.isFinal) {
					final = result[0].transcript.trim().toLowerCase();
					transcript = final;
					processCommand(final);
				}
			}
		};

		recognition.onerror = () => {
			listening = false;
		};

		recognition.onend = () => {
			if (listening) recognition.start();
		};

		recognition.start();
		listening = true;
	}

	function stopListening() {
		recognition?.stop();
		recognition = null;
		listening = false;
	}

	function processCommand(text: string) {
		const words = text.split(/\s+/);
		for (const word of words) {
			const api = commands[word];
			if (api) {
				command(api);
				addToast(`Executed: ${word}`, 'success');
				return;
			}
		}
		addToast(`Unrecognized: "${text}"`, 'warning');
	}
</script>

<div class="flex flex-col items-center justify-center h-full gap-6 p-6">
	<h1 class="text-sm font-bold text-accent-bright">Voice Commands</h1>

	<button
		class="w-24 h-24 rounded-full border-2 transition-all
			{listening
				? 'border-danger bg-danger/20 animate-pulse text-danger'
				: 'border-border bg-surface-bright text-text-dim hover:border-accent hover:text-accent'}"
		onclick={() => (listening ? stopListening() : startListening())}
	>
		<span class="text-2xl">{listening ? '||' : '\u{1F399}'}</span>
	</button>

	<p class="text-xs text-text-dim">{listening ? 'Listening...' : 'Click to start'}</p>

	{#if transcript}
		<div class="bg-surface border border-border rounded px-4 py-2 text-sm text-text max-w-md">
			"{transcript}"
		</div>
	{/if}

	<div class="text-xs text-text-dim max-w-sm text-center mt-4">
		<p class="font-bold mb-1">Available commands:</p>
		<p>stage, engage, disengage, abort, lights, gear, brakes, one&ndash;ten</p>
	</div>
</div>
