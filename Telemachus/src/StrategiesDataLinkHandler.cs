using System.Collections.Generic;
using System.Reflection;
using KSP.Localization;
using Strategies;
using UnityEngine;

namespace Telemachus
{
    /// <summary>Career-mode Administration Building strategies. Read and
    /// write actions; keys are global — vessel param ignored.</summary>
    public class StrategiesDataLinkHandler : DataLinkHandler
    {
        public StrategiesDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("strategies.all",
            "Every strategy (active + inactive). Per strategy: id, title, " +
            "description, departmentName, isActive, factor, dateActivated, " +
            "requiredReputation, initialCostFunds / initialCostScience / " +
            "initialCostReputation (nominal, pre-curve), " +
            "effectiveCostReputation (post-curve, the actual rep the " +
            "player loses on activate), leastDuration, longestDuration, " +
            "noDuration, hasFactorSlider, factorSliderDefault, " +
            "factorSliderSteps, canActivate, activateBlockedReason, " +
            "canDeactivate, deactivateBlockedReason, effect.",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "career",
            ReturnType = "object")]
        object All(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            var system = StrategySystem.Instance;
            if (system == null || system.Strategies == null) return result;

            foreach (var strat in system.Strategies)
            {
                if (strat == null || strat.Config == null) continue;

                string activateReason;
                bool canActivate = CanActivateReplica(strat, out activateReason);
                string deactivateReason;
                bool canDeactivate = CanDeactivateReplica(strat, out deactivateReason);

                result.Add(new Dictionary<string, object>
                {
                    ["id"] = strat.Config.Name ?? string.Empty,
                    ["title"] = strat.Title ?? string.Empty,
                    ["description"] = strat.Description ?? string.Empty,
                    ["departmentName"] = strat.DepartmentName ?? string.Empty,
                    ["isActive"] = strat.IsActive,
                    ["factor"] = strat.Factor,
                    ["dateActivated"] = strat.DateActivated,
                    ["requiredReputation"] = R4(strat.RequiredReputation),
                    ["requiredReputationMin"] = R4(strat.RequiredReputationMin),
                    ["requiredReputationMax"] = R4(strat.RequiredReputationMax),
                    ["initialCostFunds"] = R4(strat.InitialCostFunds),
                    ["initialCostFundsMin"] = R4(strat.InitialCostFundsMin),
                    ["initialCostFundsMax"] = R4(strat.InitialCostFundsMax),
                    ["initialCostScience"] = R4(strat.InitialCostScience),
                    ["initialCostReputation"] = R4(strat.InitialCostReputation),
                    ["effectiveCostReputation"] =
                        EffectiveReputationCost(strat.InitialCostReputation),
                    ["leastDuration"] = strat.LeastDuration,
                    ["longestDuration"] = strat.LongestDuration,
                    ["noDuration"] = strat.NoDuration,
                    ["hasFactorSlider"] = strat.HasFactorSlider,
                    ["factorSliderDefault"] = strat.FactorSliderDefault,
                    ["factorSliderSteps"] = strat.FactorSliderSteps,
                    ["canActivate"] = canActivate,
                    ["activateBlockedReason"] = activateReason ?? string.Empty,
                    ["canDeactivate"] = canDeactivate,
                    ["deactivateBlockedReason"] = deactivateReason ?? string.Empty,
                    ["effect"] = strat.Effect ?? string.Empty,
                });
            }
            return result;
        }

        [TelemetryAPI("strategies.activate",
            "Activate a strategy by id, setting its factor slider first. " +
            "Returns 0 on success, or an error string (strategy not found, " +
            "CanBeActivated reason, factor out of range, etc).",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "career",
            ReturnType = "object",
            Params = "string strategyId, float factor")]
        object Activate(DataSources ds)
        {
            if (ds.args == null || ds.args.Count < 1) return "missing strategy id";
            var id = ds.args[0];
            if (string.IsNullOrEmpty(id)) return "missing strategy id";

            var system = StrategySystem.Instance;
            if (system == null) return "no strategy system";

            var strat = FindStrategy(system, id);
            if (strat == null) return "strategy not found";

            // Factor is optional — defaults to the slider's preset value.
            // Pass NaN-as-string ("") or omit entirely to skip the set.
            if (ds.args.Count >= 2 && !string.IsNullOrEmpty(ds.args[1]))
            {
                if (!float.TryParse(ds.args[1], out var factor))
                    return "factor must be a float in [0, 1]";
                if (factor < 0f || factor > 1f) return "factor out of [0, 1]";
                strat.Factor = factor;
            }

            string reason;
            if (!CanActivateReplica(strat, out reason))
                return string.IsNullOrEmpty(reason) ? "cannot activate" : reason;

            if (!ActivateReplica(strat))
                return "activate failed";
            return 0;
        }

        [TelemetryAPI("strategies.deactivate",
            "Deactivate an active strategy by id. Returns 0 on success, or " +
            "an error string (strategy not found, CanBeDeactivated reason).",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "career",
            ReturnType = "object",
            Params = "string strategyId")]
        object Deactivate(DataSources ds)
        {
            if (ds.args == null || ds.args.Count < 1) return "missing strategy id";
            var id = ds.args[0];
            if (string.IsNullOrEmpty(id)) return "missing strategy id";

            var system = StrategySystem.Instance;
            if (system == null) return "no strategy system";

            var strat = FindStrategy(system, id);
            if (strat == null) return "strategy not found";

            string reason;
            if (!CanDeactivateReplica(strat, out reason))
                return string.IsNullOrEmpty(reason) ? "cannot deactivate" : reason;

            if (!DeactivateReplica(strat))
                return "deactivate failed";
            return 0;
        }

        // ── KSP-replica section ──────────────────────────────────────
        //
        // KSP's Strategy.CanBeActivated / Activate / Deactivate dereference
        // Administration.Instance, a MonoBehaviour live only while the
        // Admin Building dialog is open. That makes the stock methods
        // unusable from Telemachus when the player hasn't got the dialog
        // up — and we don't want consumers to need to drive the KSP UI
        // before calling our endpoints. The four methods below replicate
        // each check / mutation against publicly-accessible state
        // (StrategySystem, GameVariables, ScenarioUpgradeableFacilities,
        // Funding, Reputation, ResearchAndDevelopment, Planetarium) so
        // the same operations work in any scene. Behaviour matches stock
        // KSP 1.12.
        //
        // Reflection is used only to write `isActive` and `dateActivated`,
        // which stock KSP leaves private. Everything else is a public
        // API call.

        private static readonly FieldInfo IsActiveField =
            typeof(Strategy).GetField("isActive",
                BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo DateActivatedField =
            typeof(Strategy).GetField("dateActivated",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private static int AdminLevel()
        {
            return ScenarioUpgradeableFacilities.GetFacilityLevel(
                SpaceCenterFacility.Administration) > 0f ? 1 : 0;
        }

        private static int MaxActiveStrategies()
        {
            var gv = GameVariables.Instance;
            if (gv == null) return 0;
            return gv.GetActiveStrategyLimit(
                ScenarioUpgradeableFacilities.GetFacilityLevel(
                    SpaceCenterFacility.Administration));
        }

        private static float MaxCommitLevel()
        {
            var gv = GameVariables.Instance;
            if (gv == null) return 0f;
            return gv.GetStrategyCommitRange(
                ScenarioUpgradeableFacilities.GetFacilityLevel(
                    SpaceCenterFacility.Administration));
        }

        private static int ActiveStrategyCount(StrategySystem system)
        {
            int n = 0;
            foreach (var s in system.Strategies)
                if (s != null && s.IsActive) n++;
            return n;
        }

        // Mirrors Strategy.CanBeActivated. Same order and same messages.
        private static bool CanActivateReplica(Strategy s, out string reason)
        {
            reason = string.Empty;
            var system = StrategySystem.Instance;
            if (system == null) { reason = "no strategy system"; return false; }

            if (ActiveStrategyCount(system) >= MaxActiveStrategies())
            {
                reason = Localizer.Format("#autoLOC_304820", MaxActiveStrategies());
                return false;
            }
            if (system.HasConflictingActiveStrategies(s.GroupTags))
            {
                reason = Localizer.Format("#autoLOC_304827");
                return false;
            }
            if (s.Factor > MaxCommitLevel())
            {
                reason = Localizer.Format("#autoLOC_304834",
                    MaxCommitLevel() * 100f);
                return false;
            }
            if (s.InitialCostFunds != 0f
                && Funding.Instance != null
                && Funding.Instance.Funds < s.InitialCostFunds)
            {
                reason = Localizer.Format("#autoLOC_304845",
                    s.InitialCostFunds.ToString("N0"));
                return false;
            }
            if (s.InitialCostReputation != 0f
                && Reputation.Instance != null
                && Reputation.Instance.reputation < s.InitialCostReputation)
            {
                reason = Localizer.Format("#autoLOC_304854",
                    s.InitialCostReputation.ToString("N0"));
                return false;
            }
            if (s.InitialCostScience != 0f
                && !ResearchAndDevelopment.CanAfford(s.InitialCostScience))
            {
                reason = Localizer.Format("#autoLOC_304862",
                    s.InitialCostScience.ToString("N0"));
                return false;
            }
            if (s.RequiredReputation != 0f
                && Mathf.Ceil(Reputation.CurrentRep)
                   < Mathf.Floor(s.RequiredReputation))
            {
                reason = Localizer.Format("#autoLOC_304862",
                    s.RequiredReputation.ToString("N0"),
                    Reputation.CurrentRep.ToString("N0"));
                return false;
            }
            return true;
        }

        // Mirrors Strategy.CanBeDeactivated — much simpler.
        private static bool CanDeactivateReplica(Strategy s, out string reason)
        {
            reason = string.Empty;
            if (!s.IsActive) { reason = "Strategy is not active"; return false; }
            return true;
        }

        // Mirrors Strategy.Activate. Reflection writes the two private
        // fields; everything else is a public API call.
        private static bool ActivateReplica(Strategy s)
        {
            if (IsActiveField == null || DateActivatedField == null)
                return false;
            IsActiveField.SetValue(s, true);
            s.Register();
            DateActivatedField.SetValue(s, Planetarium.fetch.time);
            if (s.InitialCostFunds != 0f && Funding.Instance != null)
                Funding.Instance.AddFunds(
                    0f - Mathf.Abs(s.InitialCostFunds),
                    TransactionReasons.StrategySetup);
            if (s.InitialCostReputation != 0f && Reputation.Instance != null)
                Reputation.Instance.AddReputation(
                    0f - Mathf.Abs(s.InitialCostReputation),
                    TransactionReasons.StrategySetup);
            if (s.InitialCostScience != 0f
                && ResearchAndDevelopment.Instance != null)
                ResearchAndDevelopment.Instance.AddScience(
                    0f - Mathf.Abs(s.InitialCostScience),
                    TransactionReasons.StrategySetup);
            return true;
        }

        // Mirrors Strategy.Deactivate.
        private static bool DeactivateReplica(Strategy s)
        {
            if (IsActiveField == null) return false;
            IsActiveField.SetValue(s, false);
            s.Unregister();
            return true;
        }

        // ── end KSP-replica section ──────────────────────────────────

        private static Strategy FindStrategy(StrategySystem system, string id)
        {
            foreach (var s in system.Strategies)
            {
                if (s == null || s.Config == null) continue;
                if (s.Config.Name == id) return s;
            }
            return null;
        }

        // Compute the reputation cost the player will *actually* lose on
        // activate, after KSP's nonlinear reputation curve. KSP runs
        // Strategy.Activate's `Reputation.AddReputation(-nominalCost, …)`
        // through `Reputation.addReputation_granular`, which multiplies
        // each unit step by `GameVariables.Instance.reputationSubtraction
        // .Evaluate(rep / RepRange)`. The curve makes losing rep near the
        // cap cost significantly more than at zero — e.g. a nominal 14.5
        // cost can deduct ~27 when the player is at 976 rep. Reproduce the
        // per-unit walk here so the widget shows the actual cost, not the
        // misleading nominal.
        private static double EffectiveReputationCost(float nominalCost)
        {
            if (nominalCost == 0f) return 0d;
            var rep = Reputation.Instance;
            var gv = GameVariables.Instance;
            // No career / no game-variables → no curve to apply. Return
            // the nominal value so the field is at least meaningful.
            if (rep == null || gv == null || gv.reputationSubtraction == null)
                return System.Math.Round(nominalCost, 4);

            float current = rep.reputation;
            float remaining = System.Math.Abs(nominalCost);
            double totalLoss = 0d;
            // Same walk Reputation.addReputation_granular uses: subtract
            // one unit at a time, applying the curve at each step's
            // current rep position, accumulating the real total.
            int steps = (int)remaining;
            for (int i = 0; i < steps; i++)
            {
                float time = current / Reputation.RepRange;
                float mult = gv.reputationSubtraction.Evaluate(time);
                float delta = mult; // walking a unit-magnitude step
                totalLoss += delta;
                current -= delta;
                remaining -= 1f;
            }
            // Fractional remainder picks up the final partial step.
            if (remaining > 0f)
            {
                float time = current / Reputation.RepRange;
                float mult = gv.reputationSubtraction.Evaluate(time);
                totalLoss += remaining * mult;
            }
            return System.Math.Round(totalLoss, 4);
        }

        // Round-to-4 helper, matching the convention used by
        // KscDataLinkHandler / TechTreeDataLinkHandler for currency-style
        // floats. Doubles pass through unchanged — the formatter handles
        // them.
        private static double R4(float v) => System.Math.Round(v, 4);
    }
}
