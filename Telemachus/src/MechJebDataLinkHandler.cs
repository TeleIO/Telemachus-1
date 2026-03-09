using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Telemachus
{
    public class MechJebDataLinkHandler : DataLinkHandler
    {
        public MechJebDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            // MechJeb actions all use queueDelayed with predictFailure — registered manually
            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectOff(x), PredictFailure),
                "mj.smartassoff", "Smart ASS Off", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, Vector3d.forward, "MANEUVER_NODE"), PredictFailure),
                "mj.node", "Node", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, Vector3d.forward, "ORBIT"), PredictFailure),
                "mj.prograde", "Prograde", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, Vector3d.back, "ORBIT"), PredictFailure),
                "mj.retrograde", "Retrograde", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, Vector3d.left, "ORBIT"), PredictFailure),
                "mj.normalplus", "Normal Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, Vector3d.right, "ORBIT"), PredictFailure),
                "mj.normalminus", "Normal Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, Vector3d.up, "ORBIT"), PredictFailure),
                "mj.radialplus", "Radial Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, Vector3d.down, "ORBIT"), PredictFailure),
                "mj.radialminus", "Radial Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => FlightGlobals.fetch.VesselTarget != null ? ReflectAttitudeTo(x, Vector3d.forward, "TARGET") : (object)false, PredictFailure),
                "mj.targetplus", "Target Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => FlightGlobals.fetch.VesselTarget != null ? ReflectAttitudeTo(x, Vector3d.back, "TARGET") : (object)false, PredictFailure),
                "mj.targetminus", "Target Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => FlightGlobals.fetch.VesselTarget != null ? ReflectAttitudeTo(x, Vector3d.forward, "RELATIVE_VELOCITY") : (object)false, PredictFailure),
                "mj.relativeplus", "Relative Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => FlightGlobals.fetch.VesselTarget != null ? ReflectAttitudeTo(x, Vector3d.back, "RELATIVE_VELOCITY") : (object)false, PredictFailure),
                "mj.relativeminus", "Relative Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => FlightGlobals.fetch.VesselTarget != null ? ReflectAttitudeTo(x, Vector3d.forward, "TARGET_ORIENTATION") : (object)false, PredictFailure),
                "mj.parallelplus", "Parallel Plus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => FlightGlobals.fetch.VesselTarget != null ? ReflectAttitudeTo(x, Vector3d.back, "TARGET_ORIENTATION") : (object)false, PredictFailure),
                "mj.parallelminus", "Parallel Minus", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => Surface(x), PredictFailure),
                "mj.surface", "Surface [float heading, float pitch]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => ReflectAttitudeTo(x, double.Parse(x.args[0]), double.Parse(x.args[1]), double.Parse(x.args[2])), PredictFailure),
                "mj.surface2", "Surface [double heading, double pitch, double roll]", formatters.Default));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    PluginLogger.debug("Start GET");
                    return GetStagingInfo(dataSources);
                },
                "mj.stagingInfo", "Staging Info [object stagingInfo]",
                formatters.MechJebSimulation, APIEntry.UnitType.UNITLESS));
        }

        [TelemetryAPI("mj.available", "MechJeb Is Installed")]
        object Available(DataSources ds) => FindMechJeb(ds.vessel) != null;

        #region Flight Control

        private bool Surface(DataSources dataSources)
        {
            Quaternion r = Quaternion.AngleAxis(float.Parse(dataSources.args[0]), Vector3.up)
                * Quaternion.AngleAxis(-float.Parse(dataSources.args[1]), Vector3.right);
            return ReflectAttitudeTo(dataSources, r * Vector3d.forward, "SURFACE_NORTH");
        }

        private bool ReflectOff(DataSources dataSources)
        {
            object attitude = null;
            Type attitudeType = GetAttitudeType(dataSources, ref attitude);
            if (attitudeType != null)
            {
                MethodInfo methodInfo = attitudeType.GetMethod("attitudeDeactivate");
                methodInfo.Invoke(attitude, new object[] { });
                return true;
            }
            return false;
        }

        private bool ReflectAttitudeTo(DataSources dataSources, Vector3d v, string reference)
        {
            object attitude = null;
            Type attitudeType = GetAttitudeType(dataSources, ref attitude);
            if (attitudeType != null)
            {
                Type attitudeReferenceType = attitude.GetType().GetProperty("attitudeReference",
                    BindingFlags.Public | BindingFlags.Instance).GetValue(attitude, null).GetType();

                MethodInfo methodInfo = attitudeType.GetMethod("attitudeTo", new[] { typeof(Vector3d),
                    attitudeReferenceType, typeof(object) });

                methodInfo.Invoke(attitude, new object[] { v, Enum.Parse(attitudeReferenceType, reference), this });
                return true;
            }
            return false;
        }

        private bool ReflectAttitudeTo(DataSources dataSources, double heading, double pitch, double roll)
        {
            object attitude = null;
            Type attitudeType = GetAttitudeType(dataSources, ref attitude);
            if (attitudeType != null)
            {
                MethodInfo methodInfo = attitudeType.GetMethod("attitudeTo", new[] { typeof(double),
                    typeof(double), typeof(double), typeof(object) });

                methodInfo.Invoke(attitude, new object[] { heading, pitch, roll, this });
                return true;
            }
            return false;
        }

        private Type GetAttitudeType(DataSources dataSources, ref object attitude)
        {
            PartModule mechJebCore = FindMechJeb(dataSources.vessel);
            if (mechJebCore == null)
            {
                PluginLogger.debug("No Mechjeb part installed.");
                return null;
            }

            try
            {
                PluginLogger.debug("Mechjeb part installed, reflecting.");
                Type mechJebCoreType = mechJebCore.GetType();
                FieldInfo attitudeField = mechJebCoreType.GetField("Attitude", BindingFlags.Public | BindingFlags.Instance);
                attitude = attitudeField.GetValue(mechJebCore);

                attitude.GetType().GetProperty("attitudeReference",
                    BindingFlags.Public | BindingFlags.Instance).GetValue(attitude, null).GetType();

                return attitude.GetType();
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
            return null;
        }

        #endregion

        #region Staging Info

        private MechJebSimulation GetStagingInfo(DataSources dataSources)
        {
            PartModule mechJebCore = FindMechJeb(dataSources.vessel);
            if (mechJebCore == null) return null;

            try
            {
                Type mechJebCoreType = mechJebCore.GetType();
                MethodInfo getModule = mechJebCoreType.GetMethod("GetComputerModule", new[] { typeof(string) });
                object stagingInfo = getModule.Invoke(mechJebCore, new object[] { "MechJebModuleStageStats" });
                if (stagingInfo == null) return null;

                Type stageStatsType = stagingInfo.GetType();

                MethodInfo requestUpdate = stageStatsType.GetMethod("RequestUpdate", BindingFlags.Public | BindingFlags.Instance);
                if (requestUpdate != null)
                {
                    object captured = stagingInfo;
                    TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI",
                        new DelayedAPIEntry(dataSources.Clone(),
                            (x) => { requestUpdate.Invoke(captured, null); return 0; }),
                        UnityEngine.SendMessageOptions.DontRequireReceiver);
                }

                var stats = new MechJebSimulation();
                stats.ConvertMechJebData(stageStatsType, stagingInfo);
                return stats;
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
                return null;
            }
        }

        #endregion

        private static int PredictFailure(Vessel vessel)
        {
            int pause = PausedDataLinkHandler.partPaused();
            if (pause > 0) return pause;
            if (FindMechJeb(vessel) == null) return 5;
            return 0;
        }

        private static PartModule FindMechJeb(Vessel vessel)
        {
            try
            {
                List<Part> pl = vessel.parts.FindAll(p => p.Modules.Contains("MechJebCore"));
                foreach (PartModule m in pl[0].Modules)
                {
                    if (m.GetType().Name.Equals("MechJebCore"))
                        return m;
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
            return null;
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();

        #region Data Structures

        public class MechJebSimulation
        {
            public List<MechJebStageSimulationStats> vacuumStats = new();
            public List<MechJebStageSimulationStats> atmoStats = new();

            public void ConvertMechJebData(Type stagingInfoType, object stagingInfo)
            {
                FieldInfo atmoStatsField = stagingInfoType.GetField("AtmoStats", BindingFlags.Public | BindingFlags.Instance)
                    ?? stagingInfoType.GetField("atmoStats", BindingFlags.Public | BindingFlags.Instance);
                PluginLogger.debug("Getting Atmo Stats");
                IEnumerable atmoStats = (IEnumerable)atmoStatsField.GetValue(stagingInfo);
                PopulateStats(atmoStats, this.atmoStats);

                FieldInfo vacStatsField = stagingInfoType.GetField("VacStats", BindingFlags.Public | BindingFlags.Instance)
                    ?? stagingInfoType.GetField("vacStats", BindingFlags.Public | BindingFlags.Instance);
                PluginLogger.debug("Getting Vac Stats");
                IEnumerable vacStats = (IEnumerable)vacStatsField.GetValue(stagingInfo);
                PopulateStats(vacStats, this.vacuumStats);
            }

            private static float GetFloatField(Type type, object obj, string pascalName, string camelName)
            {
                FieldInfo field = type.GetField(pascalName) ?? type.GetField(camelName);
                if (field == null) return 0f;
                object val = field.GetValue(obj);
                if (val is double d) return (float)d;
                if (val is float f) return f;
                return Convert.ToSingle(val);
            }

            private void PopulateStats(IEnumerable mechJebStats, List<MechJebStageSimulationStats> destinationStats)
            {
                foreach (object mechJebStat in mechJebStats)
                {
                    var stat = new MechJebStageSimulationStats();
                    Type statType = mechJebStat.GetType();

                    stat.startMass = GetFloatField(statType, mechJebStat, "StartMass", "startMass");
                    stat.endMass = GetFloatField(statType, mechJebStat, "EndMass", "endMass");
                    stat.startThrust = GetFloatField(statType, mechJebStat, "Thrust", "startThrust");
                    stat.maxAccel = GetFloatField(statType, mechJebStat, "MaxAccel", "maxAccel");
                    stat.deltaTime = GetFloatField(statType, mechJebStat, "DeltaTime", "deltaTime");
                    stat.deltaV = GetFloatField(statType, mechJebStat, "DeltaV", "deltaV");
                    stat.resourceMass = GetFloatField(statType, mechJebStat, "ResourceMass", "resourceMass");
                    stat.isp = GetFloatField(statType, mechJebStat, "Isp", "isp");
                    stat.stagedMass = GetFloatField(statType, mechJebStat, "StagedMass", "stagedMass");

                    destinationStats.AddUnique(stat);
                }
            }
        }

        public class MechJebStageSimulationStats
        {
            public float startMass;
            public float endMass;
            public float startThrust;
            public float maxAccel;
            public float deltaTime;
            public float deltaV;
            public float resourceMass;
            public float isp;
            public float stagedMass;
        }

        #endregion
    }
}
