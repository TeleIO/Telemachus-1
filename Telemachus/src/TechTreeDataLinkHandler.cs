using System;
using System.Collections.Generic;

namespace Telemachus
{
    /// <summary>Tech-tree state. Keys are global — vessel param ignored.</summary>
    public class TechTreeDataLinkHandler : DataLinkHandler
    {
        // Sticky-cache covers the one-frame mid-load window where every node briefly reports Unavailable.
        private static List<string> _cachedUnlockedIds;
        private static List<Dictionary<string, object>> _cachedAffordable;

        public TechTreeDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // Tree owner is AssetBase.RnDTechTree; per-player state via GetTechnologyState(id), not ProtoTechNode.state.
        private static ProtoTechNode[] GetTreeTechs()
        {
            var tree = AssetBase.RnDTechTree;
            if (tree == null) return Array.Empty<ProtoTechNode>();
            return tree.GetTreeTechs() ?? Array.Empty<ProtoTechNode>();
        }

        // `start` always unlocked in any career save — if reported Unavailable, we're mid-load.
        private static bool IsTransientLoadingState()
        {
            if (ResearchAndDevelopment.Instance == null) return false;
            return ResearchAndDevelopment.GetTechnologyState("start")
                != RDTech.State.Available;
        }

        [TelemetryAPI("tech.unlockedIds",
            "Tech-tree node ids the player has researched",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "career",
            ReturnType = "object")]
        object UnlockedIds(DataSources ds)
        {
            if (ResearchAndDevelopment.Instance == null) return new List<string>();

            if (IsTransientLoadingState() && _cachedUnlockedIds != null)
                return new List<string>(_cachedUnlockedIds);

            var result = new List<string>();
            foreach (var node in GetTreeTechs())
            {
                if (node == null) continue;
                if (ResearchAndDevelopment.GetTechnologyState(node.techID)
                    == RDTech.State.Available)
                {
                    result.Add(node.techID);
                }
            }
            _cachedUnlockedIds = result;
            return result;
        }

        [TelemetryAPI("tech.unlockedPartCount",
            "Number of parts purchasable under current tech",
            AlwaysEvaluable = true,
            Category = "career",
            ReturnType = "int")]
        object UnlockedPartCount(DataSources ds)
        {
            if (PartLoader.LoadedPartsList == null) return 0;
            var count = 0;
            foreach (var part in PartLoader.LoadedPartsList)
            {
                if (ResearchAndDevelopment.PartTechAvailable(part)) count++;
            }
            return count;
        }

        [TelemetryAPI("tech.affordable",
            "Tech-tree nodes affordable right now (not yet unlocked, scienceCost <= current science)",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "career",
            ReturnType = "object")]
        object Affordable(DataSources ds)
        {
            var rd = ResearchAndDevelopment.Instance;
            if (rd == null) return new List<Dictionary<string, object>>();

            if (IsTransientLoadingState() && _cachedAffordable != null)
                return new List<Dictionary<string, object>>(_cachedAffordable);

            var result = new List<Dictionary<string, object>>();
            var available = rd.Science;
            foreach (var node in GetTreeTechs())
            {
                if (node == null) continue;
                if (ResearchAndDevelopment.GetTechnologyState(node.techID)
                    == RDTech.State.Available) continue;
                if (node.scienceCost > available) continue;
                result.Add(new Dictionary<string, object>
                {
                    ["id"] = node.techID,
                    ["scienceCost"] = node.scienceCost,
                });
            }
            _cachedAffordable = result;
            return result;
        }

        [TelemetryAPI("tech.unlock",
            "Unlock a tech-tree node — deducts science, marks node Available",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "career",
            ReturnType = "object",
            Params = "string techId")]
        object Unlock(DataSources ds)
        {
            if (ds.args == null || ds.args.Count == 0) return "missing tech id";
            var techId = ds.args[0];
            if (string.IsNullOrEmpty(techId)) return "missing tech id";

            var rd = ResearchAndDevelopment.Instance;
            if (rd == null) return "no R&D scenario";

            ProtoTechNode target = null;
            foreach (var node in GetTreeTechs())
            {
                if (node != null && node.techID == techId)
                {
                    target = node;
                    break;
                }
            }
            if (target == null) return "tech not found";
            if (ResearchAndDevelopment.GetTechnologyState(target.techID)
                == RDTech.State.Available) return 0; // idempotent
            if (target.scienceCost > rd.Science) return "insufficient science";

            // UnlockProtoTechNode is Unity-coupled — relies on IsAction auto-defer to run on main thread.
            ResearchAndDevelopment.Instance.AddScience(
                -target.scienceCost,
                TransactionReasons.RnDTechResearch);
            ResearchAndDevelopment.Instance.UnlockProtoTechNode(target);
            return 0;
        }
    }
}
