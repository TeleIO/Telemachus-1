using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Telemachus
{
    /// <summary>
    /// Exposes transfer planning data from Astrogator.
    /// Astrogator uses KSPAddon-based MonoBehaviours (FlightAstrogator, etc.)
    /// with a private AstrogationModel containing transfer calculations.
    /// All access is via reflection — soft dependency.
    /// </summary>
    public class AstrogatorDataLinkHandler : DataLinkHandler
    {
        static bool _searched;
        static Type _astrogatorType;      // Astrogator.Astrogator (base)
        static Type _flightType;          // Astrogator.FlightAstrogator
        static Type _modelType;           // Astrogator.AstrogationModel
        static Type _transferType;        // Astrogator.TransferModel
        static Type _burnType;            // Astrogator.BurnModel

        // AstrogationModel members
        static PropertyInfo _modelProp;          // Astrogator.model (private)
        static PropertyInfo _transfersProp;       // AstrogationModel.transfers
        static PropertyInfo _activeTransferProp;  // AstrogationModel.ActiveTransfer
        static PropertyInfo _errorConditionProp;
        static PropertyInfo _retrogradeOrbitProp;
        static PropertyInfo _hyperbolicOrbitProp;

        // TransferModel members
        static PropertyInfo _destinationProp;
        static PropertyInfo _ejectionBurnProp;
        static PropertyInfo _planeChangeBurnProp;
        static PropertyInfo _ejectionBurnDurationProp;
        static PropertyInfo _retrogradeTransferProp;

        // BurnModel members
        static PropertyInfo _atTimeProp;
        static PropertyInfo _progradeProp;
        static PropertyInfo _normalProp;
        static PropertyInfo _radialProp;
        static PropertyInfo _totalDeltaVProp;

        // Loader members (for lazy-triggering calculations)
        static Type _loaderType;           // Astrogator.AstrogationLoadBehaviorette
        static PropertyInfo _loaderProp;   // Astrogator.loader (private)
        static PropertyInfo _numOpenDisplaysProp;
        static MethodInfo _tryStartLoadMethod;
        static Delegate _noopCallback;
        static bool _loadTriggered;
        static object _lastInstance;

        public AstrogatorDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        static void Search()
        {
            if (_searched) return;
            _searched = true;

            foreach (var asm in AssemblyLoader.loadedAssemblies)
            {
                try
                {
                    if (_astrogatorType == null)
                        _astrogatorType = asm.assembly.GetType("Astrogator.Astrogator", false);
                    if (_flightType == null)
                        _flightType = asm.assembly.GetType("Astrogator.FlightAstrogator", false);
                    if (_modelType == null)
                        _modelType = asm.assembly.GetType("Astrogator.AstrogationModel", false);
                    if (_transferType == null)
                        _transferType = asm.assembly.GetType("Astrogator.TransferModel", false);
                    if (_burnType == null)
                        _burnType = asm.assembly.GetType("Astrogator.BurnModel", false);
                }
                catch { }
            }

            if (_astrogatorType == null)
            {
                PluginLogger.debug("Astrogator not found");
                return;
            }

            PluginLogger.debug("Astrogator detected: " + _astrogatorType.Assembly.GetName().Version);

            var nonPublicInst = BindingFlags.NonPublic | BindingFlags.Instance;
            var publicInst = BindingFlags.Public | BindingFlags.Instance;

            // model is a private property on the base Astrogator type
            _modelProp = _astrogatorType.GetProperty("model", nonPublicInst);

            if (_modelType != null)
            {
                _transfersProp = _modelType.GetProperty("transfers", publicInst);
                _activeTransferProp = _modelType.GetProperty("ActiveTransfer", publicInst);
                _errorConditionProp = _modelType.GetProperty("ErrorCondition", publicInst);
                _retrogradeOrbitProp = _modelType.GetProperty("retrogradeOrbit", publicInst);
                _hyperbolicOrbitProp = _modelType.GetProperty("hyperbolicOrbit", publicInst);
            }

            if (_transferType != null)
            {
                _destinationProp = _transferType.GetProperty("destination", publicInst);
                _ejectionBurnProp = _transferType.GetProperty("ejectionBurn", publicInst);
                _planeChangeBurnProp = _transferType.GetProperty("planeChangeBurn", publicInst);
                _ejectionBurnDurationProp = _transferType.GetProperty("ejectionBurnDuration", publicInst);
                _retrogradeTransferProp = _transferType.GetProperty("retrogradeTransfer", publicInst);
            }

            if (_burnType != null)
            {
                _atTimeProp = _burnType.GetProperty("atTime", publicInst);
                _progradeProp = _burnType.GetProperty("prograde", publicInst);
                _normalProp = _burnType.GetProperty("normal", publicInst);
                _radialProp = _burnType.GetProperty("radial", publicInst);
                _totalDeltaVProp = _burnType.GetProperty("totalDeltaV", publicInst);
            }

            // Loader — for triggering calculations without the UI
            _loaderProp = _astrogatorType.GetProperty("loader", nonPublicInst);
            if (_loaderProp != null)
            {
                _loaderType = _loaderProp.PropertyType;
                _numOpenDisplaysProp = _loaderType.GetProperty("numOpenDisplays", nonPublicInst);
                _tryStartLoadMethod = _loaderType.GetMethod("TryStartLoad",
                    BindingFlags.Public | BindingFlags.Instance);

                // Build a no-op delegate matching LoadDoneCallback
                var callbackType = _loaderType.GetNestedType("LoadDoneCallback",
                    BindingFlags.Public | BindingFlags.NonPublic);
                if (callbackType != null)
                    _noopCallback = Delegate.CreateDelegate(callbackType,
                        typeof(AstrogatorDataLinkHandler).GetMethod(nameof(Noop),
                            BindingFlags.Static | BindingFlags.NonPublic));
            }
        }

        static void Noop() { }

        /// <summary>
        /// Find the active FlightAstrogator instance via Unity's FindObjectOfType (reflected).
        /// </summary>
        static object FindInstance()
        {
            Search();
            if (_flightType == null) return null;

            // UnityEngine.Object.FindObjectOfType(Type)
            return UnityEngine.Object.FindObjectOfType(_flightType);
        }

        static object GetModel()
        {
            var instance = FindInstance();
            if (instance == null || _modelProp == null) return null;

            // Reset trigger flag when the Astrogator instance changes (new scene/vessel)
            if (instance != _lastInstance)
            {
                _lastInstance = instance;
                _loadTriggered = false;
            }

            return _modelProp.GetValue(instance);
        }

        static IList GetTransfers()
        {
            var model = GetModel();
            if (model == null || _transfersProp == null) return null;
            var transfers = _transfersProp.GetValue(model) as IList;

            // Lazy-trigger: if transfers are empty, poke Astrogator to calculate
            if ((transfers == null || transfers.Count == 0) && !_loadTriggered)
                TriggerLoad();

            return transfers;
        }

        /// <summary>
        /// Fakes the "window open" state and triggers a background calculation.
        /// Astrogator gates calculations behind numOpenDisplays > 0; we bump it
        /// once and leave it — the async coroutine needs it to stay positive for
        /// the entire multi-frame calculation.  Keeping it at 1 also lets
        /// Astrogator auto-recalculate on orbit changes, which is what we want.
        /// </summary>
        static void TriggerLoad()
        {
            _loadTriggered = true;
            var instance = FindInstance();
            if (instance == null || _loaderProp == null || _tryStartLoadMethod == null)
                return;

            var loader = _loaderProp.GetValue(instance);
            if (loader == null) return;

            // Permanently bump numOpenDisplays so AllowStart() passes and stays passing
            // through the async calculation.  Astrogator's UI increments/decrements on
            // top of this — opening the window goes to 2, closing back to 1, still fine.
            if (_numOpenDisplaysProp != null)
            {
                int cur = (int)(_numOpenDisplaysProp.GetValue(loader) ?? 0);
                if (cur < 1)
                    _numOpenDisplaysProp.SetValue(loader, 1);
            }

            try
            {
                var origin = FlightGlobals.ActiveVessel as ITargetable
                    ?? (ITargetable)FlightGlobals.getMainBody();
                _tryStartLoadMethod.Invoke(loader, new object[]
                    { origin, _noopCallback, _noopCallback, _noopCallback, true });
            }
            catch (Exception ex)
            {
                PluginLogger.debug("Astrogator TriggerLoad failed: " + ex.Message);
            }
        }

        // --- Availability ---

        [TelemetryAPI("astg.available", "Astrogator Is Installed", AlwaysEvaluable = true)]
        object Available(DataSources ds)
        {
            Search();
            return _astrogatorType != null;
        }

        [TelemetryAPI("astg.active", "Astrogator Instance Active")]
        object Active(DataSources ds) => FindInstance() != null;

        // --- Orbit status ---

        [TelemetryAPI("astg.errorCondition", "Transfer Calculation Error")]
        object ErrorCondition(DataSources ds)
        {
            var model = GetModel();
            return model != null && _errorConditionProp != null
                ? _errorConditionProp.GetValue(model) : null;
        }

        [TelemetryAPI("astg.retrogradeOrbit", "Retrograde Orbit")]
        object RetrogradeOrbit(DataSources ds)
        {
            var model = GetModel();
            return model != null && _retrogradeOrbitProp != null
                ? _retrogradeOrbitProp.GetValue(model) : null;
        }

        [TelemetryAPI("astg.hyperbolicOrbit", "Hyperbolic Orbit")]
        object HyperbolicOrbit(DataSources ds)
        {
            var model = GetModel();
            return model != null && _hyperbolicOrbitProp != null
                ? _hyperbolicOrbitProp.GetValue(model) : null;
        }

        // --- Transfer count ---

        [TelemetryAPI("astg.transferCount", "Number of Available Transfers")]
        object TransferCount(DataSources ds) => GetTransfers()?.Count ?? 0;

        // --- All transfers summary ---

        [TelemetryAPI("astg.transfers", "All Transfer Opportunities", Plotable = false)]
        object Transfers(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null) return null;

            var result = new List<Dictionary<string, object>>();
            int index = 0;

            foreach (var transfer in transfers)
            {
                var info = BuildTransferInfo(transfer, index++);
                if (info != null)
                    result.Add(info);
            }
            return result;
        }

        // --- Single transfer by index ---

        [TelemetryAPI("astg.transfer", "Transfer by Index [int index]", Plotable = false)]
        object TransferByIndex(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null || ds.args.Count < 1) return null;

            if (!int.TryParse(ds.args[0], out int index) || index < 0 || index >= transfers.Count)
                return null;

            return BuildTransferInfo(transfers[index], index);
        }

        // --- Active transfer (the one with a maneuver node) ---

        [TelemetryAPI("astg.activeTransfer", "Transfer With Active Maneuver Node", Plotable = false)]
        object ActiveTransfer(DataSources ds)
        {
            var model = GetModel();
            if (model == null || _activeTransferProp == null) return null;

            var transfer = _activeTransferProp.GetValue(model);
            if (transfer == null) return null;

            return BuildTransferInfo(transfer, -1);
        }

        // --- Best (first) transfer delta-v for quick readout ---

        [TelemetryAPI("astg.nextDeltaV", "Next Transfer Total Delta-V", Units = APIEntry.UnitType.VELOCITY)]
        object NextDeltaV(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null || transfers.Count == 0) return null;
            return GetBurnField(transfers[0], _ejectionBurnProp, _totalDeltaVProp);
        }

        [TelemetryAPI("astg.nextDestination", "Next Transfer Destination", Units = APIEntry.UnitType.STRING)]
        object NextDestination(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null || transfers.Count == 0) return null;
            return GetDestinationName(transfers[0]);
        }

        [TelemetryAPI("astg.nextBurnTime", "Next Transfer Burn Time (UT)", Units = APIEntry.UnitType.DATE)]
        object NextBurnTime(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null || transfers.Count == 0) return null;
            return GetBurnField(transfers[0], _ejectionBurnProp, _atTimeProp);
        }

        [TelemetryAPI("astg.nextBurnCountdown", "Seconds Until Next Transfer Burn", Units = APIEntry.UnitType.TIME)]
        object NextBurnCountdown(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null || transfers.Count == 0) return null;
            var ut = GetBurnField(transfers[0], _ejectionBurnProp, _atTimeProp);
            if (ut is double d)
                return d - Planetarium.GetUniversalTime();
            return null;
        }

        // --- Actions ---

        [TelemetryAPI("astg.createManeuver", "Create Maneuver for Transfer [int index]", IsAction = true)]
        object CreateManeuver(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null || ds.args.Count < 1) return false;

            if (!int.TryParse(ds.args[0], out int index) || index < 0 || index >= transfers.Count)
                return false;

            var transfer = transfers[index];
            var method = _transferType?.GetMethod("CreateManeuvers", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) return false;

            method.Invoke(transfer, null);
            return true;
        }

        [TelemetryAPI("astg.warpToBurn", "Warp to Transfer Burn [int index]", IsAction = true)]
        object WarpToBurn(DataSources ds)
        {
            var transfers = GetTransfers();
            if (transfers == null || ds.args.Count < 1) return false;

            if (!int.TryParse(ds.args[0], out int index) || index < 0 || index >= transfers.Count)
                return false;

            var transfer = transfers[index];
            var method = _transferType?.GetMethod("WarpToBurn", BindingFlags.Public | BindingFlags.Instance);
            if (method == null) return false;

            method.Invoke(transfer, null);
            return true;
        }

        // --- Helpers ---

        Dictionary<string, object> BuildTransferInfo(object transfer, int index)
        {
            if (transfer == null) return null;

            var info = new Dictionary<string, object>();
            if (index >= 0) info["index"] = index;

            info["destination"] = GetDestinationName(transfer);

            if (_retrogradeTransferProp != null)
                info["retrograde"] = _retrogradeTransferProp.GetValue(transfer);

            // Ejection burn
            var ejBurn = _ejectionBurnProp?.GetValue(transfer);
            if (ejBurn != null)
            {
                info["ejectionDeltaV"] = _totalDeltaVProp?.GetValue(ejBurn);
                info["ejectionPrograde"] = _progradeProp?.GetValue(ejBurn);
                info["ejectionNormal"] = _normalProp?.GetValue(ejBurn);
                info["ejectionRadial"] = _radialProp?.GetValue(ejBurn);

                var atTime = _atTimeProp?.GetValue(ejBurn);
                info["ejectionTime"] = atTime;
                if (atTime is double ut)
                    info["ejectionCountdown"] = ut - Planetarium.GetUniversalTime();
            }

            // Ejection burn duration
            if (_ejectionBurnDurationProp != null)
                info["ejectionDuration"] = _ejectionBurnDurationProp.GetValue(transfer);

            // Plane change burn
            var pcBurn = _planeChangeBurnProp?.GetValue(transfer);
            if (pcBurn != null)
            {
                var pcDv = _totalDeltaVProp?.GetValue(pcBurn);
                // Only include plane change if it has a nonzero delta-v
                if (pcDv is double dv && dv > 0.001)
                {
                    info["planeChangeDeltaV"] = pcDv;
                    info["planeChangePrograde"] = _progradeProp?.GetValue(pcBurn);
                    info["planeChangeNormal"] = _normalProp?.GetValue(pcBurn);
                    info["planeChangeRadial"] = _radialProp?.GetValue(pcBurn);
                    info["planeChangeTime"] = _atTimeProp?.GetValue(pcBurn);
                }
            }

            return info;
        }

        static string GetDestinationName(object transfer)
        {
            if (_destinationProp == null) return null;
            var dest = _destinationProp.GetValue(transfer);
            if (dest == null) return null;

            // ITargetable has GetName()
            var getName = dest.GetType().GetMethod("GetName",
                BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            return getName?.Invoke(dest, null) as string ?? dest.ToString();
        }

        static object GetBurnField(object transfer, PropertyInfo burnProp, PropertyInfo fieldProp)
        {
            if (burnProp == null || fieldProp == null) return null;
            var burn = burnProp.GetValue(transfer);
            return burn != null ? fieldProp.GetValue(burn) : null;
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }
}
