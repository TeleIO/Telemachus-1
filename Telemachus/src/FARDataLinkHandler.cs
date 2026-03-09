using System;
using System.Reflection;

namespace Telemachus
{
    /// <summary>
    /// Exposes aerodynamic data from Ferram Aerospace Research (FAR).
    /// All data is read via reflection against FARAPI's static methods,
    /// so FAR is a soft dependency — if it's not installed, far.available
    /// returns false and all other entries return null.
    /// </summary>
    public class FARDataLinkHandler : DataLinkHandler
    {
        static Type _farAPI;
        static bool _searched;

        public FARDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        static Type FindFARAPI()
        {
            if (_searched) return _farAPI;
            _searched = true;

            foreach (var asm in AssemblyLoader.loadedAssemblies)
            {
                try
                {
                    _farAPI = asm.assembly.GetType("FerramAerospaceResearch.FARAPI", false);
                    if (_farAPI != null) break;
                }
                catch { }
            }

            if (_farAPI != null)
                PluginLogger.debug("FAR detected: " + _farAPI.Assembly.GetName().Version);
            else
                PluginLogger.debug("FAR not found");

            return _farAPI;
        }

        /// <summary>
        /// Invoke a no-arg static method on FARAPI. Returns null if FAR is absent
        /// or the method doesn't exist.
        /// </summary>
        static object InvokeStatic(string methodName)
        {
            var api = FindFARAPI();
            if (api == null) return null;

            var method = api.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null) return null;

            return method.Invoke(null, null);
        }

        /// <summary>
        /// Invoke a static method on FARAPI with a single Vessel argument.
        /// </summary>
        static object InvokeWithVessel(string methodName, Vessel vessel)
        {
            var api = FindFARAPI();
            if (api == null) return null;

            var method = api.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(Vessel) }, null);
            if (method == null) return null;

            return method.Invoke(null, new object[] { vessel });
        }

        // --- Availability ---

        [TelemetryAPI("far.available", "FAR Is Installed", AlwaysEvaluable = true)]
        object Available(DataSources ds) => FindFARAPI() != null;

        // --- Aerodynamic coefficients ---

        [TelemetryAPI("far.liftCoeff", "Lift Coefficient (Cl)")]
        object LiftCoeff(DataSources ds) => InvokeStatic("ActiveVesselLiftCoeff");

        [TelemetryAPI("far.dragCoeff", "Drag Coefficient (Cd)")]
        object DragCoeff(DataSources ds) => InvokeStatic("ActiveVesselDragCoeff");

        [TelemetryAPI("far.refArea", "Reference Area (m\u00b2)")]
        object RefArea(DataSources ds) => InvokeStatic("ActiveVesselRefArea");

        [TelemetryAPI("far.ballisticCoeff", "Ballistic Coefficient")]
        object BallisticCoeff(DataSources ds) => InvokeStatic("ActiveVesselBallisticCoeff");

        // --- Flight parameters ---

        [TelemetryAPI("far.dynPres", "Dynamic Pressure (FAR)", Units = APIEntry.UnitType.DYNAMICPRESSURE)]
        object DynPres(DataSources ds) => InvokeStatic("ActiveVesselDynPres");

        [TelemetryAPI("far.termVel", "Terminal Velocity", Units = APIEntry.UnitType.VELOCITY)]
        object TermVel(DataSources ds) => InvokeStatic("ActiveVesselTermVelEst");

        [TelemetryAPI("far.tsfc", "Thrust Specific Fuel Consumption")]
        object TSFC(DataSources ds) => InvokeStatic("ActiveVesselTSFC");

        // --- Attitude ---

        [TelemetryAPI("far.aoa", "Angle of Attack", Units = APIEntry.UnitType.DEG)]
        object AoA(DataSources ds) => InvokeStatic("ActiveVesselAoA");

        [TelemetryAPI("far.sideslip", "Sideslip Angle", Units = APIEntry.UnitType.DEG)]
        object Sideslip(DataSources ds) => InvokeStatic("ActiveVesselSideslip");

        // --- Stall ---

        [TelemetryAPI("far.stallFrac", "Stall Fraction (0-1)")]
        object StallFrac(DataSources ds) => InvokeStatic("ActiveVesselStallFrac");

        // --- Control surfaces ---

        [TelemetryAPI("far.flapSetting", "Flap Deflection Level (0-3, -1 if no flaps)")]
        object FlapSetting(DataSources ds) => InvokeWithVessel("VesselFlapSetting", ds.vessel);

        [TelemetryAPI("far.spoiler", "Spoilers Active")]
        object Spoiler(DataSources ds) => InvokeWithVessel("VesselSpoilerSetting", ds.vessel);

        // --- Actions ---

        [TelemetryAPI("far.increaseFlaps", "Increase Flap Deflection", IsAction = true)]
        object IncreaseFlaps(DataSources ds)
        {
            InvokeWithVessel("VesselIncreaseFlapDeflection", ds.vessel);
            return true;
        }

        [TelemetryAPI("far.decreaseFlaps", "Decrease Flap Deflection", IsAction = true)]
        object DecreaseFlaps(DataSources ds)
        {
            InvokeWithVessel("VesselDecreaseFlapDeflection", ds.vessel);
            return true;
        }

        [TelemetryAPI("far.setSpoilers", "Set Spoilers [bool active]", IsAction = true)]
        object SetSpoilers(DataSources ds)
        {
            var api = FindFARAPI();
            if (api == null) return false;

            var method = api.GetMethod("VesselSetSpoilers", BindingFlags.Public | BindingFlags.Static);
            if (method == null) return false;

            bool active = bool.Parse(ds.args[0]);
            method.Invoke(null, new object[] { ds.vessel, active });
            return true;
        }

        // --- Voxelization state ---

        [TelemetryAPI("far.voxelized", "Vessel Has Valid Voxelization")]
        object Voxelized(DataSources ds) => InvokeWithVessel("VesselVoxelizationCompletedAndValid", ds.vessel);

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }
}
