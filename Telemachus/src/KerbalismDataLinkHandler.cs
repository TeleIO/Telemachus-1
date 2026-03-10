using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Telemachus
{
    /// <summary>
    /// Exposes life support, radiation, habitat, crew health, comms, science,
    /// and space weather data from Kerbalism.
    /// Kerbalism provides a public static API class (KERBALISM.API) designed
    /// for reflection-based access. Deeper data comes from VesselData, DB,
    /// and Storm classes.
    /// All access is via reflection — soft dependency.
    /// </summary>
    public class KerbalismDataLinkHandler : DataLinkHandler
    {
        static bool _searched;

        // KERBALISM.API — public static methods for mod interop
        static Type _apiType;

        // KERBALISM.Features — feature flags
        static Type _featuresType;

        // KERBALISM.DB — database access
        static Type _dbType;
        static MethodInfo _dbStormMethod;   // DB.Storm(string body_name)
        static MethodInfo _dbKerbalMethod;  // DB.Kerbal(string name)

        // KERBALISM.StormData
        static Type _stormDataType;
        static FieldInfo _stormStateField;
        static FieldInfo _stormTimeField;
        static FieldInfo _stormDurationField;

        // KERBALISM.KerbalData
        static Type _kerbalDataType;
        static FieldInfo _kerbalRulesField;  // Dictionary<string, RuleData>

        // KERBALISM.RuleData
        static Type _ruleDataType;
        static FieldInfo _ruleProblemField;

        // VesselData extension method & properties
        static Type _vesselDataType;
        static MethodInfo _kerbalismDataMethod;  // Vessel.KerbalismData() extension
        static PropertyInfo _vdEnvTemperature;
        static PropertyInfo _vdEnvTempDiff;
        static PropertyInfo _vdEnvStormRadiation;
        static PropertyInfo _vdEnvBreathable;
        static PropertyInfo _vdEnvInAtmosphere;
        static PropertyInfo _vdCrewCount;
        static PropertyInfo _vdCrewCapacity;
        static PropertyInfo _vdMalfunction;
        static PropertyInfo _vdCritical;
        static PropertyInfo _vdSolarPanelExposure;
        static PropertyInfo _vdDrivesFreeSpace;
        static PropertyInfo _vdDrivesCapacity;

        // ConnectionInfo
        static PropertyInfo _vdConnection;
        static Type _connectionInfoType;
        static PropertyInfo _connRate;
        static PropertyInfo _connLinked;
        static PropertyInfo _connStrength;
        static PropertyInfo _connStatus;
        static PropertyInfo _connTargetName;
        static PropertyInfo _connEc;
        static PropertyInfo _connEcIdle;

        // Cached API method lookups
        static readonly Dictionary<string, MethodInfo> _apiMethods = new();

        public KerbalismDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        static void Search()
        {
            if (_searched) return;
            _searched = true;

            foreach (var asm in AssemblyLoader.loadedAssemblies)
            {
                try
                {
                    _apiType ??= asm.assembly.GetType("KERBALISM.API", false);
                    _featuresType ??= asm.assembly.GetType("KERBALISM.Features", false);
                    _dbType ??= asm.assembly.GetType("KERBALISM.DB", false);
                    _stormDataType ??= asm.assembly.GetType("KERBALISM.StormData", false);
                    _kerbalDataType ??= asm.assembly.GetType("KERBALISM.KerbalData", false);
                    _ruleDataType ??= asm.assembly.GetType("KERBALISM.RuleData", false);
                    _vesselDataType ??= asm.assembly.GetType("KERBALISM.VesselData", false);
                    _connectionInfoType ??= asm.assembly.GetType("KERBALISM.ConnectionInfo", false);
                }
                catch { }
            }

            if (_apiType == null)
            {
                PluginLogger.debug("Kerbalism not found");
                return;
            }

            PluginLogger.debug("Kerbalism detected: " + _apiType.Assembly.GetName().Version);

            var pub = BindingFlags.Public | BindingFlags.Static;
            var pubInst = BindingFlags.Public | BindingFlags.Instance;
            var nonPubInst = BindingFlags.NonPublic | BindingFlags.Instance;

            // DB methods
            if (_dbType != null)
            {
                _dbStormMethod = _dbType.GetMethod("Storm", pub, null, new[] { typeof(string) }, null);
                _dbKerbalMethod = _dbType.GetMethod("Kerbal", pub, null, new[] { typeof(string) }, null);
            }

            // StormData fields
            if (_stormDataType != null)
            {
                _stormStateField = _stormDataType.GetField("storm_state", pubInst | nonPubInst);
                _stormTimeField = _stormDataType.GetField("storm_time", pubInst | nonPubInst);
                _stormDurationField = _stormDataType.GetField("storm_duration", pubInst | nonPubInst);
            }

            // KerbalData
            if (_kerbalDataType != null)
                _kerbalRulesField = _kerbalDataType.GetField("rules", pubInst | nonPubInst);

            // RuleData
            if (_ruleDataType != null)
                _ruleProblemField = _ruleDataType.GetField("problem", pubInst | nonPubInst);

            // VesselData — look for extension method KerbalismData()
            if (_vesselDataType != null)
            {
                // Extension is in KERBALISM.DB or a static helper
                foreach (var asm in AssemblyLoader.loadedAssemblies)
                {
                    try
                    {
                        foreach (var type in asm.assembly.GetTypes())
                        {
                            var m = type.GetMethod("KerbalismData", pub,
                                null, new[] { typeof(Vessel) }, null);
                            if (m != null && m.ReturnType == _vesselDataType)
                            {
                                _kerbalismDataMethod = m;
                                break;
                            }
                        }
                        if (_kerbalismDataMethod != null) break;
                    }
                    catch { }
                }

                _vdEnvTemperature = _vesselDataType.GetProperty("EnvTemperature", pubInst);
                _vdEnvTempDiff = _vesselDataType.GetProperty("EnvTempDiff", pubInst);
                _vdEnvStormRadiation = _vesselDataType.GetProperty("EnvStormRadiation", pubInst);
                _vdEnvBreathable = _vesselDataType.GetProperty("EnvBreathable", pubInst);
                _vdEnvInAtmosphere = _vesselDataType.GetProperty("EnvInAtmosphere", pubInst);
                _vdCrewCount = _vesselDataType.GetProperty("CrewCount", pubInst);
                _vdCrewCapacity = _vesselDataType.GetProperty("CrewCapacity", pubInst);
                _vdMalfunction = _vesselDataType.GetProperty("Malfunction", pubInst);
                _vdCritical = _vesselDataType.GetProperty("Critical", pubInst);
                _vdSolarPanelExposure = _vesselDataType.GetProperty("SolarPanelsAverageExposure", pubInst);
                _vdDrivesFreeSpace = _vesselDataType.GetProperty("DrivesFreeSpace", pubInst);
                _vdDrivesCapacity = _vesselDataType.GetProperty("DrivesCapacity", pubInst);
                _vdConnection = _vesselDataType.GetProperty("Connection", pubInst);
            }

            // ConnectionInfo fields
            if (_connectionInfoType != null)
            {
                _connRate = _connectionInfoType.GetProperty("rate", pubInst);
                _connLinked = _connectionInfoType.GetProperty("linked", pubInst);
                _connStrength = _connectionInfoType.GetProperty("strength", pubInst);
                _connStatus = _connectionInfoType.GetProperty("status", pubInst);
                _connTargetName = _connectionInfoType.GetProperty("target_name", pubInst);
                _connEc = _connectionInfoType.GetProperty("ec", pubInst);
                _connEcIdle = _connectionInfoType.GetProperty("ec_idle", pubInst);

            }
        }

        /// <summary>Call a static method on KERBALISM.API with no args.</summary>
        static object CallAPI(string methodName)
        {
            if (_apiType == null) return null;
            if (!_apiMethods.TryGetValue(methodName, out var method))
            {
                method = _apiType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                _apiMethods[methodName] = method;
            }
            return method?.Invoke(null, null);
        }

        /// <summary>Call a static method on KERBALISM.API with a Vessel arg.</summary>
        static object CallAPI(string methodName, Vessel v)
        {
            if (_apiType == null) return null;
            var key = methodName + "_v";
            if (!_apiMethods.TryGetValue(key, out var method))
            {
                method = _apiType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static,
                    null, new[] { typeof(Vessel) }, null);
                _apiMethods[key] = method;
            }
            return method?.Invoke(null, new object[] { v });
        }

        /// <summary>Call a static method on KERBALISM.API with a CelestialBody arg.</summary>
        static object CallAPI(string methodName, CelestialBody body)
        {
            if (_apiType == null) return null;
            var key = methodName + "_b";
            if (!_apiMethods.TryGetValue(key, out var method))
            {
                method = _apiType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static,
                    null, new[] { typeof(CelestialBody) }, null);
                _apiMethods[key] = method;
            }
            return method?.Invoke(null, new object[] { body });
        }

        /// <summary>Get the VesselData for a vessel via the extension method.</summary>
        static object GetVesselData(Vessel v)
        {
            if (_kerbalismDataMethod == null) return null;
            try { return _kerbalismDataMethod.Invoke(null, new object[] { v }); }
            catch { return null; }
        }

        /// <summary>Read a field or property from an object by name.</summary>
        static object ReadMember(object obj, Type type, string name)
        {
            if (obj == null || type == null) return null;
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(obj);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            return field?.GetValue(obj);
        }

        static bool GetFeature(string name)
        {
            if (_featuresType == null) return false;
            var prop = _featuresType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            if (prop != null) return (bool)prop.GetValue(null);
            var field = _featuresType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            return field != null && (bool)field.GetValue(null);
        }

        // =====================================================================
        // Availability & Features
        // =====================================================================

        [TelemetryAPI("kerbalism.available", "Kerbalism Is Installed",
            AlwaysEvaluable = true, Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object Available(DataSources ds)
        {
            Search();
            return _apiType != null;
        }

        [TelemetryAPI("kerbalism.features", "Kerbalism Enabled Features",
            Plotable = false, AlwaysEvaluable = true, Category = "kerbalism", ReturnType = "object", RequiresMod = "kerbalism")]
        object Features(DataSources ds)
        {
            Search();
            if (_featuresType == null) return null;
            var result = new Dictionary<string, object>();
            foreach (var name in new[] { "Radiation", "Habitat", "Pressure", "Poisoning",
                                          "Science", "Reliability", "SpaceWeather" })
                result[name.ToLower()] = GetFeature(name);
            return result;
        }

        // =====================================================================
        // Radiation (via API class)
        // =====================================================================

        [TelemetryAPI("kerbalism.radiationEnabled", "Radiation System Enabled",
            AlwaysEvaluable = true, Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object RadiationEnabled(DataSources ds) { Search(); return CallAPI("RadiationEnabled"); }

        [TelemetryAPI("kerbalism.radiation", "Environment Radiation (rad/h)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object Radiation(DataSources ds) => CallAPI("Radiation", ds.vessel);

        [TelemetryAPI("kerbalism.habitatRadiation", "Habitat Radiation (rad/h)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object HabitatRadiation(DataSources ds) => CallAPI("HabitatRadiation", ds.vessel);

        [TelemetryAPI("kerbalism.magnetosphere", "Inside Magnetosphere",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object Magnetosphere(DataSources ds) => CallAPI("Magnetosphere", ds.vessel);

        [TelemetryAPI("kerbalism.innerBelt", "Inside Inner Radiation Belt",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object InnerBelt(DataSources ds) => CallAPI("InnerBelt", ds.vessel);

        [TelemetryAPI("kerbalism.outerBelt", "Inside Outer Radiation Belt",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object OuterBelt(DataSources ds) => CallAPI("OuterBelt", ds.vessel);

        [TelemetryAPI("kerbalism.stellarActivity", "Stellar Activity (0-1)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object SolarActivity(DataSources ds)
        {
            var sun = FlightGlobals.Bodies?.Count > 0 ? FlightGlobals.Bodies[0] : null;
            return sun != null ? CallAPI("GetSolarActivity", sun) : null;
        }

        // =====================================================================
        // Habitat & Life Support (via API class)
        // =====================================================================

        [TelemetryAPI("kerbalism.habitatVolume", "Habitat Volume (m\u00b3)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object HabitatVolume(DataSources ds) => CallAPI("Volume", ds.vessel);

        [TelemetryAPI("kerbalism.habitatSurface", "Habitat Surface Area (m\u00b2)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object HabitatSurface(DataSources ds) => CallAPI("Surface", ds.vessel);

        [TelemetryAPI("kerbalism.habitatPressure", "Habitat Pressure (0-1)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object HabitatPressure(DataSources ds) => CallAPI("Pressure", ds.vessel);

        [TelemetryAPI("kerbalism.co2Level", "CO2 Poisoning Level",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object CO2Level(DataSources ds) => CallAPI("Poisoning", ds.vessel);

        [TelemetryAPI("kerbalism.radiationShielding", "Radiation Shielding (0-1)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object RadiationShielding(DataSources ds) => CallAPI("Shielding", ds.vessel);

        [TelemetryAPI("kerbalism.habitatLivingSpace", "Living Space Comfort Factor",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object HabitatLivingSpace(DataSources ds) => CallAPI("LivingSpace", ds.vessel);

        [TelemetryAPI("kerbalism.habitatComfort", "Overall Habitat Comfort Factor",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object HabitatComfort(DataSources ds) => CallAPI("Comfort", ds.vessel);

        // =====================================================================
        // Comms (via API + VesselData.Connection)
        // =====================================================================

        [TelemetryAPI("kerbalism.connectionLinked", "Signal Connected",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object ConnectionLinked(DataSources ds) => CallAPI("VesselConnectionLinked", ds.vessel);

        [TelemetryAPI("kerbalism.connectionRate", "Data Rate (MB/s)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object ConnectionRate(DataSources ds) => CallAPI("VesselConnectionRate", ds.vessel);

        [TelemetryAPI("kerbalism.connectionTransmitting", "Files Transmitting",
            Category = "kerbalism", ReturnType = "int", RequiresMod = "kerbalism")]
        object ConnectionTransmitting(DataSources ds) => CallAPI("VesselConnectionTransmitting", ds.vessel);

        [TelemetryAPI("kerbalism.connection", "Full Connection Info",
            Plotable = false, Category = "kerbalism", ReturnType = "object", RequiresMod = "kerbalism")]
        object Connection(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            if (vd == null || _vdConnection == null) return null;

            var conn = _vdConnection.GetValue(vd);
            if (conn == null) return null;

            var info = new Dictionary<string, object>();
            info["linked"] = ReadMember(conn, _connectionInfoType, "linked");
            info["rate"] = ReadMember(conn, _connectionInfoType, "rate");
            info["strength"] = ReadMember(conn, _connectionInfoType, "strength");
            info["status"] = ReadMember(conn, _connectionInfoType, "status")?.ToString();
            info["target"] = ReadMember(conn, _connectionInfoType, "target_name");
            info["ec"] = ReadMember(conn, _connectionInfoType, "ec");
            info["ecIdle"] = ReadMember(conn, _connectionInfoType, "ec_idle");
            return info;
        }

        // =====================================================================
        // Science (via API class)
        // =====================================================================

        [TelemetryAPI("kerbalism.experimentRunning", "Experiment Is Running [string experiment_id]",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object ExperimentRunning(DataSources ds)
        {
            if (ds.args.Count < 1) return null;
            var key = "ExperimentIsRunning_vs";
            if (!_apiMethods.TryGetValue(key, out var method))
            {
                method = _apiType?.GetMethod("ExperimentIsRunning",
                    BindingFlags.Public | BindingFlags.Static,
                    null, new[] { typeof(Vessel), typeof(string) }, null);
                _apiMethods[key] = method;
            }
            return method?.Invoke(null, new object[] { ds.vessel, ds.args[0] });
        }

        // =====================================================================
        // Space Weather — Storms (via DB.Storm)
        // =====================================================================

        [TelemetryAPI("kerbalism.stellarStormIncoming", "Stellar Storm Incoming",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object SolarStormIncoming(DataSources ds)
        {
            var stormData = GetStormData(ds.vessel);
            if (stormData == null) return null;
            var state = _stormStateField?.GetValue(stormData);
            return state != null && Convert.ToUInt32(state) == 1;
        }

        [TelemetryAPI("kerbalism.stellarStormInProgress", "Stellar Storm In Progress",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object SolarStormInProgress(DataSources ds)
        {
            var stormData = GetStormData(ds.vessel);
            if (stormData == null) return null;
            var state = _stormStateField?.GetValue(stormData);
            return state != null && Convert.ToUInt32(state) == 2;
        }

        [TelemetryAPI("kerbalism.stellarStormState", "Stellar Storm State (0=none, 1=incoming, 2=active)",
            Category = "kerbalism", ReturnType = "int", RequiresMod = "kerbalism")]
        object SolarStormState(DataSources ds)
        {
            var stormData = GetStormData(ds.vessel);
            if (stormData == null || _stormStateField == null) return null;
            return Convert.ToInt32(_stormStateField.GetValue(stormData));
        }

        [TelemetryAPI("kerbalism.stellarStormDuration", "Stellar Storm Duration (s)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object SolarStormDuration(DataSources ds)
        {
            var stormData = GetStormData(ds.vessel);
            if (stormData == null || _stormDurationField == null) return null;
            return _stormDurationField.GetValue(stormData);
        }

        [TelemetryAPI("kerbalism.stellarStormStartTime", "Stellar Storm Start Time (UT)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object SolarStormStartTime(DataSources ds)
        {
            var stormData = GetStormData(ds.vessel);
            if (stormData == null || _stormTimeField == null) return null;
            return _stormTimeField.GetValue(stormData);
        }

        static object GetStormData(Vessel v)
        {
            if (_dbStormMethod == null || v?.mainBody == null) return null;
            try { return _dbStormMethod.Invoke(null, new object[] { v.mainBody.name }); }
            catch { return null; }
        }

        // =====================================================================
        // Deeper VesselData — environment, reliability, drives
        // =====================================================================

        [TelemetryAPI("kerbalism.envTemperature", "Environment Temperature (K)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object EnvTemperature(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdEnvTemperature?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.envTempDiff", "Temp Difference from Survival",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object EnvTempDiff(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdEnvTempDiff?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.envStormRadiation", "Storm Radiation Dose",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object EnvStormRadiation(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdEnvStormRadiation?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.breathable", "Atmosphere Breathable",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object Breathable(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdEnvBreathable?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.inAtmosphere", "Inside Atmosphere",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object InAtmosphere(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdEnvInAtmosphere?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.malfunction", "Part Malfunction Active",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object Malfunction(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdMalfunction?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.critical", "Critical Failure Active",
            Category = "kerbalism", ReturnType = "bool", RequiresMod = "kerbalism")]
        object Critical(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdCritical?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.solarExposure", "Solar Panel Average Exposure (0-1)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object SolarExposure(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdSolarPanelExposure?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.drivesFreeSpace", "Drive Free Space (MB)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object DrivesFreeSpace(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdDrivesFreeSpace?.GetValue(vd) : null;
        }

        [TelemetryAPI("kerbalism.drivesCapacity", "Drive Total Capacity (MB)",
            Category = "kerbalism", ReturnType = "double", RequiresMod = "kerbalism")]
        object DrivesCapacity(DataSources ds)
        {
            var vd = GetVesselData(ds.vessel);
            return vd != null ? _vdDrivesCapacity?.GetValue(vd) : null;
        }

        // =====================================================================
        // Crew Health — per-kerbal radiation dose and stress
        // =====================================================================

        [TelemetryAPI("kerbalism.crew", "Crew Health Summary",
            Plotable = false, Category = "kerbalism", ReturnType = "object", RequiresMod = "kerbalism")]
        object CrewHealth(DataSources ds)
        {
            if (_dbKerbalMethod == null || _kerbalRulesField == null || _ruleProblemField == null)
                return null;

            var crew = ds.vessel?.GetVesselCrew();
            if (crew == null) return null;

            var result = new List<Dictionary<string, object>>();
            foreach (var kerbal in crew)
            {
                var info = new Dictionary<string, object>();
                info["name"] = kerbal.name;
                info["trait"] = kerbal.experienceTrait?.Title;
                info["level"] = kerbal.experienceLevel;

                try
                {
                    var kd = _dbKerbalMethod.Invoke(null, new object[] { kerbal.name });
                    if (kd != null)
                    {
                        var rules = _kerbalRulesField.GetValue(kd) as IDictionary;
                        if (rules != null)
                        {
                            foreach (DictionaryEntry entry in rules)
                            {
                                var ruleName = entry.Key as string;
                                var ruleData = entry.Value;
                                if (ruleName != null && ruleData != null)
                                    info[ruleName] = _ruleProblemField.GetValue(ruleData);
                            }
                        }
                    }
                }
                catch { }

                result.Add(info);
            }
            return result;
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }
}
