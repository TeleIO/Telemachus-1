import { mount } from "svelte";
import Console from "../pages/Console.svelte";

// Mounted on <body> so console.css's `body > header` / `body > footer` rules apply.
mount(Console, { target: document.body, props: { lang: "" } });
