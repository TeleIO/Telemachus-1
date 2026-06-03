// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import starlightOpenAPI, { openAPISidebarGroups } from "starlight-openapi";
import starlightClientMermaid from "@pasqal-io/starlight-client-mermaid";

// https://astro.build/config
export default defineConfig({
  site: "https://teleio.github.io",
  base: "/Telemachus-1",
  // Vite's SSR bundling under Deno produces a `shiki` without its bundled-themes
  // registry (a build-time module-resolution quirk), which breaks `github-dark`
  // during the static build. Keep Shiki external so the real package is loaded
  // at runtime instead — its themes resolve correctly under Deno. The matching
  // `imports` map in deno.json lets Deno resolve these now-bare imports.
  vite: {
    ssr: {
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
  integrations: [
    starlight({
      title: "Telemachus Reborn",
      social: [
        {
          icon: "github",
          label: "GitHub",
          href: "https://github.com/TeleIO/Telemachus-1",
        },
      ],
      plugins: [
        starlightClientMermaid(),
        starlightOpenAPI([
          {
            base: "api",
            schema: "./openapi.yaml",
            label: "API Reference",
            collapsed: true,
          },
        ]),
      ],
      sidebar: [
        {
          label: "Getting Started",
          items: [
            { label: "Introduction", slug: "guides/introduction" },
            { label: "Installation", slug: "guides/installation" },
          ],
        },
        {
          label: "Usage",
          items: [
            { label: "HTTP API", slug: "guides/http-api" },
            { label: "WebSocket", slug: "guides/websocket" },
          ],
        },
        ...openAPISidebarGroups,
      ],
    }),
  ],
});
