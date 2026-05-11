using System;
using System.Collections.Generic;
using System.IO;

namespace Telemachus
{
    /// <summary>Launch / recover / revert verbs — scene-coupled, run on the main thread via the base class's IsAction auto-defer.</summary>
    public class LaunchDataLinkHandler : DataLinkHandler
    {
        public LaunchDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("ksp.launch",
            "Load a saved craft to the chosen pad (shipName, facility, site, crewSemicolons). " +
            "Refuses outside SC/Editor, refuses if an active vessel exists.",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "launch",
            ReturnType = "object",
            Params = "string shipName, string facility, string site, string crewSemicolons")]
        object Launch(DataSources ds)
        {
            var args = ds.args;
            if (args == null || args.Count < 3)
                return "expected [shipName,facility,site,crew]";
            var shipName = args[0];
            // Saves directory uses VAB / SPH uppercase; normalise so callers don't have to match case on case-sensitive filesystems.
            var facility = args[1]?.ToUpperInvariant() ?? string.Empty;
            var site = args[2];
            var crewArg = args.Count >= 4 ? args[3] : string.Empty;

            if (string.IsNullOrEmpty(shipName)) return "missing ship name";

            if (HighLogic.LoadedScene != GameScenes.SPACECENTER &&
                HighLogic.LoadedScene != GameScenes.EDITOR)
                return "not in a launchable scene";

            // Launching while an unrecovered ActiveVessel exists wedges KSP — frozen Flight scene with maxed UT counters.
            if (FlightGlobals.ActiveVessel != null)
                return "active vessel exists — recover or revert before launching";

            var saveFolder = HighLogic.SaveFolder;
            if (string.IsNullOrEmpty(saveFolder)) return "no active save";
            var craftPath = Path.Combine(KSPUtil.ApplicationRootPath, "saves");
            craftPath = Path.Combine(craftPath, saveFolder);
            craftPath = Path.Combine(craftPath, "Ships");
            craftPath = Path.Combine(craftPath, facility);
            craftPath = Path.Combine(craftPath, shipName + ".craft");
            if (!File.Exists(craftPath)) return "craft file not found";

            // Crew uses ';' delimiter — bracket-form action args already split on ','.
            var crewNames = string.IsNullOrEmpty(crewArg)
                ? Array.Empty<string>()
                : crewArg.Split(';');

            // Always build manifest from .craft — passing null NREs inside setStartupNewVessel and frozen-HUDs Flight.
            VesselCrewManifest manifest;
            try
            {
                var craftNode = ConfigNode.Load(craftPath);
                if (craftNode == null) return "could not load craft node";
                manifest = VesselCrewManifest.FromConfigNode(craftNode);
                if (crewNames.Length > 0)
                {
                    AssignCrew(manifest, crewNames);
                }
            }
            catch (Exception ex)
            {
                return "manifest build failed: " + ex.Message;
            }

            var flagUrl = HighLogic.CurrentGame?.flagURL ?? "Squad/Flags/default";
            FlightDriver.StartWithNewLaunch(craftPath, flagUrl, site, manifest);
            return 0;
        }

        private static void AssignCrew(VesselCrewManifest manifest,
            string[] crewNames)
        {
            if (manifest == null || crewNames == null) return;
            var roster = HighLogic.CurrentGame?.CrewRoster;
            if (roster == null) return;

            var queue = new Queue<string>(crewNames);
            foreach (var partManifest in manifest.PartManifests)
            {
                if (partManifest == null) continue;
                if (queue.Count == 0) return;
                var existing = partManifest.GetPartCrew();
                if (existing == null) continue;
                for (var i = 0; i < existing.Length && queue.Count > 0; i++)
                {
                    if (existing[i] != null) continue;
                    var kerbalName = queue.Dequeue();
                    if (string.IsNullOrEmpty(kerbalName)) continue;
                    var kerbal = roster[kerbalName];
                    if (kerbal == null) continue;
                    if (kerbal.rosterStatus !=
                        ProtoCrewMember.RosterStatus.Available) continue;
                    partManifest.AddCrewToSeat(kerbal, i);
                }
            }
        }

        [TelemetryAPI("ksp.recover",
            "Recover the active vessel — only valid in PRELAUNCH / LANDED / SPLASHED",
            IsAction = true,
            Category = "launch",
            ReturnType = "object")]
        object Recover(DataSources ds)
        {
            var vessel = ds.vessel;
            if (vessel == null) return "no active vessel";
            var s = vessel.situation;
            if (s != Vessel.Situations.PRELAUNCH &&
                s != Vessel.Situations.LANDED &&
                s != Vessel.Situations.SPLASHED)
                return "vessel not in a recoverable state";

            // Fire the upper-case request event — lowercase onVesselRecovered is post-recovery notification.
            GameEvents.OnVesselRecoveryRequested.Fire(vessel);
            return 0;
        }

        [TelemetryAPI("ksp.revertToEditor",
            "Revert flight back to the named editor scene (vab|sph)",
            IsAction = true,
            Category = "launch",
            ReturnType = "object",
            Params = "string editor")]
        object RevertToEditor(DataSources ds)
        {
            if (ds.args == null || ds.args.Count == 0) return "expected [vab|sph]";
            var which = ds.args[0];
            EditorFacility facility;
            if (string.Equals(which, "vab", StringComparison.OrdinalIgnoreCase))
                facility = EditorFacility.VAB;
            else if (string.Equals(which, "sph", StringComparison.OrdinalIgnoreCase))
                facility = EditorFacility.SPH;
            else return "expected vab or sph";

            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return "not in flight";

            FlightDriver.RevertToPrelaunch(facility);
            return 0;
        }

        [TelemetryAPI("ksp.revertToLaunch",
            "Revert flight to the just-launched state (vessel back on pad, " +
            "PRELAUNCH situation). Same as the Flight Results dialog's " +
            "'Revert to Launch' button. Refuses outside Flight scene or " +
            "when the post-init snapshot isn't available.",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "launch",
            ReturnType = "object")]
        object RevertToLaunch(DataSources ds)
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return "not in flight";
            if (!FlightDriver.CanRevertToPostInit)
                return "revert-to-launch not available (no post-init snapshot)";
            FlightDriver.RevertToLaunch();
            return 0;
        }

        [TelemetryAPI("ksp.toSpaceCenter",
            "Switch to the Space Center scene. Mirrors the Flight Results " +
            "dialog's 'Space Center' button.",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "launch",
            ReturnType = "object")]
        object ToSpaceCenter(DataSources ds)
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER) return 0;
            HighLogic.LoadScene(GameScenes.SPACECENTER);
            return 0;
        }

        [TelemetryAPI("ksp.toTrackingStation",
            "Switch to the Tracking Station scene. Mirrors the Flight " +
            "Results dialog's 'Tracking Station' button.",
            AlwaysEvaluable = true,
            IsAction = true,
            Category = "launch",
            ReturnType = "object")]
        object ToTrackingStation(DataSources ds)
        {
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION) return 0;
            HighLogic.LoadScene(GameScenes.TRACKSTATION);
            return 0;
        }

        [TelemetryAPI("ksp.canRevert",
            "Whether any revert path is available right now (post-init " +
            "snapshot OR prelaunch snapshot). Mirrors FlightDriver.CanRevert.",
            AlwaysEvaluable = true,
            Category = "launch",
            ReturnType = "bool")]
        object CanRevert(DataSources ds) => FlightDriver.CanRevert;

        [TelemetryAPI("ksp.canRevertToLaunch",
            "Whether ksp.revertToLaunch would currently succeed " +
            "(FlightDriver.CanRevertToPostInit). Useful for graying " +
            "out the revert-to-launch UI when the snapshot isn't available.",
            AlwaysEvaluable = true,
            Category = "launch",
            ReturnType = "bool")]
        object CanRevertToLaunch(DataSources ds) => FlightDriver.CanRevertToPostInit;

        [TelemetryAPI("ksp.canRevertToEditor",
            "Whether ksp.revertToEditor would currently succeed " +
            "(FlightDriver.CanRevertToPrelaunch). Useful for graying " +
            "out the revert-to-editor UI when the snapshot isn't available.",
            AlwaysEvaluable = true,
            Category = "launch",
            ReturnType = "bool")]
        object CanRevertToEditor(DataSources ds) => FlightDriver.CanRevertToPrelaunch;
    }
}
