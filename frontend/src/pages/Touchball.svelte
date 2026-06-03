<script lang="ts">
  // Touch attitude ball. Faithful to touchball-pyr.html (same pitch/yaw/roll
  // math and FbW gating) but using unified pointer events and the shared
  // command() helper instead of jQuery + its own ad-hoc client.
  import Toast from "../components/Toast.svelte";
  import { command } from "../lib/command.ts";

  let ballEl: HTMLDivElement;
  let active = $state(false);
  let pitch = $state(0);
  let yaw = $state(0);
  let roll = $state(0);
  let tx = $state(0);
  let ty = $state(0);

  const clamp = (n: number) => (n < -1 ? -1 : n > 1 ? 1 : n);

  function move(e: PointerEvent, rollMode: boolean) {
    const rect = ballEl.getBoundingClientRect();
    const cx = e.clientX - rect.left;
    const cy = e.clientY - rect.top;
    tx = cx;
    ty = cy;
    const dh = -(2 * cy / rect.height - 1);
    const dw = 2 * cx / rect.width - 1;
    pitch = clamp(-3 * Math.pow(dh, 3));
    if (rollMode) {
      roll = clamp(5 * Math.pow(dw, 5));
      yaw = 0;
    } else {
      yaw = clamp(3 * Math.pow(dw, 3));
      roll = 0;
    }
    command(`v.setPitchYawRollXYZ[${pitch.toFixed(2)},${yaw.toFixed(2)},${roll.toFixed(2)},0,0,0]`);
  }

  function down(e: PointerEvent) {
    ballEl.setPointerCapture(e.pointerId);
    active = true;
    command("v.setFbW[1]");
    move(e, e.button === 2);
  }
  function up() {
    active = false;
    pitch = yaw = roll = 0;
    command(["v.setFbW[0]", "v.setPitchYawRollXYZ[0,0,0,0,0,0]"]);
  }
</script>

<div
  bind:this={ballEl}
  class="touchball"
  role="application"
  aria-label="Attitude control ball"
  onpointerdown={down}
  onpointermove={(e) => active && move(e, e.button === 2 || e.buttons === 2)}
  onpointerup={up}
  onpointercancel={up}
  oncontextmenu={(e) => e.preventDefault()}
>
  {#if active}<div class="target" style="left:{tx}px; top:{ty}px"></div>{/if}
  <div class="debug">
    Pitch = {pitch.toFixed(2)} · Yaw = {yaw.toFixed(2)} · Roll = {roll.toFixed(2)}
  </div>
</div>
<Toast />

<style>
  .touchball {
    position: fixed;
    inset: 0;
    margin: auto;
    width: 95vmin;
    height: 95vmin;
    background: rgb(240, 240, 240);
    border-radius: 100%;
    touch-action: none;
    overflow: hidden;
  }
  .touchball::before,
  .touchball::after {
    content: "";
    position: absolute;
    background: rgba(0, 0, 0, 0.1);
  }
  .touchball::before { left: 0; right: 0; top: 50%; height: 1px; }
  .touchball::after { top: 0; bottom: 0; left: 50%; width: 1px; }
  .target {
    position: absolute;
    width: 18%;
    height: 18%;
    transform: translate(-50%, -50%);
    background: rgba(255, 255, 0, 0.15);
    border-radius: 100%;
    pointer-events: none;
  }
  .debug {
    position: absolute;
    top: 8px;
    width: 100%;
    text-align: center;
    color: #555;
    pointer-events: none;
  }
</style>
