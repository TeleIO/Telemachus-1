using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telemachus
{
    public class TimeWarpDataLinkHandler : DataLinkHandler
    {
        public TimeWarpDataLinkHandler(FormatterProvider formatters)
            : base(formatters)
        {
            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { TimeWarp.SetRate(int.Parse(x.args[0]), false); return 0d; }),
                "t.timeWarp", "Time Warp [int rate]", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { FlightDriver.SetPause(true); return 0d; }),
                "t.pause", "Pause game", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { FlightDriver.SetPause(false); return 0d; }),
                "t.unpause", "Unpause game", formatters.Default));
        }

        [TelemetryAPI("t.universalTime", "Universal Time", Units = APIEntry.UnitType.DATE, AlwaysEvaluable = true, Category = "timewarp", ReturnType = "double")]
        object UniversalTime(DataSources ds) => Planetarium.GetUniversalTime();

        [TelemetryAPI("t.currentRate", "Current Warp Rate", Category = "timewarp", ReturnType = "double")]
        object CurrentRate(DataSources ds) => TimeWarp.CurrentRate;

        [TelemetryAPI("t.currentRateIndex", "Current Warp Rate Index", Category = "timewarp", ReturnType = "double")]
        object CurrentRateIndex(DataSources ds) => TimeWarp.CurrentRateIndex;

        [TelemetryAPI("t.warpMode", "Warp Mode (HIGH or LOW)", Units = APIEntry.UnitType.STRING, Category = "timewarp", ReturnType = "string")]
        object WarpMode(DataSources ds) => TimeWarp.WarpMode.ToString();

        [TelemetryAPI("t.maxPhysicsRate", "Max Physics Warp Rate", Category = "timewarp", ReturnType = "double")]
        object MaxPhysicsRate(DataSources ds) => TimeWarp.MaxPhysicsRate;

        [TelemetryAPI("t.deltaTime", "Delta Time", Category = "timewarp", ReturnType = "double")]
        object DeltaTime(DataSources ds) => TimeWarp.deltaTime;

        [TelemetryAPI("t.isPaused", "Game Is Paused", Category = "timewarp", ReturnType = "bool")]
        object IsPaused(DataSources ds) => FlightDriver.Pause;
    }

    public class TargetDataLinkHandler : DataLinkHandler
    {
        public TargetDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("tar.name", "Target Name", Units = APIEntry.UnitType.STRING, Category = "target", ReturnType = "string")]
        object TargetName(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetName() : "No Target Selected.";

        [TelemetryAPI("tar.type", "Target Type", Units = APIEntry.UnitType.STRING, Category = "target", ReturnType = "string")]
        object TargetType(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetType().ToString() : "";

        [TelemetryAPI("tar.distance", "Target Distance", Units = APIEntry.UnitType.DISTANCE, Category = "target", ReturnType = "double")]
        object TargetDistance(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? Vector3.Distance(FlightGlobals.fetch.VesselTarget.GetTransform().position, ds.vessel.GetTransform().position) : 0;

        [TelemetryAPI("tar.o.relativeVelocity", "Target Relative Velocity", Units = APIEntry.UnitType.VELOCITY, Category = "target", ReturnType = "double")]
        object TargetRelativeVelocity(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? (FlightGlobals.fetch.VesselTarget.GetOrbit().GetVel() - ds.vessel.orbit.GetVel()).magnitude : 0;

        [TelemetryAPI("tar.o.velocity", "Target Velocity", Units = APIEntry.UnitType.VELOCITY, Category = "target", ReturnType = "double")]
        object TargetVelocity(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().vel.magnitude : 0;

        [TelemetryAPI("tar.o.PeA", "Target Periapsis", Units = APIEntry.UnitType.DISTANCE, Category = "target", ReturnType = "double")]
        object TargetPeA(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().PeA : 0;

        [TelemetryAPI("tar.o.ApA", "Target Apoapsis", Units = APIEntry.UnitType.DISTANCE, Category = "target", ReturnType = "double")]
        object TargetApA(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().ApA : 0;

        [TelemetryAPI("tar.o.timeToAp", "Target Time to Apoapsis", Units = APIEntry.UnitType.TIME, Category = "target", ReturnType = "double")]
        object TargetTimeToAp(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToAp : 0;

        [TelemetryAPI("tar.o.timeToPe", "Target Time to Periapsis", Units = APIEntry.UnitType.TIME, Category = "target", ReturnType = "double")]
        object TargetTimeToPe(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToPe : 0;

        [TelemetryAPI("tar.o.inclination", "Target Inclination", Units = APIEntry.UnitType.DEG, Category = "target", ReturnType = "double")]
        object TargetInclination(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().inclination : 0;

        [TelemetryAPI("tar.o.eccentricity", "Target Eccentricity", Category = "target", ReturnType = "double")]
        object TargetEccentricity(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().eccentricity : 0;

        [TelemetryAPI("tar.o.period", "Target Orbital Period", Units = APIEntry.UnitType.TIME, Category = "target", ReturnType = "double")]
        object TargetPeriod(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().period : 0;

        [TelemetryAPI("tar.o.argumentOfPeriapsis", "Target Argument of Periapsis", Units = APIEntry.UnitType.DEG, Category = "target", ReturnType = "double")]
        object TargetArgumentOfPeriapsis(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().argumentOfPeriapsis : 0;

        [TelemetryAPI("tar.o.timeToTransition1", "Target Time to Transition 1", Units = APIEntry.UnitType.TIME, Category = "target", ReturnType = "double")]
        object TargetTimeToTransition1(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToTransition1 : 0;

        [TelemetryAPI("tar.o.timeToTransition2", "Target Time to Transition 2", Units = APIEntry.UnitType.TIME, Category = "target", ReturnType = "double")]
        object TargetTimeToTransition2(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToTransition2 : 0;

        [TelemetryAPI("tar.o.sma", "Target Semimajor Axis", Units = APIEntry.UnitType.DISTANCE, Category = "target", ReturnType = "double")]
        object TargetSma(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().semiMajorAxis : 0;

        [TelemetryAPI("tar.o.lan", "Target Longitude of Ascending Node", Units = APIEntry.UnitType.DEG, Category = "target", ReturnType = "double")]
        object TargetLan(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().LAN : 0;

        [TelemetryAPI("tar.o.maae", "Target Mean Anomaly at Epoch", Category = "target", ReturnType = "double")]
        object TargetMaae(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Orbit orbit = FlightGlobals.fetch.VesselTarget.GetOrbit();
            return orbit.getObtAtUT(0) / orbit.period * (2.0 * Math.PI);
        }

        [TelemetryAPI("tar.o.timeOfPeriapsisPassage", "Target Time of Periapsis Passage", Units = APIEntry.UnitType.DATE, Category = "target", ReturnType = "double")]
        object TargetTimeOfPeriapsisPassage(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? Planetarium.GetUniversalTime() - FlightGlobals.fetch.VesselTarget.GetOrbit().ObT : 0;

        [TelemetryAPI("tar.o.trueAnomaly", "Target True Anomaly", Units = APIEntry.UnitType.DEG, Category = "target", ReturnType = "double")]
        object TargetTrueAnomaly(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? FlightGlobals.fetch.VesselTarget.GetOrbit().TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI) : double.NaN;

        [TelemetryAPI("tar.o.orbitingBody", "Target Orbiting Body", Units = APIEntry.UnitType.STRING, Category = "target", ReturnType = "string")]
        object TargetOrbitingBody(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().referenceBody.name : "";

        [TelemetryAPI("tar.o.orbitPatches", "Detailed Orbit Patches Info",
            Plotable = false, Formatter = "OrbitPatchList", Category = "target", ReturnType = "double")]
        object TargetOrbitPatches(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return null;
            return OrbitPatches.getPatchesForOrbit(FlightGlobals.fetch.VesselTarget.GetOrbit());
        }

        [TelemetryAPI("tar.o.trueAnomalyAtUTForOrbitPatch",
            "The orbit patch's True Anomaly at Universal Time",
            Units = APIEntry.UnitType.DEG, Category = "target", ReturnType = "double", Params = "int orbitPatchIndex, float universalTime")]
        object TargetTrueAnomalyAtUTForOrbitPatch(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return null;
            int index = int.Parse(ds.args[0]);
            float ut = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
            if (orbitPatch == null) return null;
            return orbitPatch.TrueAnomalyAtUT(ut);
        }

        [TelemetryAPI("tar.o.UTForTrueAnomalyForOrbitPatch",
            "The orbit patch's Universal Time for True Anomaly",
            Units = APIEntry.UnitType.DATE, Category = "target", ReturnType = "double", Params = "int orbitPatchIndex, float trueAnomaly")]
        object TargetUTForTrueAnomalyForOrbitPatch(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return null;
            int index = int.Parse(ds.args[0]);
            float trueAnomaly = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
            if (orbitPatch == null) return null;
            double now = Planetarium.GetUniversalTime();
            return orbitPatch.GetUTforTrueAnomaly(trueAnomaly, now);
        }

        [TelemetryAPI("tar.o.relativePositionAtTrueAnomalyForOrbitPatch",
            "The orbit patch's predicted displacement from the center of the main body at the given true anomaly",
            Formatter = "Vector3d", Category = "target", ReturnType = "double", Params = "int orbitPatchIndex, float trueAnomaly")]
        object TargetRelativePositionAtTrueAnomalyForOrbitPatch(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return null;
            int index = int.Parse(ds.args[0]);
            float trueAnomaly = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionFromTrueAnomaly(trueAnomaly);
        }

        [TelemetryAPI("tar.o.relativePositionAtUTForOrbitPatch",
            "The orbit patch's predicted displacement from the center of the main body at the given universal time",
            Formatter = "Vector3d", Category = "target", ReturnType = "double", Params = "int orbitPatchIndex, double universalTime")]
        object TargetRelativePositionAtUTForOrbitPatch(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return null;
            int index = int.Parse(ds.args[0]);
            double ut = double.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionAtUT(ut);
        }

        // --- Target Actions ---

        [TelemetryAPI("tar.setTargetBody", "Set Target to Celestial Body", IsAction = true, Category = "target", ReturnType = "int", Params = "int bodyId")]
        object SetTargetBody(DataSources ds)
        {
            int bodyId = int.Parse(ds.args[0]);
            if (bodyId < 0 || bodyId >= FlightGlobals.Bodies.Count) return false;
            FlightGlobals.fetch.SetVesselTarget(FlightGlobals.Bodies[bodyId]);
            return true;
        }

        [TelemetryAPI("tar.setTargetVessel", "Set Target to Vessel by Index", IsAction = true, Category = "target", ReturnType = "int", Params = "int vesselIndex")]
        object SetTargetVessel(DataSources ds)
        {
            int vesselIdx = int.Parse(ds.args[0]);
            if (vesselIdx < 0 || vesselIdx >= FlightGlobals.Vessels.Count) return false;
            FlightGlobals.fetch.SetVesselTarget(FlightGlobals.Vessels[vesselIdx]);
            return true;
        }

        [TelemetryAPI("tar.clearTarget", "Clear Current Target", IsAction = true, Category = "target", ReturnType = "int")]
        object ClearTarget(DataSources ds)
        {
            FlightGlobals.fetch.SetVesselTarget(null);
            return true;
        }

        public override bool process(String API, out APIEntry result)
        {
            if (!base.process(API, out result))
                return API.StartsWith("tar.");
            return true;
        }
    }

    public class DockingDataLinkHandler : DataLinkHandler
    {
        private static Vector3 orientationDeviation = new Vector3();
        private static Vector2 translationDeviation = new Vector3();

        public DockingDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("dock.ax", "Docking x Angle", Units = APIEntry.UnitType.DEG, Category = "docking", ReturnType = "double")]
        object DockAx(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Update();
            return orientationDeviation.x;
        }

        [TelemetryAPI("dock.ay", "Relative Pitch Angle", Units = APIEntry.UnitType.DEG, Category = "docking", ReturnType = "double")]
        object DockAy(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Update();
            return orientationDeviation.y;
        }

        [TelemetryAPI("dock.az", "Docking z Angle", Units = APIEntry.UnitType.DEG, Category = "docking", ReturnType = "double")]
        object DockAz(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Update();
            return orientationDeviation.z;
        }

        [TelemetryAPI("dock.x", "Target x Distance", Units = APIEntry.UnitType.DISTANCE, Category = "docking", ReturnType = "double")]
        object DockX(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? (FlightGlobals.fetch.VesselTarget.GetTransform().position - ds.vessel.GetTransform().position).x : 0;

        [TelemetryAPI("dock.y", "Target y Distance", Units = APIEntry.UnitType.DISTANCE, Category = "docking", ReturnType = "double")]
        object DockY(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? (FlightGlobals.fetch.VesselTarget.GetTransform().position - ds.vessel.GetTransform().position).y : 0;

        // Borrowed from Docking Port Alignment Indicator by NavyFish
        private void Update()
        {
            ModuleDockingNode targetPort = FlightGlobals.fetch.VesselTarget as ModuleDockingNode;
            if (targetPort == null) return;

            Transform selfTransform = FlightGlobals.ActiveVessel.ReferenceTransform;
            Transform targetTransform = targetPort.transform;
            Vector3 targetPortOutVector;
            Vector3 targetPortRollReferenceVector;

            if (targetPort.part.name == "dockingPortLateral")
            {
                targetPortOutVector = -targetTransform.forward.normalized;
                targetPortRollReferenceVector = -targetTransform.up;
            }
            else
            {
                targetPortOutVector = targetTransform.up.normalized;
                targetPortRollReferenceVector = targetTransform.forward;
            }

            orientationDeviation.x = AngleAroundNormal(-targetPortOutVector, selfTransform.up, selfTransform.forward);
            orientationDeviation.y = AngleAroundNormal(-targetPortOutVector, selfTransform.up, -selfTransform.right);
            orientationDeviation.z = AngleAroundNormal(targetPortRollReferenceVector, selfTransform.forward, selfTransform.up);
            orientationDeviation.z = (orientationDeviation.z + 360) % 360;

            Vector3 targetToOwnship = selfTransform.position - targetTransform.position;
            translationDeviation.x = AngleAroundNormal(targetToOwnship, targetPortOutVector, selfTransform.forward);
            translationDeviation.y = AngleAroundNormal(targetToOwnship, targetPortOutVector, -selfTransform.right);
        }

        private float AngleAroundNormal(Vector3 a, Vector3 b, Vector3 up) =>
            AngleSigned(Vector3.Cross(up, a), Vector3.Cross(up, b), up);

        private float AngleSigned(Vector3 v1, Vector3 v2, Vector3 up) =>
            Vector3.Dot(Vector3.Cross(v1, v2), up) < 0 ? -Vector3.Angle(v1, v2) : Vector3.Angle(v1, v2);
    }
}
