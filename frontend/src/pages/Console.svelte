<script lang="ts">
  // Graphs & Tables console. Same layout/markup as console.html (console.css is
  // reused verbatim), but the data plumbing is the shared store and the d3
  // engine is the ported Chart. Russian variant is this same component with
  // lang="ru" (replaces the duplicated ru_console.html).
  import { onMount, onDestroy } from "svelte";
  import L from "leaflet";
  import "leaflet/dist/leaflet.css";
  import { telemetry, signal, useTelemetry } from "../lib/telemetry.ts";
  import { standardCharts, standardLayouts, defaultLayout, type ChartDef, type LayoutDef } from "../lib/charts.ts";
  import { strings, categoryList } from "../lib/strings.ts";
  import { formatValue, missionTimeString, dateString } from "../lib/format.ts";
  import { telemachus } from "../lib/telemachus.ts";
  import { Chart } from "../lib/chart.ts";
  import { baseLayers, defaultIcon } from "../lib/maps.ts";

  let { lang = "" }: { lang?: string } = $props();
  const S = strings(lang);
  const cats = categoryList(S);

  const CELESTIAL = ["Kerbol", "Kerbin", "Mun", "Minmus", "Moho", "Eve", "Duna", "Ike", "Jool", "Laythe", "Vall", "Bop", "Tylo", "Gilly", "Pol", "Dres", "Eeloo"];
  const RESOURCES = ["ElectricCharge", "SolidFuel", "LiquidFuel", "Oxidizer", "MonoPropellant", "IntakeAir", "XenonGas"];

  type ApiInfo = { name: string; units: string; plotable: boolean };
  let api = $state<Record<string, ApiInfo>>({});

  const charts: Record<string, ChartDef> = { ...standardCharts };
  let layouts = $state<Record<string, LayoutDef>>({ ...standardLayouts });
  let customLayouts = $state<Record<string, LayoutDef>>({});

  let layoutName = $state(defaultLayout);
  let telemetryKeys = $state<string[]>([]);
  let slotNames = $state<string[]>(["", "", ""]);

  let category = $state(cats[0].regex);
  let selectedApi = $state("");
  let metText = $state("T+00:00:00 MET");
  let utText = $state("Year 1, Day 1, 00:00:00 UT");

  const slotEls: (HTMLDivElement | null)[] = [null, null, null];
  const slotCharts: (Chart | null)[] = [null, null, null];
  const slotMaps: { map: L.Map; marker: L.Marker }[] = [];
  let stopKeys: (() => void) | undefined;
  let lastValues: Record<string, unknown> = {};

  // console.css sizes the charts with percentage heights that only resolve when
  // #container / #charts have an explicit pixel height — the legacy console.js
  // set this from the viewport in a resize handler. Ported here.
  let containerEl: HTMLDivElement;
  let chartsEl: HTMLDivElement;
  function resize() {
    if (!containerEl || !chartsEl) return;
    const top = containerEl.getBoundingClientRect().top;
    const footer = document.querySelector("body > footer") as HTMLElement | null;
    const footerH = footer ? footer.offsetHeight + 40 : 60;
    const h = Math.max(globalThis.innerHeight - top - footerH, 240);
    containerEl.style.height = `${h}px`;
    chartsEl.style.height = `${h}px`;
    // Bound the telemetry list so it scrolls above the (absolute) add-form,
    // rather than overflowing underneath it.
    const ul = containerEl.querySelector("#telemetry ul") as HTMLElement | null;
    const form = containerEl.querySelector("#telemetry form") as HTMLElement | null;
    if (ul && form) ul.style.height = `${Math.max(form.offsetTop - ul.offsetTop, 0)}px`;
    for (let i = 0; i < 3; i++) {
      slotCharts[i]?.resize();
      slotMaps[i]?.map.invalidateSize();
    }
  }
  const scheduleResize = () => requestAnimationFrame(resize);

  // ---- api discovery (expand b.* per body, r.* per resource) ----
  async function loadApi() {
    let list: Array<{ apistring: string; name: string; units: string; plotable: boolean }>;
    try {
      list = await telemachus.getApi();
    } catch {
      setTimeout(loadApi, 5000);
      return;
    }
    const out: Record<string, ApiInfo> = {};
    for (const e of list) {
      if (/^b\./.test(e.apistring)) {
        for (let i = 0; i < CELESTIAL.length; i++) out[`${e.apistring}[${i}]`] = e;
      } else if (/^r\./.test(e.apistring)) {
        if (e.apistring !== "r.resourceCurrent") {
          for (const r of RESOURCES) {
            const name = r.replace(/([a-z])([A-Z])/g, "$1 $2") + (/Max$/.test(e.apistring) ? " Max" : "");
            out[`${e.apistring}[${r}]`] = { ...e, name };
          }
        }
      } else if (e.plotable && e.apistring !== "s.sensor") {
        out[e.apistring] = e;
      }
    }
    api = out;
    selectedApi = Object.keys(api).find((k) => new RegExp(category).test(k)) ?? "";
    setLayout(defaultLayout);
  }

  // ---- subscription set = telemetry list + chart series + clock ----
  const neededKeys = $derived([
    ...new Set([
      "v.missionTime",
      "t.universalTime",
      ...telemetryKeys,
      ...slotNames.flatMap((n) => charts[n]?.series ?? []),
    ]),
  ]);
  $effect(() => {
    const keys = neededKeys;
    stopKeys?.();
    stopKeys = useTelemetry(keys);
  });

  // ---- live updates: readouts + charts fed from the shared store ----
  function onValues(v: Record<string, unknown>) {
    lastValues = v;
    const t = Number(v["t.universalTime"]) || 0;
    const missionTime = Number(v["v.missionTime"]) || 0;
    for (let i = 0; i < 3; i++) {
      const def = charts[slotNames[i]];
      const chart = slotCharts[i];
      if (def && !def.type && chart) {
        chart.missionTimeOffset = missionTime > 0 ? t - missionTime : undefined;
        chart.addSample(t, def.series.map((k) => {
          const val = v[k];
          return val == null || Array.isArray(val) ? null : Number(val);
        }));
      } else if (def?.type === "map" && slotMaps[i]) {
        const lat = Number(v["v.lat"]);
        const long = Number(v["v.long"]);
        if (!Number.isNaN(lat) && !Number.isNaN(long)) {
          slotMaps[i].marker.setLatLng([lat, long > 180 ? long - 360 : long]);
        }
      }
    }
  }

  // ---- chart slot lifecycle ----
  function buildSlot(i: number) {
    const def = charts[slotNames[i]];
    const el = slotEls[i];
    if (!el) return;
    slotCharts[i]?.destroy();
    slotCharts[i] = null;
    if (slotMaps[i]) { slotMaps[i].map.remove(); delete slotMaps[i]; }
    el.innerHTML = "";
    if (!def) return;
    if (def.type === "map") {
      const layers = baseLayers();
      const map = L.map(el, { crs: L.CRS.EPSG4326, center: [0, 0], zoom: 0, maxZoom: 7 });
      L.control.layers(layers).addTo(map);
      (layers["Kerbin Satellite (Stock)"] ?? Object.values(layers)[0]).addTo(map);
      map.fitWorld();
      const marker = L.marker([0, 0], { icon: defaultIcon() }).addTo(map);
      slotMaps[i] = { map, marker };
    } else if (def.yaxis) {
      const names = def.series.map((k) => api[k]?.name ?? k);
      slotCharts[i] = new Chart(el, names, def.yaxis);
    }
  }

  function setChart(i: number, name: string) {
    slotNames[i] = name;
    queueMicrotask(() => { buildSlot(i); scheduleResize(); });
  }

  // ---- telemetry list ----
  function addTelemetry(key: string) {
    if (key && api[key] && !telemetryKeys.includes(key)) telemetryKeys = [...telemetryKeys, key];
  }
  function removeTelemetry(key: string) {
    telemetryKeys = telemetryKeys.filter((k) => k !== key);
  }

  // Drag-to-reorder (replaces the jQuery-UI sortable).
  let dragIndex = $state(-1);
  function dragStart(e: DragEvent, i: number) {
    dragIndex = i;
    if (e.dataTransfer) {
      e.dataTransfer.effectAllowed = "move";
      e.dataTransfer.setData("text/plain", String(i));
    }
  }
  function drop(e: DragEvent, i: number) {
    e.preventDefault();
    if (dragIndex >= 0 && dragIndex !== i) {
      const arr = [...telemetryKeys];
      const [moved] = arr.splice(dragIndex, 1);
      arr.splice(i, 0, moved);
      telemetryKeys = arr;
    }
    dragIndex = -1;
  }

  // ---- layouts (+ localStorage custom) ----
  function setLayout(name: string) {
    if (!(name in layouts)) return;
    localStorage?.setItem("defaultLayout", name);
    layoutName = name;
    const l = layouts[name];
    telemetryKeys = l.telemetry.filter((k) => k in api);
    slotNames = [l.charts[0] ?? "", l.charts[1] ?? "", l.charts[2] ?? ""];
    queueMicrotask(() => { for (let i = 0; i < 3; i++) buildSlot(i); scheduleResize(); });
  }
  function saveLayout() {
    const name = prompt("What name would you like to save this layout under?", layoutName)?.trim();
    if (!name) return;
    if (name in layouts && !confirm("That name is already in use. Overwrite?")) return;
    layouts[name] = customLayouts[name] = { charts: [...slotNames], telemetry: [...telemetryKeys] };
    localStorage?.setItem("telemachus.console.layouts", JSON.stringify(customLayouts));
    layoutName = name;
  }
  function deleteLayout() {
    if (!(layoutName in customLayouts) || !confirm("Delete the current custom layout?")) return;
    delete customLayouts[layoutName];
    localStorage?.setItem("telemachus.console.layouts", JSON.stringify(customLayouts));
    if (layoutName in standardLayouts) layouts[layoutName] = standardLayouts[layoutName];
    else { delete layouts[layoutName]; setLayout(Object.keys(layouts)[0]); }
  }

  $effect(() => {
    selectedApi = Object.keys(api).find((k) => new RegExp(category).test(k)) ?? "";
  });

  onMount(() => {
    if (localStorage) {
      customLayouts = JSON.parse(localStorage.getItem("telemachus.console.layouts") ?? "{}");
      layouts = { ...standardLayouts, ...customLayouts };
      const saved = localStorage.getItem("defaultLayout");
      if (saved && saved in layouts) layoutName = saved;
    }
    const unsub = telemetry.subscribe(onValues);
    const clock = setInterval(() => {
      if ($signal === 1) return;
      metText = missionTimeString(Number(lastValues["v.missionTime"]) || 0);
      utText = dateString(Number(lastValues["t.universalTime"]) || 0);
    }, 1000);
    globalThis.addEventListener("resize", resize);
    scheduleResize();
    loadApi();
    onDestroy(() => {
      unsub();
      clearInterval(clock);
      stopKeys?.();
      globalThis.removeEventListener("resize", resize);
    });
  });
</script>

<header>
  <h1>{layoutName}</h1>
  <nav class="dropdown">
    <select aria-label={S.changeLayout} bind:value={layoutName} onchange={(e) => setLayout((e.target as HTMLSelectElement).value)}>
      {#each Object.keys(layouts) as l (l)}<option value={l}>{l}</option>{/each}
    </select>
  </nav>
  <p id="met">{metText}</p>
  <p id="ut">{utText}</p>
</header>

<div id="container" bind:this={containerEl}>
  <article id="telemetry">
    <header><h2>{S.telemetry}</h2></header>
    <ul>
      {#each telemetryKeys as key, i (key)}
        <li
          data-api={key}
          draggable="true"
          class:dragging={dragIndex === i}
          ondragstart={(e) => dragStart(e, i)}
          ondragover={(e) => e.preventDefault()}
          ondrop={(e) => drop(e, i)}
          ondragend={() => (dragIndex = -1)}
        >
          <h3>{api[key]?.name ?? key}</h3>
          <button class="remove" aria-label="Remove" onclick={() => removeTelemetry(key)}></button>
          <img class="handle" src="img/draghandle.png" alt="Drag to reorder" />
          <div class="telemetry-data">{formatValue($telemetry[key], api[key]?.units ?? "UNITLESS")}</div>
        </li>
      {/each}
    </ul>
    <form onsubmit={(e) => { e.preventDefault(); addTelemetry(selectedApi); }}>
      <div>
        <select id="apiCategory" bind:value={category}>
          {#each cats as c (c.regex + c.label)}<option value={c.regex}>{c.label}</option>{/each}
        </select>
        <select id="apiSelect" bind:value={selectedApi}>
          {#each Object.keys(api).filter((k) => new RegExp(category).test(k)) as k (k)}
            <option value={k}>{api[k].name}</option>
          {/each}
        </select>
        <input type="submit" value={S.add} />
      </div>
    </form>
  </article>

  <div id="charts" bind:this={chartsEl}>
    {#each [0, 1, 2] as i (i)}
      <article class={i === 0 ? "large chart" : "small chart"}>
        <header>
          <h2>{slotNames[i]}</h2>
          <nav class="dropdown">
            <select aria-label={S.changeChart} value={slotNames[i]} onchange={(e) => setChart(i, (e.target as HTMLSelectElement).value)}>
              {#each Object.keys(charts) as c (c)}<option value={c}>{c}</option>{/each}
            </select>
          </nav>
        </header>
        <div class="display" bind:this={slotEls[i]}></div>
        <p class="alert">{$signal === 4 ? "Signal Lost" : $signal === 1 ? "Game Paused" : ""}</p>
      </article>
    {/each}
  </div>
</div>

<footer>
  <button id="saveLayout" onclick={saveLayout}>{S.saveLayout}</button>
  <button id="deleteLayout" onclick={deleteLayout} disabled={!(layoutName in customLayouts)}>{S.deleteLayout}</button>
</footer>

<style>
  /* The legacy layout/chart menus were svg hamburger buttons that console.css
     aligned; our native <select> needs explicit vertical-centering against the
     heading so the title and dropdown share a centre line. */
  header h1,
  header h2,
  header select {
    vertical-align: middle;
  }
  header select {
    margin-left: 0.5ex;
  }
</style>
