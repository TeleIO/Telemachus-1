// Unified Telemachus data client. Replaces the three legacy clients:
//   - jKSPWAPICore.js  (jKSPWAPI.call / initPoll)
//   - console.js's bespoke `Telemachus` downlink loop
//   - touchball-pyr.html's promise-based `telemachus`
//
// Wire protocol is UNCHANGED (v1): the plugin serves
//   GET telemachus/datalink?<alias>=<apiExpr>&<alias2>=<apiExpr2>
// and replies with JSON `{ alias: value, ... }`. Polls additionally request
// `p=p.paused`, and the reply carries `p` (the signal/pause code). Actions are
// just reads of `ret=<command>` whose returned value is the status code.

/** Signal / pause / action status codes, as emitted in `p` and `ret`. */
export const enum Signal {
  Ok = 0,
  Paused = 1,
  PowerLoss = 2,
  Deactivated = 3,
  Unreachable = 4,
  MechJebMissing = 5,
}

export type Query = Record<string, string>;
export type Datalink = Record<string, unknown> & { p?: number };

/** Build `alias=expr&...` preserving order and encoding values. */
function encode(query: Query): string {
  return Object.entries(query)
    .map(([k, v]) => `${k}=${encodeURIComponent(v)}`)
    .join("&");
}

export interface ClientOptions {
  /** Base URL for the datalink endpoint. Default: relative `datalink`,
   *  which resolves to `/telemachus/datalink` when served from the plugin. */
  endpoint?: string;
}

export class TelemachusClient {
  readonly endpoint: string;

  constructor(opts: ClientOptions = {}) {
    this.endpoint = opts.endpoint ?? "datalink";
  }

  /** One-shot read. Resolves the raw `{alias: value}` map, or rejects on transport error. */
  async call(query: Query, signal?: AbortSignal): Promise<Datalink> {
    const res = await fetch(`${this.endpoint}?${encode(query)}`, { signal });
    if (!res.ok) throw new Error(`datalink ${res.status}`);
    // The plugin emits bare NaN in some builds; sanitise like the legacy client did.
    const text = (await res.text()).replace(/\bnan\b/gi, "0");
    return JSON.parse(text) as Datalink;
  }

  /**
   * Fire an action (or several) and resolve its status code.
   * Mirrors the legacy `command()`/`toggle()` helpers: `datalink?ret=<cmd>`.
   * Extra commands run as ret2, ret3, … in order (e.g. d-pad's FbW + setPYR).
   */
  async action(commands: string | string[]): Promise<Signal> {
    const list = Array.isArray(commands) ? commands : [commands];
    const query: Query = {};
    list.forEach((cmd, i) => (query[i === 0 ? "ret" : `ret${i + 1}`] = cmd));
    try {
      const d = await this.call(query);
      const code = Number(d.ret ?? 0);
      return (Number.isFinite(code) ? code : Signal.Unreachable) as Signal;
    } catch {
      return Signal.Unreachable;
    }
  }

  /**
   * Recurring poll. Calls `onData(values, signalCode)` every `interval` ms while
   * signalled, backing off to `idleInterval` when the antenna can't be reached.
   * `p=p.paused` is appended automatically; the raw `p` is stripped from `values`.
   * Returns a stop() function.
   */
  poll(
    query: Query,
    onData: (values: Datalink, signal: Signal) => void,
    opts: { interval?: number; idleInterval?: number } = {},
  ): () => void {
    const interval = opts.interval ?? 200;
    const idleInterval = opts.idleInterval ?? 1000;
    const ac = new AbortController();
    let timer: ReturnType<typeof setTimeout> | undefined;
    let stopped = false;

    const withPause: Query = { ...query, p: "p.paused" };

    const tick = async () => {
      if (stopped) return;
      try {
        const data = await this.call(withPause, ac.signal);
        const signal = (Number(data.p ?? 0) || 0) as Signal;
        delete data.p;
        onData(data, signal);
        timer = setTimeout(tick, interval);
      } catch (e) {
        if ((e as Error).name === "AbortError") return;
        onData({}, Signal.Unreachable);
        timer = setTimeout(tick, idleInterval);
      }
    };

    void tick();
    return () => {
      stopped = true;
      if (timer) clearTimeout(timer);
      ac.abort();
    };
  }

  /** Full live API listing (`a.api`) — used by the Information page. */
  async getApi(signal?: AbortSignal): Promise<Array<{ apistring: string; name: string; units: string; plotable: boolean }>> {
    const d = await this.call({ api: "a.api" }, signal);
    return (d.api as never) ?? [];
  }
}

/** Shared default client (same-origin), used by every page. */
export const telemachus = new TelemachusClient();
