namespace Telemachus
{
    /// <summary>Read-access to KSP's FlightLogger — same data the in-game Flight Results dialog renders. AlwaysEvaluable because the snapshot helper degrades gracefully outside flight.</summary>
    public class FlightLogHandler : DataLinkHandler
    {
        public FlightLogHandler(FormatterProvider formatters)
            : base(formatters) { }

        [TelemetryAPI("flight.events",
            "Pre-formatted event timeline from KSP's FlightLogger — the same " +
            "strings the in-game Flight Results dialog renders in the Flight " +
            "Events panel. Empty list outside flight.",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "flight",
            ReturnType = "object")]
        object Events(DataSources ds) => FlightLoggerSnapshot.CaptureEvents();

        [TelemetryAPI("flight.achievements",
            "Running mission stats from KSP's FlightLogger — same data the " +
            "Flight Results dialog renders in the Flight Achievements panel. " +
            "Fields: missionTime, liftOff, highestAltitude, highestSpeed, " +
            "highestSpeedOverLand, groundDistance, totalDistance, highestGee, " +
            "partsLost (count), kerbalsKilled (count), missionEnd, " +
            "flightEndMode. Returns null outside flight.",
            AlwaysEvaluable = true,
            Plotable = false,
            Category = "flight",
            ReturnType = "object")]
        object Achievements(DataSources ds) => FlightLoggerSnapshot.Capture();
    }
}
