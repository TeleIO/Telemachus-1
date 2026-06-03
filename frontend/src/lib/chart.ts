// Scrolling time-series chart, ported from console.js's d3 `Chart` class.
// Uses d3 v7 (ESM-native; d3 v3 crashes under ESM because its IIFE relies on
// `this === window`). Same SVG structure / CSS class names so console.css styles
// it exactly as before; jQuery sizing replaced with the DOM.
//
// deno-lint-ignore-file no-explicit-any
import * as d3 from "d3";
import type { YAxis } from "./charts.ts";

const WINDOW = 300; // seconds of history shown

export class Chart {
  private el: HTMLElement;
  private series: string[];
  private yaxis: YAxis;
  private data: number[][] = [];
  private padding = { left: 70.5, top: 13.5, right: 13.5, bottom: 30.5 };
  private width = 0;
  private height = 0;
  private x: any;
  private y: any;
  private svg: any;
  private root: any;
  /** Set by the caller each frame so the x-axis can show MET ticks. */
  missionTimeOffset: number | undefined;

  constructor(el: HTMLElement, seriesNames: string[], yaxis: YAxis) {
    this.el = el;
    this.series = seriesNames.slice();
    this.yaxis = yaxis;
    if (this.series.length <= 1) this.padding.bottom = 13.5;
    this.build();
  }

  private dataDims() {
    const w = Math.max(this.width - (this.padding.left + this.padding.right), 0);
    const h = Math.max(this.height - (this.padding.top + this.padding.bottom), 0);
    return { w, h };
  }

  private build() {
    this.width = this.el.clientWidth || 300;
    this.height = this.el.clientHeight || 150;
    const { w, h } = this.dataDims();

    this.x = d3.scaleLinear().range([0, w]).domain([0, WINDOW]);
    this.y = d3.scaleLinear().range([h, 0]).domain([this.yaxis.min ?? 0, this.yaxis.max ?? 1]);

    this.svg = d3.select(this.el).append("svg:svg").attr("width", this.width).attr("height", this.height);
    this.root = this.svg.append("svg:g").attr("transform", `translate(${this.padding.left}, ${this.padding.top})`);

    this.root.append("svg:g").attr("class", "y grid");
    const xa = this.root.append("svg:g").attr("class", "x axis");
    xa.append("svg:path").attr("class", "domain").attr("d", `M0,${h}H${w}`);

    const ya = this.root.append("svg:g").attr("class", "y axis");
    ya.append("svg:text").attr("class", "label").attr("text-anchor", "middle")
      .attr("x", -h / 2).attr("y", -(this.padding.left - 18)).attr("transform", "rotate(-90)")
      .text(this.yaxis.label + (this.yaxis.unit ? ` (${this.yaxis.unit})` : ""));

    this.root.append("svg:g").attr("class", "data").selectAll("path").data(this.series).enter().append("svg:path");

    if (this.series.length > 1) {
      const legend = this.root.append("svg:text").attr("class", "legend")
        .attr("transform", `translate(${w / 2}, ${h + 20})`).attr("text-anchor", "middle")
        .selectAll("tspan").data(this.series).enter().append("svg:tspan")
        .attr("dx", (_d: string, i: number) => (i > 0 ? 30 : 0));
      legend.append("svg:tspan").attr("class", "bullet").text("◼ ");
      legend.append("svg:tspan").attr("class", "title").text((d: string) => d);

      // Legend interactivity (ported from console.js): hover dims the other
      // series; click isolates one (toggle). Uses .inactive/.active, styled by
      // console.css. (d3 v7 event handlers receive (event, datum); the series
      // index comes from the datum.)
      const svg = this.svg;
      const indexOf = (d: string) => this.series.indexOf(d);
      legend
        .on("mouseover", (_event: Event, d: string) => {
          if (svg.select(".active").empty()) {
            const i = indexOf(d);
            svg.selectAll(".data path").classed("inactive", (_p: any, j: number) => j !== i);
            svg.selectAll(".legend > tspan").classed("inactive", (_p: any, j: number) => j !== i);
          }
        })
        .on("mouseout", () => {
          if (svg.select(".active").empty()) {
            svg.selectAll(".data path, .legend > tspan").classed("inactive", false);
          }
        })
        .on("click", function (this: Element, _event: Event, d: string) {
          if (d3.select(this).classed("active")) {
            svg.selectAll(".data path, .legend > tspan").classed("inactive", false).classed("active", false);
          } else {
            const i = indexOf(d);
            svg.selectAll(".data path").classed("inactive", (_p: any, j: number) => j !== i);
            svg.selectAll(".legend > tspan")
              .classed("inactive", (_p: any, j: number) => j !== i)
              .classed("active", (_p: any, j: number) => j === i);
          }
        });
    }
    this.redraw();
  }

  addSample(t: number, sample: (number | null)[]) {
    this.data.push([t, ...sample.map((v) => (v == null ? NaN : v))]);
    const cutoff = t - WINDOW - 5;
    this.data = this.data.filter((row) => row[0] >= cutoff);
    this.x.domain([t - WINDOW, t]);

    if (this.width !== (this.el.clientWidth || this.width) || this.height !== (this.el.clientHeight || this.height)) {
      this.resize();
    } else {
      this.autoY();
      this.redraw();
    }
  }

  private autoY() {
    if (this.yaxis.min != null && this.yaxis.max != null) return;
    const vals: number[] = [];
    for (const row of this.data) for (let i = 1; i < row.length; i++) if (!Number.isNaN(row[i])) vals.push(row[i]);
    if (!vals.length) return;
    const lo = this.yaxis.min ?? Math.min(...vals);
    const hi = this.yaxis.max ?? Math.max(...vals);
    this.y.domain([lo, hi === lo ? lo + 1 : hi]).nice();
  }

  private redraw() {
    const { h, w } = this.dataDims();
    const tickCount = Math.max((h / 39) | 0, 2);

    // y grid
    const ticks = this.y.ticks(tickCount);
    const grid = this.svg.select("g.y.grid").selectAll("line").data(ticks);
    grid.enter().append("svg:line");
    grid.exit().remove();
    this.svg.selectAll("g.y.grid line").attr("x1", 0).attr("x2", w)
      .attr("y1", (d: number) => this.y(d)).attr("y2", (d: number) => this.y(d))
      .classed("zero", (d: number) => d === 0);

    // y axis
    this.svg.select("g.y.axis").call(d3.axisLeft(this.y).ticks(tickCount));

    // x axis (MET ticks)
    const xAxis = d3.axisBottom(this.x).tickSizeInner(h).tickSizeOuter(0).tickFormat((d: any) => {
      if (this.missionTimeOffset == null) return "";
      let t = (Number(d) - this.missionTimeOffset) / 60;
      const sign = t < 0 ? "T-" : "T+";
      t = Math.abs(t);
      const hh = (t / 60) | 0;
      let mm: string | number = (t % 60) | 0;
      if (mm < 10) mm = "0" + mm;
      return `${sign}${hh}:${mm}`;
    });
    this.svg.select("g.x.axis").call(xAxis);

    // series lines
    const line = (i: number) => {
      let path = "";
      for (let j = 0; j < this.data.length; j++) {
        const row = this.data[j];
        const v = row[i + 1];
        if (Number.isNaN(v)) continue;
        const prev = this.data[j - 1];
        path += (path.length > 0 && prev && !Number.isNaN(prev[i + 1])) ? "L" : "M";
        path += `${this.x(row[0])},${this.y(v)}`;
      }
      return path;
    };
    this.svg.selectAll("g.data path").data(this.series).attr("d", (_d: string, i: number) => line(i));
  }

  resize() {
    this.svg.remove();
    this.build();
  }

  destroy() {
    this.svg?.remove();
  }
}
