// Toast notifications — replaces the global `sNotify` queue and
// `jKSPWAPI.generateNotificationWithCode`. A tiny Svelte store the <Toast/>
// component renders; auto-dismisses after `timeOpen` seconds like the original.

import { writable } from "svelte/store";
import { Signal } from "./telemachus.ts";

export interface Toast {
  id: number;
  message: string;
}

const timeOpenMs = 3000;
let nextId = 0;

export const toasts = writable<Toast[]>([]);

export function notify(message: string): void {
  const id = nextId++;
  toasts.update((list) => [...list, { id, message }]);
  setTimeout(() => dismiss(id), timeOpenMs);
}

export function dismiss(id: number): void {
  toasts.update((list) => list.filter((t) => t.id !== id));
}

/** Map a signal/return code to its message — was generateNotificationWithCode. */
export function notifySignal(code: Signal | number): void {
  switch (code) {
    case Signal.Ok:
      notify("Signal found.");
      break;
    case Signal.Paused:
      notify("Game paused.");
      break;
    case Signal.PowerLoss:
      notify("Potential power loss on antenna.");
      break;
    case Signal.Deactivated:
      notify("Antenna is deactivated.");
      break;
    case Signal.Unreachable:
      notify("Unable to reach antenna.");
      break;
    case Signal.MechJebMissing:
      notify("MechJeb not found.");
      break;
  }
}
