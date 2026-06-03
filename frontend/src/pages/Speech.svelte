<script lang="ts">
  // Voice control. The legacy page used the long-removed `x-webkit-speech`
  // input; this uses the Web Speech API. Same command vocabulary: "stage",
  // "engage" (full throttle), and action groups "1".."10".
  import Toast from "../components/Toast.svelte";
  import { command } from "../lib/command.ts";
  import { notify } from "../lib/notify.ts";

  const vocab: Record<string, string> = {
    stage: "f.stage",
    engage: "f.throttleFull",
  };
  for (let i = 1; i <= 10; i++) vocab[String(i)] = `f.ag${i}`;

  let listening = $state(false);
  let transcript = $state("");

  // deno-lint-ignore no-explicit-any
  const Recognition = (globalThis as any).SpeechRecognition ??
    (globalThis as any).webkitSpeechRecognition;
  const supported = !!Recognition;
  // deno-lint-ignore no-explicit-any
  let recognition: any = null;

  function handle(text: string) {
    transcript = text;
    for (const word of text.toLowerCase().split(/\s+/)) {
      const cmd = vocab[word];
      if (cmd) {
        command(cmd);
        notify(`Command: ${word}`);
      }
    }
  }

  function toggle() {
    if (!supported) return;
    if (listening) {
      recognition?.stop();
      return;
    }
    recognition = new Recognition();
    recognition.continuous = true;
    recognition.interimResults = true;
    recognition.onstart = () => (listening = true);
    recognition.onend = () => (listening = false);
    // deno-lint-ignore no-explicit-any
    recognition.onresult = (e: any) => {
      let text = "";
      for (let i = e.resultIndex; i < e.results.length; i++) {
        text += e.results[i][0].transcript;
      }
      handle(text);
    };
    recognition.start();
  }
</script>

<div class="speech">
  <h1>Telemachus Speech</h1>
  <p class="hint">Click the microphone and speak one of the commands in bold</p>
  <p class="hint"><b>stage</b> the craft, action groups <b>1-10</b> and <b>engage</b> full throttle</p>

  {#if supported}
    <button type="button" class="mic" class:listening onclick={toggle}>
      {listening ? "● Listening…" : "🎤 Start"}
    </button>
    <div class="transcript">{transcript}</div>
  {:else}
    <p class="hint">Speech recognition isn't supported in this browser. You can still type a command:</p>
    <input
      class="fallback"
      placeholder="stage, engage, 1–10"
      oninput={(e) => handle((e.target as HTMLInputElement).value)}
    />
  {/if}
</div>
<Toast />

<style>
  .speech {
    text-align: center;
    font-family: "Open Sans", arial, sans-serif;
    padding-top: 8vh;
  }
  h1 {
    font-weight: normal;
  }
  .hint {
    color: gray;
    font-size: 14px;
  }
  .mic {
    margin-top: 16px;
    padding: 12px 28px;
    font: inherit;
    color: #fff;
    background: linear-gradient(#3a6ea5, #2d5680);
    border: 1px solid #234468;
    border-radius: 6px;
    cursor: pointer;
  }
  .mic.listening {
    background: linear-gradient(#a53a3a, #802d2d);
  }
  .transcript {
    margin: 16px auto 0;
    max-width: 600px;
    min-height: 1.5em;
    color: #333;
  }
  .fallback {
    margin-top: 12px;
    max-width: 600px;
    width: 60%;
    padding: 8px;
  }
</style>
