using System;
using System.Collections.Generic;

namespace Telemachus
{
    public class KSPAPI : KSPAPIBase
    {
        private PluginManager _manager;

        public KSPAPI(FormatterProvider formatters, VesselChangeDetector vesselChangeDetector,
            ServerConfiguration serverConfiguration, PluginManager manager)
        {
            _manager = manager;

            APIHandlers.Add(new PausedDataLinkHandler(formatters));
            APIHandlers.Add(new FlyByWireDataLinkHandler(formatters));
            APIHandlers.Add(new FlightDataLinkHandler(formatters));
            APIHandlers.Add(new MechJebDataLinkHandler(formatters));
            APIHandlers.Add(new FARDataLinkHandler(formatters));
            APIHandlers.Add(new RealChuteDataLinkHandler(formatters));
            APIHandlers.Add(new TimeWarpDataLinkHandler(formatters));
            APIHandlers.Add(new TargetDataLinkHandler(formatters));

            APIHandlers.Add(new CompoundDataLinkHandler(
                new List<DataLinkHandler> {
                    new OrbitDataLinkHandler(formatters),
                    new SensorDataLinkHandler(vesselChangeDetector, formatters),
                    new VesselDataLinkHandler(formatters),
                    new BodyDataLinkHandler(formatters),
                    new ResourceDataLinkHandler(vesselChangeDetector, formatters),
                    new APIDataLinkHandler(this, formatters, serverConfiguration),
                    new NavBallDataLinkHandler(formatters),
                    new MapViewDataLinkHandler(formatters),
                    new DockingDataLinkHandler(formatters),
                    new DeltaVDataLinkHandler(formatters),
                    new AlarmClockDataLinkHandler(formatters)
                    }, formatters
                ));
        }

        public override Vessel getVessel()
        {
            return FlightGlobals.ActiveVessel;
        }

        public override object ProcessAPIString(string apistring)
        {
            var data = new DataSources() { vessel = getVessel() };
            // Extract any arguments/parameters in this API string
            var name = apistring;
            parseParams(ref name, ref data);

            // Are we in flight mode, with a vessel?
            var cleanFlightMode = HighLogic.LoadedSceneIsFlight && data.vessel != null;

            try
            {
                // Get the API entry
                process(name, out APIEntry apiEntry);
                if (apiEntry == null) return null;

                // Can we run this variable at the moment?
                if (!apiEntry.alwaysEvaluable && !cleanFlightMode)
                {
                    if (data.vessel == null) throw new VariableNotEvaluable(apistring, "No vessel!");
                    throw new VariableNotEvaluable(apistring, "Not in flight mode");
                }

                // run the API entry
                var result = apiEntry.function(data);
                // And return the serialization-ready value
                return apiEntry.formatter.prepareForSerialization(result);
            }
            catch (UnknownAPIException)
            {
                if (!cleanFlightMode) throw new VariableNotEvaluable(apistring, "Plugin variables not evaluable outside flight scene with vessel");

                // Try looking in the pluginManager
                var pluginAPI = _manager.GetAPIDelegate(name);
                // If no entry, just continue the throwing of the exception
                if (pluginAPI == null) throw;

                // We found an API entry! Let's use that.
                return pluginAPI(data.vessel, data.args.ToArray());
            }
        }
    }

    public abstract class KSPAPIBase
    {
        public class UnknownAPIException : ArgumentException
        {
            public string apiString = "";

            public UnknownAPIException(string apiString = "")
            {
                this.apiString = apiString;
            }

            public UnknownAPIException(string message, string apiString = "")
                : base(message)
            {
                this.apiString = apiString;
            }

            public UnknownAPIException(string message, string apiString, Exception inner)
                : base(message, inner)
            {
                this.apiString = apiString;
            }
        }

        public class VariableNotEvaluable : Exception
        {
            public VariableNotEvaluable(string apiString, string reason)
                : base("Cannot run " + apiString + ";" + reason)
            {

            }
        }

        protected List<DataLinkHandler> APIHandlers = new();

        public void getAPIList(ref List<APIEntry> APIList)
        {
            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                APIHandler.appendAPIList(ref APIList);
            }
        }

        public void getAPIEntry(string APIString, ref List<APIEntry> APIList)
        {
            APIEntry result = null;
            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                if (APIHandler.process(APIString, out result)) break;
            }
            APIList.Add(result);
        }

        public void process(String API, out APIEntry apiEntry)
        {
            APIEntry result = null;
            foreach (DataLinkHandler APIHandler in APIHandlers)
            {
                if (APIHandler.process(API, out result))
                {
                    break;
                }
            }
            if (result == null) throw new UnknownAPIException("Could not find API entry named " + API, API);
            apiEntry = result;
        }

        abstract public Vessel getVessel();

        public void parseParams(ref String arg, ref DataSources dataSources)
        {
            dataSources.args.Clear();

            try
            {
                if (arg.Contains("["))
                {
                    String[] argsSplit = arg.Split('[');
                    argsSplit[1] = argsSplit[1][..^1];
                    arg = argsSplit[0];
                    String[] paramSplit = argsSplit[1].Split(',');

                    for (int i = 0; i < paramSplit.Length; i++)
                    {
                        dataSources.args.Add(paramSplit[i]);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
        }

        /// <summary>
        /// Accepts a string, and does any API processing (with the current vessel), returning the result.
        /// </summary>
        public abstract object ProcessAPIString(string apistring);
    }
}
