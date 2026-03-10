using System;
using System.Collections.Generic;

namespace Telemachus
{
    /// <summary>
    /// Thermal monitoring telemetry: hottest parts, heat shields, engine
    /// temperatures. All data comes from iterating vessel.parts — no
    /// external mod dependencies.
    /// Algorithms ported from RasterPropMonitor (GPLv3, by Mihara/MOARdV).
    /// </summary>
    public class ThermalDataLinkHandler : DataLinkHandler
    {
        public ThermalDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // --- Hottest part (closest to thermal limit) ---

        [TelemetryAPI("therm.hottestPartTemp", "Hottest Part Temperature (C)",
            Units = APIEntry.UnitType.TEMP, Category = "thermal", ReturnType = "double")]
        object HottestPartTemp(DataSources ds)
        {
            FindHottestPart(ds.vessel, out double temp, out _, out _);
            return temp - 273.15;
        }

        [TelemetryAPI("therm.hottestPartTempKelvin", "Hottest Part Temperature (K)",
            Units = APIEntry.UnitType.TEMP, Category = "thermal", ReturnType = "double")]
        object HottestPartTempKelvin(DataSources ds)
        {
            FindHottestPart(ds.vessel, out double temp, out _, out _);
            return temp;
        }

        [TelemetryAPI("therm.hottestPartMaxTemp", "Hottest Part Max Temperature (K)",
            Units = APIEntry.UnitType.TEMP, Category = "thermal", ReturnType = "double")]
        object HottestPartMaxTemp(DataSources ds)
        {
            FindHottestPart(ds.vessel, out _, out double maxTemp, out _);
            return maxTemp;
        }

        [TelemetryAPI("therm.hottestPartTempRatio", "Hottest Part Temperature Ratio (0-1)",
            Category = "thermal", ReturnType = "double")]
        object HottestPartTempRatio(DataSources ds)
        {
            FindHottestPart(ds.vessel, out double temp, out double maxTemp, out _);
            return maxTemp > 0 ? temp / maxTemp : 0.0;
        }

        [TelemetryAPI("therm.hottestPartName", "Hottest Part Name",
            Units = APIEntry.UnitType.STRING, Plotable = false, Category = "thermal", ReturnType = "string")]
        object HottestPartName(DataSources ds)
        {
            FindHottestPart(ds.vessel, out _, out _, out string name);
            return name;
        }

        /// <summary>
        /// Finds the part closest to its thermal limit by comparing
        /// (maxTemp - currentTemp) across both skin and internal temperatures.
        /// </summary>
        static void FindHottestPart(Vessel vessel, out double temp, out double maxTemp, out string name)
        {
            temp = maxTemp = 0;
            name = "";
            double smallestMargin = double.MaxValue;

            foreach (var part in vessel.parts)
            {
                double skinMargin = part.skinMaxTemp - part.skinTemperature;
                if (skinMargin < smallestMargin)
                {
                    temp = part.skinTemperature;
                    maxTemp = part.skinMaxTemp;
                    name = part.partInfo.title;
                    smallestMargin = skinMargin;
                }

                double intMargin = part.maxTemp - part.temperature;
                if (intMargin < smallestMargin)
                {
                    temp = part.temperature;
                    maxTemp = part.maxTemp;
                    name = part.partInfo.title;
                    smallestMargin = intMargin;
                }
            }
        }

        // --- Hottest engine ---

        [TelemetryAPI("therm.hottestEngineTemp", "Hottest Engine Temperature (K)",
            Units = APIEntry.UnitType.TEMP, Category = "thermal", ReturnType = "double")]
        object HottestEngineTemp(DataSources ds)
        {
            FindHottestEngine(ds.vessel, out double temp, out _);
            return temp;
        }

        [TelemetryAPI("therm.hottestEngineMaxTemp", "Hottest Engine Max Temperature (K)",
            Units = APIEntry.UnitType.TEMP, Category = "thermal", ReturnType = "double")]
        object HottestEngineMaxTemp(DataSources ds)
        {
            FindHottestEngine(ds.vessel, out _, out double maxTemp);
            return maxTemp;
        }

        [TelemetryAPI("therm.hottestEngineTempRatio", "Hottest Engine Temperature Ratio (0-1)",
            Category = "thermal", ReturnType = "double")]
        object HottestEngineTempRatio(DataSources ds)
        {
            FindHottestEngine(ds.vessel, out double temp, out double maxTemp);
            return maxTemp > 0 ? temp / maxTemp : 0.0;
        }

        [TelemetryAPI("therm.anyEnginesOverheating", "Any Engine Near Overheat (>90%)",
            Category = "thermal", ReturnType = "bool")]
        object AnyEnginesOverheating(DataSources ds)
        {
            foreach (var part in ds.vessel.parts)
            {
                bool hasEngine = false;
                foreach (var module in part.Modules)
                {
                    if (module is ModuleEngines)
                    {
                        hasEngine = true;
                        break;
                    }
                }

                if (hasEngine)
                {
                    if (part.skinTemperature / part.skinMaxTemp > 0.9 ||
                        part.temperature / part.maxTemp > 0.9)
                        return true;
                }
            }
            return false;
        }

        static void FindHottestEngine(Vessel vessel, out double temp, out double maxTemp)
        {
            temp = maxTemp = 0;
            double smallestMargin = double.MaxValue;

            foreach (var part in vessel.parts)
            {
                bool hasEngine = false;
                foreach (var module in part.Modules)
                {
                    if (module is ModuleEngines)
                    {
                        hasEngine = true;
                        break;
                    }
                }
                if (!hasEngine) continue;

                double skinMargin = part.skinMaxTemp - part.skinTemperature;
                if (skinMargin < smallestMargin)
                {
                    temp = part.skinTemperature;
                    maxTemp = part.skinMaxTemp;
                    smallestMargin = skinMargin;
                }

                double intMargin = part.maxTemp - part.temperature;
                if (intMargin < smallestMargin)
                {
                    temp = part.temperature;
                    maxTemp = part.maxTemp;
                    smallestMargin = intMargin;
                }
            }
        }

        // --- Heat shield ---

        [TelemetryAPI("therm.heatShieldTemp", "Heat Shield Temperature (K)",
            Units = APIEntry.UnitType.TEMP, Category = "thermal", ReturnType = "double")]
        object HeatShieldTemp(DataSources ds)
        {
            FindHeatShield(ds.vessel, out double temp, out _);
            return temp;
        }

        [TelemetryAPI("therm.heatShieldTempCelsius", "Heat Shield Temperature (C)",
            Units = APIEntry.UnitType.TEMP, Category = "thermal", ReturnType = "double")]
        object HeatShieldTempCelsius(DataSources ds)
        {
            FindHeatShield(ds.vessel, out double temp, out _);
            return temp - 273.15;
        }

        [TelemetryAPI("therm.heatShieldFlux", "Heat Shield Thermal Flux (kW)",
            Category = "thermal", ReturnType = "double")]
        object HeatShieldFlux(DataSources ds)
        {
            FindHeatShield(ds.vessel, out _, out double flux);
            return flux;
        }

        /// <summary>
        /// Finds the hottest part that has a ModuleAblator (heat shield).
        /// Reports skin temperature and combined convection+radiation flux.
        /// </summary>
        static void FindHeatShield(Vessel vessel, out double temp, out double flux)
        {
            temp = flux = 0;

            foreach (var part in vessel.parts)
            {
                bool hasAblator = false;
                foreach (var module in part.Modules)
                {
                    if (module is ModuleAblator)
                    {
                        hasAblator = true;
                        break;
                    }
                }
                if (!hasAblator) continue;

                if (part.skinTemperature > temp)
                {
                    temp = part.skinTemperature;
                    flux = part.thermalConvectionFlux + part.thermalRadiationFlux;
                }
            }
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }
}
