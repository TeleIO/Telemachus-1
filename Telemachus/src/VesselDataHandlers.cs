using System;
using System.Collections.Generic;
using UnityEngine;

namespace Telemachus
{
    public class VesselDataLinkHandler : DataLinkHandler
    {
        public VesselDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // --- Position & Altitude ---

        [TelemetryAPI("v.altitude", "Altitude", Units = APIEntry.UnitType.DISTANCE)]
        object Altitude(DataSources ds) => ds.vessel.altitude;

        [TelemetryAPI("v.heightFromTerrain", "Height from Terrain", Units = APIEntry.UnitType.DISTANCE)]
        object HeightFromTerrain(DataSources ds) => ds.vessel.heightFromTerrain;

        [TelemetryAPI("v.heightFromSurface", "Height from Surface", Units = APIEntry.UnitType.DISTANCE)]
        object HeightFromSurface(DataSources ds) => ds.vessel.heightFromSurface;

        [TelemetryAPI("v.terrainHeight", "Terrain Height", Units = APIEntry.UnitType.DISTANCE)]
        object TerrainHeight(DataSources ds) => ds.vessel.terrainAltitude;

        [TelemetryAPI("v.pqsAltitude", "PQS Terrain Altitude", Units = APIEntry.UnitType.DISTANCE)]
        object PqsAltitude(DataSources ds) => ds.vessel.pqsAltitude;

        [TelemetryAPI("v.long", "Longitude", Units = APIEntry.UnitType.LATLON)]
        object Longitude(DataSources ds) =>
            ds.vessel.longitude > 180 ? ds.vessel.longitude - 360.0 : ds.vessel.longitude;

        [TelemetryAPI("v.lat", "Latitude", Units = APIEntry.UnitType.LATLON)]
        object Latitude(DataSources ds) => ds.vessel.latitude;

        // --- Velocity ---

        [TelemetryAPI("v.surfaceVelocity", "Surface Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocity(DataSources ds) => ds.vessel.srf_velocity.magnitude;

        [TelemetryAPI("v.surfaceVelocityx", "Surface Velocity x", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocityX(DataSources ds) => ds.vessel.srf_velocity.x;

        [TelemetryAPI("v.surfaceVelocityy", "Surface Velocity y", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocityY(DataSources ds) => ds.vessel.srf_velocity.y;

        [TelemetryAPI("v.surfaceVelocityz", "Surface Velocity z", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceVelocityZ(DataSources ds) => ds.vessel.srf_velocity.z;

        [TelemetryAPI("v.orbitalVelocity", "Orbital Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalVelocity(DataSources ds) => ds.vessel.obt_velocity.magnitude;

        [TelemetryAPI("v.orbitalVelocityx", "Orbital Velocity x", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalVelocityX(DataSources ds) => ds.vessel.obt_velocity.x;

        [TelemetryAPI("v.orbitalVelocityy", "Orbital Velocity y", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalVelocityY(DataSources ds) => ds.vessel.obt_velocity.y;

        [TelemetryAPI("v.orbitalVelocityz", "Orbital Velocity z", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalVelocityZ(DataSources ds) => ds.vessel.obt_velocity.z;

        [TelemetryAPI("v.surfaceSpeed", "Surface Speed", Units = APIEntry.UnitType.VELOCITY)]
        object SurfaceSpeed(DataSources ds) => ds.vessel.horizontalSrfSpeed;

        [TelemetryAPI("v.verticalSpeed", "Vertical Speed", Units = APIEntry.UnitType.VELOCITY)]
        object VerticalSpeed(DataSources ds) => ds.vessel.verticalSpeed;

        [TelemetryAPI("v.speed", "Speed", Units = APIEntry.UnitType.VELOCITY)]
        object Speed(DataSources ds) => ds.vessel.speed;

        [TelemetryAPI("v.srfSpeed", "Surface Speed (direct)", Units = APIEntry.UnitType.VELOCITY)]
        object SrfSpeed(DataSources ds) => ds.vessel.srfSpeed;

        [TelemetryAPI("v.obtSpeed", "Orbital Speed (direct)", Units = APIEntry.UnitType.VELOCITY)]
        object ObtSpeed(DataSources ds) => ds.vessel.obt_speed;

        [TelemetryAPI("v.angularVelocity", "Angular Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object AngularVelocity(DataSources ds) => ds.vessel.angularVelocity.magnitude;

        [TelemetryAPI("v.angularVelocityx", "Angular Velocity x", Units = APIEntry.UnitType.VELOCITY)]
        object AngularVelocityX(DataSources ds) => ds.vessel.angularVelocity.x;

        [TelemetryAPI("v.angularVelocityy", "Angular Velocity y", Units = APIEntry.UnitType.VELOCITY)]
        object AngularVelocityY(DataSources ds) => ds.vessel.angularVelocity.y;

        [TelemetryAPI("v.angularVelocityz", "Angular Velocity z", Units = APIEntry.UnitType.VELOCITY)]
        object AngularVelocityZ(DataSources ds) => ds.vessel.angularVelocity.z;

        // --- Acceleration & Forces ---

        [TelemetryAPI("v.geeForce", "G-Force", Units = APIEntry.UnitType.G)]
        object GeeForce(DataSources ds) => ds.vessel.geeForce;

        [TelemetryAPI("v.geeForceImmediate", "Instantaneous G-Force", Units = APIEntry.UnitType.G)]
        object GeeForceImmediate(DataSources ds) => ds.vessel.geeForce_immediate;

        [TelemetryAPI("v.acceleration", "Acceleration Magnitude", Units = APIEntry.UnitType.ACC)]
        object Acceleration(DataSources ds) => ds.vessel.acceleration.magnitude;

        [TelemetryAPI("v.accelerationx", "Acceleration x", Units = APIEntry.UnitType.ACC)]
        object AccelerationX(DataSources ds) => ds.vessel.acceleration.x;

        [TelemetryAPI("v.accelerationy", "Acceleration y", Units = APIEntry.UnitType.ACC)]
        object AccelerationY(DataSources ds) => ds.vessel.acceleration.y;

        [TelemetryAPI("v.accelerationz", "Acceleration z", Units = APIEntry.UnitType.ACC)]
        object AccelerationZ(DataSources ds) => ds.vessel.acceleration.z;

        [TelemetryAPI("v.specificAcceleration", "Specific Acceleration (thrust/mass)", Units = APIEntry.UnitType.ACC)]
        object SpecificAcceleration(DataSources ds) => ds.vessel.specificAcceleration;

        [TelemetryAPI("v.perturbation", "Orbital Perturbation Magnitude")]
        object Perturbation(DataSources ds) => ds.vessel.perturbation.magnitude;

        [TelemetryAPI("v.perturbationx", "Orbital Perturbation x")]
        object PerturbationX(DataSources ds) => ds.vessel.perturbation.x;

        [TelemetryAPI("v.perturbationy", "Orbital Perturbation y")]
        object PerturbationY(DataSources ds) => ds.vessel.perturbation.y;

        [TelemetryAPI("v.perturbationz", "Orbital Perturbation z")]
        object PerturbationZ(DataSources ds) => ds.vessel.perturbation.z;

        // --- Mass & Inertia ---

        [TelemetryAPI("v.mass", "Total Mass")]
        object Mass(DataSources ds) => ds.vessel.GetTotalMass();

        [TelemetryAPI("v.angularMomentum", "Angular Momentum Magnitude")]
        object AngularMomentum(DataSources ds) => ds.vessel.angularMomentum.magnitude;

        [TelemetryAPI("v.angularMomentumx", "Angular Momentum x")]
        object AngularMomentumX(DataSources ds) => ds.vessel.angularMomentum.x;

        [TelemetryAPI("v.angularMomentumy", "Angular Momentum y")]
        object AngularMomentumY(DataSources ds) => ds.vessel.angularMomentum.y;

        [TelemetryAPI("v.angularMomentumz", "Angular Momentum z")]
        object AngularMomentumZ(DataSources ds) => ds.vessel.angularMomentum.z;

        [TelemetryAPI("v.momentOfInertia", "Moment of Inertia", Plotable = false, Formatter = "Vector3d")]
        object MomentOfInertia(DataSources ds) => new Vector3d(ds.vessel.MOI.x, ds.vessel.MOI.y, ds.vessel.MOI.z);

        [TelemetryAPI("v.CoM", "Center of Mass", Plotable = false, Formatter = "Vector3d")]
        object CenterOfMass(DataSources ds) => new Vector3d(ds.vessel.CoM.x, ds.vessel.CoM.y, ds.vessel.CoM.z);

        // --- Atmosphere & Environment ---

        [TelemetryAPI("v.atmosphericDensity", "Atmospheric Density", Units = APIEntry.UnitType.DENSITY)]
        object AtmosphericDensity(DataSources ds) => ds.vessel.atmDensity;

        [TelemetryAPI("v.dynamicPressurekPa", "Dynamic Pressure (kPa)", Units = APIEntry.UnitType.DYNAMICPRESSURE)]
        object DynamicPressurekPa(DataSources ds) => ds.vessel.dynamicPressurekPa;

        [TelemetryAPI("v.dynamicPressure", "Dynamic Pressure (Pa)", Units = APIEntry.UnitType.DYNAMICPRESSURE)]
        object DynamicPressure(DataSources ds) =>
            (ds.vessel.atmDensity * 0.5) * Math.Pow(ds.vessel.srf_velocity.magnitude, 2);

        [TelemetryAPI("v.staticPressurekPa", "Static Pressure (kPa)", Units = APIEntry.UnitType.PRES)]
        object StaticPressurekPa(DataSources ds) => ds.vessel.staticPressurekPa;

        [TelemetryAPI("v.staticPressure", "Static Pressure (kPa, via FlightGlobals)", Units = APIEntry.UnitType.PRES)]
        object StaticPressure(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody);

        [TelemetryAPI("v.atmosphericPressurePa", "Atmospheric Pressure (Pa)", Units = APIEntry.UnitType.PRES)]
        object AtmosphericPressurePa(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody) * 1000;

        [TelemetryAPI("v.atmosphericPressure", "Atmospheric Pressure (atm)", Units = APIEntry.UnitType.PRES)]
        object AtmosphericPressure(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody) * PhysicsGlobals.KpaToAtmospheres;

        [TelemetryAPI("v.atmosphericTemperature", "Atmospheric Temperature", Units = APIEntry.UnitType.TEMP)]
        object AtmosphericTemperature(DataSources ds) => ds.vessel.atmosphericTemperature;

        [TelemetryAPI("v.externalTemperature", "External Temperature", Units = APIEntry.UnitType.TEMP)]
        object ExternalTemperature(DataSources ds) => ds.vessel.externalTemperature;

        [TelemetryAPI("v.mach", "Mach Number")]
        object MachNumber(DataSources ds) => ds.vessel.mach;

        [TelemetryAPI("v.speedOfSound", "Speed of Sound", Units = APIEntry.UnitType.VELOCITY)]
        object SpeedOfSound(DataSources ds) => ds.vessel.speedOfSound;

        [TelemetryAPI("v.indicatedAirSpeed", "Indicated Air Speed", Units = APIEntry.UnitType.VELOCITY)]
        object IndicatedAirSpeed(DataSources ds) => ds.vessel.indicatedAirSpeed;

        // --- Solar & Thermal ---

        [TelemetryAPI("v.directSunlight", "In Direct Sunlight")]
        object DirectSunlight(DataSources ds) => ds.vessel.directSunlight;

        [TelemetryAPI("v.distanceToSun", "Distance to Sun", Units = APIEntry.UnitType.DISTANCE)]
        object DistanceToSun(DataSources ds) => ds.vessel.distanceToSun;

        [TelemetryAPI("v.solarFlux", "Solar Flux")]
        object SolarFlux(DataSources ds) => ds.vessel.solarFlux;

        // --- Situation & State ---

        [TelemetryAPI("v.name", "Name", Units = APIEntry.UnitType.STRING)]
        object Name(DataSources ds) => ds.vessel.vesselName;

        [TelemetryAPI("v.body", "Body Name", Units = APIEntry.UnitType.STRING)]
        object Body(DataSources ds) => ds.vessel.orbit.referenceBody.name;

        [TelemetryAPI("v.situation", "Vessel Situation", Units = APIEntry.UnitType.STRING)]
        object Situation(DataSources ds) => ds.vessel.situation.ToString();

        [TelemetryAPI("v.situationString", "Vessel Situation (readable)", Units = APIEntry.UnitType.STRING)]
        object SituationString(DataSources ds) => Vessel.GetSituationString(ds.vessel);

        [TelemetryAPI("v.vesselType", "Vessel Type", Units = APIEntry.UnitType.STRING)]
        object VesselType(DataSources ds) => ds.vessel.vesselType.ToString();

        [TelemetryAPI("v.landed", "Is Landed")]
        object Landed(DataSources ds) => ds.vessel.Landed;

        [TelemetryAPI("v.splashed", "Is Splashed")]
        object Splashed(DataSources ds) => ds.vessel.Splashed;

        [TelemetryAPI("v.landedOrSplashed", "Is Landed or Splashed")]
        object LandedOrSplashed(DataSources ds) => ds.vessel.LandedOrSplashed;

        [TelemetryAPI("v.landedAt", "Landed At (biome/location)", Units = APIEntry.UnitType.STRING)]
        object LandedAt(DataSources ds) => ds.vessel.landedAt;

        [TelemetryAPI("v.isEVA", "Is EVA")]
        object IsEVA(DataSources ds) => ds.vessel.isEVA;

        [TelemetryAPI("v.isActiveVessel", "Is Active Vessel")]
        object IsActiveVessel(DataSources ds) => ds.vessel.isActiveVessel;

        [TelemetryAPI("v.isControllable", "Is Controllable")]
        object IsControllable(DataSources ds) => ds.vessel.IsControllable;

        [TelemetryAPI("v.isCommandable", "Is Commandable")]
        object IsCommandable(DataSources ds) => ds.vessel.isCommandable;

        // --- Time ---

        [TelemetryAPI("v.missionTime", "Mission Time", Units = APIEntry.UnitType.TIME)]
        object MissionTime(DataSources ds) => ds.vessel.missionTime;

        [TelemetryAPI("v.missionTimeString", "Mission Elapsed Time (formatted)", Units = APIEntry.UnitType.STRING)]
        object MissionTimeString(DataSources ds) => Vessel.GetMETString(ds.vessel);

        [TelemetryAPI("v.launchTime", "Launch Time", Units = APIEntry.UnitType.DATE)]
        object LaunchTime(DataSources ds) => ds.vessel.launchTime;

        // --- Stage ---

        [TelemetryAPI("v.currentStage", "Current Stage")]
        object CurrentStage(DataSources ds) => ds.vessel.currentStage;

        // --- Crew ---

        [TelemetryAPI("v.crewCount", "Crew Count")]
        object CrewCount(DataSources ds) => ds.vessel.GetCrewCount();

        [TelemetryAPI("v.crewCapacity", "Crew Capacity")]
        object CrewCapacity(DataSources ds) => ds.vessel.GetCrewCapacity();

        [TelemetryAPI("v.crew", "Crew Names", Plotable = false, Formatter = "StringArray")]
        object Crew(DataSources ds)
        {
            var names = new List<string>();
            foreach (var crew in ds.vessel.GetVesselCrew())
                names.Add(crew.name);
            return names;
        }

        // --- Orientation ---

        [TelemetryAPI("v.upAxis", "Local Up Axis", Plotable = false, Formatter = "Vector3d")]
        object UpAxis(DataSources ds) => ds.vessel.upAxis;

        [TelemetryAPI("v.terrainNormal", "Terrain Normal", Plotable = false, Formatter = "Vector3d")]
        object TerrainNormal(DataSources ds) =>
            new Vector3d(ds.vessel.terrainNormal.x, ds.vessel.terrainNormal.y, ds.vessel.terrainNormal.z);

        // --- Physics State ---

        [TelemetryAPI("v.loaded", "Is Loaded")]
        object Loaded(DataSources ds) => ds.vessel.loaded;

        [TelemetryAPI("v.packed", "Is Packed (on rails)")]
        object Packed(DataSources ds) => ds.vessel.packed;

        // --- Computed ---

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

        // --- Apsides ---

        [TelemetryAPI("o.PeA", "Periapsis Altitude", Units = APIEntry.UnitType.DISTANCE)]
        object PeA(DataSources ds) => ds.vessel.orbit.PeA;

        [TelemetryAPI("o.ApA", "Apoapsis Altitude", Units = APIEntry.UnitType.DISTANCE)]
        object ApA(DataSources ds) => ds.vessel.orbit.ApA;

        [TelemetryAPI("o.PeR", "Periapsis Radius", Units = APIEntry.UnitType.DISTANCE)]
        object PeR(DataSources ds) => ds.vessel.orbit.PeR;

        [TelemetryAPI("o.ApR", "Apoapsis Radius", Units = APIEntry.UnitType.DISTANCE)]
        object ApR(DataSources ds) => ds.vessel.orbit.ApR;

        [TelemetryAPI("o.timeToAp", "Time to Apoapsis", Units = APIEntry.UnitType.TIME)]
        object TimeToAp(DataSources ds) => ds.vessel.orbit.timeToAp;

        [TelemetryAPI("o.timeToPe", "Time to Periapsis", Units = APIEntry.UnitType.TIME)]
        object TimeToPe(DataSources ds) => ds.vessel.orbit.timeToPe;

        // --- Orbital Elements ---

        [TelemetryAPI("o.inclination", "Inclination", Units = APIEntry.UnitType.DEG)]
        object Inclination(DataSources ds) => ds.vessel.orbit.inclination;

        [TelemetryAPI("o.eccentricity", "Eccentricity")]
        object Eccentricity(DataSources ds) => ds.vessel.orbit.eccentricity;

        [TelemetryAPI("o.sma", "Semimajor Axis", Units = APIEntry.UnitType.DISTANCE)]
        object Sma(DataSources ds) => ds.vessel.orbit.semiMajorAxis;

        [TelemetryAPI("o.semiMinorAxis", "Semi-minor Axis", Units = APIEntry.UnitType.DISTANCE)]
        object SemiMinorAxis(DataSources ds) => ds.vessel.orbit.semiMinorAxis;

        [TelemetryAPI("o.semiLatusRectum", "Semi-latus Rectum", Units = APIEntry.UnitType.DISTANCE)]
        object SemiLatusRectum(DataSources ds) => ds.vessel.orbit.semiLatusRectum;

        [TelemetryAPI("o.lan", "Longitude of Ascending Node", Units = APIEntry.UnitType.DEG)]
        object Lan(DataSources ds) => ds.vessel.orbit.LAN;

        [TelemetryAPI("o.argumentOfPeriapsis", "Argument of Periapsis", Units = APIEntry.UnitType.DEG)]
        object ArgumentOfPeriapsis(DataSources ds) => ds.vessel.orbit.argumentOfPeriapsis;

        [TelemetryAPI("o.epoch", "Epoch")]
        object Epoch(DataSources ds) => ds.vessel.orbit.epoch;

        [TelemetryAPI("o.period", "Orbital Period", Units = APIEntry.UnitType.TIME)]
        object Period(DataSources ds) => ds.vessel.orbit.period;

        // --- Anomalies ---

        [TelemetryAPI("o.trueAnomaly", "True Anomaly", Units = APIEntry.UnitType.DEG)]
        object TrueAnomaly(DataSources ds) =>
            ds.vessel.orbit.TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI);

        [TelemetryAPI("o.meanAnomaly", "Mean Anomaly", Units = APIEntry.UnitType.DEG)]
        object MeanAnomaly(DataSources ds) => ds.vessel.orbit.meanAnomaly * (180.0 / Math.PI);

        [TelemetryAPI("o.eccentricAnomaly", "Eccentric Anomaly", Units = APIEntry.UnitType.DEG)]
        object EccentricAnomaly(DataSources ds) => ds.vessel.orbit.eccentricAnomaly * (180.0 / Math.PI);

        [TelemetryAPI("o.maae", "Mean Anomaly at Epoch")]
        object Maae(DataSources ds)
        {
            Orbit orbit = ds.vessel.orbit;
            return orbit.getObtAtUT(0) / orbit.period * (2.0 * Math.PI);
        }

        [TelemetryAPI("o.timeOfPeriapsisPassage", "Time of Periapsis Passage", Units = APIEntry.UnitType.DATE)]
        object TimeOfPeriapsisPassage(DataSources ds) =>
            Planetarium.GetUniversalTime() - ds.vessel.orbit.ObT;

        [TelemetryAPI("o.orbitPercent", "Orbit Percent")]
        object OrbitPercent(DataSources ds) => ds.vessel.orbit.orbitPercent;

        // --- Velocity ---

        [TelemetryAPI("o.relativeVelocity", "Relative Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object RelativeVelocity(DataSources ds) => ds.vessel.orbit.GetRelativeVel().magnitude;

        [TelemetryAPI("o.orbitalSpeed", "Orbital Speed", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalSpeed(DataSources ds) => ds.vessel.orbit.orbitalSpeed;

        [TelemetryAPI("o.vel", "Orbital Velocity Vector", Plotable = false, Formatter = "Vector3d")]
        object Vel(DataSources ds) => ds.vessel.orbit.vel;

        // --- Position ---

        [TelemetryAPI("o.radius", "Orbital Radius", Units = APIEntry.UnitType.DISTANCE)]
        object Radius(DataSources ds) => ds.vessel.orbit.radius;

        [TelemetryAPI("o.pos", "Orbital Position Vector", Plotable = false, Formatter = "Vector3d")]
        object Pos(DataSources ds) => ds.vessel.orbit.pos;

        // --- Energy ---

        [TelemetryAPI("o.orbitalEnergy", "Specific Orbital Energy")]
        object OrbitalEnergy(DataSources ds) => ds.vessel.orbit.orbitalEnergy;

        // --- Vectors ---

        [TelemetryAPI("o.orbitNormal", "Orbit Normal Vector", Plotable = false, Formatter = "Vector3d")]
        object OrbitNormal(DataSources ds) => ds.vessel.orbit.GetOrbitNormal();

        [TelemetryAPI("o.eccVec", "Eccentricity Vector", Plotable = false, Formatter = "Vector3d")]
        object EccVec(DataSources ds) => ds.vessel.orbit.GetEccVector();

        [TelemetryAPI("o.anVec", "Ascending Node Vector", Plotable = false, Formatter = "Vector3d")]
        object AnVec(DataSources ds) => ds.vessel.orbit.GetANVector();

        [TelemetryAPI("o.h", "Specific Angular Momentum Vector", Plotable = false, Formatter = "Vector3d")]
        object AngularMomentumVec(DataSources ds) => ds.vessel.orbit.h;

        // --- Reference Body ---

        [TelemetryAPI("o.referenceBody", "Reference Body Name", Units = APIEntry.UnitType.STRING)]
        object ReferenceBody(DataSources ds) => ds.vessel.orbit.referenceBody.name;

        // --- Transitions ---

        [TelemetryAPI("o.timeToTransition1", "Time to Transition 1", Units = APIEntry.UnitType.TIME)]
        object TimeToTransition1(DataSources ds) => ds.vessel.orbit.timeToTransition1;

        [TelemetryAPI("o.timeToTransition2", "Time to Transition 2", Units = APIEntry.UnitType.TIME)]
        object TimeToTransition2(DataSources ds) => ds.vessel.orbit.timeToTransition2;

        [TelemetryAPI("o.patchStartTransition", "Patch Start Transition Type", Units = APIEntry.UnitType.STRING)]
        object PatchStartTransition(DataSources ds) => ds.vessel.orbit.patchStartTransition.ToString();

        [TelemetryAPI("o.patchEndTransition", "Patch End Transition Type", Units = APIEntry.UnitType.STRING)]
        object PatchEndTransition(DataSources ds) => ds.vessel.orbit.patchEndTransition.ToString();

        [TelemetryAPI("o.StartUT", "Orbit Patch Start UT", Units = APIEntry.UnitType.DATE)]
        object StartUT(DataSources ds) => ds.vessel.orbit.StartUT;

        [TelemetryAPI("o.EndUT", "Orbit Patch End UT", Units = APIEntry.UnitType.DATE)]
        object EndUT(DataSources ds) => ds.vessel.orbit.EndUT;

        [TelemetryAPI("o.UTsoi", "UT of SOI Transition", Units = APIEntry.UnitType.DATE)]
        object UTsoi(DataSources ds) => ds.vessel.orbit.UTsoi;

        // --- Closest Encounter ---

        [TelemetryAPI("o.closestEncounterBody", "Closest Encounter Body Name", Units = APIEntry.UnitType.STRING)]
        object ClosestEncounterBody(DataSources ds) =>
            ds.vessel.orbit.closestEncounterBody != null ? ds.vessel.orbit.closestEncounterBody.name : "";

        [TelemetryAPI("o.closestTgtApprUT", "Closest Target Approach UT", Units = APIEntry.UnitType.DATE)]
        object ClosestTgtApprUT(DataSources ds) => ds.vessel.orbit.closestTgtApprUT;

        // --- Speed At ---

        [TelemetryAPI("o.orbitalSpeedAt", "Orbital Speed at Orbit Time [double obt]", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalSpeedAt(DataSources ds) =>
            ds.vessel.orbit.getOrbitalSpeedAt(double.Parse(ds.args[0]));

        [TelemetryAPI("o.orbitalSpeedAtDistance", "Orbital Speed at Distance [double distance]", Units = APIEntry.UnitType.VELOCITY)]
        object OrbitalSpeedAtDistance(DataSources ds) =>
            ds.vessel.orbit.getOrbitalSpeedAtDistance(double.Parse(ds.args[0]));

        [TelemetryAPI("o.radiusAtTrueAnomaly", "Radius at True Anomaly [double trueAnomaly]", Units = APIEntry.UnitType.DISTANCE)]
        object RadiusAtTrueAnomaly(DataSources ds) =>
            ds.vessel.orbit.RadiusAtTrueAnomaly(double.Parse(ds.args[0]));

        [TelemetryAPI("o.trueAnomalyAtRadius", "True Anomaly at Radius [double radius]", Units = APIEntry.UnitType.DEG)]
        object TrueAnomalyAtRadius(DataSources ds) =>
            ds.vessel.orbit.TrueAnomalyAtRadius(double.Parse(ds.args[0]));

        // --- Orbit Patches ---

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

        // --- Identity ---

        [TelemetryAPI("b.name", "Body Name [body id]", Units = APIEntry.UnitType.STRING)]
        object BodyName(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].name;

        [TelemetryAPI("b.description", "Body Description [body id]", Units = APIEntry.UnitType.STRING)]
        object BodyDescription(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].bodyDescription;

        [TelemetryAPI("b.number", "Number of Bodies")]
        object BodyCount(DataSources ds) => FlightGlobals.Bodies.Count;

        [TelemetryAPI("b.index", "Flight Globals Index [body id]")]
        object BodyIndex(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].flightGlobalsIndex;

        [TelemetryAPI("b.referenceBody", "Reference Body Name [body id]", Units = APIEntry.UnitType.STRING)]
        object BodyReferenceBody(DataSources ds)
        {
            var body = FlightGlobals.Bodies[int.Parse(ds.args[0])];
            return body.referenceBody != null ? body.referenceBody.name : "";
        }

        [TelemetryAPI("b.orbitingBodies", "Orbiting Body Names [body id]", Plotable = false, Formatter = "StringArray")]
        object BodyOrbitingBodies(DataSources ds)
        {
            var names = new List<string>();
            foreach (var child in FlightGlobals.Bodies[int.Parse(ds.args[0])].orbitingBodies)
                names.Add(child.name);
            return names;
        }

        // --- Physical Properties ---

        [TelemetryAPI("b.radius", "Body Radius [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object Radius(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].Radius;

        [TelemetryAPI("b.mass", "Body Mass [body id]")]
        object BodyMass(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].Mass;

        [TelemetryAPI("b.geeASL", "Surface Gravity in G [body id]", Units = APIEntry.UnitType.G)]
        object BodyGeeASL(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].GeeASL;

        [TelemetryAPI("b.soi", "Body Sphere of Influence [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object Soi(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].sphereOfInfluence;

        [TelemetryAPI("b.hillSphere", "Hill Sphere Radius [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object HillSphere(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].hillSphere;

        // --- Rotation ---

        [TelemetryAPI("b.rotationPeriod", "Rotation Period [body id]", Units = APIEntry.UnitType.TIME)]
        object RotationPeriod(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].rotationPeriod;

        [TelemetryAPI("b.rotationAngle", "Current Rotation Angle [body id]", Units = APIEntry.UnitType.DEG)]
        object BodyRotationAngle(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].rotationAngle;

        [TelemetryAPI("b.angularV", "Angular Velocity [body id]")]
        object BodyAngularV(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].angularV;

        [TelemetryAPI("b.tidallyLocked", "Tidally Locked [body id]")]
        object TidallyLocked(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].tidallyLocked;

        [TelemetryAPI("b.rotates", "Body Rotates [body id]")]
        object BodyRotates(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].rotates;

        // --- Atmosphere ---

        [TelemetryAPI("b.atmosphere", "Has Atmosphere [body id]")]
        object BodyHasAtmosphere(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphere;

        [TelemetryAPI("b.maxAtmosphere", "Body Atmosphere Depth [body id]", Units = APIEntry.UnitType.DISTANCE)]
        object MaxAtmosphere(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphereDepth;

        [TelemetryAPI("b.atmosphereContainsOxygen", "Atmosphere contains oxygen [body id]")]
        object AtmosphereContainsOxygen(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphereContainsOxygen;

        // --- Surface ---

        [TelemetryAPI("b.ocean", "Has Ocean [body id]")]
        object BodyHasOcean(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].ocean;

        // --- Position ---

        [TelemetryAPI("b.position", "Body World Position [body id]", Plotable = false, Formatter = "Vector3d")]
        object BodyPosition(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].position;

        // --- Time Warp ---

        [TelemetryAPI("b.timeWarpAltitudeLimits", "Time Warp Altitude Limits [body id]", Plotable = false)]
        object BodyTimeWarpAltitudeLimits(DataSources ds)
        {
            float[] limits = FlightGlobals.Bodies[int.Parse(ds.args[0])].timeWarpAltitudeLimits;
            var result = new List<float>(limits);
            return result;
        }

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

        [TelemetryAPI("m.mapIsEnabled", "Map View Is Enabled")]
        object MapIsEnabled(DataSources ds) => MapView.MapIsEnabled;

        [TelemetryAPI("o.maneuverNodes", "Maneuver Nodes  [object maneuverNodes]",
            Plotable = false, Formatter = "ManeuverNodeList")]
        object ManeuverNodes(DataSources ds)
        {
            PluginLogger.debug("Start GET");
            return ds.vessel.patchedConicSolver.maneuverNodes;
        }

        [TelemetryAPI("o.maneuverNodes.count", "Number of Maneuver Nodes")]
        object ManeuverNodeCount(DataSources ds) =>
            ds.vessel.patchedConicSolver.maneuverNodes.Count;

        [TelemetryAPI("o.maneuverNodes.deltaV", "Maneuver Node Delta-V Vector [int id]",
            Plotable = false, Formatter = "Vector3d")]
        object ManeuverNodeDeltaV(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.DeltaV;
        }

        [TelemetryAPI("o.maneuverNodes.deltaVMagnitude", "Maneuver Node Delta-V Magnitude [int id]",
            Units = APIEntry.UnitType.VELOCITY)]
        object ManeuverNodeDeltaVMagnitude(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.DeltaV.magnitude;
        }

        [TelemetryAPI("o.maneuverNodes.UT", "Maneuver Node Universal Time [int id]",
            Units = APIEntry.UnitType.DATE)]
        object ManeuverNodeUT(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.UT;
        }

        [TelemetryAPI("o.maneuverNodes.timeTo", "Time Until Maneuver Node [int id]",
            Units = APIEntry.UnitType.TIME)]
        object ManeuverNodeTimeTo(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.UT - Planetarium.GetUniversalTime();
        }

        [TelemetryAPI("o.maneuverNodes.burnVector", "Maneuver Node Burn Vector (world space) [int id]",
            Plotable = false, Formatter = "Vector3d")]
        object ManeuverNodeBurnVector(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.GetBurnVector(node.patch);
        }

        [TelemetryAPI("o.maneuverNodes.orbitPatches",
            "Orbit Patches for Maneuver Node [int id]",
            Plotable = false, Formatter = "OrbitPatchList")]
        object ManeuverNodeOrbitPatches(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return OrbitPatches.getPatchesForOrbit(node.nextPatch);
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
