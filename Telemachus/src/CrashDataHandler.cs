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

                // Same vessel within window → append to existing snapshot.
                if (_lastCrash != null
                    && _lastCrashVesselId == vesselId
                    && ut - _lastCrashUT < CoalesceWindowSeconds)
                {
                    AppendPart(_lastCrash, origin, report);
                    _lastCrashUT = ut;
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
            "Most recent crash snapshot. Fields: vesselName, vesselId, " +
            "body, situation, latitude, longitude, altitude, ut, what " +
            "(what was hit), msg, eventKind (Crash/CrashSplashdown), " +
            "partsLost (list of {partName, partTitle, partId, msg}), " +
            "crewAboard (names), kerbalsKilled (names). Per-vessel events " +
            "within 5 seconds coalesce into one snapshot; later vessels " +
            "or later windows start fresh. Persists across scene changes; " +
            "cleared on KSP restart.",
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
                _subscribed = true;
                Debug.Log("[Telemachus] CrashDataSubscriber: subscribed to onCrash/onCrashSplashdown/onCrewKilled");
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
    }
}
