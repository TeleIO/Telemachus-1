using System;
using System.Collections.Generic;

namespace Telemachus
{
    public class SensorDataLinkHandler : DataLinkHandler
    {
        SensorCache sensorCache = null;

        public SensorDataLinkHandler(VesselChangeDetector vesselChangeDetector, FormatterProvider formatters)
            : base(formatters)
        {
            sensorCache = new SensorCache(vesselChangeDetector);
        }

        [TelemetryAPI("s.sensor", "Sensor Information", Formatter = "SensorModuleList", Category = "sensor", ReturnType = "object", Params = "string sensorType")]
        object Sensor(DataSources ds) => GetSensorValues(ds);

        [TelemetryAPI("s.sensor.temp", "Temperature sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.TEMP, Category = "sensor", ReturnType = "object")]
        object SensorTemp(DataSources ds) { ds.args.Add("TEMP"); return GetSensorValues(ds); }

        [TelemetryAPI("s.sensor.pres", "Pressure sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.PRES, Category = "sensor", ReturnType = "object")]
        object SensorPres(DataSources ds) { ds.args.Add("PRES"); return GetSensorValues(ds); }

        [TelemetryAPI("s.sensor.grav", "Gravity sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.GRAV, Category = "sensor", ReturnType = "object")]
        object SensorGrav(DataSources ds) { ds.args.Add("GRAV"); return GetSensorValues(ds); }

        [TelemetryAPI("s.sensor.acc", "Acceleration sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.ACC, Category = "sensor", ReturnType = "object")]
        object SensorAcc(DataSources ds) { ds.args.Add("ACC"); return GetSensorValues(ds); }

        private List<ModuleEnviroSensor> GetSensorValues(DataSources datasources)
        {
            sensorCache.vessel = datasources.vessel;
            return sensorCache.get(datasources);
        }
    }

    public class ResourceDataLinkHandler : DataLinkHandler
    {
        ResourceCache resourceCache = null;
        ActiveResourceCache activeResourceCache = null;

        public ResourceDataLinkHandler(VesselChangeDetector vesselChangeDetector, FormatterProvider formatters)
            : base(formatters)
        {
            resourceCache = new ResourceCache(vesselChangeDetector);
            activeResourceCache = new ActiveResourceCache(vesselChangeDetector);
        }

        [TelemetryAPI("r.resource", "Resource Information",
            Plotable = false, Formatter = "ResourceList", Category = "resource", ReturnType = "object", Params = "string resourceName")]
        object Resource(DataSources ds) => GetResourceValues(ds);

        [TelemetryAPI("r.resourceCurrent", "Resource Information for Current Stage",
            Plotable = false, Formatter = "ActiveResourceList", Category = "resource", ReturnType = "object", Params = "string resourceName")]
        object ResourceCurrent(DataSources ds) => GetActiveResourceValues(ds);

        [TelemetryAPI("r.resourceCurrentMax", "Max Resource Information for Current Stage",
            Plotable = false, Formatter = "MaxCurrentResourceList", Category = "resource", ReturnType = "object", Params = "string resourceName")]
        object ResourceCurrentMax(DataSources ds) => GetActiveResourceValues(ds);

        [TelemetryAPI("r.resourceMax", "Max Resource Information",
            Plotable = false, Formatter = "MaxResourceList", Category = "resource", ReturnType = "object", Params = "string resourceName")]
        object ResourceMax(DataSources ds) => GetResourceValues(ds);

        [TelemetryAPI("r.resourceNameList", "List of resource names",
            Plotable = false, Formatter = "StringArray", Category = "resource", ReturnType = "string[]")]
        object ResourceNameList(DataSources ds)
        {
            List<String> names = new List<String>();
            PartResourceDefinitionList resourceDefinitionList = PartResourceLibrary.Instance.resourceDefinitions;
            foreach (PartResourceDefinition resourceDefinition in resourceDefinitionList)
                names.Add(resourceDefinition.name);
            return names;
        }

        [TelemetryAPI("r.resourceFor",
            "Live resources for a single part keyed by flightID. " +
            "Returns { resourceName: { amount, maxAmount, flow?, nominalFlow? } }. " +
            "amount / maxAmount cover storage; flow is signed units/sec " +
            "(positive = producing, negative = consuming) summed across the " +
            "part's modules; nominalFlow is the 100%-efficiency cap. " +
            "Both are omitted when the part contributes none, when no module " +
            "supports a nominal (e.g. engines), or when nominal equals flow. " +
            "Rows are emitted for resources the part contributes flow to even " +
            "when storage is zero (RTGs, solar panels). Empty object when the " +
            "flightID isn't found.",
            Plotable = false,
            Category = "resource",
            ReturnType = "object",
            Params = "uint flightId")]
        object ResourceFor(DataSources ds)
        {
            if (ds.args == null || ds.args.Count == 0)
                return new Dictionary<string, object>();
            if (!uint.TryParse(ds.args[0], out var flightId))
                return new Dictionary<string, object>();
            if (ds.vessel == null || ds.vessel.parts == null)
                return new Dictionary<string, object>();

            foreach (var part in ds.vessel.parts)
            {
                if (part == null || part.flightID != flightId) continue;
                var rows = new Dictionary<string, FlowRow>();
                // Storage first — seeds amount / maxAmount for every stored resource.
                if (part.Resources != null)
                {
                    foreach (var res in part.Resources)
                    {
                        if (res == null || string.IsNullOrEmpty(res.resourceName)) continue;
                        var row = Ensure(rows, res.resourceName);
                        row.amount = res.amount;
                        row.maxAmount = res.maxAmount;
                    }
                }
                // Module flow contributions. Per-module try/catch so one bad
                // module type / cast doesn't crater the whole payload.
                if (part.Modules != null)
                {
                    foreach (var module in part.Modules)
                    {
                        if (module == null) continue;
                        try { AddModuleFlow(module, rows); }
                        catch { /* skip silently */ }
                    }
                }
                return Serialize(rows);
            }
            return new Dictionary<string, object>();
        }

        /// Internal accumulator for one resource's row on a part. Stored in a
        /// Dictionary keyed by resource name; serialised at the end.
        private class FlowRow
        {
            public double amount;
            public double maxAmount;
            public double? flow;
            public double? nominalFlow;
            /// Goes true when any contributing module didn't supply a nominal
            /// (e.g. engines, where "nominal at full throttle" needs more
            /// integration than this v1 does). Suppresses nominalFlow in the
            /// output so the client doesn't compare a partial nominal against
            /// a full flow total.
            public bool nominalIncomplete;
        }

        private static FlowRow Ensure(Dictionary<string, FlowRow> rows, string name)
        {
            if (!rows.TryGetValue(name, out var row))
            {
                row = new FlowRow();
                rows[name] = row;
            }
            return row;
        }

        private static void AddFlow(
            Dictionary<string, FlowRow> rows,
            string name,
            double current,
            double? nominal)
        {
            if (string.IsNullOrEmpty(name)) return;
            var row = Ensure(rows, name);
            row.flow = (row.flow ?? 0.0) + current;
            if (nominal.HasValue)
                row.nominalFlow = (row.nominalFlow ?? 0.0) + nominal.Value;
            else
                row.nominalIncomplete = true;
        }

        private static void AddModuleFlow(PartModule module, Dictionary<string, FlowRow> rows)
        {
            switch (module)
            {
                case ModuleDeployableSolarPanel solar:
                    // chargeRate × efficiencyMult is the at-full-deployment cap
                    // (sunlit, no shading). flowRate is the live value.
                    AddFlow(
                        rows,
                        solar.resourceName ?? "ElectricCharge",
                        solar.flowRate,
                        solar.chargeRate * solar.efficiencyMult);
                    break;

                case ModuleGenerator gen:
                    if (!gen.generatorIsActive) break;
                    if (gen.resHandler != null && gen.resHandler.outputResources != null)
                    {
                        foreach (var output in gen.resHandler.outputResources)
                        {
                            if (output == null) continue;
                            AddFlow(rows, output.name, output.rate * gen.efficiency, output.rate);
                        }
                    }
                    if (gen.resHandler != null && gen.resHandler.inputResources != null)
                    {
                        foreach (var input in gen.resHandler.inputResources)
                        {
                            if (input == null) continue;
                            AddFlow(rows, input.name, -input.rate * gen.efficiency, -input.rate);
                        }
                    }
                    break;

                case ModuleResourceConverter conv:
                    // ModuleResourceHarvester extends ModuleResourceConverter, so
                    // a single case covers ISRU + drills + fuel cells. lastTimeFactor
                    // is the current rate factor (0..1+); outputList / inputList
                    // are full-throttle ratios.
                    if (!conv.IsActivated) break;
                    if (conv.outputList != null)
                    {
                        // ResourceRatio is a value type — iterate by value, no
                        // null check needed.
                        foreach (var output in conv.outputList)
                        {
                            AddFlow(
                                rows,
                                output.ResourceName,
                                output.Ratio * conv.lastTimeFactor,
                                output.Ratio);
                        }
                    }
                    if (conv.inputList != null)
                    {
                        foreach (var input in conv.inputList)
                        {
                            AddFlow(
                                rows,
                                input.ResourceName,
                                -input.Ratio * conv.lastTimeFactor,
                                -input.Ratio);
                        }
                    }
                    break;

                case ModuleEngines engine:
                    // ModuleEnginesFX inherits ModuleEngines so this case
                    // catches both. Per-propellant consumption in units/sec at
                    // current throttle. No nominal — at full throttle it varies
                    // by propellant ratio + density and isn't a one-liner; v1
                    // marks the row's nominal as incomplete so the client
                    // doesn't compare a partial nominal against a full flow
                    // total.
                    if (!engine.EngineIgnited || engine.flameout) break;
                    if (engine.propellants != null)
                    {
                        // Propellant.currentRequirement is units-per-physics-frame
                        // (set by KSP each FixedUpdate). Divide by the physics
                        // fixedDeltaTime to convert to units/sec, matching the
                        // unit convention used by every other dispatch case.
                        float dt = TimeWarp.fixedDeltaTime;
                        if (dt <= 0f) break;
                        foreach (var prop in engine.propellants)
                        {
                            if (prop == null || string.IsNullOrEmpty(prop.name)) continue;
                            AddFlow(rows, prop.name, -prop.currentRequirement / dt, null);
                        }
                    }
                    break;
            }
        }

        private static object Serialize(Dictionary<string, FlowRow> rows)
        {
            var output = new Dictionary<string, object>();
            foreach (var kv in rows)
            {
                var dict = new Dictionary<string, object>
                {
                    ["amount"] = kv.Value.amount,
                    ["maxAmount"] = kv.Value.maxAmount,
                };
                if (kv.Value.flow.HasValue)
                {
                    dict["flow"] = kv.Value.flow.Value;
                    if (kv.Value.nominalFlow.HasValue &&
                        !kv.Value.nominalIncomplete &&
                        Math.Abs(kv.Value.nominalFlow.Value - kv.Value.flow.Value) > 1e-9)
                    {
                        dict["nominalFlow"] = kv.Value.nominalFlow.Value;
                    }
                }
                output[kv.Key] = dict;
            }
            return output;
        }

        private List<PartResource> GetResourceValues(DataSources datasources)
        {
            resourceCache.vessel = datasources.vessel;
            return resourceCache.get(datasources);
        }

        private List<SimplifiedResource> GetActiveResourceValues(DataSources datasources)
        {
            activeResourceCache.vessel = datasources.vessel;
            return activeResourceCache.get(datasources);
        }
    }
}
