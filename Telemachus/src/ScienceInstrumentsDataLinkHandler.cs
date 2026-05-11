using System;
using System.Collections.Generic;

namespace Telemachus
{
    /// <summary>Per-instrument ModuleScienceExperiment state + deploy/transmit/dump/reset verbs. Keys are per-vessel.</summary>
    public class ScienceInstrumentsDataLinkHandler : DataLinkHandler
    {
        public ScienceInstrumentsDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        private static double R4(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return v;
            return Math.Round(v, 4);
        }

        // SubjectID shape `<expId>@<body><situation><biome>` has no separator — segment by recognising stock situation tokens.
        private static readonly string[] KnownSituations = {
            "InSpaceLow", "InSpaceHigh",
            "FlyingLow", "FlyingHigh",
            "SrfLanded", "SrfSplashed",
        };

        private static void ParseSubjectId(string subjectId,
            out string situation, out string biome)
        {
            situation = string.Empty;
            biome = string.Empty;
            if (string.IsNullOrEmpty(subjectId)) return;
            var atIdx = subjectId.IndexOf('@');
            if (atIdx < 0 || atIdx >= subjectId.Length - 1) return;

            var tail = subjectId.Substring(atIdx + 1);
            foreach (var sit in KnownSituations)
            {
                var sitIdx = tail.IndexOf(sit, StringComparison.Ordinal);
                if (sitIdx <= 0) continue;
                situation = sit;
                if (sitIdx + sit.Length < tail.Length)
                    biome = tail.Substring(sitIdx + sit.Length);
                return;
            }
        }

        [TelemetryAPI("sci.instruments",
            "Per-instrument state on the active vessel (partId, partTitle, expId, deployed, hasData, rerunnable, inoperable)",
            Plotable = false,
            Category = "science",
            ReturnType = "object")]
        object Instruments(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            var vessel = ds.vessel;
            if (vessel == null || vessel.parts == null) return result;

            foreach (var part in vessel.parts)
            {
                if (part == null || part.Modules == null) continue;
                foreach (var module in part.Modules)
                {
                    if (!(module is ModuleScienceExperiment exp)) continue;
                    var data = exp.GetData();
                    var hasData = data != null && data.Length > 0;

                    result.Add(new Dictionary<string, object>
                    {
                        ["partId"] = part.flightID,
                        ["partTitle"] = part.partInfo != null
                            ? part.partInfo.title
                            : part.name,
                        ["expId"] = exp.experimentID ?? string.Empty,
                        ["deployed"] = exp.Deployed,
                        ["hasData"] = hasData,
                        ["rerunnable"] = exp.rerunnable,
                        ["inoperable"] = exp.Inoperable,
                    });
                }
            }
            return result;
        }

        [TelemetryAPI("sci.experimentBreakdown",
            "Per-stored-data detail on the active vessel — subjectId / biome / situation / mits / transmit values / remaining potential",
            Plotable = false,
            Category = "science",
            ReturnType = "object")]
        object ExperimentBreakdown(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            var vessel = ds.vessel;
            if (vessel == null || vessel.parts == null) return result;

            foreach (var part in vessel.parts)
            {
                if (part == null || part.Modules == null) continue;
                foreach (var module in part.Modules)
                {
                    if (!(module is IScienceDataContainer container)) continue;
                    var data = container.GetData();
                    if (data == null) continue;

                    foreach (var d in data)
                    {
                        if (d == null) continue;
                        var entry = new Dictionary<string, object>
                        {
                            ["subjectId"] = d.subjectID ?? string.Empty,
                            ["expTitle"] = d.title ?? string.Empty,
                            ["dataMits"] = R4(d.dataAmount),
                            ["baseTransmitValue"] = R4(d.baseTransmitValue),
                            ["transmitBonus"] = R4(d.transmitBonus),
                        };

                        string biome = string.Empty;
                        string situation = string.Empty;
                        float subjectScience = 0f;
                        float subjectCap = 0f;

                        if (ResearchAndDevelopment.Instance != null &&
                            !string.IsNullOrEmpty(d.subjectID))
                        {
                            var subject = ResearchAndDevelopment
                                .GetSubjectByID(d.subjectID);
                            if (subject != null)
                            {
                                subjectScience = subject.science;
                                subjectCap = subject.scienceCap;
                            }
                            ParseSubjectId(d.subjectID, out situation, out biome);
                        }

                        entry["biome"] = biome;
                        entry["situation"] = situation;
                        entry["subjectScience"] = R4(subjectScience);
                        entry["subjectScienceCap"] = R4(subjectCap);
                        entry["remainingPotential"] =
                            R4(Math.Max(0f, subjectCap - subjectScience));
                        result.Add(entry);
                    }
                }
            }
            return result;
        }

        [TelemetryAPI("sci.canTransmitTotal",
            "Total dataAmount across all stored ScienceData on the active vessel",
            Category = "science",
            ReturnType = "double")]
        object CanTransmitTotal(DataSources ds) => DataAmountTotal(ds.vessel);

        [TelemetryAPI("sci.canRecoverTotal",
            "Total dataAmount across all stored ScienceData on the active vessel (alias of canTransmitTotal — separate key in case Phase 4 formulas diverge)",
            Category = "science",
            ReturnType = "double")]
        object CanRecoverTotal(DataSources ds) => DataAmountTotal(ds.vessel);

        private static double DataAmountTotal(Vessel vessel)
        {
            if (vessel == null || vessel.parts == null) return 0d;
            float total = 0f;
            foreach (var part in vessel.parts)
            {
                if (part == null || part.Modules == null) continue;
                foreach (var module in part.Modules)
                {
                    if (!(module is IScienceDataContainer container)) continue;
                    var data = container.GetData();
                    if (data == null) continue;
                    foreach (var d in data)
                    {
                        if (d != null) total += d.dataAmount;
                    }
                }
            }
            return R4(total);
        }

        private static ModuleScienceExperiment FindExperimentByPartId(
            Vessel vessel, IList<string> args)
        {
            if (vessel == null || vessel.parts == null) return null;
            if (args == null || args.Count == 0) return null;
            if (!uint.TryParse(args[0], out var partId)) return null;

            foreach (var part in vessel.parts)
            {
                if (part == null || part.flightID != partId) continue;
                foreach (var module in part.Modules)
                {
                    if (module is ModuleScienceExperiment exp) return exp;
                }
                return null;
            }
            return null;
        }

        [TelemetryAPI("sci.deploy",
            "Run an experiment by part flightID (no result dialog — direct data capture)",
            IsAction = true,
            Category = "science",
            ReturnType = "object",
            Params = "uint partId")]
        object Deploy(DataSources ds)
        {
            var exp = FindExperimentByPartId(ds.vessel, ds.args);
            if (exp == null) return "instrument not found";
            if (exp.Inoperable) return "instrument inoperable";
            if (exp.Deployed) return 0; // already deployed — idempotent

            // Reflect into private gatherData(showDialog: false) — DeployExperiment() goes through the dialog branch. Fall back to DeployExperiment() on KSP version drift.
            try
            {
                var method = typeof(ModuleScienceExperiment).GetMethod(
                    "gatherData",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (method == null)
                {
                    exp.DeployExperiment();
                    return 0;
                }
                var coroutine = method.Invoke(exp, new object[] { false })
                    as System.Collections.IEnumerator;
                if (coroutine != null)
                {
                    exp.StartCoroutine(coroutine);
                }
                else
                {
                    exp.DeployExperiment();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(
                    "[Telemachus] sci.deploy non-dialog path failed: " + ex);
                try { exp.DeployExperiment(); } catch { /* best-effort */ }
            }
            return 0;
        }

        [TelemetryAPI("sci.transmit",
            "Transmit stored data on the part via the active vessel's best transmitter",
            IsAction = true,
            Category = "science",
            ReturnType = "object",
            Params = "uint partId")]
        object Transmit(DataSources ds)
        {
            var exp = FindExperimentByPartId(ds.vessel, ds.args);
            if (exp == null) return "instrument not found";

            var data = exp.GetData();
            if (data == null || data.Length == 0) return "no data to transmit";

            var transmitter = ScienceUtil.GetBestTransmitter(ds.vessel);
            if (transmitter == null) return "no transmitter available";

            var list = new List<ScienceData>(data);
            transmitter.TransmitData(list);
            foreach (var d in data)
            {
                if (d != null) exp.DumpData(d);
            }
            return 0;
        }

        [TelemetryAPI("sci.dump",
            "Discard all stored data on the part without transmitting",
            IsAction = true,
            Category = "science",
            ReturnType = "object",
            Params = "uint partId")]
        object Dump(DataSources ds)
        {
            var exp = FindExperimentByPartId(ds.vessel, ds.args);
            if (exp == null) return "instrument not found";

            var data = exp.GetData();
            if (data == null || data.Length == 0) return 0;

            foreach (var d in data)
            {
                if (d != null) exp.DumpData(d);
            }
            return 0;
        }

        [TelemetryAPI("sci.reset",
            "Reset experiment — clears Deployed + drops data, makes rerunnable instruments ready to run again",
            IsAction = true,
            Category = "science",
            ReturnType = "object",
            Params = "uint partId")]
        object Reset(DataSources ds)
        {
            var exp = FindExperimentByPartId(ds.vessel, ds.args);
            if (exp == null) return "instrument not found";
            exp.ResetExperiment();
            return 0;
        }
    }
}
