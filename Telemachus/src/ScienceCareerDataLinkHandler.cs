using System;
using System.Collections.Generic;

namespace Telemachus
{
    /// <summary>
    /// Exposes science experiment data, career mode currencies (funds/reputation),
    /// and CommNet connectivity status. All from stock KSP APIs.
    /// </summary>
    public class ScienceCareerDataLinkHandler : DataLinkHandler
    {
        public ScienceCareerDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // --- Science ---

        [TelemetryAPI("sci.count", "Number of Science Experiments Aboard", Category = "science", ReturnType = "int")]
        object ScienceCount(DataSources ds)
        {
            int count = 0;
            foreach (var part in ds.vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (module is ModuleScienceExperiment)
                        count++;
                    if (module is ModuleScienceContainer container)
                        count += container.GetScienceCount();
                }
            }
            return count;
        }

        [TelemetryAPI("sci.dataAmount", "Total Science Data Aboard", Category = "science", ReturnType = "double")]
        object ScienceDataAmount(DataSources ds)
        {
            double total = 0;
            foreach (var part in ds.vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (module is IScienceDataContainer container)
                    {
                        var data = container.GetData();
                        if (data != null)
                        {
                            foreach (var d in data)
                                total += d.dataAmount;
                        }
                    }
                }
            }
            return total;
        }

        [TelemetryAPI("sci.experiments", "Experiments With Data", Plotable = false, Category = "science", ReturnType = "object")]
        object ScienceExperiments(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            foreach (var part in ds.vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (module is IScienceDataContainer container)
                    {
                        var data = container.GetData();
                        if (data == null || data.Length == 0) continue;

                        foreach (var d in data)
                        {
                            result.Add(new Dictionary<string, object>
                            {
                                ["part"] = part.partInfo.title,
                                ["title"] = d.title,
                                ["dataAmount"] = d.dataAmount,
                                ["scienceValueBase"] = d.baseTransmitValue,
                                ["transmitBoost"] = d.transmitBonus,
                                ["subjectId"] = d.subjectID
                            });
                        }
                    }
                }
            }
            return result;
        }

        // --- Career ---

        [TelemetryAPI("career.funds", "Available Funds", AlwaysEvaluable = true, Category = "career", ReturnType = "double")]
        object Funds(DataSources ds) =>
            Funding.Instance != null ? Funding.Instance.Funds : 0d;

        [TelemetryAPI("career.reputation", "Current Reputation", AlwaysEvaluable = true, Category = "career", ReturnType = "double")]
        object Rep(DataSources ds) => Reputation.CurrentRep;

        [TelemetryAPI("career.science", "Available Science Points", AlwaysEvaluable = true, Category = "career", ReturnType = "double")]
        object SciencePoints(DataSources ds) =>
            ResearchAndDevelopment.Instance != null ? ResearchAndDevelopment.Instance.Science : 0f;

        [TelemetryAPI("career.mode", "Game Mode (CAREER/SCIENCE/SANDBOX)", Units = APIEntry.UnitType.STRING, AlwaysEvaluable = true, Category = "career", ReturnType = "string")]
        object GameMode(DataSources ds) => HighLogic.CurrentGame?.Mode.ToString() ?? "";

        // --- CommNet ---

        [TelemetryAPI("comm.connected", "CommNet Is Connected", Category = "comms", ReturnType = "bool")]
        object CommConnected(DataSources ds) =>
            ds.vessel.Connection != null && ds.vessel.Connection.IsConnected;

        [TelemetryAPI("comm.signalStrength", "CommNet Signal Strength (0-1)", Category = "comms", ReturnType = "double")]
        object CommSignalStrength(DataSources ds) =>
            ds.vessel.Connection != null ? ds.vessel.Connection.SignalStrength : 0d;

        [TelemetryAPI("comm.controlState", "CommNet Control State (0=none, 1=partial, 2=full)", Category = "comms", ReturnType = "int")]
        object CommControlState(DataSources ds)
        {
            if (ds.vessel.Connection == null) return 0;
            // Cast the enum to int — values vary by KSP version, so use string matching
            string state = ds.vessel.Connection.ControlState.ToString();
            if (state.Contains("Full") || state == "Probe") return 2;
            if (state.Contains("Partial")) return 1;
            return 0;
        }

        [TelemetryAPI("comm.controlStateName", "CommNet Control State Name", Units = APIEntry.UnitType.STRING, Category = "comms", ReturnType = "string")]
        object CommControlStateName(DataSources ds) =>
            ds.vessel.Connection?.ControlState.ToString() ?? "None";

        [TelemetryAPI("comm.signalDelay", "CommNet Signal Delay (seconds)", Units = APIEntry.UnitType.TIME, Category = "comms", ReturnType = "double")]
        object CommSignalDelay(DataSources ds) =>
            ds.vessel.Connection != null ? ds.vessel.Connection.SignalDelay : 0d;

        protected override int pausedHandler() => 0;
    }
}
