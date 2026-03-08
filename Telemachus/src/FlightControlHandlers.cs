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

        [TelemetryAPI("v.setYaw", "Yaw [float yaw]", IsAction = true)]
        object SetYaw(DataSources ds)
        {
            lock (fbwLock) { yaw = checkFlightStateParameters(float.Parse(ds.args[0])); }
            return 0;
        }

        [TelemetryAPI("v.setPitch", "Pitch [float pitch]", IsAction = true)]
        object SetPitch(DataSources ds)
        {
            lock (fbwLock) { pitch = checkFlightStateParameters(float.Parse(ds.args[0])); }
            return 0;
        }

        [TelemetryAPI("v.setRoll", "Roll [float roll]", IsAction = true)]
        object SetRoll(DataSources ds)
        {
            lock (fbwLock) { roll = checkFlightStateParameters(float.Parse(ds.args[0])); }
            return 0;
        }

        [TelemetryAPI("v.setFbW", "Set Fly by Wire On or Off [int state]", IsAction = true)]
        object SetFbW(DataSources ds)
        {
            lock (fbwLock) { on_attitude = int.Parse(ds.args[0]); }
            return 0;
        }

        [TelemetryAPI("v.setPitchYawRollXYZ", "Set pitch, yaw, roll, X, Y and Z [float pitch, yaw, roll, x, y, z]", IsAction = true)]
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

        [TelemetryAPI("v.setAttitude", "Set pitch, yaw, roll [float pitch, yaw, roll]", IsAction = true)]
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

        [TelemetryAPI("v.setTranslation", "Set X, Y and Z [float x, y, z]", IsAction = true)]
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

        [TelemetryAPI("f.throttle", "Throttle")]
        object Throttle(DataSources ds) => ds.vessel.ctrlState.mainThrottle;

        [TelemetryAPI("v.rcsValue", "Query RCS value")]
        object RcsValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.RCS];

        [TelemetryAPI("v.sasValue", "Query SAS value")]
        object SasValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.SAS];

        [TelemetryAPI("v.lightValue", "Query light value")]
        object LightValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Light];

        [TelemetryAPI("v.brakeValue", "Query brake value")]
        object BrakeValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Brakes];

        [TelemetryAPI("v.gearValue", "Query gear value")]
        object GearValue(DataSources ds) => ds.vessel.ActionGroups[KSPActionGroup.Gear];

        [TelemetryAPI("v.precisionControlValue", "Query precision controls value")]
        object PrecisionControlValue(DataSources ds) => FlightInputHandler.fetch.precisionMode;

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
