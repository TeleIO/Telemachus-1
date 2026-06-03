<script lang="ts">
  // Renders the notification queue using the legacy `.sNotify_message` /
  // `.sNotify_close` classes (from the verbatim jKSPWAPICore.css linked by each
  // page), so toasts look identical to the original sNotify.
  import { toasts, dismiss } from "../lib/notify.ts";
</script>

<div class="sNotify_container">
  {#each $toasts as t (t.id)}
    <div class="sNotify_message">
      <span
        class="sNotify_close"
        role="button"
        tabindex="0"
        onclick={() => dismiss(t.id)}
        onkeydown={(e) => (e.key === "Enter" || e.key === " ") && dismiss(t.id)}
      >x</span>
      {t.message}
    </div>
  {/each}
</div>

<style>
  .sNotify_container {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 9999;
    display: flex;
    flex-direction: column;
    gap: 8px;
    pointer-events: none;
  }
  .sNotify_container :global(.sNotify_message) {
    position: relative;
    pointer-events: auto;
  }
</style>
