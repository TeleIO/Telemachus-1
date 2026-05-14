using System;
using System.Collections.Generic;
using ModuleWheels;

namespace Telemachus
{
    /// <summary>
    /// Live runtime behavioural state for an individual part — deployable /
    /// activation state per supported module (solar panels, radiators,
    /// parachutes, engines, drills, cargo bays, landing gear). Cached and
    /// event-invalidated; intended for "is the panel deployed?" style UI,
    /// not high-frequency telemetry.
    /// </summary>
    public class PartStateDataLinkHandler : DataLinkHandler
    {
        // Vessel-level invalidation counter — stamped into every per-part
        // response so consumers can dedup unchanged pushes without doing
        // a deep compare on the modules array.
        private static int _seq;

        // Per-flightID payload cache. Built lazily on read after the cache
        // is cleared by an invalidation event or the backstop timer.
        private static readonly Dictionary<uint, Dictionary<string, object>> _cache
            = new Dictionary<uint, Dictionary<string, object>>();

        // Backstop: 10s after the last invalidation, force a re-walk. Covers
        // PAW interactions that don't fire a global GameEvent (right-click
        // → Extend Solar Panel) — the cache catches up within 10s without
        // us hooking per-module callbacks.
        private static DateTime _lastInvalidatedAt = DateTime.UtcNow;
        private static readonly TimeSpan BACKSTOP_INTERVAL = TimeSpan.FromSeconds(10);

        public PartStateDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            // No global onActionGroup* in this KSP — toggles like G (gear)
            // or U (lights) play their animations without firing a generic
            // event we can hook. AG-driven deploys fall back on the 10s
            // backstop, which keeps the staleness bounded.
            GameEvents.onStageActivate.Add(OnStageActivate);
            GameEvents.onVesselWasModified.Add(OnVesselChanged);
            GameEvents.onPartCouple.Add(OnPartCouple);
            GameEvents.onPartUndock.Add(OnPartChanged);
            GameEvents.onPartDie.Add(OnPartChanged);
            GameEvents.onPartActionUIDismiss.Add(OnPartActionUIDismiss);
        }

        // --- Invalidation ---

        private static void Invalidate()
        {
            _seq++;
            _cache.Clear();
            _lastInvalidatedAt = DateTime.UtcNow;
        }

        private void OnStageActivate(int stage) => Invalidate();
        private void OnVesselChanged(Vessel v) => Invalidate();
        private void OnPartChanged(Part p) => Invalidate();
        private void OnPartCouple(GameEvents.FromToAction<Part, Part> data) => Invalidate();
        private void OnPartActionUIDismiss(Part p) => Invalidate();

        // --- Endpoint ---

        [TelemetryAPI("v.partState",
            "Live deployable / behavioural state for a single part keyed by " +
            "flightID. Returns { seq, modules: [{ type, state, ...extras }] }. " +
            "Cached server-side and invalidated on staging, action-group " +
            "events, vessel modifications, part death / undock / couple, and " +
            "PAW dismiss; plus a 10s backstop covering player right-click " +
            "interactions that don't fire a global event. Consumers subscribe " +
            "and dedup on the seq field rather than re-processing the modules " +
            "array on every push.",
            AlwaysEvaluable = false,
            Plotable = false,
            Category = "vessel",
            ReturnType = "object",
            Params = "uint flightId")]
        object PartState(DataSources ds)
        {
            // Backstop check on every read — cheaper than scheduling a timer.
            if (DateTime.UtcNow - _lastInvalidatedAt > BACKSTOP_INTERVAL)
            {
                Invalidate();
            }

            if (ds.args == null || ds.args.Count == 0) return null;
            if (!uint.TryParse(ds.args[0], out var flightId)) return null;
            if (ds.vessel == null || ds.vessel.parts == null) return null;

            if (_cache.TryGetValue(flightId, out var cached)) return cached;

            foreach (var part in ds.vessel.parts)
            {
                if (part == null || part.flightID != flightId) continue;
                var result = BuildPartState(part);
                _cache[flightId] = result;
                return result;
            }
            return null;
        }

        // --- Build ---

        private static Dictionary<string, object> BuildPartState(Part part)
        {
            var modules = new List<Dictionary<string, object>>();
            if (part.Modules != null)
            {
                foreach (var module in part.Modules)
                {
                    if (module == null) continue;
                    var entry = SerialiseModule(module, part);
                    if (entry != null) modules.Add(entry);
                }
            }
            return new Dictionary<string, object>
            {
                ["seq"] = _seq,
                ["modules"] = modules,
            };
        }

        private static Dictionary<string, object> SerialiseModule(PartModule module, Part part)
        {
            switch (module)
            {
                case ModuleDeployableSolarPanel solar:
                    return new Dictionary<string, object>
                    {
                        ["type"] = "solarPanel",
                        ["state"] = MapDeployState(solar.deployState),
                        ["tracking"] = solar.isTracking,
                    };

                case ModuleDeployableRadiator radiator:
                    return new Dictionary<string, object>
                    {
                        ["type"] = "radiator",
                        ["state"] = MapDeployState(radiator.deployState),
                    };

                case ModuleDeployableAntenna antenna:
                    return new Dictionary<string, object>
                    {
                        ["type"] = "antenna",
                        ["state"] = MapDeployState(antenna.deployState),
                    };

                case ModuleParachute parachute:
                    return new Dictionary<string, object>
                    {
                        ["type"] = "parachute",
                        ["state"] = MapParachuteState(parachute.deploymentState),
                    };

                case ModuleEngines engine:
                    // ModuleEnginesFX inherits ModuleEngines so this case
                    // catches both.
                    var engineEntry = new Dictionary<string, object>
                    {
                        ["type"] = "engine",
                        ["state"] = engine.EngineIgnited ? "active" : "inactive",
                    };
                    if (engine.flameout) engineEntry["flameout"] = true;
                    return engineEntry;

                case ModuleResourceHarvester harvester:
                    return new Dictionary<string, object>
                    {
                        ["type"] = "drill",
                        ["state"] = harvester.IsActivated ? "active" : "inactive",
                    };

                case ModuleCargoBay cargoBay:
                    return SerialiseCargoBay(cargoBay, part);

                case ModuleWheelDeployment wheel:
                    return new Dictionary<string, object>
                    {
                        ["type"] = "landingGear",
                        ["state"] = MapWheelState(wheel.stateString),
                    };

                default:
                    // Not a behavioural module the API surfaces — skip.
                    return null;
            }
        }

        // --- Mappings ---

        // Order standardised in the design doc:
        // extended / retracted / deploying / retracting / stowed / armed /
        // active / inactive / broken / unknown.

        private static string MapDeployState(ModuleDeployablePart.DeployState state)
        {
            switch (state)
            {
                case ModuleDeployablePart.DeployState.EXTENDED: return "extended";
                case ModuleDeployablePart.DeployState.RETRACTED: return "retracted";
                case ModuleDeployablePart.DeployState.EXTENDING: return "deploying";
                case ModuleDeployablePart.DeployState.RETRACTING: return "retracting";
                case ModuleDeployablePart.DeployState.BROKEN: return "broken";
                default: return "unknown";
            }
        }

        private static string MapParachuteState(ModuleParachute.deploymentStates state)
        {
            switch (state)
            {
                case ModuleParachute.deploymentStates.STOWED: return "stowed";
                case ModuleParachute.deploymentStates.ACTIVE: return "armed";
                case ModuleParachute.deploymentStates.SEMIDEPLOYED: return "deploying";
                case ModuleParachute.deploymentStates.DEPLOYED: return "extended";
                case ModuleParachute.deploymentStates.CUT: return "broken";
                default: return "unknown";
            }
        }

        // Stock cargo bays carry a paired ModuleAnimateGeneric on the same
        // part; animTime is 0 (closed) → 1 (open). animSpeed sign tells us
        // direction during animation. ModuleCargoBay's own deployModule ref
        // is private so we reach for the sibling module instead.
        private static Dictionary<string, object> SerialiseCargoBay(ModuleCargoBay bay, Part part)
        {
            ModuleAnimateGeneric anim = null;
            if (part.Modules != null)
            {
                foreach (var m in part.Modules)
                {
                    if (m is ModuleAnimateGeneric mag) { anim = mag; break; }
                }
            }

            string state;
            if (anim == null)
            {
                state = "unknown";
            }
            else if (anim.animTime >= 0.95f)
            {
                state = "extended";
            }
            else if (anim.animTime <= 0.05f)
            {
                state = "retracted";
            }
            else
            {
                // Mid-animation. Speed sign distinguishes opening vs closing;
                // animSpeed > 0 = playing forward = opening.
                state = anim.animSpeed >= 0f ? "deploying" : "retracting";
            }

            return new Dictionary<string, object>
            {
                ["type"] = "cargoBay",
                ["state"] = state,
            };
        }

        // ModuleWheelDeployment uses a KerbalFSM whose state names are
        // "Deployed" / "Retracted" / "Deploying" / "Retracting" / variants.
        // Normalise to the design vocabulary.
        private static string MapWheelState(string stateString)
        {
            if (string.IsNullOrEmpty(stateString)) return "unknown";
            var s = stateString.ToLowerInvariant();
            if (s.Contains("deployed")) return "extended";
            if (s.Contains("retracted")) return "retracted";
            if (s.Contains("deploying")) return "deploying";
            if (s.Contains("retracting")) return "retracting";
            if (s.Contains("broken")) return "broken";
            return s;
        }
    }
}
