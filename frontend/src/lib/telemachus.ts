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

// ---------------------------------------------------------------------------
// WebSocket transport (the push channel Houston uses, now available to the
// bundled UI too). Protocol (KSPWebSocketService): connect to ws://host/datalink
// and send `{"+":[keys]}` / `{"-":[keys]}` / `{"run":[keys]}` / `{"rate":ms}`.
// Frames arrive as JSON keyed by the api string (plus optional `unknown`/`errors`).
// ---------------------------------------------------------------------------

export interface SocketHandlers {
  onFrame: (data: Datalink) => void;
  onState: (open: boolean) => void;
}

function resolveSocketUrl(): string {
  // The WS service is mounted at root (/datalink), not under /telemachus/.
  const loc = globalThis.location;
  const proto = loc?.protocol === "https:" ? "wss:" : "ws:";
  const host = loc?.host ?? "localhost:8085";
  return `${proto}//${host}/datalink`;
}

export class TelemachusSocket {
  private ws: WebSocket | null = null;
  private readonly url: string;
  private keys = new Set<string>();
  private rate: number;
  private closed = false;
  private retry: ReturnType<typeof setTimeout> | undefined;

  constructor(private handlers: SocketHandlers, opts: { url?: string; rate?: number } = {}) {
    this.url = opts.url ?? resolveSocketUrl();
    this.rate = opts.rate ?? 500;
  }

  open() {
    this.closed = false;
    try {
      this.ws = new WebSocket(this.url);
    } catch {
      this.scheduleReconnect();
      return;
    }
    this.ws.onopen = () => {
      this.handlers.onState(true);
      this.send({ rate: this.rate });
      if (this.keys.size) this.send({ "+": [...this.keys] });
    };
    this.ws.onclose = () => {
      this.handlers.onState(false);
      this.scheduleReconnect();
    };
    this.ws.onerror = () => this.ws?.close();
    this.ws.onmessage = (ev) => {
      if (typeof ev.data !== "string") return; // ignore binary frames here
      try {
        this.handlers.onFrame(JSON.parse(ev.data.replace(/\bnan\b/gi, "0")) as Datalink);
      } catch {
        /* malformed frame */
      }
    };
  }

  private scheduleReconnect() {
    if (this.closed || this.retry) return;
    this.retry = setTimeout(() => {
      this.retry = undefined;
      if (!this.closed) this.open();
    }, 2000);
  }

  private send(msg: unknown) {
    if (this.ws?.readyState === WebSocket.OPEN) this.ws.send(JSON.stringify(msg));
  }

  /** Replace the subscription set, emitting only the +/- diff. */
  setKeys(next: string[]) {
    const nextSet = new Set(next);
    const added = next.filter((k) => !this.keys.has(k));
    const removed = [...this.keys].filter((k) => !nextSet.has(k));
    this.keys = nextSet;
    if (added.length) this.send({ "+": added });
    if (removed.length) this.send({ "-": removed });
  }

  setRate(ms: number) {
    this.rate = ms;
    this.send({ rate: ms });
  }

  close() {
    this.closed = true;
    if (this.retry) clearTimeout(this.retry);
    this.ws?.close();
    this.ws = null;
  }
}
