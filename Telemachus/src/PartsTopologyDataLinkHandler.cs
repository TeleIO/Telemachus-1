using System;
using System.Collections.Generic;
using CompoundParts;
using UnityEngine;

namespace Telemachus
{
    /// <summary>
    /// Vessel topology snapshot — assembled-space part graph for the active
    /// vessel. Cached and event-invalidated, not recomputed per tick. Lookup
    /// keys (r.resourceFor, therm.part) ride on flightID emitted here.
    /// </summary>
    public class PartsTopologyDataLinkHandler : DataLinkHandler
    {
        private static int _topologySeq;
        private static Vessel _cachedVessel;
        private static int _cachedFor;
        private static Dictionary<string, object> _cached;

        // Prefab bounds are immutable for a session — cache by AvailablePart.name.
        // Cached as a Vector3 pair (size, center). center is in part-local frame
        // relative to part.transform — non-zero for parts whose mesh does not
        // sit at the attach-node anchor (radial decouplers, surface-mounted
        // ladders, anything whose pivot is offset from its visual centroid).
        private struct PrefabBounds
        {
            public Vector3 size;
            public Vector3 center;
        }
        private static readonly Dictionary<string, PrefabBounds> _prefabBoundsCache = new();

        public PartsTopologyDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            // Subscribe only to events that actually change topology output.
            // The payload is built from prefab bounds (cached per AvailablePart),
            // the as-assembled orgPos, parent links, and the static module
            // name list — none of which change with deployable state, engine
            // ignition, parachute arming, crew transfer, or any other
            // non-structural change. onVesselWasModified fires on every such
            // change and was previously subscribed, causing topology seq to
            // bump dozens of times per flight without the payload changing.
            // Limiting the subscriptions to the genuinely-structural events
            // below keeps seq bumps load-bearing.
            GameEvents.onVesselChange.Add(OnVesselChanged);
            GameEvents.onPartCouple.Add(OnPartCouple);
            GameEvents.onPartUndock.Add(OnPartChanged);
            GameEvents.onPartDie.Add(OnPartChanged);
            GameEvents.onVesselCreate.Add(OnVesselChanged);
            GameEvents.onVesselDestroy.Add(OnVesselChanged);
            GameEvents.onFlightReady.Add(OnFlightReady);
        }

        // --- Event handlers: bump the seq, drop the cache, rebuild lazily. ---

        private static void Invalidate()
        {
            _topologySeq++;
            _cached = null;
            _cachedVessel = null;
        }

        private void OnVesselChanged(Vessel v) => Invalidate();
        private void OnPartChanged(Part p) => Invalidate();
        private void OnPartCouple(GameEvents.FromToAction<Part, Part> data) => Invalidate();
        private void OnFlightReady() => Invalidate();

        // --- Endpoints ---

        [TelemetryAPI("v.topologySeq",
            "Monotonic counter bumped whenever vessel topology changes. " +
            "Subscribe to this and refetch v.topology only when it ticks.",
            AlwaysEvaluable = true,
            Category = "vessel",
            ReturnType = "int")]
        object TopologySeq(DataSources ds) => _topologySeq;

        [TelemetryAPI("v.topology",
            "Active vessel topology: rootFlightId + per-part flightId, " +
            "persistentId, parentFlightId, fuelLineTarget (flightId of the " +
            "receiving tank for fuel-line parts, null otherwise), name, " +
            "title, manufacturer, category, inverseStage, crewCapacity, " +
            "maxTemp, crashTolerance, dryMass, orgPos[x,y,z], up[x,y,z] " +
            "(part-local up in vessel frame), bounds.size{x,y,z}, " +
            "bounds.center{x,y,z} (mesh-center offset from orgPos in " +
            "vessel-local frame — already rotated by orgRot; add it to " +
            "orgPos to get the mesh centre in assembly space), modules[]. " +
            "Cached and event-invalidated — subscribe to v.topologySeq to " +
            "detect changes rather than streaming this key.",
            AlwaysEvaluable = false,
            Plotable = false,
            Category = "vessel",
            ReturnType = "object")]
        object Topology(DataSources ds)
        {
            var vessel = ds.vessel;
            if (vessel == null) return null;

            // Cache by (vessel, seq) — a swap of ActiveVessel without an
            // event fire still needs a fresh build.
            if (_cached != null && ReferenceEquals(_cachedVessel, vessel)
                && _cachedFor == _topologySeq)
                return _cached;

            _cached = BuildTopology(vessel);
            _cachedVessel = vessel;
            _cachedFor = _topologySeq;
            return _cached;
        }

        // --- Builder ---

        private static Dictionary<string, object> BuildTopology(Vessel vessel)
        {
            var parts = new List<Dictionary<string, object>>();
            var rootFlightId = 0u;

            if (vessel.parts == null)
            {
                return new Dictionary<string, object>
                {
                    ["topologySeq"] = _topologySeq,
                    ["rootFlightId"] = 0u,
                    ["parts"] = parts,
                };
            }

            if (vessel.rootPart != null) rootFlightId = vessel.rootPart.flightID;

            foreach (var part in vessel.parts)
            {
                if (part == null) continue;
                parts.Add(SerialisePart(part));
            }

            return new Dictionary<string, object>
            {
                ["topologySeq"] = _topologySeq,
                ["rootFlightId"] = rootFlightId,
                ["parts"] = parts,
            };
        }

        private static Dictionary<string, object> SerialisePart(Part part)
        {
            var info = part.partInfo;
            var orgPos = part.orgPos;
            var prefab = GetPrefabBounds(info);
            var size = prefab.size;
            // Part's "up" axis in vessel-local frame — orgRot is the
            // as-assembled rotation relative to the vessel root, so this
            // captures whether a part was mounted axially (up ≈ +Y),
            // radially (up along ±X / ±Z), or inverted. Ship Map uses
            // this to orient nose cones, decouplers, docking ports etc.
            // without inferring orientation from neighbour geometry.
            var up = part.orgRot * Vector3.up;
            // Mesh-center offset, rotated into vessel-local frame. orgPos
            // is the attach-node anchor — for radial-mount parts (radial
            // decouplers, surface ladders, brackets) the mesh sits off-
            // anchor and a renderer that centred the body box on orgPos
            // produced visibly-sunken parts. Emitting the rotated offset
            // saves the client from needing the full orgRot quaternion
            // just to position the box correctly. Add this to orgPos to
            // get the mesh centre in vessel-local frame.
            var boundsCenterOffset = part.orgRot * prefab.center;

            var modules = new List<string>();
            object fuelLineTarget = null;
            if (part.Modules != null)
            {
                foreach (var module in part.Modules)
                {
                    if (module == null) continue;
                    modules.Add(module.moduleName ?? string.Empty);
                    // CModuleFuelLine.target (inherited from CompoundPartModule)
                    // points at the "to" tank — the receiving end of the line.
                    // The "from" end is already discoverable via parentFlightId.
                    // Resolving here keeps the wire format flat and avoids the
                    // client walking modules to fish out the linkage.
                    if (fuelLineTarget == null && module is CModuleFuelLine line
                        && line.target != null)
                    {
                        fuelLineTarget = line.target.flightID;
                    }
                }
            }

            return new Dictionary<string, object>
            {
                ["flightId"] = part.flightID,
                ["persistentId"] = part.persistentId,
                ["parentFlightId"] = part.parent != null
                    ? (object)part.parent.flightID
                    : null,
                ["fuelLineTarget"] = fuelLineTarget,

                ["name"] = info != null ? info.name ?? string.Empty : string.Empty,
                ["title"] = info != null ? info.title ?? string.Empty : string.Empty,
                ["manufacturer"] = info != null
                    ? info.manufacturer ?? string.Empty
                    : string.Empty,
                ["category"] = info != null ? info.category.ToString() : string.Empty,

                ["inverseStage"] = part.inverseStage,

                ["crewCapacity"] = part.CrewCapacity,
                ["maxTemp"] = part.maxTemp,
                ["crashTolerance"] = part.crashTolerance,

                ["dryMass"] = part.mass,

                ["orgPos"] = new object[] { orgPos.x, orgPos.y, orgPos.z },
                ["up"] = new object[] { up.x, up.y, up.z },

                ["bounds"] = new Dictionary<string, object>
                {
                    ["size"] = new Dictionary<string, object>
                    {
                        ["x"] = size.x,
                        ["y"] = size.y,
                        ["z"] = size.z,
                    },
                    ["center"] = new Dictionary<string, object>
                    {
                        ["x"] = boundsCenterOffset.x,
                        ["y"] = boundsCenterOffset.y,
                        ["z"] = boundsCenterOffset.z,
                    },
                },

                ["modules"] = modules,
            };
        }

        // Prefab bounds are stable across the session — cache per AvailablePart
        // by name. Live render bounds would inflate with vessel rotation and
        // jitter as joints flex; the prefab is the "as designed" silhouette.
        // We track both size *and* center: for parts whose mesh isn't centred
        // on the attach-node anchor (radial decouplers, surface ladders,
        // structural brackets), `center` is the offset in part-local frame
        // and the client must apply it to position the body box correctly.
        private static PrefabBounds GetPrefabBounds(AvailablePart info)
        {
            if (info == null || info.partPrefab == null)
            {
                return new PrefabBounds { size = Vector3.zero, center = Vector3.zero };
            }

            var key = info.name ?? string.Empty;
            if (_prefabBoundsCache.TryGetValue(key, out var cached)) return cached;

            var result = new PrefabBounds { size = Vector3.zero, center = Vector3.zero };
            try
            {
                var prefab = info.partPrefab;
                var bounds = PartGeometryUtil.MergeBounds(
                    PartGeometryUtil.GetPartRendererBounds(prefab),
                    prefab.transform);
                result.size = bounds.size;
                result.center = bounds.center;
            }
            catch (Exception)
            {
                // Leave both at zero.
            }

            _prefabBoundsCache[key] = result;
            return result;
        }
    }
}
