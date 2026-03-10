using System;
using System.Collections.Generic;
using System.Linq;

namespace Telemachus
{
    public class AlarmClockDataLinkHandler : DataLinkHandler
    {
        public AlarmClockDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("alarm.count", "Number of Active Alarms", Category = "alarm", ReturnType = "int")]
        object Count(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            return scenario?.alarms?.Count ?? 0;
        }

        [TelemetryAPI("alarm.list", "All Alarms", Plotable = false, Formatter = "AlarmList", Category = "alarm", ReturnType = "object")]
        object AlarmList(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            if (scenario?.alarms == null) return null;
            return scenario.alarms.Values.ToList();
        }

        [TelemetryAPI("alarm.nextAlarm", "Next Alarm to Trigger", Plotable = false, Formatter = "Alarm", Category = "alarm", ReturnType = "object")]
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

        [TelemetryAPI("alarm.timeToNext", "Time Until Next Alarm", Units = APIEntry.UnitType.TIME, Category = "alarm", ReturnType = "double")]
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
    }
}
