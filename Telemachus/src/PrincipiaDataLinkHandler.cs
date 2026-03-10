using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Telemachus
{
    /// <summary>
    /// Exposes orbital analysis data from Principia (N-body physics mod).
    /// When Principia is installed, stock KSP orbit data (o.*) represents
    /// osculating elements — instantaneous Keplerian snapshots — rather than
    /// predictive trajectories. This handler provides Principia's mean orbital
    /// elements with min/max ranges computed by n-body integration.
    ///
    /// All access is via reflection — soft dependency. If Principia is absent,
    /// principia.available returns false and all other endpoints return null.
    ///
    /// Reflection chain:
    ///   PrincipiaPluginAdapter (ScenarioModule)
    ///     → orbit_analyser_ (OrbitAnalyser)
    ///       → GetAnalysis() → OrbitAnalysis
    ///         → elements (Nullable&lt;OrbitalElements&gt;)
    ///         → recurrence (Nullable&lt;OrbitRecurrence&gt;)
    ///     → flight_planner_ (FlightPlanner)
    ///       → burn_editors_ (List&lt;BurnEditor&gt;)
    ///     → PluginRunning() → bool
    /// </summary>
    public class PrincipiaDataLinkHandler : DataLinkHandler
    {
        static bool _searched;
        static Type _adapterType;       // PrincipiaPluginAdapter
        static Type _analyserType;      // OrbitAnalyser
        static Type _analysisType;      // OrbitAnalysis
        static Type _elementsType;      // OrbitalElements
        static Type _intervalType;      // Interval
        static Type _recurrenceType;    // OrbitRecurrence
        static Type _flightPlannerType; // FlightPlanner
        static Type _burnEditorType;    // BurnEditor

        // PrincipiaPluginAdapter members
        static MethodInfo _pluginRunning;
        static FieldInfo _orbitAnalyserField;
        static FieldInfo _flightPlannerField;

        // OrbitAnalyser members
        static MethodInfo _getAnalysis;

        // OrbitAnalysis fields
        static FieldInfo _analysisElements;
        static FieldInfo _analysisRecurrence;
        static FieldInfo _analysisProgress;
        static FieldInfo _analysisMissionDuration;

        // OrbitalElements fields
        static FieldInfo _siderealPeriod;
        static FieldInfo _nodalPeriod;
        static FieldInfo _anomalisticPeriod;
        static FieldInfo _nodalPrecession;
        static FieldInfo _meanSma;
        static FieldInfo _meanEcc;
        static FieldInfo _meanInc;
        static FieldInfo _meanLan;
        static FieldInfo _meanAop;
        static FieldInfo _meanPeD;  // periapsis distance
        static FieldInfo _meanApD;  // apoapsis distance
        static FieldInfo _radialDistance;

        // Interval fields
        static FieldInfo _intervalMin;
        static FieldInfo _intervalMax;

        // OrbitRecurrence fields
        static FieldInfo _recNuo;
        static FieldInfo _recDto;
        static FieldInfo _recCto;
        static FieldInfo _recNumRevolutions;
        static FieldInfo _recEquatorialShift;
        static FieldInfo _recBaseInterval;
        static FieldInfo _recGridInterval;
        static FieldInfo _recSubcycle;

        // FlightPlanner fields
        static FieldInfo _burnEditors;
        static FieldInfo _showGuidance;

        // BurnEditor members
        static FieldInfo _burnDvTangent;    // DifferentialSlider
        static FieldInfo _burnDvNormal;
        static FieldInfo _burnDvBinormal;
        static FieldInfo _burnInitialTime;
        static FieldInfo _burnDuration;

        // DifferentialSlider value access
        static FieldInfo _sliderValue;  // Nullable<double> value_

        public PrincipiaDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        static void Search()
        {
            if (_searched) return;
            _searched = true;

            foreach (var asm in AssemblyLoader.loadedAssemblies)
            {
                try
                {
                    var a = asm.assembly;
                    if (_adapterType == null)
                        _adapterType = a.GetType("principia.ksp_plugin_adapter.PrincipiaPluginAdapter", false);
                    if (_analyserType == null)
                        _analyserType = a.GetType("principia.ksp_plugin_adapter.OrbitAnalyser", false);
                    if (_analysisType == null)
                        _analysisType = a.GetType("principia.ksp_plugin_adapter.OrbitAnalysis", false);
                    if (_elementsType == null)
                        _elementsType = a.GetType("principia.ksp_plugin_adapter.OrbitalElements", false);
                    if (_intervalType == null)
                        _intervalType = a.GetType("principia.ksp_plugin_adapter.Interval", false);
                    if (_recurrenceType == null)
                        _recurrenceType = a.GetType("principia.ksp_plugin_adapter.OrbitRecurrence", false);
                    if (_flightPlannerType == null)
                        _flightPlannerType = a.GetType("principia.ksp_plugin_adapter.FlightPlanner", false);
                    if (_burnEditorType == null)
                        _burnEditorType = a.GetType("principia.ksp_plugin_adapter.BurnEditor", false);
                }
                catch { }
            }

            if (_adapterType == null)
            {
                PluginLogger.debug("Principia not found");
                return;
            }

            PluginLogger.debug("Principia detected: " + _adapterType.Assembly.GetName().Version);

            var pub = BindingFlags.Public | BindingFlags.Instance;
            var priv = BindingFlags.NonPublic | BindingFlags.Instance;

            // PrincipiaPluginAdapter
            _pluginRunning = _adapterType.GetMethod("PluginRunning", pub);
            _orbitAnalyserField = _adapterType.GetField("orbit_analyser_", priv);
            _flightPlannerField = _adapterType.GetField("flight_planner_", priv);

            // OrbitAnalyser
            if (_analyserType != null)
                _getAnalysis = _analyserType.GetMethod("GetAnalysis", pub);

            // OrbitAnalysis
            if (_analysisType != null)
            {
                _analysisElements = _analysisType.GetField("elements", pub);
                _analysisRecurrence = _analysisType.GetField("recurrence", pub);
                _analysisProgress = _analysisType.GetField("progress_of_next_analysis", pub);
                _analysisMissionDuration = _analysisType.GetField("mission_duration", pub);
            }

            // OrbitalElements
            if (_elementsType != null)
            {
                _siderealPeriod = _elementsType.GetField("sidereal_period", pub);
                _nodalPeriod = _elementsType.GetField("nodal_period", pub);
                _anomalisticPeriod = _elementsType.GetField("anomalistic_period", pub);
                _nodalPrecession = _elementsType.GetField("nodal_precession", pub);
                _meanSma = _elementsType.GetField("mean_semimajor_axis", pub);
                _meanEcc = _elementsType.GetField("mean_eccentricity", pub);
                _meanInc = _elementsType.GetField("mean_inclination", pub);
                _meanLan = _elementsType.GetField("mean_longitude_of_ascending_nodes", pub);
                _meanAop = _elementsType.GetField("mean_argument_of_periapsis", pub);
                _meanPeD = _elementsType.GetField("mean_periapsis_distance", pub);
                _meanApD = _elementsType.GetField("mean_apoapsis_distance", pub);
                _radialDistance = _elementsType.GetField("radial_distance", pub);
            }

            // Interval
            if (_intervalType != null)
            {
                _intervalMin = _intervalType.GetField("min", pub);
                _intervalMax = _intervalType.GetField("max", pub);
            }

            // OrbitRecurrence
            if (_recurrenceType != null)
            {
                _recNuo = _recurrenceType.GetField("nuo", pub);
                _recDto = _recurrenceType.GetField("dto", pub);
                _recCto = _recurrenceType.GetField("cto", pub);
                _recNumRevolutions = _recurrenceType.GetField("number_of_revolutions", pub);
                _recEquatorialShift = _recurrenceType.GetField("equatorial_shift", pub);
                _recBaseInterval = _recurrenceType.GetField("base_interval", pub);
                _recGridInterval = _recurrenceType.GetField("grid_interval", pub);
                _recSubcycle = _recurrenceType.GetField("subcycle", pub);
            }

            // FlightPlanner
            if (_flightPlannerType != null)
            {
                _burnEditors = _flightPlannerType.GetField("burn_editors_", priv);
                _showGuidance = _flightPlannerType.GetField("show_guidance_", priv);
            }

            // BurnEditor
            if (_burnEditorType != null)
            {
                _burnDvTangent = _burnEditorType.GetField("Δv_tangent_", priv);
                _burnDvNormal = _burnEditorType.GetField("Δv_normal_", priv);
                _burnDvBinormal = _burnEditorType.GetField("Δv_binormal_", priv);
                _burnInitialTime = _burnEditorType.GetField("initial_time_", priv);
                _burnDuration = _burnEditorType.GetField("duration_", priv);
            }

            // DifferentialSlider — find value_ field
            var sliderType = _burnDvTangent?.FieldType;
            if (sliderType != null)
                _sliderValue = sliderType.GetField("value_", priv);
        }

        /// <summary>
        /// Find the PrincipiaPluginAdapter ScenarioModule instance.
        /// Uses Unity's FindObjectOfType since ScenarioModule is a MonoBehaviour.
        /// </summary>
        static object FindAdapter()
        {
            Search();
            if (_adapterType == null) return null;
            return UnityEngine.Object.FindObjectOfType(_adapterType);
        }

        static bool IsPluginRunning()
        {
            var adapter = FindAdapter();
            if (adapter == null || _pluginRunning == null) return false;
            try { return (bool)_pluginRunning.Invoke(adapter, null); }
            catch { return false; }
        }

        /// <summary>
        /// Public accessor for other handlers (e.g. a.physicsMode) to check
        /// whether Principia is installed and actively running.
        /// </summary>
        public static bool IsPrincipiaActive()
        {
            Search();
            if (_adapterType == null) return false;
            return IsPluginRunning();
        }

        static object GetOrbitAnalyser()
        {
            var adapter = FindAdapter();
            if (adapter == null || _orbitAnalyserField == null) return null;
            return _orbitAnalyserField.GetValue(adapter);
        }

        /// <summary>
        /// Call GetAnalysis() on the OrbitAnalyser. Returns the OrbitAnalysis
        /// object, which may be null if analysis hasn't been computed yet.
        /// </summary>
        static object GetAnalysis()
        {
            var analyser = GetOrbitAnalyser();
            if (analyser == null || _getAnalysis == null) return null;
            try { return _getAnalysis.Invoke(analyser, null); }
            catch { return null; }
        }

        /// <summary>
        /// Extract the OrbitalElements from the analysis. Returns null if
        /// analysis is unavailable or elements haven't been computed.
        /// The elements field is Nullable&lt;OrbitalElements&gt;, so we must
        /// unbox through the Nullable wrapper.
        /// </summary>
        static object GetElements()
        {
            var analysis = GetAnalysis();
            if (analysis == null || _analysisElements == null) return null;
            var nullable = _analysisElements.GetValue(analysis);
            // Nullable<T> boxes to either null or the T value
            return nullable;
        }

        static object GetRecurrence()
        {
            var analysis = GetAnalysis();
            if (analysis == null || _analysisRecurrence == null) return null;
            return _analysisRecurrence.GetValue(analysis);
        }

        /// <summary>
        /// Read the midpoint of an Interval field from a boxed OrbitalElements struct.
        /// </summary>
        static object ReadIntervalMid(object elements, FieldInfo intervalField)
        {
            if (elements == null || intervalField == null || _intervalMin == null || _intervalMax == null)
                return null;
            var interval = intervalField.GetValue(elements);
            if (interval == null) return null;
            double min = (double)_intervalMin.GetValue(interval);
            double max = (double)_intervalMax.GetValue(interval);
            return (min + max) / 2.0;
        }

        /// <summary>
        /// Read an Interval field as a {min, max} dictionary.
        /// </summary>
        static object ReadIntervalRange(object elements, FieldInfo intervalField)
        {
            if (elements == null || intervalField == null || _intervalMin == null || _intervalMax == null)
                return null;
            var interval = intervalField.GetValue(elements);
            if (interval == null) return null;
            return new Dictionary<string, double>
            {
                ["min"] = (double)_intervalMin.GetValue(interval),
                ["max"] = (double)_intervalMax.GetValue(interval)
            };
        }

        /// <summary>Read a double field from a boxed struct.</summary>
        static object ReadDouble(object obj, FieldInfo field)
        {
            if (obj == null || field == null) return null;
            return field.GetValue(obj);
        }

        /// <summary>Read the value from a DifferentialSlider (Nullable&lt;double&gt;).</summary>
        static double? ReadSliderValue(object slider)
        {
            if (slider == null || _sliderValue == null) return null;
            var val = _sliderValue.GetValue(slider);
            if (val == null) return null;
            return (double)val;
        }

        static object GetFlightPlanner()
        {
            var adapter = FindAdapter();
            if (adapter == null || _flightPlannerField == null) return null;
            return _flightPlannerField.GetValue(adapter);
        }

        static IList GetBurnEditors()
        {
            var planner = GetFlightPlanner();
            if (planner == null || _burnEditors == null) return null;
            return _burnEditors.GetValue(planner) as IList;
        }

        // =====================================================================
        // Telemetry API — Detection & Status
        // =====================================================================

        [TelemetryAPI("principia.available", "Principia Is Installed",
            AlwaysEvaluable = true, Category = "principia", ReturnType = "bool",
            RequiresMod = "principia")]
        object Available(DataSources ds)
        {
            Search();
            return _adapterType != null;
        }

        [TelemetryAPI("principia.version", "Principia Assembly Version",
            Units = APIEntry.UnitType.STRING, AlwaysEvaluable = true,
            Category = "principia", ReturnType = "string",
            RequiresMod = "principia")]
        object Version(DataSources ds)
        {
            Search();
            return _adapterType?.Assembly.GetName().Version.ToString();
        }

        [TelemetryAPI("principia.active", "Principia Plugin Running",
            Category = "principia", ReturnType = "bool",
            RequiresMod = "principia")]
        object Active(DataSources ds) => IsPluginRunning();

        [TelemetryAPI("principia.analysisProgress", "Orbit Analysis Progress (0-1)",
            Category = "principia", ReturnType = "double",
            RequiresMod = "principia")]
        object AnalysisProgress(DataSources ds)
        {
            var analysis = GetAnalysis();
            if (analysis == null || _analysisProgress == null) return null;
            return _analysisProgress.GetValue(analysis);
        }

        [TelemetryAPI("principia.missionDuration", "Analysis Mission Duration (s)",
            Units = APIEntry.UnitType.TIME, Category = "principia",
            ReturnType = "double", RequiresMod = "principia")]
        object MissionDuration(DataSources ds)
        {
            var analysis = GetAnalysis();
            if (analysis == null || _analysisMissionDuration == null) return null;
            return _analysisMissionDuration.GetValue(analysis);
        }

        // =====================================================================
        // Telemetry API — Mean Orbital Elements (midpoint values)
        // These go into the orbit category alongside stock o.* endpoints.
        // =====================================================================

        [TelemetryAPI("o.mean.sma", "Mean Semimajor Axis (Principia)",
            Units = APIEntry.UnitType.DISTANCE, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object MeanSma(DataSources ds) => ReadIntervalMid(GetElements(), _meanSma);

        [TelemetryAPI("o.mean.eccentricity", "Mean Eccentricity (Principia)",
            Category = "orbit", ReturnType = "double",
            RequiresMod = "principia")]
        object MeanEcc(DataSources ds) => ReadIntervalMid(GetElements(), _meanEcc);

        [TelemetryAPI("o.mean.inclination", "Mean Inclination (Principia)",
            Units = APIEntry.UnitType.DEG, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object MeanInc(DataSources ds)
        {
            var mid = ReadIntervalMid(GetElements(), _meanInc);
            if (mid is double rad) return rad * (180.0 / Math.PI);
            return null;
        }

        [TelemetryAPI("o.mean.lan", "Mean Longitude of Ascending Node (Principia)",
            Units = APIEntry.UnitType.DEG, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object MeanLan(DataSources ds)
        {
            var mid = ReadIntervalMid(GetElements(), _meanLan);
            if (mid is double rad) return rad * (180.0 / Math.PI);
            return null;
        }

        [TelemetryAPI("o.mean.argumentOfPeriapsis", "Mean Argument of Periapsis (Principia)",
            Units = APIEntry.UnitType.DEG, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object MeanAop(DataSources ds)
        {
            var mid = ReadIntervalMid(GetElements(), _meanAop);
            if (mid is double rad) return rad * (180.0 / Math.PI);
            return null;
        }

        [TelemetryAPI("o.mean.PeA", "Mean Periapsis Altitude (Principia)",
            Units = APIEntry.UnitType.DISTANCE, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object MeanPeA(DataSources ds)
        {
            // Principia stores periapsis _distance_ (from body center).
            // Subtract body radius to get altitude, matching stock o.PeA.
            var mid = ReadIntervalMid(GetElements(), _meanPeD);
            if (mid is double dist && ds.vessel?.mainBody != null)
                return dist - ds.vessel.mainBody.Radius;
            return mid;
        }

        [TelemetryAPI("o.mean.ApA", "Mean Apoapsis Altitude (Principia)",
            Units = APIEntry.UnitType.DISTANCE, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object MeanApA(DataSources ds)
        {
            var mid = ReadIntervalMid(GetElements(), _meanApD);
            if (mid is double dist && ds.vessel?.mainBody != null)
                return dist - ds.vessel.mainBody.Radius;
            return mid;
        }

        // =====================================================================
        // Telemetry API — Element Ranges (min/max from n-body integration)
        // =====================================================================

        [TelemetryAPI("o.mean.smaRange", "Mean SMA Range {min,max} (Principia)",
            Plotable = false, Category = "orbit", ReturnType = "object",
            RequiresMod = "principia")]
        object MeanSmaRange(DataSources ds) => ReadIntervalRange(GetElements(), _meanSma);

        [TelemetryAPI("o.mean.eccentricityRange", "Mean Eccentricity Range (Principia)",
            Plotable = false, Category = "orbit", ReturnType = "object",
            RequiresMod = "principia")]
        object MeanEccRange(DataSources ds) => ReadIntervalRange(GetElements(), _meanEcc);

        [TelemetryAPI("o.mean.inclinationRange", "Mean Inclination Range (Principia)",
            Plotable = false, Category = "orbit", ReturnType = "object",
            RequiresMod = "principia")]
        object MeanIncRange(DataSources ds)
        {
            var range = ReadIntervalRange(GetElements(), _meanInc);
            if (range is Dictionary<string, double> d)
                return new Dictionary<string, double>
                {
                    ["min"] = d["min"] * (180.0 / Math.PI),
                    ["max"] = d["max"] * (180.0 / Math.PI)
                };
            return null;
        }

        [TelemetryAPI("o.mean.PeARange", "Mean Periapsis Altitude Range (Principia)",
            Plotable = false, Category = "orbit", ReturnType = "object",
            RequiresMod = "principia")]
        object MeanPeARange(DataSources ds)
        {
            var range = ReadIntervalRange(GetElements(), _meanPeD);
            if (range is Dictionary<string, double> d && ds.vessel?.mainBody != null)
            {
                double r = ds.vessel.mainBody.Radius;
                return new Dictionary<string, double>
                {
                    ["min"] = d["min"] - r,
                    ["max"] = d["max"] - r
                };
            }
            return range;
        }

        [TelemetryAPI("o.mean.ApARange", "Mean Apoapsis Altitude Range (Principia)",
            Plotable = false, Category = "orbit", ReturnType = "object",
            RequiresMod = "principia")]
        object MeanApARange(DataSources ds)
        {
            var range = ReadIntervalRange(GetElements(), _meanApD);
            if (range is Dictionary<string, double> d && ds.vessel?.mainBody != null)
            {
                double r = ds.vessel.mainBody.Radius;
                return new Dictionary<string, double>
                {
                    ["min"] = d["min"] - r,
                    ["max"] = d["max"] - r
                };
            }
            return range;
        }

        // =====================================================================
        // Telemetry API — Periods & Precession
        // =====================================================================

        [TelemetryAPI("o.mean.siderealPeriod", "Sidereal Period (Principia)",
            Units = APIEntry.UnitType.TIME, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object SiderealPeriod(DataSources ds) => ReadDouble(GetElements(), _siderealPeriod);

        [TelemetryAPI("o.mean.nodalPeriod", "Nodal Period (Principia)",
            Units = APIEntry.UnitType.TIME, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object NodalPeriod(DataSources ds) => ReadDouble(GetElements(), _nodalPeriod);

        [TelemetryAPI("o.mean.anomalisticPeriod", "Anomalistic Period (Principia)",
            Units = APIEntry.UnitType.TIME, Category = "orbit",
            ReturnType = "double", RequiresMod = "principia")]
        object AnomalisticPeriod(DataSources ds) => ReadDouble(GetElements(), _anomalisticPeriod);

        [TelemetryAPI("o.mean.nodalPrecession", "Nodal Precession Rate (rad/s, Principia)",
            Category = "orbit", ReturnType = "double",
            RequiresMod = "principia")]
        object NodalPrecession(DataSources ds) => ReadDouble(GetElements(), _nodalPrecession);

        // =====================================================================
        // Telemetry API — Orbit Recurrence (ground track)
        // =====================================================================

        [TelemetryAPI("o.mean.recurrence", "Orbit Recurrence Info (Principia)",
            Plotable = false, Category = "orbit", ReturnType = "object",
            RequiresMod = "principia")]
        object Recurrence(DataSources ds)
        {
            var rec = GetRecurrence();
            if (rec == null) return null;

            var info = new Dictionary<string, object>();
            if (_recNuo != null) info["nuo"] = _recNuo.GetValue(rec);
            if (_recDto != null) info["dto"] = _recDto.GetValue(rec);
            if (_recCto != null) info["cto"] = _recCto.GetValue(rec);
            if (_recNumRevolutions != null) info["revolutions"] = _recNumRevolutions.GetValue(rec);
            if (_recEquatorialShift != null) info["equatorialShift"] = _recEquatorialShift.GetValue(rec);
            if (_recBaseInterval != null) info["baseInterval"] = _recBaseInterval.GetValue(rec);
            if (_recGridInterval != null) info["gridInterval"] = _recGridInterval.GetValue(rec);
            if (_recSubcycle != null) info["subcycle"] = _recSubcycle.GetValue(rec);
            return info;
        }

        // =====================================================================
        // Telemetry API — Flight Plan
        // =====================================================================

        [TelemetryAPI("principia.plan.count", "Number of Planned Burns",
            Category = "principia", ReturnType = "int",
            RequiresMod = "principia")]
        object PlanCount(DataSources ds) => GetBurnEditors()?.Count ?? 0;

        [TelemetryAPI("principia.plan.guidance", "Navball Guidance Active",
            Category = "principia", ReturnType = "bool",
            RequiresMod = "principia")]
        object PlanGuidance(DataSources ds)
        {
            var planner = GetFlightPlanner();
            if (planner == null || _showGuidance == null) return null;
            return _showGuidance.GetValue(planner);
        }

        [TelemetryAPI("principia.plan.burns", "All Planned Burns",
            Plotable = false, Category = "principia", ReturnType = "object",
            RequiresMod = "principia")]
        object PlanBurns(DataSources ds)
        {
            var editors = GetBurnEditors();
            if (editors == null) return null;

            var result = new List<Dictionary<string, object>>();
            int index = 0;

            foreach (var editor in editors)
            {
                if (editor == null) continue;
                var info = new Dictionary<string, object> { ["index"] = index++ };

                // Delta-V components from DifferentialSliders
                if (_burnDvTangent != null)
                {
                    var slider = _burnDvTangent.GetValue(editor);
                    info["tangent"] = ReadSliderValue(slider);
                }
                if (_burnDvNormal != null)
                {
                    var slider = _burnDvNormal.GetValue(editor);
                    info["normal"] = ReadSliderValue(slider);
                }
                if (_burnDvBinormal != null)
                {
                    var slider = _burnDvBinormal.GetValue(editor);
                    info["binormal"] = ReadSliderValue(slider);
                }
                if (_burnInitialTime != null)
                    info["initialTime"] = _burnInitialTime.GetValue(editor);
                if (_burnDuration != null)
                    info["duration"] = _burnDuration.GetValue(editor);

                // Compute total dV magnitude
                double? t = info.ContainsKey("tangent") ? info["tangent"] as double? : null;
                double? n = info.ContainsKey("normal") ? info["normal"] as double? : null;
                double? b = info.ContainsKey("binormal") ? info["binormal"] as double? : null;
                if (t.HasValue && n.HasValue && b.HasValue)
                    info["totalDeltaV"] = Math.Sqrt(t.Value * t.Value + n.Value * n.Value + b.Value * b.Value);

                result.Add(info);
            }
            return result;
        }

        [TelemetryAPI("principia.plan.burn", "Planned Burn by Index",
            Plotable = false, Category = "principia", ReturnType = "object",
            RequiresMod = "principia", Params = "int index")]
        object PlanBurn(DataSources ds)
        {
            var editors = GetBurnEditors();
            if (editors == null || ds.args.Count < 1) return null;

            if (!int.TryParse(ds.args[0], out int index) || index < 0 || index >= editors.Count)
                return null;

            var editor = editors[index];
            if (editor == null) return null;

            var info = new Dictionary<string, object> { ["index"] = index };

            if (_burnDvTangent != null)
                info["tangent"] = ReadSliderValue(_burnDvTangent.GetValue(editor));
            if (_burnDvNormal != null)
                info["normal"] = ReadSliderValue(_burnDvNormal.GetValue(editor));
            if (_burnDvBinormal != null)
                info["binormal"] = ReadSliderValue(_burnDvBinormal.GetValue(editor));
            if (_burnInitialTime != null)
                info["initialTime"] = _burnInitialTime.GetValue(editor);
            if (_burnDuration != null)
                info["duration"] = _burnDuration.GetValue(editor);

            double? t = info.ContainsKey("tangent") ? info["tangent"] as double? : null;
            double? n = info.ContainsKey("normal") ? info["normal"] as double? : null;
            double? b = info.ContainsKey("binormal") ? info["binormal"] as double? : null;
            if (t.HasValue && n.HasValue && b.HasValue)
                info["totalDeltaV"] = Math.Sqrt(t.Value * t.Value + n.Value * n.Value + b.Value * b.Value);

            return info;
        }

        // =====================================================================
        // Telemetry API — Full analysis dump
        // =====================================================================

        [TelemetryAPI("principia.analysis", "Complete Orbit Analysis (Principia)",
            Plotable = false, Category = "principia", ReturnType = "object",
            RequiresMod = "principia")]
        object FullAnalysis(DataSources ds)
        {
            var elements = GetElements();
            if (elements == null) return null;

            double bodyRadius = ds.vessel?.mainBody?.Radius ?? 0;
            var result = new Dictionary<string, object>();

            // Periods
            result["siderealPeriod"] = ReadDouble(elements, _siderealPeriod);
            result["nodalPeriod"] = ReadDouble(elements, _nodalPeriod);
            result["anomalisticPeriod"] = ReadDouble(elements, _anomalisticPeriod);
            result["nodalPrecession"] = ReadDouble(elements, _nodalPrecession);

            // Mean elements (midpoints)
            result["sma"] = ReadIntervalMid(elements, _meanSma);
            result["eccentricity"] = ReadIntervalMid(elements, _meanEcc);

            var inc = ReadIntervalMid(elements, _meanInc);
            result["inclination"] = inc is double incRad ? incRad * (180.0 / Math.PI) : inc;

            var lan = ReadIntervalMid(elements, _meanLan);
            result["lan"] = lan is double lanRad ? lanRad * (180.0 / Math.PI) : lan;

            var aop = ReadIntervalMid(elements, _meanAop);
            result["argumentOfPeriapsis"] = aop is double aopRad ? aopRad * (180.0 / Math.PI) : aop;

            var peD = ReadIntervalMid(elements, _meanPeD);
            result["periapsisAltitude"] = peD is double peDist ? peDist - bodyRadius : peD;

            var apD = ReadIntervalMid(elements, _meanApD);
            result["apoapsisAltitude"] = apD is double apDist ? apDist - bodyRadius : apD;

            // Ranges
            result["smaRange"] = ReadIntervalRange(elements, _meanSma);
            result["eccentricityRange"] = ReadIntervalRange(elements, _meanEcc);

            // Analysis metadata
            var analysis = GetAnalysis();
            if (analysis != null)
            {
                if (_analysisProgress != null)
                    result["progress"] = _analysisProgress.GetValue(analysis);
                if (_analysisMissionDuration != null)
                    result["missionDuration"] = _analysisMissionDuration.GetValue(analysis);
            }

            return result;
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }
}
