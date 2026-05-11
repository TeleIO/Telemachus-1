using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Telemachus
{
    public class AlarmClockDataLinkHandler : DataLinkHandler
    {
        public AlarmClockDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("alarm.count", "Number of Active Alarms", AlwaysEvaluable = true, Category = "alarm", ReturnType = "int")]
        object Count(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            return scenario?.alarms?.Count ?? 0;
        }

        [TelemetryAPI("alarm.list", "All Alarms", AlwaysEvaluable = true, Plotable = false, Formatter = "AlarmList", Category = "alarm", ReturnType = "object")]
        object AlarmList(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            if (scenario?.alarms == null) return null;
            return scenario.alarms.Values.ToList();
        }

        [TelemetryAPI("alarm.nextAlarm", "Next Alarm to Trigger", AlwaysEvaluable = true, Plotable = false, Formatter = "Alarm", Category = "alarm", ReturnType = "object")]
        object NextAlarm(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            if (scenario?.alarms == null || scenario.alarms.Count == 0) return null;
            double now = Planetarium.GetUniversalTime();
            AlarmTypeBase nearest = null;
            double nearestTime = double.MaxValue;
            foreach (var alarm in scenario.alarms.Values)
            {
                double timeToAlarm = alarm.ut - now;
                if (timeToAlarm > 0 && timeToAlarm < nearestTime)
                {
                    nearestTime = timeToAlarm;
                    nearest = alarm;
                }
            }
            return nearest;
        }

        [TelemetryAPI("alarm.timeToNext", "Time Until Next Alarm", AlwaysEvaluable = true, Units = APIEntry.UnitType.TIME, Category = "alarm", ReturnType = "double")]
        object TimeToNext(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            if (scenario?.alarms == null || scenario.alarms.Count == 0) return -1;
            double now = Planetarium.GetUniversalTime();
            double nearestTime = double.MaxValue;
            foreach (var alarm in scenario.alarms.Values)
            {
                double timeToAlarm = alarm.ut - now;
                if (timeToAlarm > 0 && timeToAlarm < nearestTime)
                    nearestTime = timeToAlarm;
            }
            return nearestTime < double.MaxValue ? nearestTime : -1;
        }

        // Manual main-thread queue (not IsAction) so the new alarm id returns sync.
        [TelemetryAPI("alarm.add",
            "Create a stock-clock alarm. Args: title, ut, [warpAction], [message]. " +
            "warpAction = DoNothing | KillWarp | PauseGame (default KillWarp). " +
            "message = No | Yes | YesIfOtherVessel (default Yes). " +
            "Returns the new alarm's id (uint), or an error string.",
            AlwaysEvaluable = true,
            Category = "alarm",
            ReturnType = "object",
            Params = "string title, double ut, string warpAction, string message")]
        object Add(DataSources ds)
        {
            var args = ds.args;
            if (args == null || args.Count < 2)
                return "expected [title, ut, warpAction?, message?]";

            var title = args[0] ?? string.Empty;
            if (!double.TryParse(args[1], out var ut))
                return "ut not a number";

            var warpName = args.Count >= 3 ? args[2] : "KillWarp";
            var messageName = args.Count >= 4 ? args[3] : "Yes";

            if (!Enum.TryParse<AlarmActions.WarpEnum>(warpName, true, out var warp))
                return "warpAction must be DoNothing | KillWarp | PauseGame";
            if (!Enum.TryParse<AlarmActions.MessageEnum>(messageName, true, out var msg))
                return "message must be No | Yes | YesIfOtherVessel";

            var scenario = AlarmClockScenario.Instance;
            if (scenario == null) return "AlarmClock scenario not loaded";

            // Ctor assigns alarm.Id sync via GetUniqueAlarmID.
            var alarm = new AlarmTypeRaw
            {
                title = string.IsNullOrEmpty(title) ? "Telemachus Alarm" : title,
                ut = ut,
                actions =
                {
                    warp = warp,
                    message = msg,
                },
            };
            QueueOnMainThread(ds, _ =>
            {
                AlarmClockScenario.AddAlarm(alarm);
                return null;
            });
            return alarm.Id;
        }

        [TelemetryAPI("alarm.delete",
            "Remove a stock-clock alarm by id. Args: id. Returns true if " +
            "the alarm was found and queued for removal, false if no alarm " +
            "with that id exists right now.",
            AlwaysEvaluable = true,
            Category = "alarm",
            ReturnType = "bool",
            Params = "uint id")]
        object Delete(DataSources ds)
        {
            var args = ds.args;
            if (args == null || args.Count == 0) return "expected [id]";
            if (!uint.TryParse(args[0], out var id)) return "id not a uint";

            var scenario = AlarmClockScenario.Instance;
            if (scenario?.alarms == null) return false;
            if (!scenario.alarms.ContainsKey(id)) return false;

            QueueOnMainThread(ds, _ =>
            {
                AlarmClockScenario.DeleteAlarm(id);
                return null;
            });
            return true;
        }

        // Like queueDelayed but lets the caller return its own value sync.
        private static void QueueOnMainThread(DataSources ds, DataLinkHandler.APIDelegate action)
        {
            var entry = new DelayedAPIEntry(ds.Clone(), action);
            TelemachusBehaviour.instance.BroadcastMessage(
                "queueDelayedAPI",
                entry,
                SendMessageOptions.DontRequireReceiver);
        }
    }
}
