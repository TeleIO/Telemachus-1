using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Telemachus
{
    /// <summary>
    /// Exposes parachute data from RealChute.
    /// Scans the vessel for RealChuteModule PartModules via reflection.
    /// </summary>
    public class RealChuteDataLinkHandler : DataLinkHandler
    {
        static Type _rcModuleType;
        static bool _searched;
        static FieldInfo _parachutesField;
        static FieldInfo _safeStateProp;
        static PropertyInfo _anyDeployedProp;

        // Per-chute fields
        static PropertyInfo _deploymentStateProp;
        static PropertyInfo _isDeployedProp;
        static FieldInfo _currentAreaField;
        static FieldInfo _deployedDiameterField;
        static FieldInfo _deploymentAltField;
        static FieldInfo _cutAltField;

        // Action methods
        static MethodInfo _guiDeploy;
        static MethodInfo _guiCut;
        static MethodInfo _guiArm;
        static MethodInfo _guiDisarm;

        public RealChuteDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        static Type FindRealChute()
        {
            if (_searched) return _rcModuleType;
            _searched = true;

            foreach (var asm in AssemblyLoader.loadedAssemblies)
            {
                try
                {
                    _rcModuleType = asm.assembly.GetType("RealChute.RealChuteModule", false);
                    if (_rcModuleType != null) break;
                }
                catch { }
            }

            if (_rcModuleType != null)
            {
                PluginLogger.debug("RealChute detected: " + _rcModuleType.Assembly.GetName().Version);
                _parachutesField = _rcModuleType.GetField("parachutes", BindingFlags.Public | BindingFlags.Instance);
                _safeStateProp = _rcModuleType.GetField("safeState", BindingFlags.Public | BindingFlags.Instance);
                _anyDeployedProp = _rcModuleType.GetProperty("AnyDeployed", BindingFlags.Public | BindingFlags.Instance);

                // Resolve chute type from the list's generic arg
                if (_parachutesField != null)
                {
                    var listType = _parachutesField.FieldType;
                    if (listType.IsGenericType)
                    {
                        var chuteType = listType.GetGenericArguments()[0];
                        _deploymentStateProp = chuteType.GetProperty("DeploymentState", BindingFlags.Public | BindingFlags.Instance);
                        _isDeployedProp = chuteType.GetProperty("IsDeployed", BindingFlags.Public | BindingFlags.Instance);
                        _currentAreaField = chuteType.GetField("currentArea", BindingFlags.Public | BindingFlags.Instance);
                        _deployedDiameterField = chuteType.GetField("deployedDiameter", BindingFlags.Public | BindingFlags.Instance);
                        _deploymentAltField = chuteType.GetField("deploymentAlt", BindingFlags.Public | BindingFlags.Instance);
                        _cutAltField = chuteType.GetField("cutAlt", BindingFlags.Public | BindingFlags.Instance);
                    }
                }

                // Action methods
                _guiDeploy = _rcModuleType.GetMethod("GUIDeploy", BindingFlags.Public | BindingFlags.Instance);
                _guiCut = _rcModuleType.GetMethod("GUICut", BindingFlags.Public | BindingFlags.Instance);
                _guiArm = _rcModuleType.GetMethod("GUIArm", BindingFlags.Public | BindingFlags.Instance);
                _guiDisarm = _rcModuleType.GetMethod("GUIDisarm", BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                PluginLogger.debug("RealChute not found");
            }

            return _rcModuleType;
        }

        static List<PartModule> FindModules(Vessel vessel)
        {
            var type = FindRealChute();
            if (type == null || vessel == null) return null;

            var result = new List<PartModule>();
            foreach (var part in vessel.parts)
            {
                foreach (var module in part.Modules)
                {
                    if (module.GetType() == type)
                        result.Add(module);
                }
            }
            return result.Count > 0 ? result : null;
        }

        // --- Availability ---

        [TelemetryAPI("rc.available", "RealChute Is Installed", AlwaysEvaluable = true, Category = "realchute", ReturnType = "bool", RequiresMod = "realchute")]
        object Available(DataSources ds) => FindRealChute() != null;

        // --- Vessel-level summary ---

        [TelemetryAPI("rc.count", "Number of RealChute Parts", Category = "realchute", ReturnType = "int", RequiresMod = "realchute")]
        object Count(DataSources ds) => FindModules(ds.vessel)?.Count ?? 0;

        [TelemetryAPI("rc.anyDeployed", "Any Chute Deployed", Category = "realchute", ReturnType = "bool", RequiresMod = "realchute")]
        object AnyDeployed(DataSources ds)
        {
            var modules = FindModules(ds.vessel);
            if (modules == null || _anyDeployedProp == null) return false;
            foreach (var module in modules)
            {
                if ((bool)_anyDeployedProp.GetValue(module, null))
                    return true;
            }
            return false;
        }

        [TelemetryAPI("rc.safeState", "Deployment Safety (SAFE/RISKY/DANGEROUS)", Units = APIEntry.UnitType.STRING, Category = "realchute", ReturnType = "string", RequiresMod = "realchute")]
        object SafeState(DataSources ds)
        {
            var modules = FindModules(ds.vessel);
            if (modules == null || _safeStateProp == null) return null;
            // Return worst safety state across all modules
            int worst = 0;
            foreach (var module in modules)
            {
                int val = (int)_safeStateProp.GetValue(module);
                if (val > worst) worst = val;
            }
            // 0=SAFE, 1=RISKY, 2=DANGEROUS
            return worst switch { 0 => "SAFE", 1 => "RISKY", 2 => "DANGEROUS", _ => worst.ToString() };
        }

        // --- Per-chute detail ---

        [TelemetryAPI("rc.chutes", "All Chute Status", Plotable = false, Category = "realchute", ReturnType = "object", RequiresMod = "realchute")]
        object Chutes(DataSources ds)
        {
            var modules = FindModules(ds.vessel);
            if (modules == null || _parachutesField == null) return null;

            var result = new List<Dictionary<string, object>>();
            int index = 0;

            foreach (var module in modules)
            {
                var chutes = _parachutesField.GetValue(module) as IList;
                if (chutes == null) continue;

                foreach (var chute in chutes)
                {
                    var info = new Dictionary<string, object> { ["index"] = index++ };

                    if (_deploymentStateProp != null)
                        info["state"] = _deploymentStateProp.GetValue(chute, null)?.ToString();
                    if (_isDeployedProp != null)
                        info["deployed"] = _isDeployedProp.GetValue(chute, null);
                    if (_currentAreaField != null)
                        info["currentArea"] = _currentAreaField.GetValue(chute);
                    if (_deployedDiameterField != null)
                        info["diameter"] = _deployedDiameterField.GetValue(chute);
                    if (_deploymentAltField != null)
                        info["deployAlt"] = _deploymentAltField.GetValue(chute);
                    if (_cutAltField != null)
                        info["cutAlt"] = _cutAltField.GetValue(chute);

                    result.Add(info);
                }
            }
            return result;
        }

        // --- Actions ---

        [TelemetryAPI("rc.deploy", "Deploy All Chutes", IsAction = true, Category = "realchute", ReturnType = "bool", RequiresMod = "realchute")]
        object Deploy(DataSources ds) => InvokeOnAll(ds.vessel, _guiDeploy);

        [TelemetryAPI("rc.cut", "Cut All Chutes", IsAction = true, Category = "realchute", ReturnType = "bool", RequiresMod = "realchute")]
        object Cut(DataSources ds) => InvokeOnAll(ds.vessel, _guiCut);

        [TelemetryAPI("rc.arm", "Arm All Chutes", IsAction = true, Category = "realchute", ReturnType = "bool", RequiresMod = "realchute")]
        object Arm(DataSources ds) => InvokeOnAll(ds.vessel, _guiArm);

        [TelemetryAPI("rc.disarm", "Disarm All Chutes", IsAction = true, Category = "realchute", ReturnType = "bool", RequiresMod = "realchute")]
        object Disarm(DataSources ds) => InvokeOnAll(ds.vessel, _guiDisarm);

        static object InvokeOnAll(Vessel vessel, MethodInfo method)
        {
            var modules = FindModules(vessel);
            if (modules == null || method == null) return false;
            foreach (var module in modules)
                method.Invoke(module, null);
            return true;
        }

        protected override int pausedHandler() => PausedDataLinkHandler.partPaused();
    }
}
