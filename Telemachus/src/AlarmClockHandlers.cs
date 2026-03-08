using System;
using System.Collections.Generic;
using System.Linq;

namespace Telemachus
{
    public class AlarmClockDataLinkHandler : DataLinkHandler
    {
        public AlarmClockDataLinkHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("alarm.count", "Number of Active Alarms")]
        object Count(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            return scenario?.alarms?.Count ?? 0;
        }

        [TelemetryAPI("alarm.list", "All Alarms", Plotable = false, Formatter = "AlarmList")]
        object AlarmList(DataSources ds)
        {
            var scenario = AlarmClockScenario.Instance;
            if (scenario?.alarms == null) return null;
            return scenario.alarms.Values.ToList();
        }

        [TelemetryAPI("alarm.nextAlarm", "Next Alarm to Trigger", Plotable = false, Formatter = "Alarm")]
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

        [TelemetryAPI("alarm.timeToNext", "Time Until Next Alarm", Units = APIEntry.UnitType.TIME)]
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
