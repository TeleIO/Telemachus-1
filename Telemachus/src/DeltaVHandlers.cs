using System;
using System.Collections.Generic;

namespace Telemachus
{
    public class DeltaVDataLinkHandler : DataLinkHandler
    {
        public DeltaVDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // --- Totals ---

        [TelemetryAPI("dv.ready", "Delta-V Calculator Is Ready", Category = "deltav", ReturnType = "double")]
        object Ready(DataSources ds) => ds.vessel.VesselDeltaV?.IsReady ?? false;

        [TelemetryAPI("dv.totalDVVac", "Total Delta-V (Vacuum)", Units = APIEntry.UnitType.VELOCITY, Category = "deltav", ReturnType = "double")]
        object TotalDVVac(DataSources ds) => ds.vessel.VesselDeltaV?.TotalDeltaVVac ?? 0;

        [TelemetryAPI("dv.totalDVASL", "Total Delta-V (Sea Level)", Units = APIEntry.UnitType.VELOCITY, Category = "deltav", ReturnType = "double")]
        object TotalDVASL(DataSources ds) => ds.vessel.VesselDeltaV?.TotalDeltaVASL ?? 0;

        [TelemetryAPI("dv.totalDVActual", "Total Delta-V (Current Atmosphere)", Units = APIEntry.UnitType.VELOCITY, Category = "deltav", ReturnType = "double")]
        object TotalDVActual(DataSources ds) => ds.vessel.VesselDeltaV?.TotalDeltaVActual ?? 0;

        [TelemetryAPI("dv.totalBurnTime", "Total Burn Time", Units = APIEntry.UnitType.TIME, Category = "deltav", ReturnType = "double")]
        object TotalBurnTime(DataSources ds) => ds.vessel.VesselDeltaV?.TotalBurnTime ?? 0;

        [TelemetryAPI("dv.stageCount", "Number of Stages with Delta-V Info", Category = "deltav", ReturnType = "int")]
        object StageCount(DataSources ds) => ds.vessel.VesselDeltaV?.OperatingStageInfo?.Count ?? 0;

        // --- Full stage objects (formatted) ---

        [TelemetryAPI("dv.stage", "Full Stage Info", Plotable = false, Formatter = "DeltaVStageInfo", Category = "deltav", ReturnType = "object", Params = "int stage")]
        object Stage(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]));

        [TelemetryAPI("dv.stages", "All Stages Info", Plotable = false, Formatter = "DeltaVStageInfoList", Category = "deltav", ReturnType = "object")]
        object Stages(DataSources ds) => ds.vessel.VesselDeltaV?.OperatingStageInfo;

        // --- Per-stage scalar queries ---

        [TelemetryAPI("dv.stageDVVac", "Stage Delta-V Vacuum", Units = APIEntry.UnitType.VELOCITY, Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageDVVac(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.deltaVinVac ?? 0;

        [TelemetryAPI("dv.stageDVASL", "Stage Delta-V ASL", Units = APIEntry.UnitType.VELOCITY, Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageDVASL(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.deltaVatASL ?? 0;

        [TelemetryAPI("dv.stageDVActual", "Stage Delta-V Actual", Units = APIEntry.UnitType.VELOCITY, Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageDVActual(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.deltaVActual ?? 0;

        [TelemetryAPI("dv.stageTWRVac", "Stage TWR Vacuum", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageTWRVac(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.TWRVac ?? 0;

        [TelemetryAPI("dv.stageTWRASL", "Stage TWR ASL", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageTWRASL(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.TWRASL ?? 0;

        [TelemetryAPI("dv.stageTWRActual", "Stage TWR Actual", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageTWRActual(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.TWRActual ?? 0;

        [TelemetryAPI("dv.stageISPVac", "Stage ISP Vacuum", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageISPVac(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.ispVac ?? 0;

        [TelemetryAPI("dv.stageISPASL", "Stage ISP ASL", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageISPASL(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.ispASL ?? 0;

        [TelemetryAPI("dv.stageISPActual", "Stage ISP Actual", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageISPActual(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.ispActual ?? 0;

        [TelemetryAPI("dv.stageThrustVac", "Stage Thrust Vacuum", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageThrustVac(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.thrustVac ?? 0;

        [TelemetryAPI("dv.stageThrustASL", "Stage Thrust ASL", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageThrustASL(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.thrustASL ?? 0;

        [TelemetryAPI("dv.stageThrustActual", "Stage Thrust Actual", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageThrustActual(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.thrustActual ?? 0;

        [TelemetryAPI("dv.stageBurnTime", "Stage Burn Time", Units = APIEntry.UnitType.TIME, Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageBurnTime(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.stageBurnTime ?? 0;

        [TelemetryAPI("dv.stageMass", "Stage Total Mass", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageMass(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.stageMass ?? 0;

        [TelemetryAPI("dv.stageDryMass", "Stage Dry Mass", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageDryMass(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.dryMass ?? 0;

        [TelemetryAPI("dv.stageFuelMass", "Stage Fuel Mass", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageFuelMass(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.fuelMass ?? 0;

        [TelemetryAPI("dv.stageStartMass", "Stage Start Mass", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageStartMass(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.startMass ?? 0;

        [TelemetryAPI("dv.stageEndMass", "Stage End Mass", Category = "deltav", ReturnType = "double", Params = "int stage")]
        object StageEndMass(DataSources ds) => GetStageInfo(ds, int.Parse(ds.args[0]))?.endMass ?? 0;

        private DeltaVStageInfo GetStageInfo(DataSources ds, int stage)
        {
            var info = ds.vessel.VesselDeltaV?.OperatingStageInfo;
            if (info == null) return null;
            for (int i = 0; i < info.Count; i++)
                if (info[i].stage == stage)
                    return info[i];
            return null;
        }
    }
}
