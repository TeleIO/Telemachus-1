using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using KSP.UI.Screens;
using UnityEngine;

namespace Telemachus
{
    /// <summary>Mission-summary snapshot captured via GameEvents — subscription deferred to RecoveryDialogSubscriber because subscribing from KSPAPI..ctor throws NRE in EvtDelegate.</summary>
    public class RecoveryDialogHandler : DataLinkHandler
    {
        private static FieldInfo _scienceField;
        private static FieldInfo _partField;
        private static FieldInfo _resourceField;
        private static FieldInfo _crewField;
        private static bool _reflectionResolved;

        private static Dictionary<string, object> _lastSummary;

        // Stashed by onVesselRecovered for the 1-param fallback path (spawn doesn't pass ProtoVessel).
        private static string _pendingVesselName;

        public RecoveryDialogHandler(FormatterProvider formatters)
            : base(formatters) { }

        internal static void OnRecoveryProcessingComplete(
            ProtoVessel pv, MissionRecoveryDialog dialog, float progress)
        {
            // Progress fires throughout the tally animation; snapshot only at 100%.
            if (progress < 0.999f) return;
            if (dialog == null) return;
            _pendingVesselName = pv?.vesselName ?? string.Empty;
            CaptureSnapshot(dialog);
        }

        internal static void OnVesselRecovered(ProtoVessel pv, bool quick)
        {
            _pendingVesselName = pv?.vesselName ?? string.Empty;
        }

        internal static void OnDialogSpawn(MissionRecoveryDialog dialog)
        {
            CaptureSnapshot(dialog);
        }

        private static double R4(double v)
        {
            return Math.Round(v, 4);
        }

        private static double R4(float v) => R4((double)v);

        private static void EnsureReflection()
        {
            if (_reflectionResolved) return;
            var t = typeof(MissionRecoveryDialog);
            const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
            _scienceField = t.GetField("scienceWidgets", bf);
            _partField = t.GetField("partWidgets", bf);
            _resourceField = t.GetField("resourceWidgets", bf);
            _crewField = t.GetField("crewWidgets", bf);
            _reflectionResolved = true;
        }

        private static void CaptureSnapshot(MissionRecoveryDialog dialog)
        {
            if (dialog == null) return;

            try
            {
                EnsureReflection();
                var snap = new Dictionary<string, object>
                {
                    ["vesselName"] = _pendingVesselName ?? string.Empty,
                    ["recoveryLocation"] = dialog.recoveryLocation ?? string.Empty,
                    ["recoveryFactor"] = dialog.recoveryFactor ?? string.Empty,
                    ["scienceEarned"] = R4(dialog.scienceEarned),
                    ["beforeMissionScience"] = R4(dialog.beforeMissionScience),
                    ["totalScience"] = R4(dialog.totalScience),
                    ["scienceModifier"] = dialog.ScienceModifier ?? string.Empty,
                    ["fundsEarned"] = R4(dialog.fundsEarned),
                    ["beforeMissionFunds"] = R4(dialog.beforeMissionFunds),
                    ["totalFunds"] = R4(dialog.totalFunds),
                    ["fundsModifier"] = dialog.FundsModifier ?? string.Empty,
                    ["reputationEarned"] = R4(dialog.reputationEarned),
                    ["beforeMissionReputation"] = R4(dialog.beforeMissionReputation),
                    ["totalReputation"] = R4(dialog.totalReputation),
                    ["repModifier"] = dialog.RepModifier ?? string.Empty,
                    ["displayReputation"] = dialog.displayReputation,
                    ["scienceModeAvailable"] = dialog.ScienceModeAvailable,
                    ["partsModeAvailable"] = dialog.PartsModeAvailable,
                    ["crewModeAvailable"] = dialog.CrewModeAvailable,
                    ["capturedAtUT"] = Planetarium.GetUniversalTime(),
                    ["scienceBreakdown"] = ExtractScience(dialog),
                    ["partBreakdown"] = ExtractParts(dialog),
                    ["resourceBreakdown"] = ExtractResources(dialog),
                    ["crewBreakdown"] = ExtractCrew(dialog),
                    ["events"] = FlightLoggerSnapshot.CaptureEvents(),
                    ["flightStats"] = FlightLoggerSnapshot.Capture(),
                };
                _lastSummary = snap;
                _pendingVesselName = null;
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    "[Telemachus] recovery snapshot capture failed: " + e.Message);
            }
        }

        private static List<Dictionary<string, object>> ExtractScience(MissionRecoveryDialog dialog)
        {
            var result = new List<Dictionary<string, object>>();
            var raw = _scienceField?.GetValue(dialog) as IEnumerable;
            if (raw == null) return result;
            foreach (var item in raw)
            {
                if (item is not KSP.UI.Screens.SpaceCenter.MissionSummaryDialog.ScienceSubjectWidget w)
                    continue;
                result.Add(new Dictionary<string, object>
                {
                    ["subjectId"] = w.subject?.id ?? string.Empty,
                    ["subjectTitle"] = w.subject?.title ?? string.Empty,
                    ["dataGathered"] = R4(w.dataGathered),
                    ["scienceAmount"] = R4(w.scienceAmount),
                });
            }
            return result;
        }

        private static List<Dictionary<string, object>> ExtractParts(MissionRecoveryDialog dialog)
        {
            var result = new List<Dictionary<string, object>>();
            var raw = _partField?.GetValue(dialog) as IEnumerable;
            if (raw == null) return result;
            foreach (var item in raw)
            {
                if (item is not KSP.UI.Screens.SpaceCenter.MissionSummaryDialog.PartWidget w)
                    continue;
                result.Add(new Dictionary<string, object>
                {
                    ["partName"] = w.partInfo?.name ?? string.Empty,
                    ["partTitle"] = w.partInfo?.title ?? string.Empty,
                    ["count"] = w.count,
                    ["partValue"] = R4(w.partValue),
                    ["resourcesValue"] = R4(w.resourcesValue),
                    ["totalValue"] = R4(w.totalValue),
                });
            }
            return result;
        }

        private static List<Dictionary<string, object>> ExtractResources(MissionRecoveryDialog dialog)
        {
            var result = new List<Dictionary<string, object>>();
            var raw = _resourceField?.GetValue(dialog) as IEnumerable;
            if (raw == null) return result;
            foreach (var item in raw)
            {
                if (item is not KSP.UI.Screens.SpaceCenter.MissionSummaryDialog.ResourceWidget w)
                    continue;
                result.Add(new Dictionary<string, object>
                {
                    ["resourceName"] = w.rscDef?.name ?? string.Empty,
                    ["amount"] = R4(w.amount),
                    ["unitValue"] = R4(w.unitValue),
                    ["totalValue"] = R4(w.totalValue),
                });
            }
            return result;
        }

        private static List<Dictionary<string, object>> ExtractCrew(MissionRecoveryDialog dialog)
        {
            var result = new List<Dictionary<string, object>>();
            var raw = _crewField?.GetValue(dialog) as IEnumerable;
            if (raw == null) return result;
            foreach (var item in raw)
            {
                if (item is not KSP.UI.Screens.SpaceCenter.MissionSummaryDialog.CrewWidget w)
                    continue;
                result.Add(new Dictionary<string, object>
                {
                    ["name"] = w.crew?.name ?? string.Empty,
                    ["trait"] = w.crew?.trait ?? string.Empty,
                    ["isTourist"] = w.isTourist,
                    ["xpGained"] = R4(w.xpGained),
                    ["levelsGained"] = w.levelsGained,
                    ["newLevel"] = w.newLevel,
                });
            }
            return result;
        }

        [TelemetryAPI("recovery.hasRecent",
            "Whether a mission-summary snapshot has been captured this " +
            "session (true after the first vessel is recovered).",
            AlwaysEvaluable = true,
            Category = "recovery",
            ReturnType = "bool")]
        object HasRecent(DataSources ds) => _lastSummary != null;

        [TelemetryAPI("recovery.lastSummary",
            "Most recent mission-summary snapshot. Includes vessel name, " +
            "recovery location and factor, science/funds/reputation " +
            "totals + earned + modifiers, and per-experiment, per-part, " +
            "per-resource, per-crew breakdowns. Captures automatically " +
            "when KSP completes recovery processing (no dialog dismissal " +
            "needed). Persists across scene changes; cleared on KSP " +
            "restart.",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "recovery",
            ReturnType = "object")]
        object LastSummary(DataSources ds) => _lastSummary;
    }

    /// <summary>Deferred subscriber — handlers must be instance methods because EvtDelegate ctor reads Target.GetType() and rejects static delegates.</summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RecoveryDialogSubscriber : MonoBehaviour
    {
        private static bool _subscribed;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (_subscribed) return;

            try
            {
                GameEvents.onVesselRecoveryProcessingComplete.Add(HandleProcessingComplete);
                _subscribed = true;
                Debug.Log("[Telemachus] RecoveryDialogSubscriber: subscribed to onVesselRecoveryProcessingComplete");
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    "[Telemachus] RecoveryDialogSubscriber: 3-param event subscription failed, " +
                    "falling back to spawn+recovered: " + e.Message);
            }

            try
            {
                GameEvents.onVesselRecovered.Add(HandleVesselRecovered);
                GameEvents.onGUIRecoveryDialogSpawn.Add(HandleDialogSpawn);
                _subscribed = true;
                Debug.Log("[Telemachus] RecoveryDialogSubscriber: subscribed to spawn+recovered fallback");
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    "[Telemachus] RecoveryDialogSubscriber: fallback subscription failed; " +
                    "recovery.lastSummary will stay empty this session: " + e.Message);
            }
        }

        private void HandleProcessingComplete(ProtoVessel pv, MissionRecoveryDialog dialog, float progress)
        {
            RecoveryDialogHandler.OnRecoveryProcessingComplete(pv, dialog, progress);
        }

        private void HandleVesselRecovered(ProtoVessel pv, bool quick)
        {
            RecoveryDialogHandler.OnVesselRecovered(pv, quick);
        }

        private void HandleDialogSpawn(MissionRecoveryDialog dialog)
        {
            RecoveryDialogHandler.OnDialogSpawn(dialog);
        }
    }
}
