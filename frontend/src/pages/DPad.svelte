<script lang="ts">
  // Attitude + translation D-Pad. The duplicated touchstart/mousedown/touchend
  // handlers collapse into unified pointer events; the big down()/up() switch
  // becomes a per-button {axis, sign} descriptor. Wire protocol unchanged:
  //   press   -> v.setFbW[1] + v.setPitchYawRollXYZ[p,y,r,x,y,z]
  //   release -> (when all zero) v.setFbW[0] + zeros
  import Toast from "../components/Toast.svelte";
  import { command } from "../lib/command.ts";

  type Axis = "pitch" | "yaw" | "roll" | "x" | "y" | "z";
  interface Pad {
    id: string;
    glyph: string;
    axis: Axis;
    sign: -1 | 1;
  }

  // Order + glyphs preserved from d-pad.html.
  const pads: Pad[] = [
    { id: "rollleft", glyph: "↺", axis: "roll", sign: -1 },
    { id: "pitchup", glyph: "↑", axis: "pitch", sign: -1 },
    { id: "rollright", glyph: "↻", axis: "roll", sign: 1 },
    { id: "yawleft", glyph: "←", axis: "yaw", sign: -1 },
    { id: "pitchdown", glyph: "↓", axis: "pitch", sign: 1 },
    { id: "yawright", glyph: "→", axis: "yaw", sign: 1 },
    { id: "forwards", glyph: "⇟", axis: "y", sign: 1 },
    { id: "up", glyph: "↑", axis: "z", sign: 1 },
    { id: "right", glyph: "→", axis: "x", sign: 1 },
    { id: "backwards", glyph: "⇡", axis: "y", sign: -1 },
    { id: "down", glyph: "↓", axis: "z", sign: -1 },
    { id: "left", glyph: "←", axis: "x", sign: -1 },
  ];

  let power = $state(10);
  const v = { pitch: 0, yaw: 0, roll: 0, x: 0, y: 0, z: 0 };

  function pyr() {
    return `v.setPitchYawRollXYZ[${v.pitch},${v.yaw},${v.roll},${v.x},${v.y},${v.z}]`;
  }
  function allZero() {
    return v.pitch === 0 && v.yaw === 0 && v.roll === 0 && v.x === 0 && v.y === 0 && v.z === 0;
  }

  function press(p: Pad) {
    v[p.axis] = p.sign * (power / 10);
    command(["v.setFbW[1]", pyr()]);
  }
  function release(p: Pad) {
    v[p.axis] = 0;
    if (allZero()) command(["v.setFbW[0]", "v.setPitchYawRollXYZ[0,0,0,0,0,0]"]);
    else command(["v.setFbW[1]", pyr()]);
  }
</script>

<div class="dpad">
  {#each pads as p (p.id)}
    <div class="button">
      <div
        class="inner"
        role="button"
        tabindex="0"
        onpointerdown={(e) => {
          (e.target as HTMLElement).setPointerCapture(e.pointerId);
          press(p);
        }}
        onpointerup={() => release(p)}
        onpointercancel={() => release(p)}
        onpointerleave={() => release(p)}
      >
        <p>{p.glyph}</p>
      </div>
    </div>
  {/each}
</div>
<div class="power-box">
  <p class="power-value">Power: {(power / 10).toFixed(1)}</p>
  <input type="range" min="0" max="10" bind:value={power} />
</div>
<Toast />

<style>
  .dpad {
    display: flex;
    flex-wrap: wrap;
  }
  .button {
    position: relative;
    float: left;
    width: 32.3%;
    height: 19vh;
    margin: 0.5%;
  }
  .inner {
    width: 99%;
    height: 99%;
    margin: auto;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #e0e0e0;
    border: 2px solid;
    border-left-color: darkgray;
    border-top-color: darkgray;
    border-right-color: gray;
    border-bottom-color: gray;
    cursor: pointer;
    user-select: none;
    -webkit-tap-highlight-color: transparent;
  }
  .inner:hover {
    background-color: #f0f0f0;
  }
  .inner:active {
    background-color: grey;
  }
  .inner p {
    margin: 0;
    text-align: center;
    font-size: 40pt;
  }
  .power-box {
    clear: both;
    width: 96%;
    margin: 0 2%;
    text-align: center;
  }
  .power-value {
    padding: 4px;
  }
  .power-box input {
    width: 100%;
  }
</style>
