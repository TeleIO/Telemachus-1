import { defineConfig } from "vite";
import { svelte } from "@sveltejs/vite-plugin-svelte";
import { resolve } from "node:path";

// One HTML entry per page, mirroring the legacy multi-page layout. The Telemachus
// static server (IOPageResponsibility) has no SPA fallback, so we stay multi-page
// rather than client-side routing. `base: "./"` keeps every emitted asset URL
// relative, so the bundle works when served from `/telemachus/`.
//
// Add a page here once its Svelte port lands under src/pages + src/entries + <page>.html.
const PAGES = [
  "index",
  "information",
  "flight-control",
  "smart-ass",
  "speech",
  "d-pad",
];

const here = import.meta.dirname;

export default defineConfig({
  base: "./",
  plugins: [svelte()],
  build: {
    outDir: "dist",
    emptyOutDir: true,
    // `.js`/`.css` are served with valid MIME types by the plugin; the legacy
    // server lacks `.mjs`/`.wasm`, and Vite emits neither by default.
    target: "es2019",
    rollupOptions: {
      input: Object.fromEntries(
        PAGES.map((p) => [p, resolve(here, `${p}.html`)]),
      ),
    },
  },
});
