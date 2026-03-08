using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telemachus
{
    public class VesselDataLinkHandler : DataLinkHandler
    {
        public VesselDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("v.altitude", "Altitude", Units = APIEntry.UnitType.DISTANCE)]
        object Altitude(DataSources ds) => ds.vessel.altitude;

        [TelemetryAPI("v.heightFromTerrain", "Height from Terrain", Units = APIEntry.UnitType.DISTANCE)]
        object HeightFromTerrain(DataSources ds) => ds.vessel.heightFromTerrain;

        [TelemetryAPI("v.terrainHeight", "Terrain Height", Units = APIEntry.UnitType.DISTANCE)]
        object TerrainHeight(DataSources ds) => ds.vessel.altitude - ds.vessel.heightFromTerrain;

        [TelemetryAPI("v.missionTime", "Mission Time", Units = APIEntry.UnitType.TIME)]
        object MissionTime(DataSources ds) => ds.vessel.missionTime;

        [TelemetryAPI("v.surfaceVelocity", "Surface Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocity(DataSources ds) => ds.vessel.srf_velocity.magnitude;

        [TelemetryAPI("v.surfaceVelocityx", "Surface Velocity x", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocityX(DataSources ds) => ds.vessel.srf_velocity.x;

        [TelemetryAPI("v.surfaceVelocityy", "Surface Velocity y", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocityY(DataSources ds) => ds.vessel.srf_velocity.y;

        [TelemetryAPI("v.surfaceVelocityz", "Surface Velocity z", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocityZ(DataSources ds) => ds.vessel.srf_velocity.z;

        [TelemetryAPI("v.angularVelocity", "Angular Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object AngularVelocity(DataSources ds) => ds.vessel.angularVelocity.magnitude;

        [TelemetryAPI("v.orbitalVelocity", "Orbital Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalVelocity(DataSources ds) => ds.vessel.obt_velocity.magnitude;

        [TelemetryAPI("v.surfaceSpeed", "Surface Speed", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceSpeed(DataSources ds) => ds.vessel.horizontalSrfSpeed;

        [TelemetryAPI("v.verticalSpeed", "Vertical Speed", Units = APIEntry.UnitType.VELOCITY)]
        object VerticalSpeed(DataSources ds) => ds.vessel.verticalSpeed;

        [TelemetryAPI("v.geeForce", "G-Force", Units = APIEntry.UnitType.G)]
        object GeeForce(DataSources ds) => ds.vessel.geeForce;

        [TelemetryAPI("v.atmosphericDensity", "Atmospheric Density")]
        object AtmosphericDensity(DataSources ds)
        {
            double atmosphericPressure = FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody);
            double temperature = FlightGlobals.getExternalTemperature(ds.vessel.altitude, ds.vessel.mainBody);
            double atmosphericDensityinKilograms = FlightGlobals.getAtmDensity(atmosphericPressure, temperature);
            return atmosphericDensityinKilograms * 1000;
        }

        [TelemetryAPI("v.long", "Longitude", Units = APIEntry.UnitType.LATLON)]
        object Longitude(DataSources ds) =>
            ds.vessel.longitude > 180 ? ds.vessel.longitude - 360.0 : ds.vessel.longitude;

        [TelemetryAPI("v.lat", "Latitude", Units = APIEntry.UnitType.LATLON)]
        object Latitude(DataSources ds) => ds.vessel.latitude;

        [TelemetryAPI("v.dynamicPressure", "Dynamic Pressure")]
        object DynamicPressure(DataSources ds) =>
            (ds.vessel.atmDensity * 0.5) * Math.Pow(ds.vessel.srf_velocity.magnitude, 2);

        [TelemetryAPI("v.atmosphericPressurePa", "Atmospheric Pressure (Pa)")]
        object AtmosphericPressurePa(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody) * 1000;

        [TelemetryAPI("v.atmosphericPressure", "Atmospheric Pressure")]
        object AtmosphericPressure(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody) * PhysicsGlobals.KpaToAtmospheres;

        [TelemetryAPI("v.name", "Name", Units = APIEntry.UnitType.STRING)]
        object Name(DataSources ds) => ds.vessel.name;

        [TelemetryAPI("v.body", "Body Name", Units = APIEntry.UnitType.STRING)]
        object Body(DataSources ds) => ds.vessel.orbit.referenceBody.name;

        [TelemetryAPI("v.angleToPrograde", "Angle to Prograde", Units = APIEntry.UnitType.DEG)]
        object AngleToPrograde(DataSources ds)
        {
            if (ds.vessel.mainBody == Planetarium.fetch.Sun)
                return double.NaN;

            double ut = Planetarium.GetUniversalTime();
            CelestialBody body = ds.vessel.mainBody;
            Vector3d bodyPrograde = body.orbit.getOrbitalVelocityAtUT(ut);
            Vector3d bodyNormal = body.orbit.GetOrbitNormal();
            Vector3d vesselPos = ds.vessel.orbit.getRelativePositionAtUT(ut);
            Vector3d vesselPosInPlane = Vector3d.Exclude(bodyNormal, vesselPos);
            double angle = Vector3d.Angle(vesselPosInPlane, bodyPrograde);
            if (Vector3d.Dot(Vector3d.Cross(vesselPosInPlane, bodyPrograde), bodyNormal) < 0)
                angle = 360 - angle;
            if (ds.vessel.orbit.GetOrbitNormal().z < 0)
                angle = 360 - angle;
            return angle;
        }
    }

    public class OrbitDataLinkHandler : DataLinkHandler
    {
        public OrbitDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("o.relativeVelocity", "Relative Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object RelativeVelocity(DataSources ds) => ds.vessel.orbit.GetRelativeVel().magnitude;

        [TelemetryAPI("o.PeA", "Periapsis", Units = APIEntry.UnitType.DISTANCE)]
        object PeA(DataSources ds) => ds.vessel.orbit.PeA;

        [TelemetryAPI("o.ApA", "Apoapsis", Units = APIEntry.UnitType.DISTANCE)]
        object ApA(DataSources ds) => ds.vessel.orbit.ApA;

        [TelemetryAPI("o.timeToAp", "Time to Apoapsis", Units = APIEntry.UnitType.TIME)]
        object TimeToAp(DataSources ds) => ds.vessel.orbit.timeToAp;

        [TelemetryAPI("o.timeToPe", "Time to Periapsis", Units = APIEntry.UnitType.TIME)]
        object TimeToPe(DataSources ds) => ds.vessel.orbit.timeToPe;

        [TelemetryAPI("o.inclination", "Inclination", Units = APIEntry.UnitType.DEG)]
        object Inclination(DataSources ds) => ds.vessel.orbit.inclination;

        [TelemetryAPI("o.eccentricity", "Eccentricity")]
        object Eccentricity(DataSources ds) => ds.vessel.orbit.eccentricity;

        [TelemetryAPI("o.epoch", "Epoch")]
        object Epoch(DataSources ds) => ds.vessel.orbit.epoch;

        [TelemetryAPI("o.period", "Orbital Period", Units = APIEntry.UnitType.TIME)]
        object Period(DataSources ds) => ds.vessel.orbit.period;

        [TelemetryAPI("o.argumentOfPeriapsis", "Argument of Periapsis", Units = APIEntry.UnitType.DEG)]
        object ArgumentOfPeriapsis(DataSources ds) => ds.vessel.orbit.argumentOfPeriapsis;

        [TelemetryAPI("o.timeToTransition1", "Time to Transition 1", Units = APIEntry.UnitType.TIME)]
        object TimeToTransition1(DataSources ds) => ds.vessel.orbit.timeToTransition1;

        [TelemetryAPI("o.timeToTransition2", "Time to Transition 2", Units = APIEntry.UnitType.TIME)]
        object TimeToTransition2(DataSources ds) => ds.vessel.orbit.timeToTransition2;

        [TelemetryAPI("o.sma", "Semimajor Axis", Units = APIEntry.UnitType.DISTANCE)]
        object Sma(DataSources ds) => ds.vessel.orbit.semiMajorAxis;

        [TelemetryAPI("o.lan", "Longitude of Ascending Node", Units = APIEntry.UnitType.DEG)]
        object Lan(DataSources ds) => ds.vessel.orbit.LAN;

        [TelemetryAPI("o.maae", "Mean Anomaly at Epoch")]
        object Maae(DataSources ds)
        {
            Orbit orbit = ds.vessel.orbit;
            return orbit.getObtAtUT(0) / orbit.period * (2.0 * Math.PI);
        }

        [TelemetryAPI("o.timeOfPeriapsisPassage", "Time of Periapsis Passage", Units = APIEntry.UnitType.DATE)]
        object TimeOfPeriapsisPassage(DataSources ds) =>
            Planetarium.GetUniversalTime() - ds.vessel.orbit.ObT;

        [TelemetryAPI("o.trueAnomaly", "True Anomaly", Units = APIEntry.UnitType.DEG)]
        object TrueAnomaly(DataSources ds) =>
            ds.vessel.orbit.TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI);

        [TelemetryAPI("o.orbitPatches", "Detailed Orbit Patches Info [object orbitPatchInfo]",
            Plotable = false, Formatter = "OrbitPatchList")]
        object GetOrbitPatches(DataSources ds) =>
            OrbitPatches.getPatchesForOrbit(ds.vessel.orbit);

        [TelemetryAPI("o.trueAnomalyAtUTForOrbitPatch",
            "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]",
            Units = APIEntry.UnitType.DEG)]
        object TrueAnomalyAtUTForOrbitPatch(DataSources ds)
        {
            int index = int.Parse(ds.args[0]);
            float ut = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(ds.vessel.orbit, index);
            if (orbitPatch == null) return null;
            return orbitPatch.TrueAnomalyAtUT(ut);
        }

        [TelemetryAPI("o.UTForTrueAnomalyForOrbitPatch",
            "The orbit patch's True Anomaly at Universal Time [orbit patch index, universal time]",
            Units = APIEntry.UnitType.DATE)]
        object UTForTrueAnomalyForOrbitPatch(DataSources ds)
        {
            int index = int.Parse(ds.args[0]);
            float trueAnomaly = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(ds.vessel.orbit, index);
            if (orbitPatch == null) return null;
            double now = Planetarium.GetUniversalTime();
            return orbitPatch.GetUTforTrueAnomaly(trueAnomaly, now);
        }

        [TelemetryAPI("o.relativePositionAtTrueAnomalyForOrbitPatch",
            "The orbit patch's predicted displacement from the center of the main body at the given true anomaly [orbit patch index, true anomaly]",
            Formatter = "Vector3d")]
        object RelativePositionAtTrueAnomalyForOrbitPatch(DataSources ds)
        {
            int index = int.Parse(ds.args[0]);
            float trueAnomaly = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(ds.vessel.orbit, index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionFromTrueAnomaly(trueAnomaly);
        }

        [TelemetryAPI("o.relativePositionAtUTForOrbitPatch",
            "The orbit patch's predicted displacement from the center of the main body at the given universal time [orbit patch index, universal time]",
            Formatter = "Vector3d")]
        object RelativePositionAtUTForOrbitPatch(DataSources ds)
        {
            int index = int.Parse(ds.args[0]);
            double ut = double.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(ds.vessel.orbit, index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionAtUT(ut);
        }
    }

    public class NavBallDataLinkHandler : DataLinkHandler
    {
        public NavBallDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("n.heading2", "Heading", Units = APIEntry.UnitType.DEG)]
        object Heading2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.y;

        [TelemetryAPI("n.pitch2", "Pitch", Units = APIEntry.UnitType.DEG)]
        object Pitch2(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM);
            return (result.eulerAngles.x > 180) ? (360.0 - result.eulerAngles.x) : -result.eulerAngles.x;
        }

        [TelemetryAPI("n.roll2", "Roll", Units = APIEntry.UnitType.DEG)]
        object Roll2(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM);
            return (result.eulerAngles.z > 180) ? (result.eulerAngles.z - 360.0) : result.eulerAngles.z;
        }

        [TelemetryAPI("n.rawheading2", "Raw Heading", Units = APIEntry.UnitType.DEG)]
        object RawHeading2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.y;

        [TelemetryAPI("n.rawpitch2", "Raw Pitch", Units = APIEntry.UnitType.DEG)]
        object RawPitch2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.x;

        [TelemetryAPI("n.rawroll2", "Raw Roll", Units = APIEntry.UnitType.DEG)]
        object RawRoll2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.z;

        [TelemetryAPI("n.heading", "Heading calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG)]
        object Heading(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position).eulerAngles.y;

        [TelemetryAPI("n.pitch", "Pitch calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG)]
        object Pitch(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position);
            return (result.eulerAngles.x > 180) ? (360.0 - result.eulerAngles.x) : -result.eulerAngles.x;
        }

        [TelemetryAPI("n.roll", "Roll calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG)]
        object Roll(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position);
            return (result.eulerAngles.z > 180) ? (result.eulerAngles.z - 360.0) : result.eulerAngles.z;
        }

        [TelemetryAPI("n.rawheading", "Raw Heading calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG)]
        object RawHeading(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position).eulerAngles.y;

        [TelemetryAPI("n.rawpitch", "Raw Pitch calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG)]
        object RawPitch(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position).eulerAngles.x;

        [TelemetryAPI("n.rawroll", "Raw Roll calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG)]
        object RawRoll(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position).eulerAngles.z;

        // Borrowed from MechJeb2
        private Quaternion UpdateHeadingPitchRoll(Vessel v, Vector3d CoM)
        {
            Vector3d up = (CoM - v.mainBody.position).normalized;
            Vector3d north = Vector3d.Exclude(up, (v.mainBody.position + v.mainBody.transform.up *
                (float)v.mainBody.Radius) - CoM).normalized;
            Quaternion rotationSurface = Quaternion.LookRotation(north, up);
            return Quaternion.Inverse(Quaternion.Euler(90, 0, 0) *
                Quaternion.Inverse(v.GetTransform().rotation) * rotationSurface);
        }
    }

    public class BodyDataLinkHandler : DataLinkHandler
    {
        public BodyDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("b.name", "Body Name [body id]", Units = APIEntry.UnitType.STRING)]
        object BodyName(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].name;

        [TelemetryAPI("b.maxAtmosphere", "Body Atmosphere Depth [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object MaxAtmosphere(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphereDepth;

        [TelemetryAPI("b.radius", "Body Radius [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object Radius(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].Radius;

        [TelemetryAPI("b.atmosphereContainsOxygen", "Atmosphere contains oxygen [body id]")]
        object AtmosphereContainsOxygen(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphereContainsOxygen;

        [TelemetryAPI("b.soi", "Body Sphere of Influence [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object Soi(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].sphereOfInfluence;

        [TelemetryAPI("b.rotationPeriod", "Rotation Period [body id]")]
        object RotationPeriod(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].rotationPeriod;

        [TelemetryAPI("b.tidallyLocked", "Tidally Locked [body id]")]
        object TidallyLocked(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].tidallyLocked;

        [TelemetryAPI("b.number", "Number of Bodies")]
        object BodyCount(DataSources ds) => FlightGlobals.Bodies.Count;

        [TelemetryAPI("b.o.gravParameter", "Body Gravitational Parameter [body id]", Units = APIEntry.UnitType.GRAV)]
        object GravParameter(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].gravParameter;

        [TelemetryAPI("b.o.relativeVelocity", "Relative Velocity [body id]", Units = APIEntry.UnitType.VELOCITY)]
        object BodyRelativeVelocity(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.GetRelativeVel().magnitude;

        [TelemetryAPI("b.o.PeA", "Periapsis [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object BodyPeA(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.PeA;

        [TelemetryAPI("b.o.ApA", "Apoapsis [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object BodyApA(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.ApA;

        [TelemetryAPI("b.o.timeToAp", "Time to Apoapsis [body id]", Units = APIEntry.UnitType.TIME)]
        object BodyTimeToAp(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToAp;

        [TelemetryAPI("b.o.timeToPe", "Time to Periapsis [body id]", Units = APIEntry.UnitType.TIME)]
        object BodyTimeToPe(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToPe;

        [TelemetryAPI("b.o.inclination", "Inclination [body id]", Units = APIEntry.UnitType.DEG)]
        object BodyInclination(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.inclination;

        [TelemetryAPI("b.o.eccentricity", "Eccentricity [body id]")]
        object BodyEccentricity(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.eccentricity;

        [TelemetryAPI("b.o.period", "Orbital Period [body id]", Units = APIEntry.UnitType.TIME)]
        object BodyPeriod(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.period;

        [TelemetryAPI("b.o.argumentOfPeriapsis", "Argument of Periapsis [body id]", Units = APIEntry.UnitType.DEG)]
        object BodyArgumentOfPeriapsis(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.argumentOfPeriapsis;

        [TelemetryAPI("b.o.timeToTransition1", "Time to Transition 1 [body id]", Units = APIEntry.UnitType.TIME)]
        object BodyTimeToTransition1(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToTransition1;

        [TelemetryAPI("b.o.timeToTransition2", "Time to Transition 2 [body id]", Units = APIEntry.UnitType.TIME)]
        object BodyTimeToTransition2(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToTransition2;

        [TelemetryAPI("b.o.sma", "Semimajor Axis [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object BodySma(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.semiMajorAxis;

        [TelemetryAPI("b.o.lan", "Longitude of Ascending Node [body id]", Units = APIEntry.UnitType.DEG)]
        object BodyLan(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.LAN;

        [TelemetryAPI("b.o.maae", "Mean Anomaly at Epoch [body id]")]
        object BodyMaae(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.meanAnomalyAtEpoch;

        [TelemetryAPI("b.o.timeOfPeriapsisPassage", "Time of Periapsis Passage [body id]", Units = APIEntry.UnitType.DATE)]
        object BodyTimeOfPeriapsisPassage(DataSources ds) =>
            Planetarium.GetUniversalTime() - FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.ObT;

        [TelemetryAPI("b.o.trueAnomaly", "True Anomaly [body id]", Units = APIEntry.UnitType.DEG)]
        object BodyTrueAnomaly(DataSources ds) =>
            FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI);

        [TelemetryAPI("b.o.phaseAngle", "Phase Angle [body id]", Units = APIEntry.UnitType.DEG)]
        object BodyPhaseAngle(DataSources ds)
        {
            CelestialBody body = FlightGlobals.Bodies[int.Parse(ds.args[0])];

            var parentBodies = new List<CelestialBody>();
            CelestialBody parentBody = ds.vessel.mainBody;
            while (true)
            {
                if (parentBody == body) return double.NaN;
                parentBodies.Add(parentBody);
                if (parentBody == Planetarium.fetch.Sun) break;
                parentBody = parentBody.referenceBody;
            }

            while (!parentBodies.Contains(body.referenceBody))
                body = body.referenceBody;

            Orbit orbit = ds.vessel.orbit;
            while (orbit.referenceBody != body.referenceBody)
                orbit = orbit.referenceBody.orbit;

            double ut = Planetarium.GetUniversalTime();
            Vector3d vesselPos = orbit.getRelativePositionAtUT(ut);
            Vector3d bodyPos = body.orbit.getRelativePositionAtUT(ut);
            double phaseAngle = (Math.Atan2(bodyPos.y, bodyPos.x) - Math.Atan2(vesselPos.y, vesselPos.x)) * (180.0 / Math.PI);
            return (phaseAngle < 0) ? phaseAngle + 360 : phaseAngle;
        }

        [TelemetryAPI("b.o.truePositionAtUT", "True Position at the given UT [body id, universal time]",
            Plotable = false, Formatter = "Vector3d")]
        object BodyTruePositionAtUT(DataSources ds)
        {
            int bodyId = int.Parse(ds.args[0]);
            float universalTime = float.Parse(ds.args[1]);
            return FlightGlobals.Bodies[bodyId].getTruePositionAtUT(universalTime);
        }
    }

    public class MapViewDataLinkHandler : DataLinkHandler
    {
        static float ut = 0, x = 0, y = 0, z = 0;

        public MapViewDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("m.toggleMapView", " Toggle Map View", IsAction = true)]
        object ToggleMapView(DataSources ds)
        {
            if (MapView.MapIsEnabled) MapView.ExitMapView(); else MapView.EnterMapView();
            return 0d;
        }

        [TelemetryAPI("m.enterMapView", " Enter Map View", IsAction = true)]
        object EnterMapView(DataSources ds) { MapView.EnterMapView(); return 0d; }

        [TelemetryAPI("m.exitMapView", " Exit Map View", IsAction = true)]
        object ExitMapView(DataSources ds) { MapView.ExitMapView(); return 0d; }

        [TelemetryAPI("o.maneuverNodes", "Maneuver Nodes  [object maneuverNodes]",
            Plotable = false, Formatter = "ManeuverNodeList")]
        object ManeuverNodes(DataSources ds)
        {
            PluginLogger.debug("Start GET");
            return ds.vessel.patchedConicSolver.maneuverNodes;
        }

        [TelemetryAPI("o.maneuverNodes.trueAnomalyAtUTForManeuverNodesOrbitPatch",
            "For a maneuver node, The orbit patch's True Anomaly at Universal Time [int id, orbit patch index, universal time]",
            Units = APIEntry.UnitType.DEG)]
        object TrueAnomalyAtUTForManeuverNodesOrbitPatch(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            int index = int.Parse(ds.args[1]);
            float utArg = float.Parse(ds.args[2]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
            if (orbitPatch == null) return null;
            return orbitPatch.TrueAnomalyAtUT(utArg);
        }

        [TelemetryAPI("o.maneuverNodes.UTForTrueAnomalyForManeuverNodesOrbitPatch",
            "For a maneuver node, The orbit patch's True Anomaly at Universal Time [int id, orbit patch index, universal time]",
            Units = APIEntry.UnitType.DATE)]
        object UTForTrueAnomalyForManeuverNodesOrbitPatch(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            int index = int.Parse(ds.args[1]);
            float trueAnomaly = float.Parse(ds.args[2]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
            if (orbitPatch == null) return null;
            double now = Planetarium.GetUniversalTime();
            return orbitPatch.GetUTforTrueAnomaly(trueAnomaly, now);
        }

        [TelemetryAPI("o.maneuverNodes.relativePositionAtTrueAnomalyForManeuverNodesOrbitPatch",
            "For a maneuver node, The orbit patch's predicted displacement from the center of the main body at the given true anomaly [int id, orbit patch index, true anomaly]",
            Formatter = "Vector3d")]
        object RelativePositionAtTrueAnomalyForManeuverNodesOrbitPatch(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            int index = int.Parse(ds.args[1]);
            float trueAnomaly = float.Parse(ds.args[2]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionFromTrueAnomaly(trueAnomaly);
        }

        [TelemetryAPI("o.maneuverNodes.relativePositionAtUTForManeuverNodesOrbitPatch",
            "For a maneuver node, The orbit patch's predicted displacement from the center of the main body at the given universal time [int id, orbit patch index, universal time]",
            Formatter = "Vector3d")]
        object RelativePositionAtUTForManeuverNodesOrbitPatch(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            int index = int.Parse(ds.args[1]);
            double utArg = double.Parse(ds.args[2]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(node.nextPatch, index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionAtUT(utArg);
        }

        // These stay as manual registerAPI since they need queueDelayed wrapping
        // or special formatter handling that doesn't fit the attribute pattern cleanly.
        // The constructor calls base() which handles [TelemetryAPI] methods,
        // then we add the remaining manual registrations.

        [TelemetryAPI("o.addManeuverNode",
            "Add a manuever based on a UT and DeltaV X, Y and Z [float ut, float x, y, z]",
            IsAction = true, Formatter = "ManeuverNode")]
        object AddManeuverNode(DataSources ds)
        {
            ut = float.Parse(ds.args[0]);
            ManeuverNode node = ds.vessel.patchedConicSolver.AddManeuverNode(ut);
            x = float.Parse(ds.args[1]);
            y = float.Parse(ds.args[2]);
            z = float.Parse(ds.args[3]);
            PluginLogger.debug("x: " + x + "y: " + y + "z: " + z);
            Vector3d deltaV = new Vector3d(x, y, z);
            node.OnGizmoUpdated(deltaV, ut);
            return node;
        }

        [TelemetryAPI("o.updateManeuverNode",
            "Set a manuever node's UT and DeltaV X, Y and Z [int id, float ut, float x, y, z]",
            IsAction = true, Formatter = "ManeuverNode")]
        object UpdateManeuverNode(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            ut = float.Parse(ds.args[1]);
            x = float.Parse(ds.args[2]);
            y = float.Parse(ds.args[3]);
            z = float.Parse(ds.args[4]);
            Vector3d deltaV = new Vector3d(x, y, z);
            node.OnGizmoUpdated(deltaV, ut);
            return node;
        }

        [TelemetryAPI("o.removeManeuverNode", "Remove a manuever node [int id]", IsAction = true)]
        object RemoveManeuverNode(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return false;
            ds.vessel.patchedConicSolver.RemoveManeuverNode(node);
            return true;
        }

        private ManeuverNode GetManeuverNode(DataSources datasources, int id)
        {
            PluginLogger.debug("GETTING NODE");
            if (datasources.vessel.patchedConicSolver.maneuverNodes.Count <= id || id < 0)
                return null;
            PluginLogger.debug("FINDING THE RIGHT NODE. ID: " + id);
            ManeuverNode[] nodes = datasources.vessel.patchedConicSolver.maneuverNodes.ToArray();
            return (ManeuverNode)nodes.GetValue(id);
        }
    }

    public class LangDataLinkHandler : DataLinkHandler
    {
        public LangDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("o.gameLanguage", "Language  [object gameLanguage]", Units = APIEntry.UnitType.STRING)]
        object GameLanguage(DataSources ds)
        {
            PluginLogger.debug("Start GET");
            return KSP.Localization.Localizer.CurrentLanguage;
        }
    }
}
