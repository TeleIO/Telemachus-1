// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import starlightOpenAPI, { openAPISidebarGroups } from "starlight-openapi";

// https://astro.build/config
export default defineConfig({
  site: "https://teleio.github.io",
  base: "/Telemachus-1",
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
