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

        [TelemetryAPI("s.sensor", "Sensor Information [string sensor type]", Formatter = "SensorModuleList")]
        object Sensor(DataSources ds) => GetSensorValues(ds);

        [TelemetryAPI("s.sensor.temp", "Temperature sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.TEMP)]
        object SensorTemp(DataSources ds) { ds.args.Add("TEMP"); return GetSensorValues(ds); }

        [TelemetryAPI("s.sensor.pres", "Pressure sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.PRES)]
        object SensorPres(DataSources ds) { ds.args.Add("PRES"); return GetSensorValues(ds); }

        [TelemetryAPI("s.sensor.grav", "Gravity sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.GRAV)]
        object SensorGrav(DataSources ds) { ds.args.Add("GRAV"); return GetSensorValues(ds); }

        [TelemetryAPI("s.sensor.acc", "Acceleration sensor information",
            Formatter = "SensorModuleList", Units = APIEntry.UnitType.ACC)]
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

        [TelemetryAPI("r.resource", "Resource Information [string resource type]",
            Plotable = false, Formatter = "ResourceList")]
        object Resource(DataSources ds) => GetResourceValues(ds);

        [TelemetryAPI("r.resourceCurrent", "Resource Information for Current Stage [string resource type]",
            Plotable = false, Formatter = "ActiveResourceList")]
        object ResourceCurrent(DataSources ds) => GetActiveResourceValues(ds);

        [TelemetryAPI("r.resourceCurrentMax", "Max Resource Information for Current Stage [string resource type]",
            Plotable = false, Formatter = "MaxCurrentResourceList")]
        object ResourceCurrentMax(DataSources ds) => GetActiveResourceValues(ds);

        [TelemetryAPI("r.resourceMax", "Max Resource Information [string resource type]",
            Plotable = false, Formatter = "MaxResourceList")]
        object ResourceMax(DataSources ds) => GetResourceValues(ds);

        [TelemetryAPI("r.resourceNameList", "List of resource names",
            Plotable = false, Formatter = "StringArray")]
        object ResourceNameList(DataSources ds)
        {
            List<String> names = new List<String>();
            PartResourceDefinitionList resourceDefinitionList = PartResourceLibrary.Instance.resourceDefinitions;
            foreach (PartResourceDefinition resourceDefinition in resourceDefinitionList)
                names.Add(resourceDefinition.name);
            return names;
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
