using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telemachus
{
    /// <summary>Vessel-crash snapshot — coalesces per-part onCrash events within a 5s window and buffers onCrewKilled (which fires before onCrash, so kills must be staged).</summary>
    public class CrashDataHandler : DataLinkHandler
    {
        private const double CoalesceWindowSeconds = 5.0;

        private static Dictionary<string, object> _lastCrash;
        private static double _lastCrashUT;
        private static string _lastCrashVesselId;

        // Wall-clock (realtimeSinceStartup) of the last capture. Used ONLY to
        // dedup the onCrash→onVesselWillDestroy pair of a single collision crash
        // (they fire in the same frame). Game-UT can't be used for this: a
        // revert resets it and reuses the vesselId, which made a separate later
        // burn-up look "already captured". Wall-clock doesn't reset on revert.
        private static float _lastCaptureRealtime = -999f;

        // Suppression for the onVesselWillDestroy detector — set while a benign
        // destruction is in progress (revert / recovery / scene change). All
        // cleared when the next scene finishes loading, so a stuck flag can
        // never make us miss a later real crash.
        private static bool _reverting;
        private static bool _recovering;
        private static bool _sceneChanging;

        // onCrewKilled fires before onCrash — buffer kill names so they don't get lost when the snapshot is built.
        private static readonly List<string> _pendingKerbalsKilled = new List<string>();

        // Pre-tracked because KSP flips dead kerbals to RosterStatus.Dead before onCrash, and GetVesselCrew() filters Dead out.
        private static List<string> _lastKnownCrew = new List<string>();
        private static string _lastKnownCrewVesselId;

        public CrashDataHandler(FormatterProvider formatters)
            : base(formatters) { }

        private static double R4(double v) => Math.Round(v, 4);
        private static double R4(float v) => R4((double)v);

        internal static void OnCrash(EventReport report) => CaptureCrash(report, "Crash");
        internal static void OnCrashSplashdown(EventReport report) => CaptureCrash(report, "CrashSplashdown");

        // ── Vessel-destroyed detector ───────────────────────────────────────
        // Collision crashes fire onCrash/onCrashSplashdown (handled above).
        // Non-collision deaths — re-entry burn-up, structural/aero failure —
        // fire NEITHER, but every fatal outcome fires onVesselWillDestroy. We use
        // it as the universal "the mission ended by mishap" signal, scoped to the
        // active vessel and gated against benign destroys (revert / recovery /
        // scene change). A collision crash already captured by onCrash in the
        // same frame is deduped, so it is not double-recorded.
        internal static void OnVesselWillDestroy(Vessel v)
        {
            try
            {
                if (!IsRecordableMishap(v)) return;
                CaptureVesselDestroyed(v);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Telemachus] vessel-destroy capture failed: " + e.Message);
            }
        }

        private static bool IsRecordableMishap(Vessel v)
        {
            if (v == null || v != FlightGlobals.ActiveVessel) return false;
            if (_reverting || _recovering || _sceneChanging) return false;
            if (v.situation == Vessel.Situations.PRELAUNCH) return false;
            var vType = v.vesselType;
            if (vType == VesselType.Debris || vType == VesselType.Flag || vType == VesselType.Unknown)
                return false;
            // Dedup the same collision crash onCrash just captured — those fire in
            // the same frame, so use WALL-CLOCK, not game-UT (a revert resets game-
            // UT while reusing the vesselId, which would falsely match a separate
            // later crash). A burn-up minutes later won't fall in this window.
            if (_lastCrash != null && _lastCrashVesselId == v.id.ToString()
                && Time.realtimeSinceStartup - _lastCaptureRealtime < 2f)
                return false;
            return true;
        }

        private static void CaptureVesselDestroyed(Vessel v)
        {
            double ut = Planetarium.GetUniversalTime();
            var partsLost = new List<Dictionary<string, object>>();
            if (v.parts != null)
            {
                foreach (var p in v.parts)
                {
                    if (p == null) continue;
                    partsLost.Add(new Dictionary<string, object>
                    {
                        ["partName"] = p.partInfo?.name ?? p.name ?? string.Empty,
                        ["partTitle"] = p.partInfo?.title ?? string.Empty,
                        ["partId"] = p.flightID,
                        ["msg"] = string.Empty,
                    });
                }
            }
            var crewAboard = _lastKnownCrew != null && _lastKnownCrew.Count > 0
                ? new List<string>(_lastKnownCrew)
                : ListCrewAboard(v);
            var kerbalsKilled = new List<string>(_pendingKerbalsKilled);
            _pendingKerbalsKilled.Clear();
            var snap = new Dictionary<string, object>
            {
                // "Destroyed" = non-collision loss (burn-up / structural). The
                // banner shows VESSEL DESTROYED for any eventKind; this just
                // distinguishes it from terrain (Crash) / water (CrashSplashdown).
                ["eventKind"] = "Destroyed",
                ["vesselName"] = v.vesselName ?? string.Empty,
                ["vesselType"] = v.vesselType.ToString(),
                ["vesselId"] = v.id.ToString(),
                ["body"] = v.mainBody?.bodyName ?? string.Empty,
                ["situation"] = v.situation.ToString(),
                ["latitude"] = R4(v.latitude),
                ["longitude"] = R4(v.longitude),
                ["altitude"] = R4(v.altitude),
                ["ut"] = R4(ut),
                ["what"] = string.Empty,
                ["msg"] = string.Empty,
                ["partsLost"] = partsLost,
                ["crewAboard"] = crewAboard,
                ["kerbalsKilled"] = kerbalsKilled,
                ["events"] = FlightLoggerSnapshot.CaptureEvents(),
                ["flightStats"] = FlightLoggerSnapshot.Capture(),
            };
            _lastCrash = snap;
            _lastCrashUT = ut;
            _lastCrashVesselId = v.id.ToString();
            _lastCaptureRealtime = Time.realtimeSinceStartup;
        }

        // Suppression bookkeeping — benign destroys must not register as crashes.
        internal static void NoteRevert() => _reverting = true;
        internal static void NoteRecoveryRequested() => _recovering = true;
        internal static void NoteVesselRecovered() => _recovering = false;
        internal static void NoteSceneLoad() => _sceneChanging = true;
        internal static void NoteLevelLoaded()
        {
            _reverting = false;
            _recovering = false;
            _sceneChanging = false;
        }

        internal static void OnCrewKilled(EventReport report)
        {
            try
            {
                var name = report?.sender ?? string.Empty;
                if (string.IsNullOrEmpty(name)) return;
                if (!_pendingKerbalsKilled.Contains(name)) _pendingKerbalsKilled.Add(name);
                if (_lastCrash != null
                    && _lastCrash.TryGetValue("kerbalsKilled", out var raw)
                    && raw is List<string> list
                    && !list.Contains(name))
                {
                    list.Add(name);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Telemachus] crash crew capture failed: " + e.Message);
            }
        }

        /// <summary>Sampled by the addon every ~0.5s — pre-crash crew, kept fresh because KSP wipes the live list before onCrash fires.</summary>
        internal static void RefreshLiveCrew(string vesselId, List<string> crew)
        {
            if (vesselId == null) return;
            // Non-empty wins on same vessel: a mid-crash wipe must not overwrite the last good sample.
            if (vesselId != _lastKnownCrewVesselId)
            {
                _lastKnownCrewVesselId = vesselId;
                _lastKnownCrew = new List<string>(crew);
                return;
            }
            if (crew.Count > 0)
            {
                _lastKnownCrew = new List<string>(crew);
            }
        }

        private static void CaptureCrash(EventReport report, string eventKind)
        {
            if (report == null) return;
            try
            {
                var origin = report.origin;
                var vessel = origin?.vessel;
                var vesselId = vessel?.id.ToString() ?? string.Empty;
                var ut = Planetarium.GetUniversalTime();

                // crash.lastCrash is a single-slot "last notable crash" record,
                // not a raw impact feed. Spent debris (decoupled boosters/
                // stages), planted flags, and unclassified objects aren't a
                // vessel the operator is flying — recording them would clobber
                // the slot and lose the real crash. Drop them at the source so
                // every consumer (banner, flight-history annotation, reconnect
                // replay) sees the right value. SpaceObject/asteroids are
                // pilotable, so they stay.
                var vType = vessel?.vesselType;
                if (vType == VesselType.Debris
                    || vType == VesselType.Flag
                    || vType == VesselType.Unknown)
                {
                    return;
                }

                // Same vessel within window → append to existing snapshot.
                if (_lastCrash != null
                    && _lastCrashVesselId == vesselId
                    && ut - _lastCrashUT < CoalesceWindowSeconds)
                {
                    AppendPart(_lastCrash, origin, report);
                    _lastCrashUT = ut;
                    _lastCaptureRealtime = Time.realtimeSinceStartup;
                    return;
                }

                var crewAboard = _lastKnownCrew != null && _lastKnownCrew.Count > 0
                    ? new List<string>(_lastKnownCrew)
                    : ListCrewAboard(vessel);
                var kerbalsKilled = new List<string>(_pendingKerbalsKilled);
                _pendingKerbalsKilled.Clear();
                var snap = new Dictionary<string, object>
                {
                    ["eventKind"] = eventKind,
                    ["vesselName"] = vessel?.vesselName ?? report.sender ?? string.Empty,
                    ["vesselType"] = vessel?.vesselType.ToString() ?? string.Empty,
                    ["vesselId"] = vesselId,
                    ["body"] = vessel?.mainBody?.bodyName ?? string.Empty,
                    ["situation"] = vessel?.situation.ToString() ?? string.Empty,
                    ["latitude"] = R4(vessel?.latitude ?? 0.0),
                    ["longitude"] = R4(vessel?.longitude ?? 0.0),
                    ["altitude"] = R4(vessel?.altitude ?? 0.0),
                    ["ut"] = R4(ut),
                    ["what"] = report.other ?? string.Empty,
                    ["msg"] = report.msg ?? string.Empty,
                    ["partsLost"] = new List<Dictionary<string, object>>(),
                    ["crewAboard"] = crewAboard,
                    ["kerbalsKilled"] = kerbalsKilled,
                    ["events"] = FlightLoggerSnapshot.CaptureEvents(),
                    ["flightStats"] = FlightLoggerSnapshot.Capture(),
                };
                AppendPart(snap, origin, report);
                _lastCrash = snap;
                _lastCrashUT = ut;
                _lastCrashVesselId = vesselId;
                _lastCaptureRealtime = Time.realtimeSinceStartup;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Telemachus] crash capture failed: " + e.Message);
            }
        }

        private static void AppendPart(Dictionary<string, object> snap, Part origin, EventReport report)
        {
            if (snap == null || origin == null) return;
            if (!snap.TryGetValue("partsLost", out var raw)) return;
            if (raw is not List<Dictionary<string, object>> list) return;
            list.Add(new Dictionary<string, object>
            {
                ["partName"] = origin.partInfo?.name ?? origin.name ?? string.Empty,
                ["partTitle"] = origin.partInfo?.title ?? string.Empty,
                ["partId"] = origin.flightID,
                ["msg"] = report.msg ?? string.Empty,
            });
        }

        private static List<string> ListCrewAboard(Vessel vessel)
        {
            var result = new List<string>();
            if (vessel == null) return result;
            try
            {
                var crew = vessel.GetVesselCrew();
                if (crew == null) return result;
                foreach (var member in crew)
                {
                    if (member?.name != null) result.Add(member.name);
                }
            }
            catch (Exception)
            {
                // Protected vessels in odd states can throw — treat as no crew.
            }
            return result;
        }

        [TelemetryAPI("crash.hasRecent",
            "Whether a crash snapshot has been captured this session.",
            AlwaysEvaluable = true,
            Category = "crash",
            ReturnType = "bool")]
        object HasRecent(DataSources ds) => _lastCrash != null;

        [TelemetryAPI("crash.lastCrash",
            "Most recent notable-vessel crash snapshot. Captures terrain " +
            "(eventKind Crash), water (CrashSplashdown), and non-collision " +
            "losses such as re-entry burn-up or structural break-up " +
            "(Destroyed). Debris, flags, and unclassified vessels are " +
            "excluded so they don't overwrite the last real crash. Fields: " +
            "vesselName, vesselType (KSP VesselType e.g. Ship/Probe/" +
            "SpaceObject), vesselId, body, situation, latitude, longitude, " +
            "altitude, ut, what (what was hit; empty for Destroyed), msg, " +
            "eventKind (Crash/CrashSplashdown/Destroyed), partsLost (list " +
            "of {partName, partTitle, partId, msg}), crewAboard (names), " +
            "kerbalsKilled (names), events (flight log), flightStats. " +
            "Per-vessel collision events within 5 seconds coalesce into one " +
            "snapshot. Persists across scene changes; cleared on KSP restart.",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "crash",
            ReturnType = "object")]
        object LastCrash(DataSources ds) => _lastCrash;
    }

    /// <summary>Deferred subscriber — instance handlers required because EvtDelegate ctor reads Target.GetType().</summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class CrashDataSubscriber : MonoBehaviour
    {
        private static bool _subscribed;

        private float _crewSampleAccumulator;
        private const float CrewSampleIntervalSeconds = 0.5f;
        private readonly List<string> _crewScratch = new List<string>();

        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (_subscribed) return;
            try
            {
                GameEvents.onCrash.Add(HandleCrash);
                GameEvents.onCrashSplashdown.Add(HandleCrashSplashdown);
                GameEvents.onCrewKilled.Add(HandleCrewKilled);

                // Universal mishap signal for non-collision deaths (burn-up,
                // structural) that fire no onCrash. Gated against benign
                // destroys by the revert/recovery/scene-load subscriptions.
                GameEvents.onVesselWillDestroy.Add(HandleVesselWillDestroy);
                GameEvents.OnRevertToLaunchFlightState.Add(HandleRevertLaunch);
                GameEvents.OnRevertToPrelaunchFlightState.Add(HandleRevertPrelaunch);
                GameEvents.OnVesselRecoveryRequested.Add(HandleRecoveryRequested);
                GameEvents.onVesselRecovered.Add(HandleVesselRecovered);
                GameEvents.onGameSceneLoadRequested.Add(HandleSceneLoadRequested);
                GameEvents.onLevelWasLoaded.Add(HandleLevelLoaded);

                _subscribed = true;
                Debug.Log("[Telemachus] CrashDataSubscriber: subscribed to onCrash/onCrashSplashdown/onCrewKilled + onVesselWillDestroy (+ suppression)");
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    "[Telemachus] CrashDataSubscriber: subscription failed; " +
                    "crash.lastCrash will stay empty this session: " + e.Message);
            }
        }

        private void Update()
        {
            _crewSampleAccumulator += Time.unscaledDeltaTime;
            if (_crewSampleAccumulator < CrewSampleIntervalSeconds) return;
            _crewSampleAccumulator = 0f;
            try
            {
                var vessel = FlightGlobals.ActiveVessel;
                if (vessel == null) return;
                _crewScratch.Clear();
                var crew = vessel.GetVesselCrew();
                if (crew != null)
                {
                    foreach (var member in crew)
                    {
                        if (member?.name != null) _crewScratch.Add(member.name);
                    }
                }
                CrashDataHandler.RefreshLiveCrew(vessel.id.ToString(), _crewScratch);
            }
            catch (Exception)
            {
                // Scene transitions can throw inside GetVesselCrew() — keep the last-known sample.
            }
        }

        private void HandleCrash(EventReport r) => CrashDataHandler.OnCrash(r);
        private void HandleCrashSplashdown(EventReport r) => CrashDataHandler.OnCrashSplashdown(r);
        private void HandleCrewKilled(EventReport r) => CrashDataHandler.OnCrewKilled(r);

        // Instance handlers (EvtDelegate needs a real Target.GetType()).
        private void HandleVesselWillDestroy(Vessel v) => CrashDataHandler.OnVesselWillDestroy(v);
        private void HandleRevertLaunch(FlightState s) => CrashDataHandler.NoteRevert();
        private void HandleRevertPrelaunch(FlightState s) => CrashDataHandler.NoteRevert();
        private void HandleRecoveryRequested(Vessel v) => CrashDataHandler.NoteRecoveryRequested();
        private void HandleVesselRecovered(ProtoVessel pv, bool quick) => CrashDataHandler.NoteVesselRecovered();
        private void HandleSceneLoadRequested(GameScenes s) => CrashDataHandler.NoteSceneLoad();
        private void HandleLevelLoaded(GameScenes s) => CrashDataHandler.NoteLevelLoaded();
    }
}
