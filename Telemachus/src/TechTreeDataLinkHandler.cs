using System;
using System.Collections.Generic;
using KSP.Localization;
using KSP.UI.Screens;

namespace Telemachus
{
    /// <summary>Tech-tree state. Keys are global — vessel param ignored.</summary>
    public class TechTreeDataLinkHandler : DataLinkHandler
    {
        // Sticky-cache covers the one-frame mid-load window where every node briefly reports Unavailable.
        private static List<string> _cachedUnlockedIds;
        private static List<Dictionary<string, object>> _cachedAffordable;
        // Built once per game-load: parsed from the TechTree.cfg ConfigNode
        // and PartLoader respectively. Both are invariant within a session.
        private static Dictionary<string, string> _descriptionsByTech;
        private static Dictionary<string, List<AvailablePart>> _partsByTech;

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

        // Sticky-cache covers the same mid-load window as unlockedIds /
        // affordable: every node briefly reports Unavailable while RnD
        // rehydrates, which would clobber the prerequisite graph for any
        // subscriber that polls during that window.
        private static List<Dictionary<string, object>> _cachedNodes;

        // Build session-stable indexes for description (parsed from the
        // tree's underlying ConfigNode) and parts-per-tech (from PartLoader).
        // ResearchAndDevelopment.GetTechnologyTitle covers titles already;
        // there's no equivalent helper for descriptions, hence the cfg parse.
        // PartLoader's LoadedPartsList stamps AvailablePart.TechRequired
        // on every loaded part, so the inverse index is a single pass.
        private static void EnsureIndexes(RDTechTree tree)
        {
            if (_descriptionsByTech == null)
            {
                var dict = new Dictionary<string, string>();
                var cfg = tree.GetTreeConfigNode();
                if (cfg != null)
                {
                    foreach (var sub in cfg.GetNodes("RDNode"))
                    {
                        if (sub == null) continue;
                        var id = sub.GetValue("id");
                        if (string.IsNullOrEmpty(id)) continue;
                        var raw = sub.GetValue("description");
                        if (string.IsNullOrEmpty(raw))
                        {
                            dict[id] = string.Empty;
                            continue;
                        }
                        // Resolve #autoLOC_xxx tokens. Localizer.Format passes
                        // plain strings through unchanged, so this is safe for
                        // non-localized custom trees too.
                        dict[id] = Localizer.Format(raw);
                    }
                }
                _descriptionsByTech = dict;
            }

            if (_partsByTech == null)
            {
                var byTech = new Dictionary<string, List<AvailablePart>>();
                var list = PartLoader.LoadedPartsList;
                if (list != null)
                {
                    foreach (var p in list)
                    {
                        if (p == null || string.IsNullOrEmpty(p.TechRequired)) continue;
                        if (!byTech.TryGetValue(p.TechRequired, out var bucket))
                        {
                            bucket = new List<AvailablePart>();
                            byTech[p.TechRequired] = bucket;
                        }
                        bucket.Add(p);
                    }
                }
                _partsByTech = byTech;
            }
        }

        [TelemetryAPI("tech.nodes",
            "Full tech tree — every node with id, title, description, scienceCost, state, parents, parts",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "career",
            ReturnType = "object")]
        object Nodes(DataSources ds)
        {
            if (ResearchAndDevelopment.Instance == null) return new List<Dictionary<string, object>>();

            var tree = AssetBase.RnDTechTree;
            if (tree == null) return new List<Dictionary<string, object>>();

            if (IsTransientLoadingState() && _cachedNodes != null)
                return new List<Dictionary<string, object>>(_cachedNodes);

            EnsureIndexes(tree);

            // GetTreeNodes() returns ProtoRDNode[] — runtime tree snapshot.
            // Each ProtoRDNode has `tech` (ProtoTechNode) and `parents`
            // (List<ProtoRDNode> — elements ARE the parent nodes, no
            // wrapper). Title comes from ResearchAndDevelopment, description
            // from the cfg-parsed index, parts from the PartLoader index.
            var nodes = tree.GetTreeNodes();
            if (nodes == null) return new List<Dictionary<string, object>>();

            var result = new List<Dictionary<string, object>>();
            foreach (var node in nodes)
            {
                if (node == null || node.tech == null) continue;
                var techID = node.tech.techID;
                if (string.IsNullOrEmpty(techID)) continue;

                var parents = new List<string>();
                if (node.parents != null)
                {
                    foreach (var p in node.parents)
                    {
                        if (p == null || p.tech == null) continue;
                        var pid = p.tech.techID;
                        if (!string.IsNullOrEmpty(pid)) parents.Add(pid);
                    }
                }

                var title = ResearchAndDevelopment.GetTechnologyTitle(techID);
                if (string.IsNullOrEmpty(title)) title = techID;
                _descriptionsByTech.TryGetValue(techID, out var description);
                _partsByTech.TryGetValue(techID, out var partsList);

                var parts = new List<Dictionary<string, object>>();
                if (partsList != null)
                {
                    foreach (var p in partsList)
                    {
                        if (p == null) continue;
                        parts.Add(new Dictionary<string, object>
                        {
                            ["name"] = p.name,
                            ["title"] = p.title ?? p.name,
                            ["manufacturer"] = p.manufacturer ?? string.Empty,
                            ["category"] = p.category.ToString(),
                            ["entryCost"] = p.entryCost,
                            ["purchased"] = ResearchAndDevelopment.PartTechAvailable(p)
                                && ResearchAndDevelopment.PartModelPurchased(p),
                        });
                    }
                }

                result.Add(new Dictionary<string, object>
                {
                    ["id"] = techID,
                    ["title"] = title,
                    ["description"] = description ?? string.Empty,
                    ["scienceCost"] = node.tech.scienceCost,
                    ["state"] = ResearchAndDevelopment.GetTechnologyState(techID).ToString(),
                    ["parents"] = parents,
                    ["parts"] = parts,
                });
            }
            _cachedNodes = result;
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
