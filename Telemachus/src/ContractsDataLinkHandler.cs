using System;
using System.Collections.Generic;
using Contracts;

namespace Telemachus
{
    /// <summary>Contract lists + accept/decline/cancel verbs. Wire shape mirrors KSP's Contract directly; deadline arithmetic happens client-side via t.universalTime.</summary>
    public class ContractsDataLinkHandler : DataLinkHandler
    {
        private const int RECENT_LIMIT = 20;

        public ContractsDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        private static double R4(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return v;
            return Math.Round(v, 4);
        }

        [TelemetryAPI("contracts.active",
            "All Active contracts with parameters + reward shape",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "career",
            ReturnType = "object")]
        object Active(DataSources ds) => ContractsByState(Contract.State.Active);

        [TelemetryAPI("contracts.offered",
            "Contracts in Mission Control awaiting accept",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "career",
            ReturnType = "object")]
        object Offered(DataSources ds) => ContractsByState(Contract.State.Offered);

        [TelemetryAPI("contracts.completedRecent",
            "Last N completed-or-failed contracts, newest-first",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "career",
            ReturnType = "object")]
        object CompletedRecent(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            var system = ContractSystem.Instance;
            if (system == null) return result;

            var finished = system.ContractsFinished;
            if (finished == null && system.Contracts != null)
            {
                finished = new List<Contract>();
                foreach (var c in system.Contracts)
                {
                    if (c == null) continue;
                    if (c.ContractState == Contract.State.Completed ||
                        c.ContractState == Contract.State.Failed)
                        finished.Add(c);
                }
            }
            if (finished == null) return result;

            var ordered = new List<Contract>(finished);
            ordered.Sort((a, b) =>
            {
                var ad = a != null ? a.DateFinished : 0;
                var bd = b != null ? b.DateFinished : 0;
                return bd.CompareTo(ad);
            });

            for (var i = 0; i < ordered.Count && result.Count < RECENT_LIMIT; i++)
            {
                if (ordered[i] == null) continue;
                result.Add(SerialiseContract(ordered[i]));
            }
            return result;
        }

        private static List<Dictionary<string, object>> ContractsByState(Contract.State state)
        {
            var result = new List<Dictionary<string, object>>();
            var system = ContractSystem.Instance;
            if (system == null || system.Contracts == null) return result;

            foreach (var c in system.Contracts)
            {
                if (c == null || c.ContractState != state) continue;
                result.Add(SerialiseContract(c));
            }
            return result;
        }

        private static Dictionary<string, object> SerialiseContract(Contract c)
        {
            return new Dictionary<string, object>
            {
                // String — contract IDs are 64-bit and exceed JSON-number 2^53 precision.
                ["id"] = c.ContractID.ToString(),
                ["title"] = c.Title ?? string.Empty,
                ["agency"] = c.Agent != null ? c.Agent.Name : string.Empty,
                ["state"] = c.ContractState.ToString(),
                ["fundsAdvance"] = R4(c.FundsAdvance),
                ["fundsCompletion"] = R4(c.FundsCompletion),
                ["fundsFailure"] = R4(c.FundsFailure),
                ["scienceCompletion"] = R4(c.ScienceCompletion),
                ["repCompletion"] = R4(c.ReputationCompletion),
                ["deadlineUt"] = R4(c.DateExpire),
                ["parameters"] = SerialiseParameters(c),
            };
        }

        private static List<Dictionary<string, object>> SerialiseParameters(Contract c)
        {
            var result = new List<Dictionary<string, object>>();
            var parameters = c.AllParameters;
            if (parameters == null) return result;
            foreach (var p in parameters)
            {
                if (p == null) continue;
                var entry = new Dictionary<string, object>
                {
                    ["title"] = p.Title ?? string.Empty,
                    ["state"] = p.State.ToString(),
                    ["optional"] = p.Optional,
                };
                EmitTypeSpecificFields(p, entry);
                result.Add(entry);
            }
            return result;
        }

        // Merges per-subclass condition fields (altitude band, body, part name, …) so clients can render progress beyond the binary State enum.
        private static void EmitTypeSpecificFields(ContractParameter p, Dictionary<string, object> entry)
        {
            switch (p)
            {
                case Contracts.Parameters.ReachAltitudeEnvelope alt:
                    entry["parameterType"] = "ReachAltitudeEnvelope";
                    entry["minAltitude"] = R4(alt.minAltitude);
                    entry["maxAltitude"] = R4(alt.maxAltitude);
                    break;

                case Contracts.Parameters.ReachSituation sit:
                    entry["parameterType"] = "ReachSituation";
                    entry["situation"] = sit.Situation.ToString();
                    break;

                case Contracts.Parameters.ReachDestination dest:
                    entry["parameterType"] = "ReachDestination";
                    entry["body"] = dest.Destination?.bodyName ?? string.Empty;
                    break;

                case Contracts.Parameters.PartTest pt:
                    entry["parameterType"] = "PartTest";
                    entry["partName"] = pt.partName ?? string.Empty;
                    entry["body"] = pt.body ?? string.Empty;
                    entry["situation"] = pt.situation ?? string.Empty;
                    entry["hauled"] = pt.hauled;
                    break;
            }
        }

        private static Contract FindContract(IList<string> args)
        {
            if (args == null || args.Count == 0) return null;
            if (!long.TryParse(args[0], out var id)) return null;
            var system = ContractSystem.Instance;
            if (system == null || system.Contracts == null) return null;
            foreach (var c in system.Contracts)
            {
                if (c != null && c.ContractID == id) return c;
            }
            return null;
        }

        [TelemetryAPI("contracts.accept",
            "Accept an Offered contract (moves to Active)",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "career",
            ReturnType = "object",
            Params = "long contractId")]
        object Accept(DataSources ds)
        {
            if (ds.args == null || ds.args.Count == 0) return "missing contract id";
            if (!long.TryParse(ds.args[0], out _)) return "invalid contract id";
            var found = FindContract(ds.args);
            if (found == null) return "contract not found";
            if (found.ContractState == Contract.State.Active) return 0; // idempotent
            if (found.ContractState != Contract.State.Offered)
                return "contract not in Offered state";
            found.Accept();
            return 0;
        }

        [TelemetryAPI("contracts.decline",
            "Decline an Offered contract — different verb from cancel; only valid for Offered",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "career",
            ReturnType = "object",
            Params = "long contractId")]
        object Decline(DataSources ds)
        {
            if (ds.args == null || ds.args.Count == 0) return "missing contract id";
            if (!long.TryParse(ds.args[0], out _)) return "invalid contract id";
            var found = FindContract(ds.args);
            if (found == null) return "contract not found";
            if (found.ContractState != Contract.State.Offered)
                return "contract not in Offered state";
            found.Decline();
            return 0;
        }

        [TelemetryAPI("contracts.cancel",
            "Cancel an Active contract — forfeits work in progress; only valid for Active",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "career",
            ReturnType = "object",
            Params = "long contractId")]
        object Cancel(DataSources ds)
        {
            if (ds.args == null || ds.args.Count == 0) return "missing contract id";
            if (!long.TryParse(ds.args[0], out _)) return "invalid contract id";
            var found = FindContract(ds.args);
            if (found == null) return "contract not found";
            if (found.ContractState != Contract.State.Active)
                return "contract not in Active state";
            found.Cancel();
            return 0;
        }
    }
}
