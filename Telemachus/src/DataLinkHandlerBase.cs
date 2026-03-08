using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Telemachus
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TelemetryAPIAttribute : Attribute
    {
        public string Key { get; }
        public string Description { get; }
        public APIEntry.UnitType Units { get; set; } = APIEntry.UnitType.UNITLESS;
        public bool Plotable { get; set; } = true;
        public bool IsAction { get; set; } = false;
        public bool AlwaysEvaluable { get; set; } = false;
        /// <summary>
        /// Name of a property on FormatterProvider. Null means Default.
        /// </summary>
        public string Formatter { get; set; } = null;

        public TelemetryAPIAttribute(string key, string description)
        {
            Key = key;
            Description = description;
        }
    }

    public abstract class DataLinkHandler
    {
        public delegate object APIDelegate(DataSources datasources);

        private Dictionary<string, APIEntry> APIEntries = new();
        APIEntry nullAPI = null;
        protected FormatterProvider formatters = null;

        private static readonly Dictionary<Type, PropertyInfo> formatterPropertyCache = new();

        public DataLinkHandler(FormatterProvider formatters)
        {
            this.formatters = formatters;
            nullAPI = new APIEntry(
                dataSources => pausedHandler(),
                "", "", formatters.Default, APIEntry.UnitType.UNITLESS);

            RegisterAnnotatedMethods();
        }

        private void RegisterAnnotatedMethods()
        {
            var methods = GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<TelemetryAPIAttribute>();
                if (attr == null) continue;

                var fn = (APIDelegate)Delegate.CreateDelegate(typeof(APIDelegate), this, method);
                var formatter = ResolveFormatter(attr.Formatter);

                if (attr.IsAction)
                    registerAPI(new ActionAPIEntry(fn, attr.Key, attr.Description, formatter));
                else if (attr.Plotable)
                    registerAPI(new PlotableAPIEntry(fn, attr.Key, attr.Description,
                        formatter, attr.Units, attr.AlwaysEvaluable));
                else
                    registerAPI(new APIEntry(fn, attr.Key, attr.Description,
                        formatter, attr.Units, attr.AlwaysEvaluable));
            }
        }

        private DataSourceResultFormatter ResolveFormatter(string name)
        {
            if (name == null) return formatters.Default;

            var providerType = typeof(FormatterProvider);
            if (!formatterPropertyCache.TryGetValue(providerType, out _))
            {
                foreach (var prop in providerType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    formatterPropertyCache[prop.PropertyType] = prop; // just warm the cache
            }

            var property = providerType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                PluginLogger.debug("Unknown formatter: " + name);
                return formatters.Default;
            }
            return (DataSourceResultFormatter)property.GetValue(formatters);
        }

        public IEnumerable<KeyValuePair<string, APIEntry>> API => APIEntries;

        public virtual bool process(String API, out APIEntry result)
        {
            if (!APIEntries.TryGetValue(API, out APIEntry entry))
            {
                result = null;
                return false;
            }

            result = pausedHandler() == 0 ? entry : nullAPI;
            return true;
        }

        public void appendAPIList(ref List<APIEntry> APIList)
        {
            foreach (var entry in APIEntries)
                APIList.Add(entry.Value);
        }

        protected void registerAPI(APIEntry entry)
        {
            APIEntries.Add(entry.APIString, entry);
        }

        protected virtual int pausedHandler()
        {
            return 0;
        }

        protected static float checkFlightStateParameters(float f)
        {
            if (float.IsNaN(f))
                f = 0;
            return Mathf.Clamp(f, -1f, 1f);
        }

        protected static APIDelegate queueDelayed(APIDelegate action)
        {
            return dataSources =>
            {
                TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI",
                    new DelayedAPIEntry(dataSources.Clone(), action),
                    UnityEngine.SendMessageOptions.DontRequireReceiver);
                return false;
            };
        }

        protected static APIDelegate queueDelayed(APIDelegate action, Func<Vessel, int> predictor)
        {
            return dataSources =>
            {
                TelemachusBehaviour.instance.BroadcastMessage("queueDelayedAPI",
                    new DelayedAPIEntry(dataSources.Clone(), action),
                    UnityEngine.SendMessageOptions.DontRequireReceiver);
                return predictor(dataSources.vessel);
            };
        }
    }

    public class APIEntry
    {
        public enum UnitType { UNITLESS, VELOCITY, DEG, DISTANCE, TIME, STRING, TEMP, PRES, GRAV, ACC, DENSITY, DYNAMICPRESSURE, G, DATE, LATLON };

        public DataLinkHandler.APIDelegate function { get; set; }
        public string APIString { get; set; }
        public string name { get; set; }
        public UnitType units { get; set; }
        public bool plotable { get; set; }
        public DataSourceResultFormatter formatter { get; set; }
        public bool alwaysEvaluable { get; set; }

        public APIEntry(DataLinkHandler.APIDelegate function, string APIString,
            string name, DataSourceResultFormatter formatter, UnitType units, bool alwaysEvaluable = false)
        {
            this.function = function;
            this.APIString = APIString;
            this.name = name;
            this.formatter = formatter;
            this.units = units;
            this.alwaysEvaluable = alwaysEvaluable;
        }
    }

    public class ActionAPIEntry : APIEntry
    {
        public ActionAPIEntry(DataLinkHandler.APIDelegate function,
            string APIString, string name, DataSourceResultFormatter formatter)
            : base(function, APIString, name, formatter, APIEntry.UnitType.UNITLESS)
        {
            plotable = false;
        }
    }

    public class PlotableAPIEntry : APIEntry
    {
        public PlotableAPIEntry(DataLinkHandler.APIDelegate function, string APIString, string name,
           DataSourceResultFormatter formatter, UnitType units, bool alwaysEvaluable = false)
            : base(function, APIString, name, formatter, units, alwaysEvaluable)
        {
            this.plotable = true;
        }
    }

    public class DelayedAPIEntry : APIEntry
    {
        private DataSources dataSources = null;

        public DelayedAPIEntry(DataSources dataSources, DataLinkHandler.APIDelegate function)
            : base(function, "", "", null, UnitType.UNITLESS)
        {
            this.dataSources = dataSources;
        }

        public void call()
        {
            try
            {
                function(dataSources);
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message);
            }
        }
    }

    public class OrbitPatches
    {
        public static List<Orbit> getPatchesForOrbit(Orbit orbit)
        {
            var orbitPatches = new List<Orbit>();
            var nextOrbitPatch = orbit;

            while (nextOrbitPatch != null && nextOrbitPatch.activePatch)
            {
                orbitPatches.Add(nextOrbitPatch);
                if (nextOrbitPatch.patchEndTransition == Orbit.PatchTransitionType.MANEUVER)
                    break;
                else
                    nextOrbitPatch = nextOrbitPatch.nextPatch;
            }

            return orbitPatches;
        }

        public static Orbit getOrbitPatch(Orbit orbit, int index)
        {
            List<Orbit> orbitPatches = getPatchesForOrbit(orbit);
            if (index >= orbitPatches.Count) { return null; }
            return orbitPatches[index];
        }
    }
}
