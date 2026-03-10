using System;
using UnityEngine;

namespace Telemachus
{
    /// <summary>
    /// Landing prediction and descent telemetry.
    /// Computes time to impact, suicide burn timing, speed at impact,
    /// predicted landing coordinates, and terrain slope — all natively,
    /// no external mod dependencies.
    /// Algorithms ported from RasterPropMonitor (GPLv3, by Mihara/MOARdV).
    /// </summary>
    public class LandingDataLinkHandler : DataLinkHandler
    {
        public LandingDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // --- Time to impact ---

        [TelemetryAPI("land.timeToImpact", "Estimated Seconds to Impact",
            Units = APIEntry.UnitType.TIME, Category = "landing", ReturnType = "double")]
        object TimeToImpact(DataSources ds)
        {
            var vessel = ds.vessel;
            if (vessel.situation != Vessel.Situations.SUB_ORBITAL &&
                vessel.situation != Vessel.Situations.FLYING)
                return double.NaN;

            Vector3d up = (vessel.CoMD - vessel.mainBody.position).normalized;
            double speedVertical = Vector3d.Dot(vessel.srf_velocity, up);
            double accelUp = Vector3d.Dot(vessel.acceleration, up);

            double altitudeTrue = vessel.altitude - vessel.terrainAltitude;
            double altitudeASL = vessel.altitude;
            double altitude = altitudeTrue;
            if (vessel.mainBody.ocean && altitudeASL > 0.0)
                altitude = Math.Min(altitudeASL, altitudeTrue);

            double geeASL = vessel.mainBody.GeeASL * 9.80665;

            if (accelUp < 0.0 || speedVertical >= 0.0 || Planetarium.TimeScale > 1.0)
            {
                return (speedVertical + Math.Sqrt(speedVertical * speedVertical + 2 * geeASL * altitude)) / geeASL;
            }
            else if (accelUp > 0.005)
            {
                return (speedVertical + Math.Sqrt(speedVertical * speedVertical + 2 * accelUp * altitude)) / accelUp;
            }
            else
            {
                return speedVertical < 0 ? altitude / -speedVertical : double.NaN;
            }
        }

        // --- Speed at impact ---

        [TelemetryAPI("land.speedAtImpact", "Predicted Speed at Impact (current thrust)",
            Units = APIEntry.UnitType.VELOCITY, Category = "landing", ReturnType = "double")]
        object SpeedAtImpact(DataSources ds)
        {
            return ComputeSpeedAtImpact(ds.vessel, false);
        }

        [TelemetryAPI("land.bestSpeedAtImpact", "Predicted Speed at Impact (max thrust)",
            Units = APIEntry.UnitType.VELOCITY, Category = "landing", ReturnType = "double")]
        object BestSpeedAtImpact(DataSources ds)
        {
            return ComputeSpeedAtImpact(ds.vessel, true);
        }

        static double ComputeSpeedAtImpact(Vessel vessel, bool useMaxThrust)
        {
            Vector3d up = (vessel.CoMD - vessel.mainBody.position).normalized;
            double speedVertical = Vector3d.Dot(vessel.srf_velocity, up);
            double geeASL = vessel.mainBody.GeeASL * 9.80665;
            double altitudeTrue = vessel.altitude - vessel.terrainAltitude;

            float thrust = useMaxThrust
                ? GetTotalMaxThrust(vessel)
                : GetTotalCurrentThrust(vessel);
            float mass = vessel.GetTotalMass();

            double accel = geeASL - (mass > 0 ? thrust / mass : 0);
            double tti = accel > 0
                ? (speedVertical + Math.Sqrt(speedVertical * speedVertical + 2.0 * accel * altitudeTrue)) / accel
                : 0;
            double result = speedVertical - accel * tti;
            return double.IsNaN(result) ? 0.0 : result;
        }

        // --- Suicide burn countdown ---

        [TelemetryAPI("land.suicideBurnCountdown", "Seconds Until Suicide Burn Start",
            Units = APIEntry.UnitType.TIME, Category = "landing", ReturnType = "double")]
        object SuicideBurnCountdown(DataSources ds)
        {
            var vessel = ds.vessel;
            var orbit = vessel.orbit;
            if (orbit.PeA > 0.0)
                return double.NaN;

            Vector3d up = (vessel.CoMD - vessel.mainBody.position).normalized;
            double angleFromHorizontal = 90.0 - Vector3d.Angle(-vessel.srf_velocity, up);
            angleFromHorizontal = Math.Max(0.0, Math.Min(90.0, angleFromHorizontal));
            double sine = Math.Sin(angleFromHorizontal * Math.PI / 180.0);

            double g = (vessel.mainBody.gravParameter /
                ((vessel.altitude + vessel.mainBody.Radius) * (vessel.altitude + vessel.mainBody.Radius)));
            float maxThrust = GetTotalMaxThrust(vessel);
            float mass = vessel.GetTotalMass();
            double T = mass > 0 ? maxThrust / mass : 0;

            double decelTerm = (2.0 * g * sine) * (2.0 * g * sine) + 4.0 * (T * T - g * g);
            if (decelTerm < 0.0)
                return double.NaN;

            double effectiveDecel = 0.5 * (-2.0 * g * sine + Math.Sqrt(decelTerm));
            if (effectiveDecel <= 0.0)
                return double.NaN;

            double speedHorizontal = Vector3d.Exclude(up, vessel.srf_velocity).magnitude;
            double decelTime = speedHorizontal / effectiveDecel;

            Vector3d estimatedLandingSite = vessel.CoMD + 0.5 * decelTime * vessel.srf_velocity;
            double estLat = vessel.mainBody.GetLatitude(estimatedLandingSite);
            double estLon = vessel.mainBody.GetLongitude(estimatedLandingSite);
            double terrainRadius = vessel.mainBody.Radius +
                vessel.mainBody.TerrainAltitude(estLat, estLon);

            double impactTime;
            try
            {
                impactTime = orbit.GetNextTimeOfRadius(Planetarium.GetUniversalTime(), terrainRadius);
            }
            catch (ArgumentException)
            {
                return double.NaN;
            }
            return impactTime - decelTime / 2.0 - Planetarium.GetUniversalTime();
        }

        // --- Predicted landing position ---

        [TelemetryAPI("land.predictedLat", "Predicted Landing Latitude",
            Units = APIEntry.UnitType.LATLON, Category = "landing", ReturnType = "double")]
        object PredictedLat(DataSources ds)
        {
            PredictLanding(ds.vessel, out double lat, out _, out _);
            return lat;
        }

        [TelemetryAPI("land.predictedLon", "Predicted Landing Longitude",
            Units = APIEntry.UnitType.LATLON, Category = "landing", ReturnType = "double")]
        object PredictedLon(DataSources ds)
        {
            PredictLanding(ds.vessel, out _, out double lon, out _);
            return lon;
        }

        [TelemetryAPI("land.predictedAlt", "Predicted Landing Terrain Altitude",
            Units = APIEntry.UnitType.DISTANCE, Category = "landing", ReturnType = "double")]
        object PredictedAlt(DataSources ds)
        {
            PredictLanding(ds.vessel, out _, out _, out double alt);
            return alt;
        }

        /// <summary>
        /// Iterative landing prediction: finds where the orbit intersects terrain
        /// by repeatedly refining the target radius. Converges in ~5 iterations.
        /// </summary>
        static void PredictLanding(Vessel vessel, out double lat, out double lon, out double alt)
        {
            lat = lon = alt = 0.0;

            var orbit = vessel.orbit;
            if (orbit.eccentricity >= 1.0 && orbit.timeToPe < 0)
                return; // hyperbolic, already past Pe
            if (orbit.PeA > 0.0)
                return; // periapsis is above ground

            try
            {
                double targetRadius = orbit.PeR;
                double ut = Planetarium.GetUniversalTime();

                for (int i = 0; i < 8; i++)
                {
                    double nextUt = orbit.GetNextTimeOfRadius(ut, Math.Max(orbit.PeR, targetRadius));
                    Vector3d pos = orbit.getPositionAtUT(nextUt);
                    lat = vessel.mainBody.GetLatitude(pos);
                    lon = vessel.mainBody.GetLongitude(pos);
                    alt = vessel.mainBody.TerrainAltitude(lat, lon);
                    if (vessel.mainBody.ocean)
                        alt = Math.Max(alt, 0.0);

                    double newRadius = alt + vessel.mainBody.Radius;
                    if (Math.Abs(newRadius - targetRadius) < 10.0)
                        break; // converged
                    targetRadius = newRadius;
                }

                if (lon > 180.0)
                    lon -= 360.0;
            }
            catch
            {
                lat = lon = alt = 0.0;
            }
        }

        // --- Terrain slope ---

        [TelemetryAPI("land.slopeAngle", "Terrain Slope Angle Under Vessel",
            Units = APIEntry.UnitType.DEG, Category = "landing", ReturnType = "double")]
        object SlopeAngle(DataSources ds)
        {
            var vessel = ds.vessel;
            // terrainNormal is only meaningful near the surface
            if (vessel.altitude - vessel.terrainAltitude > 5000)
                return -1.0;

            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;
            Vector3 normal = vessel.terrainNormal;
            if (normal.sqrMagnitude < 0.01f)
                return -1.0;

            return (double)Vector3.Angle(up, normal);
        }

        // --- Helpers ---

        static float GetTotalCurrentThrust(Vessel vessel)
        {
            float total = 0f;
            foreach (var part in vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (module is ModuleEngines engine && engine.EngineIgnited && !engine.flameout)
                        total += engine.finalThrust;
                }
            }
            return total;
        }

        static float GetTotalMaxThrust(Vessel vessel)
        {
            float total = 0f;
            foreach (var part in vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (module is ModuleEngines engine && engine.EngineIgnited && !engine.flameout)
                        total += engine.maxThrust * (engine.thrustPercentage / 100f);
                }
            }
            return total;
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }
}
