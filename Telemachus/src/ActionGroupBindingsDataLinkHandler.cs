using System;
using System.Collections.Generic;

namespace Telemachus
{
    /// <summary>Reverse-index of action-group → bound parts/actions on the active vessel. Poll on demand, not at telemetry tick rate.</summary>
    public class ActionGroupBindingsDataLinkHandler : DataLinkHandler
    {
        public ActionGroupBindingsDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // Explicit list skips REPLACEWITHDEFAULT/None from Enum.GetValues and pins iteration order.
        private static readonly KSPActionGroup[] AllGroups = new[]
        {
            KSPActionGroup.Stage,
            KSPActionGroup.Gear,
            KSPActionGroup.Light,
            KSPActionGroup.RCS,
            KSPActionGroup.SAS,
            KSPActionGroup.Brakes,
            KSPActionGroup.Abort,
            KSPActionGroup.Custom01,
            KSPActionGroup.Custom02,
            KSPActionGroup.Custom03,
            KSPActionGroup.Custom04,
            KSPActionGroup.Custom05,
            KSPActionGroup.Custom06,
            KSPActionGroup.Custom07,
            KSPActionGroup.Custom08,
            KSPActionGroup.Custom09,
            KSPActionGroup.Custom10,
        };

        [TelemetryAPI("f.ag.bindings",
            "Per-action flat list of action-group bindings on the active " +
            "vessel: { actionGroup, partId, partName, partTitle, " +
            "moduleName, actionName, actionGuiName }. One row per " +
            "(action, group) pair — an action bound to two groups emits " +
            "two rows. Actions with no group (None) are omitted.",
            Plotable = false,
            Category = "control",
            ReturnType = "object")]
        object Bindings(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            var vessel = ds.vessel;
            if (vessel?.parts == null) return result;

            foreach (var part in vessel.parts)
            {
                if (part == null) continue;
                var partId = part.flightID;
                var partName = part.partInfo?.name ?? part.name ?? string.Empty;
                var partTitle = part.partInfo?.title ?? string.Empty;

                if (part.Actions != null)
                {
                    foreach (var action in part.Actions)
                    {
                        EmitIfBound(result, action, string.Empty, partId, partName, partTitle);
                    }
                }

                if (part.Modules != null)
                {
                    foreach (PartModule module in part.Modules)
                    {
                        if (module?.Actions == null) continue;
                        var moduleName = module.moduleName ?? string.Empty;
                        foreach (var action in module.Actions)
                        {
                            EmitIfBound(result, action, moduleName, partId, partName, partTitle);
                        }
                    }
                }
            }
            return result;
        }

        private static void EmitIfBound(
            List<Dictionary<string, object>> result,
            BaseAction action,
            string moduleName,
            uint partId,
            string partName,
            string partTitle)
        {
            if (action == null) return;
            var bound = action.actionGroup;
            if (bound == KSPActionGroup.None) return;

            // actionGroup is a flags bitmask — mods can set multiple bits; emit one row per matched flag.
            foreach (var ag in AllGroups)
            {
                if ((bound & ag) == 0) continue;
                result.Add(new Dictionary<string, object>
                {
                    ["actionGroup"] = ag.ToString(),
                    ["partId"] = partId,
                    ["partName"] = partName,
                    ["partTitle"] = partTitle,
                    ["moduleName"] = moduleName,
                    ["actionName"] = action.name ?? string.Empty,
                    ["actionGuiName"] = action.guiName ?? string.Empty,
                });
            }
        }
    }
}
