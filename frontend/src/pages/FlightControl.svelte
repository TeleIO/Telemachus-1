<script lang="ts">
  // Basic flight control. The 21 copy-pasted <button> rows from
  // flight-control.html become one data list rendered through ControlButton.
  import ControlButton from "../components/ControlButton.svelte";
  import Toast from "../components/Toast.svelte";

  const actions: { label: string; cmd: string }[] = [
    { label: "Stage+", cmd: "f.stage" },
    { label: "Throttle+", cmd: "f.throttleUp" },
    { label: "Throttle-", cmd: "f.throttleDown" },
    { label: "0%", cmd: "f.throttleZero" },
    { label: "100%", cmd: "f.throttleFull" },
    { label: "RCS", cmd: "f.rcs" },
    { label: "SAS", cmd: "f.sas" },
    { label: "Light", cmd: "f.light" },
    { label: "Gear", cmd: "f.gear" },
    { label: "Brake", cmd: "f.brake" },
    { label: "Abort", cmd: "f.abort" },
    ...Array.from({ length: 10 }, (_, i) => ({
      label: `Group ${i + 1}`,
      cmd: `f.ag${i + 1}`,
    })),
  ];
</script>

<div class="ctl-grid">
  {#each actions as a (a.cmd)}
    <ControlButton label={a.label} commands={a.cmd} />
  {/each}
</div>
<Toast />

<style>
  .ctl-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    padding: 10px;
  }
</style>
