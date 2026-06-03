// Deno-only Astro config. The original astro.config.mjs is left untouched (it's
// what the npm/Node toolchain uses); the Deno build task selects this file via
// `astro build --config astro.config.deno.mjs`.
//
// Under Deno, Vite's SSR bundling produces a `shiki` stripped of its bundled-
// themes registry (a build-time module-resolution quirk — Shiki itself works
// fine under Deno at runtime). So Shiki is kept external here and loaded from
// node_modules at runtime; deno.json's `imports` map resolves these now-bare
// specifiers. Rendered output is identical to the default config.
import base from "./astro.config.mjs";

export default {
  ...base,
  vite: {
    ...base.vite,
    ssr: {
      ...base.vite?.ssr,
      external: [
        "shiki",
        "@shikijs/core",
        "@shikijs/themes",
        "@shikijs/langs",
        "@shikijs/engine-javascript",
        "@shikijs/engine-oniguruma",
        "@shikijs/types",
        "@shikijs/vscode-textmate",
      ],
    },
  },
};
