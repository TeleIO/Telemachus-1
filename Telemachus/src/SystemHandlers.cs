using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Telemachus
{
    public class PausedDataLinkHandler : DataLinkHandler
    {
        public PausedDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("p.paused",
            "Returns an integer indicating the state of antenna. 0 - Flight scene; 1 - Paused; 2 - No power; 3 - Off; 4 - Not found.",
            AlwaysEvaluable = true, Category = "system", ReturnType = "int")]
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

        [TelemetryAPI("a.version", "Telemachus Version", AlwaysEvaluable = true, Category = "system", ReturnType = "string")]
        object Version(DataSources ds) =>
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        [TelemetryAPI("a.mods", "Detected Mod Integrations", Plotable = false, AlwaysEvaluable = true, Category = "system", ReturnType = "object")]
        object Mods(DataSources ds) => ModDetector.Detect();

        [TelemetryAPI("a.physicsMode", "Physics Mode (patched_conics or n_body)",
            Units = APIEntry.UnitType.STRING, AlwaysEvaluable = true,
            Category = "system", ReturnType = "string")]
        object PhysicsMode(DataSources ds) => PrincipiaDataLinkHandler.IsPrincipiaActive() ? "n_body" : "patched_conics";

        [TelemetryAPI("a.schema", "Full API Schema (JSON)", Plotable = false, AlwaysEvaluable = true,
            Category = "system", ReturnType = "object")]
        object Schema(DataSources ds) => Telemachus.Generated.TelemetrySchema.Json;
    }

    /// <summary>
    /// Checks which supported mods are installed by looking for known types
    /// in loaded assemblies. Results are cached after first scan.
    /// </summary>
    static class ModDetector
    {
        static Dictionary<string, object> _cached;

        static readonly (string key, string typeName)[] KnownMods =
        {
            ("far",       "FerramAerospaceResearch.FARAPI"),
            ("mechjeb",   "MuMech.MechJebCore"),
            ("scansat",   "SCANsat.SCANutil"),
            ("rpm",       "JSI.RPMVesselComputer"),
            ("realchute", "RealChute.RealChuteModule"),
            ("kos",       "kOS.Core"),
            ("vesselview", "VesselView.VesselViewer"),
            ("astrogator", "Astrogator.Astrogator"),
            ("principia",  "principia.ksp_plugin_adapter.PrincipiaPluginAdapter"),
        };

        public static Dictionary<string, object> Detect()
        {
            if (_cached != null) return _cached;

            var result = new Dictionary<string, object>();
            var loadedTypes = new HashSet<string>();

            foreach (var asm in AssemblyLoader.loadedAssemblies)
            {
                try
                {
                    foreach (var type in asm.assembly.GetExportedTypes())
                        loadedTypes.Add(type.FullName);
                }
                catch { }
            }

            foreach (var (key, typeName) in KnownMods)
                result[key] = loadedTypes.Contains(typeName);

            _cached = result;
            return result;
        }
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
