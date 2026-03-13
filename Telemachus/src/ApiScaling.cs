using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Telemachus
{
    /// <summary>
    /// Shared scaling/precision transforms for embedded controllers.
    ///
    /// Input scaling:  map an integer range to 0.0–1.0 (or any normalized range)
    /// Output scaling: round to N decimals, optionally return as integer * 10^N
    ///
    /// Used by both /api/ (APIRouteResponsibility) and /telemachus/datalink
    /// (DataLinkResponsibility) endpoints.
    /// </summary>
    public static class ApiScaling
    {
        /// <summary>
        /// Scale the first bracket argument from an integer range to 0.0–1.0.
        /// scale format: "min,max" e.g. "0,1024"
        /// Only the first comma-separated arg is scaled; the rest pass through.
        /// </summary>
        public static string ScaleInput(string args, string scale)
        {
            var scaleParts = scale.Split(',');
            if (scaleParts.Length != 2) return args;

            if (!double.TryParse(scaleParts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double scaleMin) ||
                !double.TryParse(scaleParts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double scaleMax))
                return args;

            if (Math.Abs(scaleMax - scaleMin) < double.Epsilon) return args;

            var argParts = args.Split(',');
            if (!double.TryParse(argParts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double inputVal))
                return args;

            double scaled = (inputVal - scaleMin) / (scaleMax - scaleMin);
            argParts[0] = scaled.ToString(CultureInfo.InvariantCulture);
            return string.Join(",", argParts);
        }

        /// <summary>
        /// Apply precision rounding and optional integer conversion to a result value.
        /// </summary>
        public static object ApplyOutputScaling(object result, int precision, bool asInt)
        {
            if (result == null) return result;
            if (precision < 0 || precision > 15) return result;

            double value;
            if (result is double d) value = d;
            else if (result is float f) value = f;
            else if (result is int i) value = i;
            else if (result is long l) value = l;
            else if (double.TryParse(result.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
                value = parsed;
            else return result;

            value = Math.Round(value, precision, MidpointRounding.AwayFromZero);

            if (asInt)
                return (long)(value * Math.Pow(10, precision));

            return value;
        }

        /// <summary>
        /// Parse pipe-delimited modifiers from an API string.
        /// Input:  "v.altitude|precision:2|int"  or  "f.setThrottle[512]|scale:0,1023"
        /// Returns the clean API string (without modifiers) and populates the modifiers.
        /// </summary>
        public static string ParseModifiers(string apiString, out string scale, out int precision, out bool asInt,
            string defaultScale, int defaultPrecision, bool defaultAsInt)
        {
            scale = defaultScale;
            precision = defaultPrecision;
            asInt = defaultAsInt;

            var pipeIdx = apiString.IndexOf('|');
            if (pipeIdx < 0) return apiString;

            var clean = apiString.Substring(0, pipeIdx);
            var modifiers = apiString.Substring(pipeIdx + 1).Split('|');

            foreach (var mod in modifiers)
            {
                if (mod.StartsWith("scale:"))
                    scale = mod.Substring(6);
                else if (mod.StartsWith("precision:"))
                    int.TryParse(mod.Substring(10), out precision);
                else if (mod == "int")
                    asInt = true;
            }

            return clean;
        }

        /// <summary>
        /// Apply input scaling to bracket args within an API string if a scale is set.
        /// "f.setThrottle[512]" with scale "0,1023" → "f.setThrottle[0.5...]"
        /// </summary>
        public static string ApplyInputScaling(string apiString, string scale)
        {
            if (scale == null) return apiString;

            var bracketStart = apiString.IndexOf('[');
            var bracketEnd = apiString.LastIndexOf(']');
            if (bracketStart < 0 || bracketEnd < 0 || bracketEnd <= bracketStart)
                return apiString;

            var before = apiString.Substring(0, bracketStart + 1);
            var args = apiString.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
            var after = apiString.Substring(bracketEnd);

            return before + ScaleInput(args, scale) + after;
        }
    }
}
