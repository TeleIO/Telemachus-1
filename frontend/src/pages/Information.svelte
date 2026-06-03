<script lang="ts">
  // Information page. Replaces jQuery-UI tabs/accordion (dead CDN) with native
  // tabs + <details>. The page list comes from the shared PAGES registry; the
  // API list / version / IPs come live from the plugin.
  import { onMount } from "svelte";
  import { PAGES } from "../lib/pages.ts";
  import { telemachus } from "../lib/telemachus.ts";

  type Tab = "telemachus" | "api" | "about";
  let tab = $state<Tab>("telemachus");

  let version = $state("Unknown");
  let ips = $state<string[]>([]);
  let api = $state<Array<{ apistring: string; name: string; units: string; plotable: boolean }>>([]);

  const open = (href: string) => window.open(href);

  onMount(async () => {
    try {
      version = String((await telemachus.call({ version: "a.version" })).version ?? "Unknown");
    } catch { /* offline */ }
    try {
      ips = ((await telemachus.call({ ip: "a.ip" })).ip as string[]) ?? [];
    } catch { /* offline */ }
    try {
      api = await telemachus.getApi();
    } catch { /* offline */ }
  });
</script>

<div class="tabs">
  <ul class="tab-bar">
    <li><button class:active={tab === "telemachus"} onclick={() => (tab = "telemachus")}>Telemachus</button></li>
    <li><button class:active={tab === "api"} onclick={() => (tab = "api")}>API</button></li>
    <li><button class:active={tab === "about"} onclick={() => (tab = "about")}>About</button></li>
  </ul>

  {#if tab === "telemachus"}
    <div class="panel">
      {#each PAGES as p (p.id)}
        <details>
          <summary>{p.title}</summary>
          <div class="body">
            <p>{p.description}</p>
            <button type="button" onclick={() => open(p.href)}>Open</button>
          </div>
        </details>
      {/each}
    </div>
  {:else if tab === "api"}
    <div class="panel">
      {#each api as entry (entry.apistring)}
        <details>
          <summary>{entry.name}</summary>
          <div class="body">
            <p>{entry.apistring}</p>
            <p>{entry.units}</p>
            <p>{entry.plotable ? "This value can be plotted/placed into a table" : "This value cannot be plotted/placed into a table"}</p>
          </div>
        </details>
      {:else}
        <p>No API data — is the antenna reachable?</p>
      {/each}
    </div>
  {:else}
    <div class="panel">
      <details open>
        <summary>Version</summary>
        <div class="body"><p>{version}</p></div>
      </details>
      <details>
        <summary>IP Addresses</summary>
        <div class="body">
          {#if ips.length}<ul>{#each ips as ip (ip)}<li>{ip}</li>{/each}</ul>{:else}<p>Unknown</p>{/if}
        </div>
      </details>
      <details>
        <summary>Credits</summary>
        <div class="body">
          <p>Telemachus Reborn — see <a href="https://github.com/TeaGuild/Telemachus-1">the repository</a> and CONTRIBUTORS.md for the full list.</p>
          <p>Map data: <a href="https://kerbal-maps.finitemonkeys.org/">Kerbal Maps</a>. Thanks to the authors of MechJeb for providing their source to learn from.</p>
        </div>
      </details>
    </div>
  {/if}
</div>

<style>
  .tabs {
    max-width: 800px;
    margin: 0 auto;
    font-family: "Open Sans", arial, sans-serif;
  }
  .tab-bar {
    display: flex;
    list-style: none;
    padding: 0;
    margin: 0 0 -1px;
  }
  .tab-bar button {
    padding: 10px 18px;
    font: inherit;
    background: #ececec;
    border: 1px solid #ccc;
    border-bottom: none;
    border-radius: 6px 6px 0 0;
    cursor: pointer;
  }
  .tab-bar button.active {
    background: #fff;
    font-weight: bold;
  }
  .panel {
    border: 1px solid #ccc;
    padding: 12px;
  }
  details {
    border-bottom: 1px solid #eee;
    padding: 6px 0;
  }
  summary {
    cursor: pointer;
    font-weight: bold;
  }
  .body {
    padding: 8px 4px;
  }
  .body button {
    padding: 6px 16px;
    cursor: pointer;
  }
</style>
