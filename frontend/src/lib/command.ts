// The one button-action helper. Dedups the copy-pasted command()/toggle()/
// execute()/commandactivate() functions from flight-control, smart-ass, d-pad
// and speech: fire the action, surface any non-OK status as a toast.

import { Signal, telemachus } from "./telemachus.ts";
import { notifySignal } from "./notify.ts";

export async function command(commands: string | string[]): Promise<Signal> {
  const code = await telemachus.action(commands);
  if (code > Signal.Ok) notifySignal(code);
  return code;
}
