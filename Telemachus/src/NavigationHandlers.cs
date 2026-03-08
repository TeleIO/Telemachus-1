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

        [TelemetryAPI("t.universalTime", "Universal Time", Units = APIEntry.UnitType.DATE, AlwaysEvaluable = true)]
        object UniversalTime(DataSources ds) => Planetarium.GetUniversalTime();
    }

    public class TargetDataLinkHandler : DataLinkHandler
    {
        public TargetDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("tar.name", "Target Name", Units = APIEntry.UnitType.STRING)]
        object TargetName(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetName() : "No Target Selected.";

        [TelemetryAPI("tar.type", "Target Type", Units = APIEntry.UnitType.STRING)]
        object TargetType(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetType().ToString() : "";

        [TelemetryAPI("tar.distance", "Target Distance", Units = APIEntry.UnitType.DISTANCE)]
        object TargetDistance(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? Vector3.Distance(FlightGlobals.fetch.VesselTarget.GetTransform().position, ds.vessel.GetTransform().position) : 0;

        [TelemetryAPI("tar.o.relativeVelocity", "Target Relative Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object TargetRelativeVelocity(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? (FlightGlobals.fetch.VesselTarget.GetOrbit().GetVel() - ds.vessel.orbit.GetVel()).magnitude : 0;

        [TelemetryAPI("tar.o.velocity", "Target Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object TargetVelocity(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().vel.magnitude : 0;

        [TelemetryAPI("tar.o.PeA", "Target Periapsis", Units = APIEntry.UnitType.DISTANCE)]
        object TargetPeA(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().PeA : 0;

        [TelemetryAPI("tar.o.ApA", "Target Apoapsis", Units = APIEntry.UnitType.DISTANCE)]
        object TargetApA(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().ApA : 0;

        [TelemetryAPI("tar.o.timeToAp", "Target Time to Apoapsis", Units = APIEntry.UnitType.TIME)]
        object TargetTimeToAp(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToAp : 0;

        [TelemetryAPI("tar.o.timeToPe", "Target Time to Periapsis", Units = APIEntry.UnitType.TIME)]
        object TargetTimeToPe(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToPe : 0;

        [TelemetryAPI("tar.o.inclination", "Target Inclination", Units = APIEntry.UnitType.DEG)]
        object TargetInclination(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().inclination : 0;

        [TelemetryAPI("tar.o.eccentricity", "Target Eccentricity")]
        object TargetEccentricity(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().eccentricity : 0;

        [TelemetryAPI("tar.o.period", "Target Orbital Period", Units = APIEntry.UnitType.TIME)]
        object TargetPeriod(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().period : 0;

        [TelemetryAPI("tar.o.argumentOfPeriapsis", "Target Argument of Periapsis", Units = APIEntry.UnitType.DEG)]
        object TargetArgumentOfPeriapsis(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().argumentOfPeriapsis : 0;

        [TelemetryAPI("tar.o.timeToTransition1", "Target Time to Transition 1", Units = APIEntry.UnitType.TIME)]
        object TargetTimeToTransition1(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToTransition1 : 0;

        [TelemetryAPI("tar.o.timeToTransition2", "Target Time to Transition 2", Units = APIEntry.UnitType.TIME)]
        object TargetTimeToTransition2(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().timeToTransition2 : 0;

        [TelemetryAPI("tar.o.sma", "Target Semimajor Axis", Units = APIEntry.UnitType.DISTANCE)]
        object TargetSma(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().semiMajorAxis : 0;

        [TelemetryAPI("tar.o.lan", "Target Longitude of Ascending Node", Units = APIEntry.UnitType.DEG)]
        object TargetLan(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().LAN : 0;

        [TelemetryAPI("tar.o.maae", "Target Mean Anomaly at Epoch")]
        object TargetMaae(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Orbit orbit = FlightGlobals.fetch.VesselTarget.GetOrbit();
            return orbit.getObtAtUT(0) / orbit.period * (2.0 * Math.PI);
        }

        [TelemetryAPI("tar.o.timeOfPeriapsisPassage", "Target Time of Periapsis Passage", Units = APIEntry.UnitType.DATE)]
        object TargetTimeOfPeriapsisPassage(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? Planetarium.GetUniversalTime() - FlightGlobals.fetch.VesselTarget.GetOrbit().ObT : 0;

        [TelemetryAPI("tar.o.trueAnomaly", "Target True Anomaly", Units = APIEntry.UnitType.DEG)]
        object TargetTrueAnomaly(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? FlightGlobals.fetch.VesselTarget.GetOrbit().TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI) : double.NaN;

        [TelemetryAPI("tar.o.orbitingBody", "Target Orbiting Body", Units = APIEntry.UnitType.STRING)]
        object TargetOrbitingBody(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null ? FlightGlobals.fetch.VesselTarget.GetOrbit().referenceBody.name : "";

        [TelemetryAPI("tar.o.orbitPatches", "Detailed Orbit Patches Info [object orbitPatchInfo]",
            Plotable = false, Formatter = "OrbitPatchList")]
        object TargetOrbitPatches(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return null;
            return OrbitPatches.getPatchesForOrbit(FlightGlobals.fetch.VesselTarget.GetOrbit());
        }

        [TelemetryAPI("tar.o.trueAnomalyAtUTForOrbitPatch",
            "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]",
            Units = APIEntry.UnitType.DEG)]
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
            "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]",
            Units = APIEntry.UnitType.DATE)]
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
            "The orbit patch's predicted displacement from the center of the main body at the given true anomaly [orbit patch index, true anomaly]",
            Formatter = "Vector3d")]
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
            "The orbit patch's predicted displacement from the center of the main body at the given universal time [orbit patch index, universal time]",
            Formatter = "Vector3d")]
        object TargetRelativePositionAtUTForOrbitPatch(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return null;
            int index = int.Parse(ds.args[0]);
            double ut = double.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(FlightGlobals.fetch.VesselTarget.GetOrbit(), index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionAtUT(ut);
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

        [TelemetryAPI("dock.ax", "Docking x Angle", Units = APIEntry.UnitType.DEG)]
        object DockAx(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Update();
            return orientationDeviation.x;
        }

        [TelemetryAPI("dock.ay", "Relative Pitch Angle", Units = APIEntry.UnitType.DEG)]
        object DockAy(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Update();
            return orientationDeviation.y;
        }

        [TelemetryAPI("dock.az", "Docking z Angle", Units = APIEntry.UnitType.DEG)]
        object DockAz(DataSources ds)
        {
            if (FlightGlobals.fetch.VesselTarget == null) return 0;
            Update();
            return orientationDeviation.z;
        }

        [TelemetryAPI("dock.x", "Target x Distance", Units = APIEntry.UnitType.DISTANCE)]
        object DockX(DataSources ds) =>
            FlightGlobals.fetch.VesselTarget != null
                ? (FlightGlobals.fetch.VesselTarget.GetTransform().position - ds.vessel.GetTransform().position).x : 0;

        [TelemetryAPI("dock.y", "Target y Distance", Units = APIEntry.UnitType.DISTANCE)]
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
