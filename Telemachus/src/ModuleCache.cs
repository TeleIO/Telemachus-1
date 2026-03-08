using System;
using System.Collections.Generic;
using System.Linq;

namespace Telemachus
{
    public abstract class ModuleCache<T>
    {
        protected const int ACCESS_REFRESH = 10;

        protected event EventHandler<EventArgs> VesselPropertyChanged;

        private Vessel theVessel = null;
        public Vessel vessel
        {
            get { return theVessel; }
            set
            {
                if (theVessel == value) return;
                theVessel = value;
                VesselPropertyChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected Dictionary<string, List<T>> partModules = new();

        readonly protected object cacheLock = new();

        public List<T> get(DataSources dataSources)
        {
            string ID = dataSources.args[0].ToLowerInvariant();

            lock (cacheLock)
            {
                if (partModules.TryGetValue(ID, out List<T> avail))
                    return new List<T>(avail);
            }

            return new List<T>();
        }

        protected abstract void refresh(Vessel vessel);
    }

    public class SimplifiedResource
    {
        public double amount { get; set; }
        public double maxAmount { get; set; }

        public SimplifiedResource(double amount, double maxAmount)
        {
            this.amount = amount;
            this.maxAmount = maxAmount;
        }
    }

    public class ActiveResourceCache : ModuleCache<SimplifiedResource>
    {
        private float lastRefreshTime = 0f;
        private const float REFRESH_INTERVAL = 0.5f;

        public ActiveResourceCache(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.UpdateNotify += update;
        }

        private void update(object sender, EventArgs eventArgs)
        {
            if (vessel != null && UnityEngine.Time.time - lastRefreshTime > REFRESH_INTERVAL)
            {
                lock (cacheLock)
                {
                    refresh(vessel);
                    lastRefreshTime = UnityEngine.Time.time;
                }
            }
        }

        protected override void refresh(Vessel vessel)
        {
            try
            {
                partModules.Clear();
                var activeParts = new HashSet<Part>();
                foreach (Part part in vessel.GetActiveParts())
                {
                    if (part.inverseStage == vessel.currentStage)
                    {
                        activeParts.Add(part);
                        activeParts.UnionWith(part.crossfeedPartSet.GetParts());
                    }
                }

                PartSet activePartSet = new PartSet(activeParts);
                PartResourceDefinitionList resourceDefinitionList = PartResourceLibrary.Instance.resourceDefinitions;

                foreach (PartResourceDefinition resourceDefinition in resourceDefinitionList)
                {
                    String key = resourceDefinition.name.ToString().ToLowerInvariant();
                    double amount = 0;
                    double maxAmount = 0;
                    bool pulling = true;

                    activePartSet.GetConnectedResourceTotals(resourceDefinition.id, out amount, out maxAmount, pulling);

                    if (!partModules.ContainsKey(key))
                        partModules[key] = new List<SimplifiedResource>();

                    partModules[key].Add(new SimplifiedResource(amount, maxAmount));
                    PluginLogger.debug("SIZE OF " + key + " " + partModules[key].Count + " " + amount);
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message);
            }
        }
    }

    public class ResourceCache : ModuleCache<PartResource>
    {
        public ResourceCache(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.UpdateNotify += update;
        }

        private void update(object sender, EventArgs eventArgs)
        {
            if (vessel != null)
            {
                lock (cacheLock)
                {
                    refresh(vessel);
                }
            }
        }

        protected override void refresh(Vessel vessel)
        {
            try
            {
                partModules.Clear();
                foreach (Part part in vessel.parts)
                {
                    if (part.Resources.Count > 0)
                    {
                        foreach (PartResource partResource in part.Resources)
                        {
                            String key = partResource.resourceName.ToLowerInvariant();
                            if (!partModules.TryGetValue(key, out List<PartResource> list))
                            {
                                list = new List<PartResource>();
                                partModules[key] = list;
                            }
                            list.Add(partResource);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message);
            }
        }
    }

    public class SensorCache : ModuleCache<ModuleEnviroSensor>
    {
        public SensorCache(VesselChangeDetector vesselChangeDetector)
        {
            vesselChangeDetector.UpdateNotify += update;
            VesselPropertyChanged += update;
        }

        private void update(object sender, EventArgs eventArgs)
        {
            if (vessel != null)
            {
                lock (cacheLock)
                {
                    refresh(vessel);
                }
            }
        }

        protected override void refresh(Vessel vessel)
        {
            try
            {
                partModules.Clear();
                List<Part> partsWithSensors = vessel.parts.FindAll(p => p.Modules.Contains("ModuleEnviroSensor"));
                foreach (Part part in partsWithSensors)
                {
                    foreach (var module in part.Modules.OfType<ModuleEnviroSensor>())
                    {
                        string sensorKey = module.sensorType.ToString().ToLowerInvariant();
                        if (!partModules.ContainsKey(sensorKey))
                            partModules[sensorKey] = new List<ModuleEnviroSensor>();
                        partModules[sensorKey].Add(module);
                    }
                }
            }
            catch (Exception e)
            {
                PluginLogger.debug(e.Message + " " + e.StackTrace);
            }
        }
    }
}
