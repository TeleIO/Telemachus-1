<script lang="ts">
  // One reusable command button with press feedback. Replaces the per-page
  // command()/toggle() handlers + jQuery-Mobile buttonMarkup spinner across
  // flight-control, smart-ass and speech. (jQuery Mobile's CSS came from a dead
  // CDN, so the styling here is a clean, bundled equivalent.)
  import { command } from "../lib/command.ts";

  let {
    label,
    commands,
  }: { label: string; commands: string | string[] } = $props();

  let pending = $state(false);

  async function fire() {
    if (pending) return;
    pending = true;
    try {
      await command(commands);
    } finally {
      setTimeout(() => (pending = false), 300);
    }
  }
</script>

<button type="button" class="ctl-btn" class:pending onclick={fire}>
  <span class="ctl-icon" aria-hidden="true">{pending ? "⟳" : ""}</span>
  <span class="ctl-label">{label}</span>
</button>

<style>
  .ctl-btn {
    display: inline-flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 2px;
    min-width: 5.5em;
    min-height: 3.2em;
    margin: 3px;
    padding: 8px 12px;
    font: inherit;
    color: #fff;
    background: linear-gradient(#3a3f44, #2b2f33);
    border: 1px solid #1c1f22;
    border-radius: 6px;
    cursor: pointer;
    user-select: none;
    -webkit-tap-highlight-color: transparent;
  }
  .ctl-btn:active {
    background: linear-gradient(#2b2f33, #3a3f44);
  }
  .ctl-btn.pending {
    opacity: 0.6;
  }
  .ctl-icon {
    height: 1em;
    line-height: 1;
    animation: ctl-spin 0.8s linear infinite;
  }
  .ctl-btn:not(.pending) .ctl-icon {
    animation: none;
  }
  .ctl-label {
    font-size: 0.9em;
  }
  @keyframes ctl-spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
