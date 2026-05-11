using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Telemachus
{
    /// <summary>Shared snapshot helper for FlightLogger — most fields are private instance fields, accessed via cached reflection.</summary>
    public static class FlightLoggerSnapshot
    {
        private static bool _reflectionResolved;
        private static FieldInfo _highestAltitude;
        private static FieldInfo _groundDistance;
        private static FieldInfo _totalDistance;
        private static FieldInfo _highestGee;
        private static FieldInfo _highestSpeed;
        private static FieldInfo _highestSpeedOverLand;
        private static FieldInfo _liftOff;
        private static FieldInfo _missionEnd;
        private static FieldInfo _partsLost;
        private static FieldInfo _kerbalsKilled;
        private static FieldInfo _flightEndMode;

        private static void EnsureReflection()
        {
            if (_reflectionResolved) return;
            var t = typeof(FlightLogger);
            const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
            _highestAltitude = t.GetField("highestAltitude", bf);
            _groundDistance = t.GetField("groundDistance", bf);
            _totalDistance = t.GetField("totalDistance", bf);
            _highestGee = t.GetField("highestGee", bf);
            _highestSpeed = t.GetField("highestSpeed", bf);
            _highestSpeedOverLand = t.GetField("highestSpeedOverLand", bf);
            _liftOff = t.GetField("liftOff", bf);
            _missionEnd = t.GetField("missionEnd", bf);
            _partsLost = t.GetField("partsLost", bf);
            _kerbalsKilled = t.GetField("kerbalsKilled", bf);
            _flightEndMode = t.GetField("flightEndMode", bf);
            _reflectionResolved = true;
        }

        private static double R4(double v) => Math.Round(v, 4);

        private static T ReadField<T>(FieldInfo f, FlightLogger logger, T fallback)
        {
            if (f == null || logger == null) return fallback;
            try
            {
                var v = f.GetValue(logger);
                if (v is T typed) return typed;
                return fallback;
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        /// <summary>Snapshot current FlightLogger state — returns null when FlightLogger.fetch isn't available (no flight scene).</summary>
        public static Dictionary<string, object> Capture()
        {
            try
            {
                var logger = FlightLogger.fetch;
                if (logger == null) return null;
                EnsureReflection();
                return new Dictionary<string, object>
                {
                    ["missionTime"] = R4(FlightLogger.met),
                    ["liftOff"] = FlightLogger.LiftOff,
                    ["highestAltitude"] = R4(ReadField(_highestAltitude, logger, 0.0)),
                    ["highestSpeed"] = R4(ReadField(_highestSpeed, logger, 0.0)),
                    ["highestSpeedOverLand"] = R4(ReadField(_highestSpeedOverLand, logger, 0.0)),
                    ["groundDistance"] = R4(ReadField(_groundDistance, logger, 0.0)),
                    ["totalDistance"] = R4(ReadField(_totalDistance, logger, 0.0)),
                    ["highestGee"] = R4(ReadField(_highestGee, logger, 0.0)),
                    ["partsLost"] = ReadField(_partsLost, logger, 0),
                    ["kerbalsKilled"] = ReadField(_kerbalsKilled, logger, 0),
                    ["missionEnd"] = ReadField(_missionEnd, logger, false),
                    ["flightEndMode"] =
                        ReadField<object>(_flightEndMode, logger, null)?.ToString() ?? string.Empty,
                };
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Telemachus] FlightLoggerSnapshot.Capture failed: " + e.Message);
                return null;
            }
        }

        /// <summary>Copy of FlightLogger.eventLog so later mutations don't change captured snapshots.</summary>
        public static List<string> CaptureEvents()
        {
            try
            {
                var log = FlightLogger.eventLog;
                if (log == null) return new List<string>();
                return new List<string>(log);
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
