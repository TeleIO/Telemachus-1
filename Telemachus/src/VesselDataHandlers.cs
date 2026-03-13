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

        [TelemetryAPI("v.altitude", "Altitude", Units = APIEntry.UnitType.DISTANCE, Category = "vessel", ReturnType = "double")]
        object Altitude(DataSources ds) => ds.vessel.altitude;

        [TelemetryAPI("v.heightFromTerrain", "Height from Terrain", Units = APIEntry.UnitType.DISTANCE, Category = "vessel", ReturnType = "double")]
        object HeightFromTerrain(DataSources ds) => ds.vessel.heightFromTerrain;

        [TelemetryAPI("v.heightFromSurface", "Height from Surface", Units = APIEntry.UnitType.DISTANCE, Category = "vessel", ReturnType = "double")]
        object HeightFromSurface(DataSources ds) => ds.vessel.heightFromSurface;

        [TelemetryAPI("v.terrainHeight", "Terrain Height", Units = APIEntry.UnitType.DISTANCE, Category = "vessel", ReturnType = "double")]
        object TerrainHeight(DataSources ds) => ds.vessel.terrainAltitude;

        [TelemetryAPI("v.pqsAltitude", "PQS Terrain Altitude", Units = APIEntry.UnitType.DISTANCE, Category = "vessel", ReturnType = "double")]
        object PqsAltitude(DataSources ds) => ds.vessel.pqsAltitude;

        [TelemetryAPI("v.long", "Longitude", Units = APIEntry.UnitType.LATLON, Category = "vessel", ReturnType = "double")]
        object Longitude(DataSources ds) =>
            ds.vessel.longitude > 180 ? ds.vessel.longitude - 360.0 : ds.vessel.longitude;

        [TelemetryAPI("v.lat", "Latitude", Units = APIEntry.UnitType.LATLON, Category = "vessel", ReturnType = "double")]
        object Latitude(DataSources ds) => ds.vessel.latitude;

        // --- Velocity ---

        [TelemetryAPI("v.surfaceVelocity", "Surface Velocity", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object SurfaceVelocity(DataSources ds) => ds.vessel.srf_velocity.magnitude;

        [TelemetryAPI("v.surfaceVelocityx", "Surface Velocity x", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object SurfaceVelocityX(DataSources ds) => ds.vessel.srf_velocity.x;

        [TelemetryAPI("v.surfaceVelocityy", "Surface Velocity y", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object SurfaceVelocityY(DataSources ds) => ds.vessel.srf_velocity.y;

        [TelemetryAPI("v.surfaceVelocityz", "Surface Velocity z", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object SurfaceVelocityZ(DataSources ds) => ds.vessel.srf_velocity.z;

        [TelemetryAPI("v.orbitalVelocity", "Orbital Velocity", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object OrbitalVelocity(DataSources ds) => ds.vessel.obt_velocity.magnitude;

        [TelemetryAPI("v.orbitalVelocityx", "Orbital Velocity x", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object OrbitalVelocityX(DataSources ds) => ds.vessel.obt_velocity.x;

        [TelemetryAPI("v.orbitalVelocityy", "Orbital Velocity y", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object OrbitalVelocityY(DataSources ds) => ds.vessel.obt_velocity.y;

        [TelemetryAPI("v.orbitalVelocityz", "Orbital Velocity z", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object OrbitalVelocityZ(DataSources ds) => ds.vessel.obt_velocity.z;

        [TelemetryAPI("v.surfaceSpeed", "Surface Speed", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object SurfaceSpeed(DataSources ds) => ds.vessel.horizontalSrfSpeed;

        [TelemetryAPI("v.verticalSpeed", "Vertical Speed", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object VerticalSpeed(DataSources ds) => ds.vessel.verticalSpeed;

        [TelemetryAPI("v.speed", "Speed", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object Speed(DataSources ds) => ds.vessel.speed;

        [TelemetryAPI("v.srfSpeed", "Surface Speed (direct)", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object SrfSpeed(DataSources ds) => ds.vessel.srfSpeed;

        [TelemetryAPI("v.obtSpeed", "Orbital Speed (direct)", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object ObtSpeed(DataSources ds) => ds.vessel.obt_speed;

        [TelemetryAPI("v.angularVelocity", "Angular Velocity", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object AngularVelocity(DataSources ds) => ds.vessel.angularVelocity.magnitude;

        [TelemetryAPI("v.angularVelocityx", "Angular Velocity x", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object AngularVelocityX(DataSources ds) => ds.vessel.angularVelocity.x;

        [TelemetryAPI("v.angularVelocityy", "Angular Velocity y", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object AngularVelocityY(DataSources ds) => ds.vessel.angularVelocity.y;

        [TelemetryAPI("v.angularVelocityz", "Angular Velocity z", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object AngularVelocityZ(DataSources ds) => ds.vessel.angularVelocity.z;

        // --- Acceleration & Forces ---

        [TelemetryAPI("v.geeForce", "G-Force", Units = APIEntry.UnitType.G, Category = "vessel", ReturnType = "double")]
        object GeeForce(DataSources ds) => ds.vessel.geeForce;

        [TelemetryAPI("v.geeForceImmediate", "Instantaneous G-Force", Units = APIEntry.UnitType.G, Category = "vessel", ReturnType = "double")]
        object GeeForceImmediate(DataSources ds) => ds.vessel.geeForce_immediate;

        [TelemetryAPI("v.acceleration", "Acceleration Magnitude", Units = APIEntry.UnitType.ACC, Category = "vessel", ReturnType = "double")]
        object Acceleration(DataSources ds) => ds.vessel.acceleration.magnitude;

        [TelemetryAPI("v.accelerationx", "Acceleration x", Units = APIEntry.UnitType.ACC, Category = "vessel", ReturnType = "double")]
        object AccelerationX(DataSources ds) => ds.vessel.acceleration.x;

        [TelemetryAPI("v.accelerationy", "Acceleration y", Units = APIEntry.UnitType.ACC, Category = "vessel", ReturnType = "double")]
        object AccelerationY(DataSources ds) => ds.vessel.acceleration.y;

        [TelemetryAPI("v.accelerationz", "Acceleration z", Units = APIEntry.UnitType.ACC, Category = "vessel", ReturnType = "double")]
        object AccelerationZ(DataSources ds) => ds.vessel.acceleration.z;

        [TelemetryAPI("v.specificAcceleration", "Specific Acceleration (thrust/mass)", Units = APIEntry.UnitType.ACC, Category = "vessel", ReturnType = "double")]
        object SpecificAcceleration(DataSources ds) => ds.vessel.specificAcceleration;

        [TelemetryAPI("v.perturbation", "Orbital Perturbation Magnitude", Category = "vessel", ReturnType = "double")]
        object Perturbation(DataSources ds) => ds.vessel.perturbation.magnitude;

        [TelemetryAPI("v.perturbationx", "Orbital Perturbation x", Category = "vessel", ReturnType = "double")]
        object PerturbationX(DataSources ds) => ds.vessel.perturbation.x;

        [TelemetryAPI("v.perturbationy", "Orbital Perturbation y", Category = "vessel", ReturnType = "double")]
        object PerturbationY(DataSources ds) => ds.vessel.perturbation.y;

        [TelemetryAPI("v.perturbationz", "Orbital Perturbation z", Category = "vessel", ReturnType = "double")]
        object PerturbationZ(DataSources ds) => ds.vessel.perturbation.z;

        // --- Mass & Inertia ---

        [TelemetryAPI("v.mass", "Total Mass", Category = "vessel", ReturnType = "double")]
        object Mass(DataSources ds) => ds.vessel.GetTotalMass();

        [TelemetryAPI("v.angularMomentum", "Angular Momentum Magnitude", Category = "vessel", ReturnType = "double")]
        object AngularMomentum(DataSources ds) => ds.vessel.angularMomentum.magnitude;

        [TelemetryAPI("v.angularMomentumx", "Angular Momentum x", Category = "vessel", ReturnType = "double")]
        object AngularMomentumX(DataSources ds) => ds.vessel.angularMomentum.x;

        [TelemetryAPI("v.angularMomentumy", "Angular Momentum y", Category = "vessel", ReturnType = "double")]
        object AngularMomentumY(DataSources ds) => ds.vessel.angularMomentum.y;

        [TelemetryAPI("v.angularMomentumz", "Angular Momentum z", Category = "vessel", ReturnType = "double")]
        object AngularMomentumZ(DataSources ds) => ds.vessel.angularMomentum.z;

        [TelemetryAPI("v.momentOfInertia", "Moment of Inertia", Plotable = false, Formatter = "Vector3d", Category = "vessel", ReturnType = "Vector3d")]
        object MomentOfInertia(DataSources ds) => new Vector3d(ds.vessel.MOI.x, ds.vessel.MOI.y, ds.vessel.MOI.z);

        [TelemetryAPI("v.CoM", "Center of Mass", Plotable = false, Formatter = "Vector3d", Category = "vessel", ReturnType = "Vector3d")]
        object CenterOfMass(DataSources ds) => new Vector3d(ds.vessel.CoM.x, ds.vessel.CoM.y, ds.vessel.CoM.z);

        // --- Atmosphere & Environment ---

        [TelemetryAPI("v.atmosphericDensity", "Atmospheric Density", Units = APIEntry.UnitType.DENSITY, Category = "vessel", ReturnType = "double")]
        object AtmosphericDensity(DataSources ds) => ds.vessel.atmDensity;

        [TelemetryAPI("v.dynamicPressurekPa", "Dynamic Pressure (kPa)", Units = APIEntry.UnitType.DYNAMICPRESSURE, Category = "vessel", ReturnType = "double")]
        object DynamicPressurekPa(DataSources ds) => ds.vessel.dynamicPressurekPa;

        [TelemetryAPI("v.dynamicPressure", "Dynamic Pressure (Pa)", Units = APIEntry.UnitType.DYNAMICPRESSURE, Category = "vessel", ReturnType = "double")]
        object DynamicPressure(DataSources ds) =>
            (ds.vessel.atmDensity * 0.5) * Math.Pow(ds.vessel.srf_velocity.magnitude, 2);

        [TelemetryAPI("v.staticPressurekPa", "Static Pressure (kPa)", Units = APIEntry.UnitType.PRES, Category = "vessel", ReturnType = "double")]
        object StaticPressurekPa(DataSources ds) => ds.vessel.staticPressurekPa;

        [TelemetryAPI("v.staticPressure", "Static Pressure (kPa, via FlightGlobals)", Units = APIEntry.UnitType.PRES, Category = "vessel", ReturnType = "double")]
        object StaticPressure(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody);

        [TelemetryAPI("v.atmosphericPressurePa", "Atmospheric Pressure (Pa)", Units = APIEntry.UnitType.PRES, Category = "vessel", ReturnType = "double")]
        object AtmosphericPressurePa(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody) * 1000;

        [TelemetryAPI("v.atmosphericPressure", "Atmospheric Pressure (atm)", Units = APIEntry.UnitType.PRES, Category = "vessel", ReturnType = "double")]
        object AtmosphericPressure(DataSources ds) =>
            FlightGlobals.getStaticPressure(ds.vessel.altitude, ds.vessel.mainBody) * PhysicsGlobals.KpaToAtmospheres;

        [TelemetryAPI("v.atmosphericTemperature", "Atmospheric Temperature", Units = APIEntry.UnitType.TEMP, Category = "vessel", ReturnType = "double")]
        object AtmosphericTemperature(DataSources ds) => ds.vessel.atmosphericTemperature;

        [TelemetryAPI("v.externalTemperature", "External Temperature", Units = APIEntry.UnitType.TEMP, Category = "vessel", ReturnType = "double")]
        object ExternalTemperature(DataSources ds) => ds.vessel.externalTemperature;

        [TelemetryAPI("v.mach", "Mach Number", Category = "vessel", ReturnType = "double")]
        object MachNumber(DataSources ds) => ds.vessel.mach;

        [TelemetryAPI("v.speedOfSound", "Speed of Sound", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object SpeedOfSound(DataSources ds) => ds.vessel.speedOfSound;

        [TelemetryAPI("v.indicatedAirSpeed", "Indicated Air Speed", Units = APIEntry.UnitType.VELOCITY, Category = "vessel", ReturnType = "double")]
        object IndicatedAirSpeed(DataSources ds) => ds.vessel.indicatedAirSpeed;

        // --- Solar & Thermal ---

        [TelemetryAPI("v.directSunlight", "In Direct Sunlight", Category = "vessel", ReturnType = "bool")]
        object DirectSunlight(DataSources ds) => ds.vessel.directSunlight;

        [TelemetryAPI("v.distanceToSun", "Distance to Sun", Units = APIEntry.UnitType.DISTANCE, Category = "vessel", ReturnType = "double")]
        object DistanceToSun(DataSources ds) => ds.vessel.distanceToSun;

        [TelemetryAPI("v.solarFlux", "Solar Flux", Category = "vessel", ReturnType = "double")]
        object SolarFlux(DataSources ds) => ds.vessel.solarFlux;

        // --- Situation & State ---

        [TelemetryAPI("v.name", "Name", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object Name(DataSources ds) => ds.vessel.vesselName;

        [TelemetryAPI("v.body", "Body Name", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object Body(DataSources ds) => ds.vessel.orbit.referenceBody.name;

        [TelemetryAPI("v.situation", "Vessel Situation", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object Situation(DataSources ds) => ds.vessel.situation.ToString();

        [TelemetryAPI("v.situationString", "Vessel Situation (readable)", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object SituationString(DataSources ds) => Vessel.GetSituationString(ds.vessel);

        [TelemetryAPI("v.vesselType", "Vessel Type", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object VesselType(DataSources ds) => ds.vessel.vesselType.ToString();

        [TelemetryAPI("v.landed", "Is Landed", Category = "vessel", ReturnType = "bool")]
        object Landed(DataSources ds) => ds.vessel.Landed;

        [TelemetryAPI("v.splashed", "Is Splashed", Category = "vessel", ReturnType = "bool")]
        object Splashed(DataSources ds) => ds.vessel.Splashed;

        [TelemetryAPI("v.landedOrSplashed", "Is Landed or Splashed", Category = "vessel", ReturnType = "bool")]
        object LandedOrSplashed(DataSources ds) => ds.vessel.LandedOrSplashed;

        [TelemetryAPI("v.landedAt", "Landed At (biome/location)", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object LandedAt(DataSources ds) => ds.vessel.landedAt;

        [TelemetryAPI("v.biome", "Current Biome Name", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object Biome(DataSources ds) => ScienceUtil.GetExperimentBiome(ds.vessel.mainBody, ds.vessel.latitude, ds.vessel.longitude);

        [TelemetryAPI("v.biomeLocalized", "Current Biome Name (Localized)", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object BiomeLocalized(DataSources ds) => ScienceUtil.GetExperimentBiomeLocalized(ds.vessel.mainBody, ds.vessel.latitude, ds.vessel.longitude);

        [TelemetryAPI("v.isEVA", "Is EVA", Category = "vessel", ReturnType = "bool")]
        object IsEVA(DataSources ds) => ds.vessel.isEVA;

        [TelemetryAPI("v.isActiveVessel", "Is Active Vessel", Category = "vessel", ReturnType = "bool")]
        object IsActiveVessel(DataSources ds) => ds.vessel.isActiveVessel;

        [TelemetryAPI("v.isControllable", "Is Controllable", Category = "vessel", ReturnType = "bool")]
        object IsControllable(DataSources ds) => ds.vessel.IsControllable;

        [TelemetryAPI("v.isCommandable", "Is Commandable", Category = "vessel", ReturnType = "bool")]
        object IsCommandable(DataSources ds) => ds.vessel.isCommandable;

        // --- Time ---

        [TelemetryAPI("v.missionTime", "Mission Time", Units = APIEntry.UnitType.TIME, Category = "vessel", ReturnType = "double")]
        object MissionTime(DataSources ds) => ds.vessel.missionTime;

        [TelemetryAPI("v.missionTimeString", "Mission Elapsed Time (formatted)", Units = APIEntry.UnitType.STRING, Category = "vessel", ReturnType = "string")]
        object MissionTimeString(DataSources ds) => Vessel.GetMETString(ds.vessel);

        [TelemetryAPI("v.launchTime", "Launch Time", Units = APIEntry.UnitType.DATE, Category = "vessel", ReturnType = "double")]
        object LaunchTime(DataSources ds) => ds.vessel.launchTime;

        // --- Stage ---

        [TelemetryAPI("v.currentStage", "Current Stage", Category = "vessel", ReturnType = "int")]
        object CurrentStage(DataSources ds) => ds.vessel.currentStage;

        // --- Crew ---

        [TelemetryAPI("v.crewCount", "Crew Count", Category = "vessel", ReturnType = "int")]
        object CrewCount(DataSources ds) => ds.vessel.GetCrewCount();

        [TelemetryAPI("v.crewCapacity", "Crew Capacity", Category = "vessel", ReturnType = "int")]
        object CrewCapacity(DataSources ds) => ds.vessel.GetCrewCapacity();

        [TelemetryAPI("v.crew", "Crew Names", Plotable = false, Formatter = "StringArray", Category = "vessel", ReturnType = "string[]")]
        object Crew(DataSources ds)
        {
            var names = new List<string>();
            foreach (var crew in ds.vessel.GetVesselCrew())
                names.Add(crew.name);
            return names;
        }

        // --- Orientation ---

        [TelemetryAPI("v.upAxis", "Local Up Axis", Plotable = false, Formatter = "Vector3d", Category = "vessel", ReturnType = "Vector3d")]
        object UpAxis(DataSources ds) => ds.vessel.upAxis;

        [TelemetryAPI("v.terrainNormal", "Terrain Normal", Plotable = false, Formatter = "Vector3d", Category = "vessel", ReturnType = "Vector3d")]
        object TerrainNormal(DataSources ds) =>
            new Vector3d(ds.vessel.terrainNormal.x, ds.vessel.terrainNormal.y, ds.vessel.terrainNormal.z);

        // --- Physics State ---

        [TelemetryAPI("v.loaded", "Is Loaded", Category = "vessel", ReturnType = "bool")]
        object Loaded(DataSources ds) => ds.vessel.loaded;

        [TelemetryAPI("v.packed", "Is Packed (on rails)", Category = "vessel", ReturnType = "bool")]
        object Packed(DataSources ds) => ds.vessel.packed;

        // --- Computed ---

        [TelemetryAPI("v.angleToPrograde", "Angle to Prograde", Units = APIEntry.UnitType.DEG, Category = "vessel", ReturnType = "double")]
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

        [TelemetryAPI("o.PeA", "Periapsis Altitude", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object PeA(DataSources ds) => ds.vessel.orbit.PeA;

        [TelemetryAPI("o.ApA", "Apoapsis Altitude", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object ApA(DataSources ds) => ds.vessel.orbit.ApA;

        [TelemetryAPI("o.PeR", "Periapsis Radius", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object PeR(DataSources ds) => ds.vessel.orbit.PeR;

        [TelemetryAPI("o.ApR", "Apoapsis Radius", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object ApR(DataSources ds) => ds.vessel.orbit.ApR;

        [TelemetryAPI("o.timeToAp", "Time to Apoapsis", Units = APIEntry.UnitType.TIME, Category = "orbit", ReturnType = "double")]
        object TimeToAp(DataSources ds) => ds.vessel.orbit.timeToAp;

        [TelemetryAPI("o.timeToPe", "Time to Periapsis", Units = APIEntry.UnitType.TIME, Category = "orbit", ReturnType = "double")]
        object TimeToPe(DataSources ds) => ds.vessel.orbit.timeToPe;

        // --- Orbital Elements ---

        [TelemetryAPI("o.inclination", "Inclination", Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double")]
        object Inclination(DataSources ds) => ds.vessel.orbit.inclination;

        [TelemetryAPI("o.eccentricity", "Eccentricity", Category = "orbit", ReturnType = "double")]
        object Eccentricity(DataSources ds) => ds.vessel.orbit.eccentricity;

        [TelemetryAPI("o.sma", "Semimajor Axis", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object Sma(DataSources ds) => ds.vessel.orbit.semiMajorAxis;

        [TelemetryAPI("o.semiMinorAxis", "Semi-minor Axis", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object SemiMinorAxis(DataSources ds) => ds.vessel.orbit.semiMinorAxis;

        [TelemetryAPI("o.semiLatusRectum", "Semi-latus Rectum", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object SemiLatusRectum(DataSources ds) => ds.vessel.orbit.semiLatusRectum;

        [TelemetryAPI("o.lan", "Longitude of Ascending Node", Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double")]
        object Lan(DataSources ds) => ds.vessel.orbit.LAN;

        [TelemetryAPI("o.argumentOfPeriapsis", "Argument of Periapsis", Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double")]
        object ArgumentOfPeriapsis(DataSources ds) => ds.vessel.orbit.argumentOfPeriapsis;

        [TelemetryAPI("o.epoch", "Epoch", Category = "orbit", ReturnType = "double")]
        object Epoch(DataSources ds) => ds.vessel.orbit.epoch;

        [TelemetryAPI("o.period", "Orbital Period", Units = APIEntry.UnitType.TIME, Category = "orbit", ReturnType = "double")]
        object Period(DataSources ds) => ds.vessel.orbit.period;

        // --- Anomalies ---

        [TelemetryAPI("o.trueAnomaly", "True Anomaly", Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double")]
        object TrueAnomaly(DataSources ds) =>
            ds.vessel.orbit.TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI);

        [TelemetryAPI("o.meanAnomaly", "Mean Anomaly", Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double")]
        object MeanAnomaly(DataSources ds) => ds.vessel.orbit.meanAnomaly * (180.0 / Math.PI);

        [TelemetryAPI("o.eccentricAnomaly", "Eccentric Anomaly", Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double")]
        object EccentricAnomaly(DataSources ds) => ds.vessel.orbit.eccentricAnomaly * (180.0 / Math.PI);

        [TelemetryAPI("o.maae", "Mean Anomaly at Epoch", Category = "orbit", ReturnType = "double")]
        object Maae(DataSources ds)
        {
            Orbit orbit = ds.vessel.orbit;
            return orbit.getObtAtUT(0) / orbit.period * (2.0 * Math.PI);
        }

        [TelemetryAPI("o.timeOfPeriapsisPassage", "Time of Periapsis Passage", Units = APIEntry.UnitType.DATE, Category = "orbit", ReturnType = "double")]
        object TimeOfPeriapsisPassage(DataSources ds) =>
            Planetarium.GetUniversalTime() - ds.vessel.orbit.ObT;

        [TelemetryAPI("o.orbitPercent", "Orbit Percent", Category = "orbit", ReturnType = "double")]
        object OrbitPercent(DataSources ds) => ds.vessel.orbit.orbitPercent;

        // --- Velocity ---

        [TelemetryAPI("o.relativeVelocity", "Relative Velocity", Units = APIEntry.UnitType.VELOCITY, Category = "orbit", ReturnType = "double")]
        object RelativeVelocity(DataSources ds) => ds.vessel.orbit.GetRelativeVel().magnitude;

        [TelemetryAPI("o.orbitalSpeed", "Orbital Speed", Units = APIEntry.UnitType.VELOCITY, Category = "orbit", ReturnType = "double")]
        object OrbitalSpeed(DataSources ds) => ds.vessel.orbit.orbitalSpeed;

        [TelemetryAPI("o.vel", "Orbital Velocity Vector", Plotable = false, Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d")]
        object Vel(DataSources ds) => ds.vessel.orbit.vel;

        // --- Position ---

        [TelemetryAPI("o.radius", "Orbital Radius", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double")]
        object Radius(DataSources ds) => ds.vessel.orbit.radius;

        [TelemetryAPI("o.pos", "Orbital Position Vector", Plotable = false, Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d")]
        object Pos(DataSources ds) => ds.vessel.orbit.pos;

        // --- Energy ---

        [TelemetryAPI("o.orbitalEnergy", "Specific Orbital Energy", Category = "orbit", ReturnType = "double")]
        object OrbitalEnergy(DataSources ds) => ds.vessel.orbit.orbitalEnergy;

        // --- Vectors ---

        [TelemetryAPI("o.orbitNormal", "Orbit Normal Vector", Plotable = false, Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d")]
        object OrbitNormal(DataSources ds) => ds.vessel.orbit.GetOrbitNormal();

        [TelemetryAPI("o.eccVec", "Eccentricity Vector", Plotable = false, Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d")]
        object EccVec(DataSources ds) => ds.vessel.orbit.GetEccVector();

        [TelemetryAPI("o.anVec", "Ascending Node Vector", Plotable = false, Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d")]
        object AnVec(DataSources ds) => ds.vessel.orbit.GetANVector();

        [TelemetryAPI("o.h", "Specific Angular Momentum Vector", Plotable = false, Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d")]
        object AngularMomentumVec(DataSources ds) => ds.vessel.orbit.h;

        // --- Reference Body ---

        [TelemetryAPI("o.referenceBody", "Reference Body Name", Units = APIEntry.UnitType.STRING, Category = "orbit", ReturnType = "string")]
        object ReferenceBody(DataSources ds) => ds.vessel.orbit.referenceBody.name;

        // --- Next Apsis ---

        [TelemetryAPI("o.nextApsisType", "Next Apsis Type (-1=Pe, 1=Ap, 0=N/A)",
            Category = "orbit", ReturnType = "double")]
        object NextApsisType(DataSources ds)
        {
            var orbit = ds.vessel.orbit;
            if (orbit.eccentricity < 1.0)
                return orbit.timeToPe < orbit.timeToAp ? -1.0 : 1.0;
            // Hyperbolic: check if Pe is still ahead
            return (-orbit.meanAnomaly / (2 * Math.PI / orbit.period) > 0.0) ? -1.0 : 0.0;
        }

        [TelemetryAPI("o.timeToNextApsis", "Time to Next Apsis",
            Units = APIEntry.UnitType.TIME, Category = "orbit", ReturnType = "double")]
        object TimeToNextApsis(DataSources ds)
        {
            var orbit = ds.vessel.orbit;
            if (orbit.eccentricity < 1.0)
                return Math.Min(orbit.timeToPe, orbit.timeToAp);
            // Hyperbolic: only Pe exists
            double timeToPe = -orbit.meanAnomaly / (2 * Math.PI / orbit.period);
            return timeToPe > 0 ? timeToPe : double.NaN;
        }

        // --- Transitions ---

        [TelemetryAPI("o.timeToTransition1", "Time to Transition 1", Units = APIEntry.UnitType.TIME, Category = "orbit", ReturnType = "double")]
        object TimeToTransition1(DataSources ds) => ds.vessel.orbit.timeToTransition1;

        [TelemetryAPI("o.timeToTransition2", "Time to Transition 2", Units = APIEntry.UnitType.TIME, Category = "orbit", ReturnType = "double")]
        object TimeToTransition2(DataSources ds) => ds.vessel.orbit.timeToTransition2;

        [TelemetryAPI("o.patchStartTransition", "Patch Start Transition Type", Units = APIEntry.UnitType.STRING, Category = "orbit", ReturnType = "string")]
        object PatchStartTransition(DataSources ds) => ds.vessel.orbit.patchStartTransition.ToString();

        [TelemetryAPI("o.patchEndTransition", "Patch End Transition Type", Units = APIEntry.UnitType.STRING, Category = "orbit", ReturnType = "string")]
        object PatchEndTransition(DataSources ds) => ds.vessel.orbit.patchEndTransition.ToString();

        [TelemetryAPI("o.StartUT", "Orbit Patch Start UT", Units = APIEntry.UnitType.DATE, Category = "orbit", ReturnType = "double")]
        object StartUT(DataSources ds) => ds.vessel.orbit.StartUT;

        [TelemetryAPI("o.EndUT", "Orbit Patch End UT", Units = APIEntry.UnitType.DATE, Category = "orbit", ReturnType = "double")]
        object EndUT(DataSources ds) => ds.vessel.orbit.EndUT;

        [TelemetryAPI("o.UTsoi", "UT of SOI Transition", Units = APIEntry.UnitType.DATE, Category = "orbit", ReturnType = "double")]
        object UTsoi(DataSources ds) => ds.vessel.orbit.UTsoi;

        // --- Closest Encounter ---

        [TelemetryAPI("o.closestEncounterBody", "Closest Encounter Body Name", Units = APIEntry.UnitType.STRING, Category = "orbit", ReturnType = "string")]
        object ClosestEncounterBody(DataSources ds) =>
            ds.vessel.orbit.closestEncounterBody != null ? ds.vessel.orbit.closestEncounterBody.name : "";

        [TelemetryAPI("o.closestTgtApprUT", "Closest Target Approach UT", Units = APIEntry.UnitType.DATE, Category = "orbit", ReturnType = "double")]
        object ClosestTgtApprUT(DataSources ds) => ds.vessel.orbit.closestTgtApprUT;

        // --- Encounter Detection ---

        [TelemetryAPI("o.encounterExists", "SOI Encounter Exists (-1=escape, 0=none, 1=encounter)", Category = "orbit", ReturnType = "int")]
        object EncounterExists(DataSources ds)
        {
            return ds.vessel.orbit.patchEndTransition switch
            {
                Orbit.PatchTransitionType.ENCOUNTER => 1,
                Orbit.PatchTransitionType.ESCAPE => -1,
                _ => 0
            };
        }

        [TelemetryAPI("o.encounterBody", "Encounter/Escape Body Name", Units = APIEntry.UnitType.STRING, Category = "orbit", ReturnType = "string")]
        object EncounterBody(DataSources ds)
        {
            return ds.vessel.orbit.patchEndTransition switch
            {
                Orbit.PatchTransitionType.ENCOUNTER => ds.vessel.orbit.nextPatch.referenceBody.bodyName,
                Orbit.PatchTransitionType.ESCAPE => ds.vessel.orbit.referenceBody.referenceBody?.bodyName ?? "",
                _ => ""
            };
        }

        [TelemetryAPI("o.encounterTime", "Time Until SOI Encounter/Escape", Units = APIEntry.UnitType.TIME, Category = "orbit", ReturnType = "double")]
        object EncounterTime(DataSources ds)
        {
            if (ds.vessel.orbit.patchEndTransition == Orbit.PatchTransitionType.ENCOUNTER ||
                ds.vessel.orbit.patchEndTransition == Orbit.PatchTransitionType.ESCAPE)
                return ds.vessel.orbit.UTsoi - Planetarium.GetUniversalTime();
            return -1d;
        }

        // --- Speed At ---

        [TelemetryAPI("o.orbitalSpeedAt", "Orbital Speed at Orbit Time", Units = APIEntry.UnitType.VELOCITY, Category = "orbit", ReturnType = "double", Params = "double obt")]
        object OrbitalSpeedAt(DataSources ds) =>
            ds.vessel.orbit.getOrbitalSpeedAt(double.Parse(ds.args[0]));

        [TelemetryAPI("o.orbitalSpeedAtDistance", "Orbital Speed at Distance", Units = APIEntry.UnitType.VELOCITY, Category = "orbit", ReturnType = "double", Params = "double distance")]
        object OrbitalSpeedAtDistance(DataSources ds) =>
            ds.vessel.orbit.getOrbitalSpeedAtDistance(double.Parse(ds.args[0]));

        [TelemetryAPI("o.radiusAtTrueAnomaly", "Radius at True Anomaly", Units = APIEntry.UnitType.DISTANCE, Category = "orbit", ReturnType = "double", Params = "double trueAnomaly")]
        object RadiusAtTrueAnomaly(DataSources ds) =>
            ds.vessel.orbit.RadiusAtTrueAnomaly(double.Parse(ds.args[0]));

        [TelemetryAPI("o.trueAnomalyAtRadius", "True Anomaly at Radius", Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double", Params = "double radius")]
        object TrueAnomalyAtRadius(DataSources ds) =>
            ds.vessel.orbit.TrueAnomalyAtRadius(double.Parse(ds.args[0]));

        // --- Orbit Patches ---

        [TelemetryAPI("o.orbitPatches", "Detailed Orbit Patches Info",
            Plotable = false, Formatter = "OrbitPatchList", Category = "orbit", ReturnType = "object")]
        object GetOrbitPatches(DataSources ds) =>
            OrbitPatches.getPatchesForOrbit(ds.vessel.orbit);

        [TelemetryAPI("o.trueAnomalyAtUTForOrbitPatch",
            "The orbit patch's True Anomaly at Universal Time",
            Units = APIEntry.UnitType.DEG, Category = "orbit", ReturnType = "double", Params = "int patchIndex, double UT")]
        object TrueAnomalyAtUTForOrbitPatch(DataSources ds)
        {
            int index = int.Parse(ds.args[0]);
            float ut = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(ds.vessel.orbit, index);
            if (orbitPatch == null) return null;
            return orbitPatch.TrueAnomalyAtUT(ut);
        }

        [TelemetryAPI("o.UTForTrueAnomalyForOrbitPatch",
            "The orbit patch's Universal Time at True Anomaly",
            Units = APIEntry.UnitType.DATE, Category = "orbit", ReturnType = "double", Params = "int patchIndex, double trueAnomaly")]
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
            "The orbit patch's predicted displacement from the center of the main body at the given true anomaly",
            Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d", Params = "int patchIndex, double trueAnomaly")]
        object RelativePositionAtTrueAnomalyForOrbitPatch(DataSources ds)
        {
            int index = int.Parse(ds.args[0]);
            float trueAnomaly = float.Parse(ds.args[1]);
            Orbit orbitPatch = OrbitPatches.getOrbitPatch(ds.vessel.orbit, index);
            if (orbitPatch == null) return null;
            return orbitPatch.getRelativePositionFromTrueAnomaly(trueAnomaly);
        }

        [TelemetryAPI("o.relativePositionAtUTForOrbitPatch",
            "The orbit patch's predicted displacement from the center of the main body at the given universal time",
            Formatter = "Vector3d", Category = "orbit", ReturnType = "Vector3d", Params = "int patchIndex, double UT")]
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

        [TelemetryAPI("n.heading2", "Heading", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object Heading2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.y;

        [TelemetryAPI("n.pitch2", "Pitch", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object Pitch2(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM);
            return (result.eulerAngles.x > 180) ? (360.0 - result.eulerAngles.x) : -result.eulerAngles.x;
        }

        [TelemetryAPI("n.roll2", "Roll", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object Roll2(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM);
            return (result.eulerAngles.z > 180) ? (result.eulerAngles.z - 360.0) : result.eulerAngles.z;
        }

        [TelemetryAPI("n.rawheading2", "Raw Heading", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object RawHeading2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.y;

        [TelemetryAPI("n.rawpitch2", "Raw Pitch", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object RawPitch2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.x;

        [TelemetryAPI("n.rawroll2", "Raw Roll", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object RawRoll2(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.CoM).eulerAngles.z;

        [TelemetryAPI("n.heading", "Heading calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object Heading(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position).eulerAngles.y;

        [TelemetryAPI("n.pitch", "Pitch calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object Pitch(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position);
            return (result.eulerAngles.x > 180) ? (360.0 - result.eulerAngles.x) : -result.eulerAngles.x;
        }

        [TelemetryAPI("n.roll", "Roll calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object Roll(DataSources ds)
        {
            var result = UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position);
            return (result.eulerAngles.z > 180) ? (result.eulerAngles.z - 360.0) : result.eulerAngles.z;
        }

        [TelemetryAPI("n.rawheading", "Raw Heading calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object RawHeading(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position).eulerAngles.y;

        [TelemetryAPI("n.rawpitch", "Raw Pitch calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
        object RawPitch(DataSources ds) =>
            UpdateHeadingPitchRoll(ds.vessel, ds.vessel.rootPart.transform.position).eulerAngles.x;

        [TelemetryAPI("n.rawroll", "Raw Roll calculated using the position of the vessels root part", Units = APIEntry.UnitType.DEG, Category = "navigation", ReturnType = "double")]
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

        [TelemetryAPI("b.name", "Body Name", Units = APIEntry.UnitType.STRING, Category = "body", ReturnType = "string", Params = "int bodyId")]
        object BodyName(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].name;

        [TelemetryAPI("b.description", "Body Description", Units = APIEntry.UnitType.STRING, Category = "body", ReturnType = "string", Params = "int bodyId")]
        object BodyDescription(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].bodyDescription;

        [TelemetryAPI("b.number", "Number of Bodies", Category = "body", ReturnType = "int")]
        object BodyCount(DataSources ds) => FlightGlobals.Bodies.Count;

        [TelemetryAPI("b.index", "Flight Globals Index", Category = "body", ReturnType = "int", Params = "int bodyId")]
        object BodyIndex(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].flightGlobalsIndex;

        [TelemetryAPI("b.referenceBody", "Reference Body Name", Units = APIEntry.UnitType.STRING, Category = "body", ReturnType = "string", Params = "int bodyId")]
        object BodyReferenceBody(DataSources ds)
        {
            var body = FlightGlobals.Bodies[int.Parse(ds.args[0])];
            return body.referenceBody != null ? body.referenceBody.name : "";
        }

        [TelemetryAPI("b.orbitingBodies", "Orbiting Body Names", Plotable = false, Formatter = "StringArray", Category = "body", ReturnType = "string[]", Params = "int bodyId")]
        object BodyOrbitingBodies(DataSources ds)
        {
            var names = new List<string>();
            foreach (var child in FlightGlobals.Bodies[int.Parse(ds.args[0])].orbitingBodies)
                names.Add(child.name);
            return names;
        }

        // --- Physical Properties ---

        [TelemetryAPI("b.radius", "Body Radius", Units = APIEntry.UnitType.DISTANCE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object Radius(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].Radius;

        [TelemetryAPI("b.mass", "Body Mass", Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyMass(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].Mass;

        [TelemetryAPI("b.geeASL", "Surface Gravity in G", Units = APIEntry.UnitType.G, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyGeeASL(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].GeeASL;

        [TelemetryAPI("b.soi", "Body Sphere of Influence", Units = APIEntry.UnitType.DISTANCE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object Soi(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].sphereOfInfluence;

        [TelemetryAPI("b.hillSphere", "Hill Sphere Radius", Units = APIEntry.UnitType.DISTANCE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object HillSphere(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].hillSphere;

        // --- Rotation ---

        [TelemetryAPI("b.rotationPeriod", "Rotation Period", Units = APIEntry.UnitType.TIME, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object RotationPeriod(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].rotationPeriod;

        [TelemetryAPI("b.rotationAngle", "Current Rotation Angle", Units = APIEntry.UnitType.DEG, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyRotationAngle(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].rotationAngle;

        [TelemetryAPI("b.angularV", "Angular Velocity", Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyAngularV(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].angularV;

        [TelemetryAPI("b.tidallyLocked", "Tidally Locked", Category = "body", ReturnType = "bool", Params = "int bodyId")]
        object TidallyLocked(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].tidallyLocked;

        [TelemetryAPI("b.rotates", "Body Rotates", Category = "body", ReturnType = "bool", Params = "int bodyId")]
        object BodyRotates(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].rotates;

        // --- Atmosphere ---

        [TelemetryAPI("b.atmosphere", "Has Atmosphere", Category = "body", ReturnType = "bool", Params = "int bodyId")]
        object BodyHasAtmosphere(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphere;

        [TelemetryAPI("b.maxAtmosphere", "Body Atmosphere Depth", Units = APIEntry.UnitType.DISTANCE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object MaxAtmosphere(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphereDepth;

        [TelemetryAPI("b.atmosphereContainsOxygen", "Atmosphere contains oxygen", Category = "body", ReturnType = "bool", Params = "int bodyId")]
        object AtmosphereContainsOxygen(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].atmosphereContainsOxygen;

        // --- Surface ---

        [TelemetryAPI("b.ocean", "Has Ocean", Category = "body", ReturnType = "bool", Params = "int bodyId")]
        object BodyHasOcean(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].ocean;

        // --- Position ---

        [TelemetryAPI("b.position", "Body World Position", Plotable = false, Formatter = "Vector3d", Category = "body", ReturnType = "Vector3d", Params = "int bodyId")]
        object BodyPosition(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].position;

        // --- Time Warp ---

        [TelemetryAPI("b.timeWarpAltitudeLimits", "Time Warp Altitude Limits", Plotable = false, Category = "body", ReturnType = "object", Params = "int bodyId")]
        object BodyTimeWarpAltitudeLimits(DataSources ds)
        {
            float[] limits = FlightGlobals.Bodies[int.Parse(ds.args[0])].timeWarpAltitudeLimits;
            var result = new List<float>(limits);
            return result;
        }

        [TelemetryAPI("b.o.gravParameter", "Body Gravitational Parameter", Units = APIEntry.UnitType.GRAV, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object GravParameter(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].gravParameter;

        [TelemetryAPI("b.o.relativeVelocity", "Relative Velocity", Units = APIEntry.UnitType.VELOCITY, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyRelativeVelocity(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.GetRelativeVel().magnitude;

        [TelemetryAPI("b.o.PeA", "Periapsis", Units = APIEntry.UnitType.DISTANCE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyPeA(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.PeA;

        [TelemetryAPI("b.o.ApA", "Apoapsis", Units = APIEntry.UnitType.DISTANCE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyApA(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.ApA;

        [TelemetryAPI("b.o.timeToAp", "Time to Apoapsis", Units = APIEntry.UnitType.TIME, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyTimeToAp(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToAp;

        [TelemetryAPI("b.o.timeToPe", "Time to Periapsis", Units = APIEntry.UnitType.TIME, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyTimeToPe(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToPe;

        [TelemetryAPI("b.o.inclination", "Inclination", Units = APIEntry.UnitType.DEG, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyInclination(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.inclination;

        [TelemetryAPI("b.o.eccentricity", "Eccentricity", Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyEccentricity(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.eccentricity;

        [TelemetryAPI("b.o.period", "Orbital Period", Units = APIEntry.UnitType.TIME, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyPeriod(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.period;

        [TelemetryAPI("b.o.argumentOfPeriapsis", "Argument of Periapsis", Units = APIEntry.UnitType.DEG, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyArgumentOfPeriapsis(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.argumentOfPeriapsis;

        [TelemetryAPI("b.o.timeToTransition1", "Time to Transition 1", Units = APIEntry.UnitType.TIME, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyTimeToTransition1(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToTransition1;

        [TelemetryAPI("b.o.timeToTransition2", "Time to Transition 2", Units = APIEntry.UnitType.TIME, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyTimeToTransition2(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.timeToTransition2;

        [TelemetryAPI("b.o.sma", "Semimajor Axis", Units = APIEntry.UnitType.DISTANCE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodySma(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.semiMajorAxis;

        [TelemetryAPI("b.o.lan", "Longitude of Ascending Node", Units = APIEntry.UnitType.DEG, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyLan(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.LAN;

        [TelemetryAPI("b.o.maae", "Mean Anomaly at Epoch", Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyMaae(DataSources ds) => FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.meanAnomalyAtEpoch;

        [TelemetryAPI("b.o.timeOfPeriapsisPassage", "Time of Periapsis Passage", Units = APIEntry.UnitType.DATE, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyTimeOfPeriapsisPassage(DataSources ds) =>
            Planetarium.GetUniversalTime() - FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.ObT;

        [TelemetryAPI("b.o.trueAnomaly", "True Anomaly", Units = APIEntry.UnitType.DEG, Category = "body", ReturnType = "double", Params = "int bodyId")]
        object BodyTrueAnomaly(DataSources ds) =>
            FlightGlobals.Bodies[int.Parse(ds.args[0])].orbit.TrueAnomalyAtUT(Planetarium.GetUniversalTime()) * (180.0 / Math.PI);

        [TelemetryAPI("b.o.phaseAngle", "Phase Angle", Units = APIEntry.UnitType.DEG, Category = "body", ReturnType = "double", Params = "int bodyId")]
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

        [TelemetryAPI("b.o.truePositionAtUT", "True Position at the given UT",
            Plotable = false, Formatter = "Vector3d", Category = "body", ReturnType = "Vector3d", Params = "int bodyId, double UT")]
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
            : base(formatters)
        {
            // Map view transitions must run on the main Unity thread
            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { if (MapView.MapIsEnabled) MapView.ExitMapView(); else MapView.EnterMapView(); return 0d; }),
                "m.toggleMapView", "Toggle Map View", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { MapView.EnterMapView(); return 0d; }),
                "m.enterMapView", "Enter Map View", formatters.Default));

            registerAPI(new ActionAPIEntry(
                queueDelayed(x => { MapView.ExitMapView(); return 0d; }),
                "m.exitMapView", "Exit Map View", formatters.Default));
        }

        [TelemetryAPI("m.mapIsEnabled", "Map View Is Enabled", Category = "map", ReturnType = "bool")]
        object MapIsEnabled(DataSources ds) => MapView.MapIsEnabled;

        [TelemetryAPI("o.maneuverNodes", "Maneuver Nodes",
            Plotable = false, Formatter = "ManeuverNodeList", Category = "maneuver", ReturnType = "object")]
        object ManeuverNodes(DataSources ds)
        {
            PluginLogger.debug("Start GET");
            return ds.vessel.patchedConicSolver.maneuverNodes;
        }

        [TelemetryAPI("o.maneuverNodes.count", "Number of Maneuver Nodes", Category = "maneuver", ReturnType = "int")]
        object ManeuverNodeCount(DataSources ds) =>
            ds.vessel.patchedConicSolver.maneuverNodes.Count;

        [TelemetryAPI("o.maneuverNodes.deltaV", "Maneuver Node Delta-V Vector",
            Plotable = false, Formatter = "Vector3d", Category = "maneuver", ReturnType = "Vector3d", Params = "int id")]
        object ManeuverNodeDeltaV(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.DeltaV;
        }

        [TelemetryAPI("o.maneuverNodes.deltaVMagnitude", "Maneuver Node Delta-V Magnitude",
            Units = APIEntry.UnitType.VELOCITY, Category = "maneuver", ReturnType = "double", Params = "int id")]
        object ManeuverNodeDeltaVMagnitude(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.DeltaV.magnitude;
        }

        [TelemetryAPI("o.maneuverNodes.UT", "Maneuver Node Universal Time",
            Units = APIEntry.UnitType.DATE, Category = "maneuver", ReturnType = "double", Params = "int id")]
        object ManeuverNodeUT(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.UT;
        }

        [TelemetryAPI("o.maneuverNodes.timeTo", "Time Until Maneuver Node",
            Units = APIEntry.UnitType.TIME, Category = "maneuver", ReturnType = "double", Params = "int id")]
        object ManeuverNodeTimeTo(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.UT - Planetarium.GetUniversalTime();
        }

        [TelemetryAPI("o.maneuverNodes.burnVector", "Maneuver Node Burn Vector (world space)",
            Plotable = false, Formatter = "Vector3d", Category = "maneuver", ReturnType = "Vector3d", Params = "int id")]
        object ManeuverNodeBurnVector(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return node.GetBurnVector(node.patch);
        }

        [TelemetryAPI("o.maneuverNodes.orbitPatches",
            "Orbit Patches for Maneuver Node",
            Plotable = false, Formatter = "OrbitPatchList", Category = "maneuver", ReturnType = "object", Params = "int id")]
        object ManeuverNodeOrbitPatches(DataSources ds)
        {
            ManeuverNode node = GetManeuverNode(ds, int.Parse(ds.args[0]));
            if (node == null) return null;
            return OrbitPatches.getPatchesForOrbit(node.nextPatch);
        }

        [TelemetryAPI("o.maneuverNodes.trueAnomalyAtUTForManeuverNodesOrbitPatch",
            "For a maneuver node, The orbit patch's True Anomaly at Universal Time",
            Units = APIEntry.UnitType.DEG, Category = "maneuver", ReturnType = "double", Params = "int id, int patchIndex, double UT")]
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
            "For a maneuver node, The orbit patch's Universal Time at True Anomaly",
            Units = APIEntry.UnitType.DATE, Category = "maneuver", ReturnType = "double", Params = "int id, int patchIndex, double trueAnomaly")]
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
            "For a maneuver node, The orbit patch's predicted displacement from the center of the main body at the given true anomaly",
            Formatter = "Vector3d", Category = "maneuver", ReturnType = "Vector3d", Params = "int id, int patchIndex, double trueAnomaly")]
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
            "For a maneuver node, The orbit patch's predicted displacement from the center of the main body at the given universal time",
            Formatter = "Vector3d", Category = "maneuver", ReturnType = "Vector3d", Params = "int id, int patchIndex, double UT")]
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
            "Add a manuever based on a UT and DeltaV X, Y and Z",
            IsAction = true, Formatter = "ManeuverNode", Category = "maneuver", ReturnType = "object", Params = "float ut, float x, float y, float z")]
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
            "Set a manuever node's UT and DeltaV X, Y and Z",
            IsAction = true, Formatter = "ManeuverNode", Category = "maneuver", ReturnType = "object", Params = "int id, float ut, float x, float y, float z")]
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

        [TelemetryAPI("o.removeManeuverNode", "Remove a manuever node", IsAction = true, Category = "maneuver", ReturnType = "bool", Params = "int id")]
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

        [TelemetryAPI("o.gameLanguage", "Language", Units = APIEntry.UnitType.STRING, Category = "orbit", ReturnType = "string")]
        object GameLanguage(DataSources ds)
        {
            PluginLogger.debug("Start GET");
            return KSP.Localization.Localizer.CurrentLanguage;
        }
    }
}
