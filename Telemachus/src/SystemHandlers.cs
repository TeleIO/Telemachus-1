using System;
using System.Collections.Generic;

namespace Telemachus
{
    public class PausedDataLinkHandler : DataLinkHandler
    {
        public PausedDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("p.paused",
            "Returns an integer indicating the state of antenna. 0 - Flight scene; 1 - Paused; 2 - No power; 3 - Off; 4 - Not found.",
            AlwaysEvaluable = true)]
        object Paused(DataSources ds) => partPaused();

        public static int partPaused()
        {
            if (!HighLogic.LoadedSceneIsFlight) return 5;
            if (FlightDriver.Pause) return 1;
            if (!TelemachusPowerDrain.IsAnyActive) return 2;
            if (!TelemachusPowerDrain.IsAnyToggled) return 3;
            if (!VesselChangeDetector.hasTelemachusPart) return 4;
            return 0;
        }
    }

    public class APIDataLinkHandler : DataLinkHandler
    {
        public APIDataLinkHandler(KSPAPIBase kspAPI, FormatterProvider formatters,
            ServerConfiguration serverConfiguration)
            : base(formatters)
        {
            registerAPI(new APIEntry(
                dataSources =>
                {
                    var APIList = new List<APIEntry>();
                    kspAPI.getAPIList(ref APIList);
                    return APIList;
                },
                "a.api", "API Listing", formatters.APIEntry, APIEntry.UnitType.UNITLESS, true));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    var IPList = new List<String>();
                    foreach (System.Net.IPAddress a in serverConfiguration.ValidIpAddresses)
                        IPList.Add(a.ToString());
                    return IPList;
                },
                "a.ip", "IP Addresses", formatters.StringArray, APIEntry.UnitType.UNITLESS, true));

            registerAPI(new APIEntry(
                dataSources =>
                {
                    var APIList = new List<APIEntry>();
                    foreach (string apiRequest in dataSources.args)
                        kspAPI.getAPIEntry(apiRequest, ref APIList);
                    return APIList;
                },
                "a.apiSubSet",
                "Subset of the API Listing [string api1, string api2, ... , string apiN]",
                formatters.APIEntry, APIEntry.UnitType.STRING, true));
        }

        [TelemetryAPI("a.version", "Telemachus Version", AlwaysEvaluable = true)]
        object Version(DataSources ds) =>
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public class CompoundDataLinkHandler : DataLinkHandler
    {
        public CompoundDataLinkHandler(List<DataLinkHandler> APIHandlers, FormatterProvider formatters)
            : base(formatters)
        {
            foreach (DataLinkHandler dlh in APIHandlers)
            {
                foreach (KeyValuePair<string, APIEntry> entry in dlh.API)
                    registerAPI(entry.Value);
            }
        }
    }
}
