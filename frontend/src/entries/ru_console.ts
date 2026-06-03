import { mount } from "svelte";
import Console from "../pages/Console.svelte";

// Russian variant — same component, lang="ru" (replaces the duplicated ru_console.html).
mount(Console, { target: document.body, props: { lang: "ru" } });
