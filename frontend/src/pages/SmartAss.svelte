<script lang="ts">
  // MechJeb Smart A.S.S. The repeated direction buttons become a data list;
  // the three sliders + Execute build the mj.surface2[heading,pitch,roll] call.
  import ControlButton from "../components/ControlButton.svelte";
  import Toast from "../components/Toast.svelte";
  import { command } from "../lib/command.ts";

  const directions: { label: string; cmd: string }[] = [
    { label: "Off", cmd: "mj.smartassoff" },
    { label: "Node", cmd: "mj.node" },
    { label: "Retrograde", cmd: "mj.retrograde" },
    { label: "Prograde", cmd: "mj.prograde" },
    { label: "Normal +", cmd: "mj.normalplus" },
    { label: "Normal -", cmd: "mj.normalminus" },
    { label: "Radial +", cmd: "mj.radialplus" },
    { label: "Radial -", cmd: "mj.radialminus" },
    { label: "Target +", cmd: "mj.targetplus" },
    { label: "Target -", cmd: "mj.targetminus" },
    { label: "Relative +", cmd: "mj.relativeplus" },
    { label: "Relative -", cmd: "mj.relativeminus" },
    { label: "Parallel +", cmd: "mj.parallelplus" },
    { label: "Parallel -", cmd: "mj.parallelminus" },
  ];

  let heading = $state(90);
  let pitch = $state(90);
  let roll = $state(90);

  function execute() {
    // Argument order preserved from the legacy page: heading, pitch, roll.
    command(`mj.surface2[${heading},${pitch},${roll}]`);
  }
</script>

<div class="ctl-grid">
  {#each directions as d (d.cmd)}
    <ControlButton label={d.label} commands={d.cmd} />
  {/each}
</div>

<div class="sliders">
  <label>Pitch <input type="range" min="-90" max="90" bind:value={pitch} /> <span>{pitch}</span></label>
  <label>Heading <input type="range" min="0" max="360" bind:value={heading} /> <span>{heading}</span></label>
  <label>Roll <input type="range" min="0" max="360" bind:value={roll} /> <span>{roll}</span></label>
  <button type="button" class="execute" onclick={execute}>Execute</button>
</div>
<Toast />

<style>
  .ctl-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    padding: 10px;
  }
  .sliders {
    max-width: 480px;
    margin: 10px auto;
    padding: 0 16px;
  }
  .sliders label {
    display: grid;
    grid-template-columns: 5em 1fr 3em;
    align-items: center;
    gap: 8px;
    margin: 10px 0;
  }
  .sliders input[type="range"] {
    width: 100%;
  }
  .execute {
    display: block;
    width: 100%;
    margin-top: 8px;
    padding: 12px;
    font: inherit;
    color: #fff;
    background: linear-gradient(#3a6ea5, #2d5680);
    border: 1px solid #234468;
    border-radius: 6px;
    cursor: pointer;
  }
</style>
