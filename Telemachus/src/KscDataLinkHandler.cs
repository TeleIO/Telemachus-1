using System;
using System.Collections.Generic;
using System.IO;

namespace Telemachus
{
    /// <summary>Space Center state — building levels, parts catalogue, rosters, scene id. Keys are global (vessel param ignored).</summary>
    public class KscDataLinkHandler : DataLinkHandler
    {
        public KscDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        // Stock subset — mods add more facilities. Case-insensitive so callers don't have to match the exact casing.
        private static readonly Dictionary<string, string> Facilities =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["launchPad"] = "SpaceCenter/LaunchPad",
                ["runway"] = "SpaceCenter/Runway",
                ["vab"] = "SpaceCenter/VehicleAssemblyBuilding",
                ["sph"] = "SpaceCenter/SpaceplaneHangar",
                ["mission"] = "SpaceCenter/MissionControl",
                ["tracking"] = "SpaceCenter/TrackingStation",
                ["admin"] = "SpaceCenter/Administration",
                ["rd"] = "SpaceCenter/ResearchAndDevelopment",
                ["astronaut"] = "SpaceCenter/AstronautComplex",
            };

        private static double R4(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return v;
            return Math.Round(v, 4);
        }

        [TelemetryAPI("kc.scene",
            "Current KSP scene (Flight, SpaceCenter, Editor, TrackingStation, MainMenu, Other)",
            Units = APIEntry.UnitType.STRING,
            AlwaysEvaluable = true,
            Category = "ksc",
            ReturnType = "string")]
        object Scene(DataSources ds)
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.FLIGHT: return "Flight";
                case GameScenes.SPACECENTER: return "SpaceCenter";
                case GameScenes.EDITOR: return "Editor";
                case GameScenes.TRACKSTATION: return "TrackingStation";
                case GameScenes.MAINMENU: return "MainMenu";
                default: return "Other";
            }
        }

        [TelemetryAPI("kc.partsAvailable",
            "Number of parts purchasable under current tech",
            AlwaysEvaluable = true,
            Category = "ksc",
            ReturnType = "int")]
        object PartsAvailable(DataSources ds)
        {
            if (PartLoader.LoadedPartsList == null) return 0;
            var count = 0;
            foreach (var part in PartLoader.LoadedPartsList)
            {
                if (ResearchAndDevelopment.PartTechAvailable(part)) count++;
            }
            return count;
        }

        [TelemetryAPI("kc.launchSite",
            "Active flight's launch site name",
            Units = APIEntry.UnitType.STRING,
            AlwaysEvaluable = true,
            Category = "ksc",
            ReturnType = "string")]
        object LaunchSite(DataSources ds)
        {
            try { return FlightDriver.LaunchSiteName ?? string.Empty; }
            catch (Exception) { return string.Empty; }
        }

        [TelemetryAPI("kc.padOccupied",
            "True iff the active vessel is in PRELAUNCH situation",
            AlwaysEvaluable = true,
            Category = "ksc",
            ReturnType = "bool")]
        object PadOccupied(DataSources ds)
        {
            var vessel = ds.vessel;
            if (vessel == null) return false;
            return vessel.situation == Vessel.Situations.PRELAUNCH;
        }

        [TelemetryAPI("kc.padVesselTitle",
            "Vessel name when on the pad; empty otherwise",
            Units = APIEntry.UnitType.STRING,
            AlwaysEvaluable = true,
            Category = "ksc",
            ReturnType = "string")]
        object PadVesselTitle(DataSources ds)
        {
            var vessel = ds.vessel;
            if (vessel == null) return string.Empty;
            if (vessel.situation != Vessel.Situations.PRELAUNCH) return string.Empty;
            return vessel.vesselName ?? string.Empty;
        }

        [TelemetryAPI("kc.facilityLevels",
            "Per-facility { level, max, upgradeFunds } for the 9 stock SC buildings",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "ksc",
            ReturnType = "object")]
        object FacilityLevels(DataSources ds)
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in Facilities)
            {
                try
                {
                    var normalised = ScenarioUpgradeableFacilities.GetFacilityLevel(pair.Value);
                    var max = ScenarioUpgradeableFacilities.GetFacilityLevelCount(pair.Value);
                    var level = max > 0 ? (int)Math.Round(normalised * max) : 0;
                    result[pair.Key] = new Dictionary<string, object>
                    {
                        ["level"] = level,
                        ["max"] = max,
                        ["upgradeFunds"] = R4(ReadUpgradeFunds(pair.Value, level)),
                    };
                }
                catch (Exception)
                {
                    result[pair.Key] = new Dictionary<string, object>
                    {
                        ["level"] = 0,
                        ["max"] = 0,
                        ["upgradeFunds"] = 0d,
                    };
                }
            }
            return result;
        }

        // Reads UpgradeLevels[FacilityLevel + 1].levelCost via protoUpgradeables; returns 0 outside SC / at max / pre-init.
        private static double ReadUpgradeFunds(string facilityId, int currentLevel)
        {
            try
            {
                var dict = ScenarioUpgradeableFacilities.protoUpgradeables;
                if (dict == null) return 0d;
                if (!dict.TryGetValue(facilityId, out var proto) || proto == null)
                    return 0d;
                if (proto.facilityRefs == null || proto.facilityRefs.Count == 0)
                    return 0d;
                var fac = proto.facilityRefs[0];
                if (fac == null) return 0d;
                var levels = fac.UpgradeLevels;
                if (levels == null) return 0d;
                var nextIdx = fac.FacilityLevel + 1;
                if (nextIdx >= levels.Length) return 0d;
                return levels[nextIdx]?.levelCost ?? 0d;
            }
            catch (Exception)
            {
                return 0d;
            }
        }

        [TelemetryAPI("kc.crewRoster",
            "Whole-program kerbal roster — name, trait, type, gender, experience, experienceLevel, " +
            "courage, stupidity, isBadass, veteran, careerFlights, careerEntries, " +
            "currentVesselId, currentVesselName, available, unavailableReason",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "ksc",
            ReturnType = "object")]
        object CrewRoster(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            var roster = HighLogic.CurrentGame?.CrewRoster;
            if (roster == null) return result;

            // Pre-built name→vessel map keeps the per-kerbal walk O(K); empty in non-flight scenes (FlightGlobals.Vessels empty).
            var crewVesselByName = new Dictionary<string, KeyValuePair<string, string>>(
                StringComparer.Ordinal);
            var vessels = FlightGlobals.Vessels;
            if (vessels != null)
            {
                foreach (var vessel in vessels)
                {
                    if (vessel == null) continue;
                    var vid = vessel.id.ToString();
                    var vname = vessel.GetDisplayName() ?? vessel.vesselName ?? string.Empty;
                    var crew = vessel.GetVesselCrew();
                    if (crew == null) continue;
                    foreach (var member in crew)
                    {
                        if (member?.name == null) continue;
                        crewVesselByName[member.name] =
                            new KeyValuePair<string, string>(vid, vname);
                    }
                }
            }

            foreach (var kerbal in roster.Crew)
            {
                if (kerbal == null) continue;
                var status = kerbal.rosterStatus.ToString();
                var available = kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available;
                var vesselId = string.Empty;
                var vesselName = string.Empty;
                if (kerbal.name != null &&
                    crewVesselByName.TryGetValue(kerbal.name, out var pair))
                {
                    vesselId = pair.Key;
                    vesselName = pair.Value;
                }

                int careerFlights = 0;
                int careerEntries = 0;
                if (kerbal.careerLog != null)
                {
                    careerFlights = kerbal.careerLog.Flight;
                    careerEntries = kerbal.careerLog.Count;
                }

                result.Add(new Dictionary<string, object>
                {
                    ["name"] = kerbal.name ?? string.Empty,
                    ["trait"] = kerbal.trait ?? string.Empty,
                    ["type"] = kerbal.type.ToString(),
                    ["gender"] = kerbal.gender.ToString(),
                    ["experience"] = R4(kerbal.experience),
                    ["experienceLevel"] = kerbal.experienceLevel,
                    ["courage"] = R4(kerbal.courage),
                    ["stupidity"] = R4(kerbal.stupidity),
                    ["isBadass"] = kerbal.isBadass,
                    ["veteran"] = kerbal.veteran,
                    ["careerFlights"] = careerFlights,
                    ["careerEntries"] = careerEntries,
                    ["currentVesselId"] = vesselId,
                    ["currentVesselName"] = vesselName,
                    ["available"] = available,
                    ["unavailableReason"] = available ? string.Empty : status,
                });
            }
            return result;
        }

        [TelemetryAPI("kc.savedShips",
            "Saved craft files in VAB+SPH — name, partCount, totalMass, facility, requiresFunds, missingParts",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "ksc",
            ReturnType = "object")]
        object SavedShips(DataSources ds)
        {
            var result = new List<Dictionary<string, object>>();
            var saveFolder = HighLogic.SaveFolder;
            if (string.IsNullOrEmpty(saveFolder)) return result;
            var rootPath = Path.Combine(KSPUtil.ApplicationRootPath, "saves");
            rootPath = Path.Combine(rootPath, saveFolder);
            rootPath = Path.Combine(rootPath, "Ships");
            if (!Directory.Exists(rootPath)) return result;

            foreach (var facility in new[] { "VAB", "SPH" })
            {
                var dir = Path.Combine(rootPath, facility);
                if (!Directory.Exists(dir)) continue;
                foreach (var craftPath in Directory.GetFiles(dir, "*.craft"))
                {
                    result.Add(SerialiseCraftFile(craftPath, facility));
                }
            }
            return result;
        }

        private static Dictionary<string, object> SerialiseCraftFile(
            string craftPath, string facility)
        {
            var name = Path.GetFileNameWithoutExtension(craftPath);
            int partCount = 0;
            double totalMass = 0;
            double requiresFunds = 0;
            var missing = new HashSet<string>();

            try
            {
                var node = ConfigNode.Load(craftPath);
                if (node != null)
                {
                    var partNodes = node.GetNodes("PART");
                    partCount = partNodes.Length;
                    foreach (var p in partNodes)
                    {
                        WalkPart(p, ref totalMass, ref requiresFunds, missing);
                    }
                }
            }
            catch (Exception)
            {
                // Corrupt or in-progress .craft — surface partial parse rather than dropping the file.
            }

            return new Dictionary<string, object>
            {
                ["name"] = name,
                ["partCount"] = partCount,
                ["totalMass"] = R4(totalMass),
                ["facility"] = facility,
                ["requiresFunds"] = R4(requiresFunds),
                ["missingParts"] = new List<string>(missing),
            };
        }

        // .craft writes `<partName>_<flightId>`; strip only the trailing _<digits> so modded names with legit underscores survive.
        private static string ExtractPartName(ConfigNode partNode)
        {
            var raw = partNode.HasValue("name")
                ? partNode.GetValue("name")
                : partNode.HasValue("part")
                    ? partNode.GetValue("part")
                    : null;
            if (string.IsNullOrEmpty(raw)) return null;
            var underscore = raw.LastIndexOf('_');
            if (underscore <= 0) return raw;
            for (var i = underscore + 1; i < raw.Length; i++)
            {
                if (raw[i] < '0' || raw[i] > '9') return raw;
            }
            return raw.Substring(0, underscore);
        }

        private static void WalkPart(ConfigNode partNode,
            ref double totalMass, ref double requiresFunds,
            HashSet<string> missing)
        {
            var partName = ExtractPartName(partNode);
            if (partNode.HasValue("mass") &&
                double.TryParse(partNode.GetValue("mass"), out var dryMass))
                totalMass += dryMass;

            AvailablePart available = null;
            if (!string.IsNullOrEmpty(partName) && PartLoader.LoadedPartsList != null)
            {
                foreach (var ap in PartLoader.LoadedPartsList)
                {
                    if (ap != null && ap.name == partName)
                    {
                        available = ap;
                        break;
                    }
                }
            }

            if (available == null)
            {
                if (!string.IsNullOrEmpty(partName)) missing.Add(partName);
            }
            else
            {
                requiresFunds += available.cost;
                if (ResearchAndDevelopment.Instance != null &&
                    !ResearchAndDevelopment.PartTechAvailable(available))
                    missing.Add(partName);
            }

            foreach (var resNode in partNode.GetNodes("RESOURCE"))
            {
                if (!resNode.HasValue("name")) continue;
                if (!resNode.HasValue("amount")) continue;
                if (!double.TryParse(resNode.GetValue("amount"), out var amount)) continue;
                var def = PartResourceLibrary.Instance?.GetDefinition(
                    resNode.GetValue("name"));
                if (def == null) continue;
                totalMass += amount * def.density;
                requiresFunds += amount * def.unitCost;
            }
        }

        [TelemetryAPI("kc.upgradeFacility",
            "Upgrade a SC facility by short name (launchPad, vab, sph, …)",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "ksc",
            ReturnType = "object",
            Params = "string facilityShortName")]
        object UpgradeFacility(DataSources ds)
        {
            if (ds.args == null || ds.args.Count == 0) return "missing facility id";
            var shortName = ds.args[0];
            if (string.IsNullOrEmpty(shortName)) return "missing facility id";
            if (!Facilities.TryGetValue(shortName, out var facilityId))
                return "unknown facility";

            // Programmatic path (SetLevel + AddFunds) — refuse outside SC so a stray call mid-flight can't upgrade.
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                return "not in Space Center scene";

            try
            {
                var dict = ScenarioUpgradeableFacilities.protoUpgradeables;
                if (dict == null) return "no upgradeables";
                if (!dict.TryGetValue(facilityId, out var proto) || proto == null)
                    return "facility not found";
                if (proto.facilityRefs == null || proto.facilityRefs.Count == 0)
                    return "facility refs empty";
                var fac = proto.facilityRefs[0];
                if (fac == null) return "facility ref null";
                var levels = fac.UpgradeLevels;
                if (levels == null) return "no upgrade levels";
                var nextIdx = fac.FacilityLevel + 1;
                if (nextIdx >= levels.Length) return "already at max";
                var cost = (double)(levels[nextIdx]?.levelCost ?? 0f);
                if (Funding.Instance != null && Funding.Instance.Funds < cost)
                    return "insufficient funds";

                // SetLevel routes through OnUpgradeableObjLevelChange so persistence + scene state stay in sync. Funds deduction is separate — KSP only auto-charges through the SC UI.
                foreach (var refFac in proto.facilityRefs)
                {
                    refFac?.SetLevel(nextIdx);
                }
                if (Funding.Instance != null && cost > 0d)
                {
                    Funding.Instance.AddFunds(
                        -cost,
                        TransactionReasons.StructureConstruction);
                }
                return 0;
            }
            catch (Exception ex)
            {
                return "upgrade failed: " + ex.Message;
            }
        }
    }
}
