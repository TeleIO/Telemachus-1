// Shared telemetry datastore. ONE connection feeds ALL data pages — the modern
// equivalent of console.js's single `downlink` union query, but reactive and
// ref-counted, and reused across console/map/touchball instead of each page
// opening its own poll loop.
//
// Transport: WebSocket first (the push channel); if the socket can't establish,
// it falls back to HTTP polling for the session. Either way, subscribers just
// read `$telemetry[key]`.

import { writable, type Readable } from "svelte/store";
import { Signal, TelemachusSocket, telemachus, type Datalink } from "./telemachus.ts";

/** Latest value for every subscribed api key, keyed by the api string. */
export const telemetry = writable<Record<string, unknown>>({});
/** Current signal / pause code (from p.paused). */
export const signal = writable<Signal>(Signal.Ok);
/** Whether the shared connection is currently live. */
export const connected = writable<boolean>(false);

const PAUSE_KEY = "p.paused";

class TelemetryHub {
  private refs = new Map<string, number>();
  private socket: TelemachusSocket | null = null;
  private stopPoll: (() => void) | null = null;
  private everOpen = false;
  private started = false;

  /** Subscribe to a set of keys; returns an unsubscribe function. */
  subscribe(keys: string[]): () => void {
    for (const k of keys) this.refs.set(k, (this.refs.get(k) ?? 0) + 1);
    this.ensureStarted();
    this.pushKeys();
    return () => {
      for (const k of keys) {
        const n = (this.refs.get(k) ?? 1) - 1;
        if (n <= 0) this.refs.delete(k);
        else this.refs.set(k, n);
      }
      this.pushKeys();
    };
  }

  private union(): string[] {
    return [PAUSE_KEY, ...this.refs.keys()];
  }

  private ensureStarted() {
    if (this.started) return;
    this.started = true;
    this.socket = new TelemachusSocket({
      onFrame: (frame) => this.applyFrame(frame),
      onState: (open) => {
        connected.set(open);
        if (open) this.everOpen = true;
        // If the very first connection attempt fails, drop to HTTP polling.
        else if (!this.everOpen) this.fallbackToPoll();
      },
    });
    this.socket.open();
  }

  /** WebSocket frames are keyed by api string; strip diagnostics, lift p.paused. */
  private applyFrame(frame: Datalink) {
    const { unknown: _u, errors: _e, ...values } = frame as Record<string, unknown>;
    if (PAUSE_KEY in values) {
      signal.set((Number(values[PAUSE_KEY]) || 0) as Signal);
    }
    telemetry.update((cur) => ({ ...cur, ...values }));
  }

  private pushKeys() {
    if (this.socket && !this.stopPoll) {
      this.socket.setKeys(this.union());
    } else if (this.stopPoll) {
      this.startPoll(); // restart with the new union
    }
  }

  private fallbackToPoll() {
    this.socket?.close();
    this.socket = null;
    this.startPoll();
  }

  private startPoll() {
    this.stopPoll?.();
    const union = [...this.refs.keys()];
    // Index aliases avoid bracket/dot ambiguity in the query's LHS.
    const query: Record<string, string> = {};
    union.forEach((k, i) => (query[`a${i}`] = k));
    this.stopPoll = telemachus.poll(query, (vals, sig) => {
      connected.set(sig !== Signal.Unreachable);
      signal.set(sig);
      const merged: Record<string, unknown> = {};
      union.forEach((k, i) => (merged[k] = vals[`a${i}`]));
      telemetry.update((cur) => ({ ...cur, ...merged }));
    });
  }
}

const hub = new TelemetryHub();

/** Subscribe a component to live telemetry keys. Call in onMount; the returned
 *  function unsubscribes (call in onDestroy). Read values via `$telemetry[key]`. */
export function useTelemetry(keys: string[]): () => void {
  return hub.subscribe(keys);
}

/** Convenience: the shared values store typed as Readable. */
export const values: Readable<Record<string, unknown>> = telemetry;
