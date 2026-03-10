using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using KSP.UI.Screens.Flight;
using UnityEngine;

namespace Telemachus
{
    public class FlyByWireDataLinkHandler : DataLinkHandler
    {
        static readonly object fbwLock = new();
        static float yaw = 0, pitch = 0, roll = 0, x = 0, y = 0, z = 0;
        static int on_attitude = 0;

        public FlyByWireDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("v.setYaw", "Yaw", IsAction = true, Category = "fbw", ReturnType = "int", Params = "float yaw")]
        object SetYaw(DataSources ds)
        {
            lock (fbwLock) { yaw = checkFlightStateParameters(float.Parse(ds.args[0])); }
            return 0;
        }

        [TelemetryAPI("v.setPitch", "Pitch", IsAction = true, Category = "fbw", ReturnType = "int", Params = "float pitch")]
        object SetPitch(DataSources ds)
        {
            lock (fbwLock) { pitch = checkFlightStateParameters(float.Parse(ds.args[0])); }
            return 0;
        }

        [TelemetryAPI("v.setRoll", "Roll", IsAction = true, Category = "fbw", ReturnType = "int", Params = "float roll")]
        object SetRoll(DataSources ds)
        {
            lock (fbwLock) { roll = checkFlightStateParameters(float.Parse(ds.args[0])); }
            return 0;
        }

        [TelemetryAPI("v.setFbW", "Set Fly by Wire On or Off", IsAction = true, Category = "fbw", ReturnType = "int", Params = "int state")]
        object SetFbW(DataSources ds)
        {
            lock (fbwLock) { on_attitude = int.Parse(ds.args[0]); }
            return 0;
        }

        [TelemetryAPI("v.setPitchYawRollXYZ", "Set pitch, yaw, roll, X, Y and Z", IsAction = true, Category = "fbw", ReturnType = "int", Params = "float pitch, float yaw, float roll, float x, float y, float z")]
        object SetPitchYawRollXYZ(DataSources ds)
        {
            lock (fbwLock)
            {
                pitch = checkFlightStateParameters(float.Parse(ds.args[0]));
                yaw = checkFlightStateParameters(float.Parse(ds.args[1]));
                roll = checkFlightStateParameters(float.Parse(ds.args[2]));
                x = checkFlightStateParameters(float.Parse(ds.args[3]));
                y = checkFlightStateParameters(float.Parse(ds.args[4]));
                z = checkFlightStateParameters(float.Parse(ds.args[5]));
            }
            return 0;
        }

        [TelemetryAPI("v.setAttitude", "Set pitch, yaw, roll", IsAction = true, Category = "fbw", ReturnType = "int", Params = "float pitch, float yaw, float roll")]
        object SetAttitude(DataSources ds)
        {
            lock (fbwLock)
            {
                pitch = checkFlightStateParameters(float.Parse(ds.args[0]));
                yaw = checkFlightStateParameters(float.Parse(ds.args[1]));
                roll = checkFlightStateParameters(float.Parse(ds.args[2]));
            }
            return 0;
        }

        [TelemetryAPI("v.setTranslation", "Set X, Y and Z", IsAction = true, Category = "fbw", ReturnType = "int", Params = "float x, float y, float z")]
        object SetTranslation(DataSources ds)
        {
            lock (fbwLock)
            {
                x = checkFlightStateParameters(float.Parse(ds.args[0]));
                y = checkFlightStateParameters(float.Parse(ds.args[1]));
                z = checkFlightStateParameters(float.Parse(ds.args[2]));
            }
            return 0;
        }

        public static void onFlyByWire(FlightCtrlState fcs)
        {
            lock (fbwLock)
            {
                if (on_attitude > 0)
                {
                    fcs.yaw = yaw;
                    fcs.pitch = pitch;
                    fcs.roll = roll;
                    fcs.X = x < 0 ? -1 : (x > 0 ? 1 : 0);
                    fcs.Y = y < 0 ? -1 : (y > 0 ? 1 : 0);
                    fcs.Z = z < 0 ? -1 : (z > 0 ? 1 : 0);
                }
            }
        }

        public static void reset()
        {
            lock (fbwLock)
            {
                yaw = 0; pitch = 0; roll = 0;
                x = 0; y = 0; z = 0;
                on_attitude = 0;
            }
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }

    public class FlightDataLinkHandler : DataLinkHandler
    {
        public FlightDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            // These need queueDelayed wrapping — registered manually
            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { StageManager.ActivateNextStage(); return 0d; }, PredictFailure),
                "f.stage", "Stage", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { SetThrottle(x); return 0d; }, PredictFailure),
                "f.setThrottle", "Set Throttle [float magnitude]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { ThrottleUp(); return 0d; }, PredictFailure),
                "f.throttleUp", "Throttle Up", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { ThrottleZero(); return 0d; }, PredictFailure),
                "f.throttleZero", "Throttle Zero", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { ThrottleFull(); return 0d; }, PredictFailure),
                "f.throttleFull", "Throttle Full", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { ThrottleDown(); return 0d; }, PredictFailure),
                "f.throttleDown", "Throttle Down", formatters.Default));

            registerAPI(new ActionAPIEntry(
                BuildActionGroupToggle(KSPActionGroup.RCS),
                "f.rcs", "RCS [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                BuildActionGroupToggle(KSPActionGroup.SAS),
                "f.sas", "SAS [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                BuildActionGroupToggle(KSPActionGroup.Light),
                "f.light", "Light [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                BuildActionGroupToggle(KSPActionGroup.Gear),
                "f.gear", "Gear [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                BuildActionGroupToggle(KSPActionGroup.Brakes),
                "f.brake", "Brake [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                BuildActionGroupToggle(KSPActionGroup.Abort),
                "f.abort", "Abort [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x =>
                {
                    bool state = x.args.Count > 0 ? bool.Parse(x.args[0]) : !FlightInputHandler.fetch.precisionMode;
                    FlightInputHandler.fetch.precisionMode = state;
                    var gauges = UnityEngine.Object.FindObjectOfType<LinearControlGauges>();
                    if (gauges != null)
                    {
                        for (int i = 0; i < gauges.inputGaugeImages.Count; ++i)
                            gauges.inputGaugeImages[i].color = state ? XKCDColors.BrightCyan : XKCDColors.Orange;
                    }
                    return Convert.ToInt32(state);
                }, PredictFailure),
                "f.precisionControl", "Precision controls [optional bool on/off]", formatters.Default));

            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom01), "f.ag1", "Action Group 1 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom02), "f.ag2", "Action Group 2 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom03), "f.ag3", "Action Group 3 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom04), "f.ag4", "Action Group 4 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom05), "f.ag5", "Action Group 5 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom06), "f.ag6", "Action Group 6 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom07), "f.ag7", "Action Group 7 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom08), "f.ag8", "Action Group 8 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom09), "f.ag9", "Action Group 9 [optional bool on/off]", formatters.Default));
            registerAPI(new ActionAPIEntry(BuildActionGroupToggle(KSPActionGroup.Custom10), "f.ag10", "Action Group 10 [optional bool on/off]", formatters.Default));
        }

        // --- Throttle ---

        [TelemetryAPI("f.throttle", "Throttle", Category = "flight", ReturnType = "double")]
        object Throttle(DataSources ds) => ds.vessel.ctrlState.mainThrottle;

        // --- Control Inputs (current frame) ---

        [TelemetryAPI("f.pitchInput", "Pitch Control Input", Category = "flight", ReturnType = "double")]
        object PitchInput(DataSources ds) => ds.vessel.ctrlState.pitch;

        [TelemetryAPI("f.yawInput", "Yaw Control Input", Category = "flight", ReturnType = "double")]
        object YawInput(DataSources ds) => ds.vessel.ctrlState.yaw;

        [TelemetryAPI("f.rollInput", "Roll Control Input", Category = "flight", ReturnType = "double")]
        object RollInput(DataSources ds) => ds.vessel.ctrlState.roll;

        [TelemetryAPI("f.xInput", "RCS X Translation Input", Category = "flight", ReturnType = "double")]
        object XInput(DataSources ds) => ds.vessel.ctrlState.X;

        [TelemetryAPI("f.yInput", "RCS Y Translation Input", Category = "flight", ReturnType = "double")]
        object YInput(DataSources ds) => ds.vessel.ctrlState.Y;

        [TelemetryAPI("f.zInput", "RCS Z Translation Input", Category = "flight", ReturnType = "double")]
        object ZInput(DataSources ds) => ds.vessel.ctrlState.Z;

        // --- Trim ---

        [TelemetryAPI("f.pitchTrim", "Pitch Trim", Category = "flight", ReturnType = "double")]
        object PitchTrim(DataSources ds) => ds.vessel.ctrlState.pitchTrim;

        [TelemetryAPI("f.yawTrim", "Yaw Trim", Category = "flight", ReturnType = "double")]
        object YawTrim(DataSources ds) => ds.vessel.ctrlState.yawTrim;

        [TelemetryAPI("f.rollTrim", "Roll Trim", Category = "flight", ReturnType = "double")]
        object RollTrim(DataSources ds) => ds.vessel.ctrlState.rollTrim;

        // --- Control State ---

        [TelemetryAPI("f.isNeutral", "Controls Are Neutral", Category = "flight", ReturnType = "bool")]
        object IsNeutral(DataSources ds) => ds.vessel.ctrlState.isNeutral;

        [TelemetryAPI("f.killRot", "SAS Kill Rotation Active", Category = "flight", ReturnType = "bool")]
        object KillRot(DataSources ds) => ds.vessel.ctrlState.killRot;

        // --- Action Group Values ---

        [TelemetryAPI("v.rcsValue", "Query RCS value", Category = "flight", ReturnType = "bool")]
        object RcsValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.RCS];

        [TelemetryAPI("v.sasValue", "Query SAS value", Category = "flight", ReturnType = "bool")]
        object SasValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.SAS];

        [TelemetryAPI("v.lightValue", "Query light value", Category = "flight", ReturnType = "bool")]
        object LightValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Light];

        [TelemetryAPI("v.brakeValue", "Query brake value", Category = "flight", ReturnType = "bool")]
        object BrakeValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Brakes];

        [TelemetryAPI("v.gearValue", "Query gear value", Category = "flight", ReturnType = "bool")]
        object GearValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Gear];

        [TelemetryAPI("v.abortValue", "Query abort value", Category = "flight", ReturnType = "bool")]
        object AbortValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Abort];

        [TelemetryAPI("v.ag1Value", "Query Action Group 1 value", Category = "flight", ReturnType = "bool")]
        object Ag1Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom01];

        [TelemetryAPI("v.ag2Value", "Query Action Group 2 value", Category = "flight", ReturnType = "bool")]
        object Ag2Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom02];

        [TelemetryAPI("v.ag3Value", "Query Action Group 3 value", Category = "flight", ReturnType = "bool")]
        object Ag3Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom03];

        [TelemetryAPI("v.ag4Value", "Query Action Group 4 value", Category = "flight", ReturnType = "bool")]
        object Ag4Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom04];

        [TelemetryAPI("v.ag5Value", "Query Action Group 5 value", Category = "flight", ReturnType = "bool")]
        object Ag5Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom05];

        [TelemetryAPI("v.ag6Value", "Query Action Group 6 value", Category = "flight", ReturnType = "bool")]
        object Ag6Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom06];

        [TelemetryAPI("v.ag7Value", "Query Action Group 7 value", Category = "flight", ReturnType = "bool")]
        object Ag7Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom07];

        [TelemetryAPI("v.ag8Value", "Query Action Group 8 value", Category = "flight", ReturnType = "bool")]
        object Ag8Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom08];

        [TelemetryAPI("v.ag9Value", "Query Action Group 9 value", Category = "flight", ReturnType = "bool")]
        object Ag9Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom09];

        [TelemetryAPI("v.ag10Value", "Query Action Group 10 value", Category = "flight", ReturnType = "bool")]
        object Ag10Value(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Custom10];

        [TelemetryAPI("v.precisionControlValue", "Query precision controls value", Category = "flight", ReturnType = "bool")]
        object PrecisionControlValue(DataSources ds) => FlightInputHandler.fetch.precisionMode;

        // --- SAS / Autopilot ---

        [TelemetryAPI("f.sasMode", "Current SAS Mode", Units = APIEntry.UnitType.STRING, Category = "flight", ReturnType = "string")]
        object SasMode(DataSources ds) => ds.vessel.Autopilot.Mode.ToString();

        [TelemetryAPI("f.sasEnabled", "SAS Autopilot Enabled", Category = "flight", ReturnType = "bool")]
        object SasEnabled(DataSources ds) => ds.vessel.Autopilot.Enabled;

        [TelemetryAPI("f.setSASMode", "Set SAS Mode", IsAction = true, Category = "flight", ReturnType = "string", Params = "string mode")]
        object SetSASMode(DataSources ds)
        {
            var mode = (VesselAutopilot.AutopilotMode)Enum.Parse(
                typeof(VesselAutopilot.AutopilotMode), ds.args[0], true);
            ds.vessel.Autopilot.SetMode(mode);
            return mode.ToString();
        }

        // --- Trim Actions ---

        [TelemetryAPI("f.setPitchTrim", "Set Pitch Trim", IsAction = true, Category = "flight", ReturnType = "int", Params = "float trim")]
        object SetPitchTrim(DataSources ds)
        {
            ds.vessel.ctrlState.pitchTrim = checkFlightStateParameters(float.Parse(ds.args[0]));
            return 0;
        }

        [TelemetryAPI("f.setYawTrim", "Set Yaw Trim", IsAction = true, Category = "flight", ReturnType = "int", Params = "float trim")]
        object SetYawTrim(DataSources ds)
        {
            ds.vessel.ctrlState.yawTrim = checkFlightStateParameters(float.Parse(ds.args[0]));
            return 0;
        }

        [TelemetryAPI("f.setRollTrim", "Set Roll Trim", IsAction = true, Category = "flight", ReturnType = "int", Params = "float trim")]
        object SetRollTrim(DataSources ds)
        {
            ds.vessel.ctrlState.rollTrim = checkFlightStateParameters(float.Parse(ds.args[0]));
            return 0;
        }

        private APIDelegate BuildActionGroupToggle(KSPActionGroup actionGroup)
        {
            return queueDelayed(x =>
            {
                if (x.args.Count == 0)
                    x.vessel.ActionGroups.ToggleGroup(actionGroup);
                else
                    x.vessel.ActionGroups.SetGroup(actionGroup, bool.Parse(x.args[0]));
                return 0d;
            }, PredictFailure);
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();

        private void ThrottleUp()
        {
            FlightInputHandler.state.mainThrottle += 0.1f;
            if (FlightInputHandler.state.mainThrottle > 1)
                FlightInputHandler.state.mainThrottle = 1f;
        }

        private void ThrottleDown()
        {
            FlightInputHandler.state.mainThrottle -= 0.1f;
            if (FlightInputHandler.state.mainThrottle < 0)
                FlightInputHandler.state.mainThrottle = 0f;
        }

        private void ThrottleZero() => FlightInputHandler.state.mainThrottle = 0f;
        private void ThrottleFull() => FlightInputHandler.state.mainThrottle = 1f;

        private void SetThrottle(DataSources ds) =>
            FlightInputHandler.state.mainThrottle = float.Parse(ds.args[0]);

        private static int PredictFailure(Vessel vessel) =>
            PausedDataLinkHandler.partPaused();
    }
}
